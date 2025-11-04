using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjectDefense.Web.Migrations
{
    /// <inheritdoc />
    public partial class PropertyRenameToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlokadyStudentow");

            migrationBuilder.DropTable(
                name: "Rezerwacje");

            migrationBuilder.DropTable(
                name: "DostepnosciProwadzacych");

            migrationBuilder.DropTable(
                name: "Sale");

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RoomNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<string>(type: "text", nullable: false),
                    BlockReason = table.Column<string>(type: "text", nullable: false),
                    BlockDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BlockingInstructorId = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentBlocks_AspNetUsers_BlockingInstructorId",
                        column: x => x.BlockingInstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentBlocks_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstructorAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstructorId = table.Column<string>(type: "text", nullable: true),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartHour = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndHour = table.Column<TimeSpan>(type: "interval", nullable: false),
                    TimespanOfSlotMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorAvailabilities_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstructorAvailabilities_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstructorAvailabilityId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StudentId = table.Column<string>(type: "text", nullable: true),
                    ReservationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reservations_InstructorAvailabilities_InstructorAvailabilit~",
                        column: x => x.InstructorAvailabilityId,
                        principalTable: "InstructorAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstructorAvailabilities_InstructorId",
                table: "InstructorAvailabilities",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorAvailabilities_RoomId_StartTime_EndTime",
                table: "InstructorAvailabilities",
                columns: new[] { "RoomId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_InstructorAvailabilityId",
                table: "Reservations",
                column: "InstructorAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_StartTime",
                table: "Reservations",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_StudentId",
                table: "Reservations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBlocks_BlockingInstructorId",
                table: "StudentBlocks",
                column: "BlockingInstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBlocks_StudentId",
                table: "StudentBlocks",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "StudentBlocks");

            migrationBuilder.DropTable(
                name: "InstructorAvailabilities");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.CreateTable(
                name: "BlokadyStudentow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlokowalProwadzacyId = table.Column<string>(type: "text", nullable: false),
                    StudentId = table.Column<string>(type: "text", nullable: false),
                    DataBlokady = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Powod = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlokadyStudentow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlokadyStudentow_AspNetUsers_BlokowalProwadzacyId",
                        column: x => x.BlokowalProwadzacyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BlokadyStudentow_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Nazwa = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NumerSali = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DostepnosciProwadzacych",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProwadzacyId = table.Column<string>(type: "text", nullable: true),
                    SalaId = table.Column<int>(type: "integer", nullable: false),
                    CzasTrwaniaSlotuWMin = table.Column<int>(type: "integer", nullable: false),
                    DataKoncowa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPoczatkowa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GodzinaRozpoczecia = table.Column<TimeSpan>(type: "interval", nullable: false),
                    GodzinaZakonczenia = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DostepnosciProwadzacych", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DostepnosciProwadzacych_AspNetUsers_ProwadzacyId",
                        column: x => x.ProwadzacyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DostepnosciProwadzacych_Sale_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rezerwacje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DostepnoscProwadzacegoId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<string>(type: "text", nullable: true),
                    CzasRozpoczecia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CzasZakonczenia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataRezerwacji = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezerwacje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rezerwacje_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Rezerwacje_DostepnosciProwadzacych_DostepnoscProwadzacegoId",
                        column: x => x.DostepnoscProwadzacegoId,
                        principalTable: "DostepnosciProwadzacych",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlokadyStudentow_BlokowalProwadzacyId",
                table: "BlokadyStudentow",
                column: "BlokowalProwadzacyId");

            migrationBuilder.CreateIndex(
                name: "IX_BlokadyStudentow_StudentId",
                table: "BlokadyStudentow",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_DostepnosciProwadzacych_ProwadzacyId",
                table: "DostepnosciProwadzacych",
                column: "ProwadzacyId");

            migrationBuilder.CreateIndex(
                name: "IX_DostepnosciProwadzacych_SalaId_DataPoczatkowa_DataKoncowa",
                table: "DostepnosciProwadzacych",
                columns: new[] { "SalaId", "DataPoczatkowa", "DataKoncowa" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezerwacje_CzasRozpoczecia",
                table: "Rezerwacje",
                column: "CzasRozpoczecia");

            migrationBuilder.CreateIndex(
                name: "IX_Rezerwacje_DostepnoscProwadzacegoId",
                table: "Rezerwacje",
                column: "DostepnoscProwadzacegoId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezerwacje_StudentId",
                table: "Rezerwacje",
                column: "StudentId");
        }
    }
}
