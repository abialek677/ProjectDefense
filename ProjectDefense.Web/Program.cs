using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;
using ProjectDefense.Web.Services;
using ProjectDefense.Web.Services.Email;
using ProjectDefense.Web.Services.Export;
using ProjectDefense.Shared.DTOs;
using QuestPDF.Infrastructure;
using System.Globalization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF setup
QuestPDF.Settings.License = LicenseType.Community;


// Identity configuration
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password = new()
    {
        RequireDigit = true,
        RequireLowercase = true,
        RequireUppercase = true,
        RequireNonAlphanumeric = true,
        RequiredLength = 8
    };

    options.Lockout = new()
    {
        DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5),
        MaxFailedAccessAttempts = 5,
        AllowedForNewUsers = true
    };

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// SendGrid setup
builder.Services.Configure<SendGridOptions>(
    builder.Configuration.GetSection("SendGrid"));
builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();

// Razor Pages & role-based auth
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Instructor", "RequireInstructorRole");
    options.Conventions.AuthorizeFolder("/Student", "RequireStudentRole");
});

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Project Defense API", 
        Version = "v1",
        Description = "API for Project Defense Reservation System"
    });
});

// Localization setup
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("pl-PL"), new CultureInfo("en-US") };
    options.DefaultRequestCulture = new RequestCulture("pl-PL");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireInstructorRole", p => p.RequireRole("Instructor"));
    options.AddPolicy("RequireStudentRole", p => p.RequireRole("Student"));
});

builder.Services.AddScoped<ExportService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ProjectDefense.Shared")
            .EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)));

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("pl-PL"), new CultureInfo("en-US") };
    options.DefaultRequestCulture = new RequestCulture("pl-PL");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});


var app = builder.Build();

// Database initialization & roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        foreach (var role in new[] { "Instructor", "Student" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }
    catch (Exception ex)
    {
        services.GetRequiredService<ILogger<Program>>()
            .LogError(ex, "Error while migrating or seeding the database.");
    }
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Project Defense API v1"));
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Minimal API: available slots
app.MapGet("/api/slots/available", async (ApplicationDbContext db) =>
{
    var now = DateTime.UtcNow;
    var availableSlots = await db.Reservations
        .Include(r => r.InstructorAvailability.Room)
        .Include(r => r.InstructorAvailability.Instructor)
        .Where(r => r.StudentId == null && r.IsActive && r.StartTime > now && !r.InstructorAvailability.IsBlocked)
        .Select(r => new SlotDto
        {
            Id = r.Id,
            RoomId = r.InstructorAvailability.RoomId,
            RoomName = r.InstructorAvailability.Room.Name,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            IsAvailable = true,
            InstructorName = r.InstructorAvailability.Instructor.FirstName + " " +
                             r.InstructorAvailability.Instructor.LastName
        })
        .OrderBy(s => s.StartTime)
        .ToListAsync();

    return Results.Ok(availableSlots);
})
.WithName("GetAvailableSlots")
.WithTags("Slots");

// Minimal API: rooms
app.MapGet("/api/rooms", async (ApplicationDbContext db) =>
{
    var rooms = await db.Rooms
        .Where(s => s.IsActive)
        .Select(s => new RoomDto
        {
            Id = s.Id,
            Name = s.Name,
            RoomNumber = s.RoomNumber
        })
        .ToListAsync();

    return Results.Ok(rooms);
})
.WithName("GetRooms")
.WithTags("Rooms");

// Minimal API: booking
app.MapPost("/api/slots/{id}/book", async (
    int id,
    BookingRequestDto request,
    ApplicationDbContext db) =>
{
    var reservation = await db.Reservations
        .Include(r => r.InstructorAvailability)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (reservation is null)
        return Results.NotFound("Slot not found");

    if (reservation.StudentId is not null)
        return Results.BadRequest("Slot already booked");

    if (reservation.StartTime <= DateTime.UtcNow)
        return Results.BadRequest("Cannot book past slots");

    if (reservation.InstructorAvailability.IsBlocked)
        return Results.BadRequest("Slot is blocked");

    var hasActiveReservation = await db.Reservations
        .AnyAsync(r => r.StudentId == request.StudentId && r.IsActive && r.StartTime > DateTime.UtcNow);

    if (hasActiveReservation)
        return Results.BadRequest("Student already has an active reservation");

    var isBanned = await db.StudentBlocks
        .AnyAsync(b => b.StudentId == request.StudentId && b.IsActive);

    if (isBanned)
        return Results.BadRequest("Student is banned");

    reservation.StudentId = request.StudentId;
    reservation.ReservationDate = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new { Message = "Booking successful", ReservationId = reservation.Id });
})
.WithName("BookSlot")
.WithTags("Slots");

app.Run();
