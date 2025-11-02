using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;
using ProjectDefense.Web.Services;
using QuestPDF.Infrastructure;
using System.Globalization;
using Microsoft.AspNetCore.Identity.UI.Services;
using ProjectDefense.Shared.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja licencji QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// Dodanie DbContext z PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

// Konfiguracja Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Ustawienia hasła
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Ustawienia blokady konta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Ustawienia użytkownika
    options.User.RequireUniqueEmail = true;
    
    // Wymaganie potwierdzenia email
    options.SignIn.RequireConfirmedEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Konfiguracja SendGrid
builder.Services.Configure<SendGridOptions>(
    builder.Configuration.GetSection("SendGrid"));
builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();

// Dodanie Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Prowadzacy", "RequireProwadzacyRole");
    options.Conventions.AuthorizeFolder("/Student", "RequireStudentRole");
});

// Dodanie kontrolerów dla API
builder.Services.AddControllers();

// Konfiguracja Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Project Defense API",
        Version = "v1",
        Description = "API for Project Defense reservation system"
    });
});

// Konfiguracja internacjonalizacji
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("pl-PL"),
        new CultureInfo("en-US")
    };
    
    options.DefaultRequestCulture = new RequestCulture("pl-PL");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});

// Autoryzacja z politykami
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireProwadzacyRole", policy =>
        policy.RequireRole("Prowadzący"));
    options.AddPolicy("RequireStudentRole", policy =>
        policy.RequireRole("Student"));
});

builder.Services.AddScoped<ExportService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ProjectDefense.Web")));

var app = builder.Build();

// Inicjalizacja bazy danych i ról
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Wykonanie migracji
        await context.Database.MigrateAsync();
        
        // Utworzenie ról
        if (!await roleManager.RoleExistsAsync("Prowadzący"))
        {
            await roleManager.CreateAsync(new IdentityRole("Prowadzący"));
        }
        
        if (!await roleManager.RoleExistsAsync("Student"))
        {
            await roleManager.CreateAsync(new IdentityRole("Student"));
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Konfiguracja pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Project Defense API v1");
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware lokalizacji
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Minimal API endpoints
app.MapGet("/api/slots/available", async (ApplicationDbContext db) =>
{
    var now = DateTime.UtcNow;
    var availableSlots = await db.Rezerwacje
        .Include(r => r.DostepnoscProwadzacego)
        .ThenInclude(d => d.Sala)
        .Include(r => r.DostepnoscProwadzacego)
        .ThenInclude(d => d.Prowadzacy)
        .Where(r => r.StudentId == null && 
                    r.IsActive && 
                    r.CzasRozpoczecia > now &&
                    !r.DostepnoscProwadzacego.IsBlocked)
        .Select(r => new SlotDto
        {
            Id = r.Id,
            SalaId = r.DostepnoscProwadzacego.SalaId,
            NazwaSali = r.DostepnoscProwadzacego.Sala.Nazwa,
            CzasRozpoczecia = r.CzasRozpoczecia,
            CzasZakonczenia = r.CzasZakonczenia,
            IsAvailable = true,
            ProwadzacyName = r.DostepnoscProwadzacego.Prowadzacy.FirstName + " " + 
                            r.DostepnoscProwadzacego.Prowadzacy.LastName
        })
        .OrderBy(s => s.CzasRozpoczecia)
        .ToListAsync();
    
    return Results.Ok(availableSlots);
})
.WithName("GetAvailableSlots")
.WithTags("Slots")
.Produces<List<SlotDto>>(200);

app.MapGet("/api/rooms", async (ApplicationDbContext db) =>
{
    var rooms = await db.Sale
        .Where(s => s.IsActive)
        .Select(s => new RoomDto
        {
            Id = s.Id,
            Nazwa = s.Nazwa,
            NumerSali = s.NumerSali
        })
        .ToListAsync();
    
    return Results.Ok(rooms);
})
.WithName("GetRooms")
.WithTags("Rooms")
.Produces<List<RoomDto>>(200);

app.MapPost("/api/slots/{id}/book", async (
    int id,
    BookingRequestDto request,
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager) =>
{
    var reservation = await db.Rezerwacje
        .Include(r => r.DostepnoscProwadzacego)
        .FirstOrDefaultAsync(r => r.Id == id);
    
    if (reservation == null)
        return Results.NotFound("Slot not found");
    
    if (reservation.StudentId != null)
        return Results.BadRequest("Slot already booked");
    
    if (reservation.CzasRozpoczecia <= DateTime.UtcNow)
        return Results.BadRequest("Cannot book past slots");
    
    if (reservation.DostepnoscProwadzacego.IsBlocked)
        return Results.BadRequest("This slot is blocked");
    
    // Sprawdź czy student już ma rezerwację
    var existingReservation = await db.Rezerwacje
        .AnyAsync(r => r.StudentId == request.StudentId && 
                      r.IsActive && 
                      r.CzasRozpoczecia > DateTime.UtcNow);
    
    if (existingReservation)
        return Results.BadRequest("Student already has a reservation");
    
    // Sprawdź czy student nie jest zbanowany
    var isBanned = await db.BlokadyStudentow
        .AnyAsync(b => b.StudentId == request.StudentId && b.IsActive);
    
    if (isBanned)
        return Results.BadRequest("Student is banned");
    
    reservation.StudentId = request.StudentId;
    reservation.DataRezerwacji = DateTime.UtcNow;
    
    await db.SaveChangesAsync();
    
    return Results.Ok(new { Message = "Booking successful", ReservationId = reservation.Id });
})
.WithName("BookSlot")
.WithTags("Slots")
.Produces(200)
.Produces(400)
.Produces(404);

app.Run();
