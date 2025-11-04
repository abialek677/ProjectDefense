using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectDefense.Web.Migrations
{
    /// <inheritdoc />
    public partial class FinalNamingFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimespanOfSlotMinutes",
                table: "InstructorAvailabilities",
                newName: "SlotDurationMinutes");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "InstructorAvailabilities",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "InstructorAvailabilities",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorAvailabilities_RoomId_StartTime_EndTime",
                table: "InstructorAvailabilities",
                newName: "IX_InstructorAvailabilities_RoomId_StartDate_EndDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "InstructorAvailabilities",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "SlotDurationMinutes",
                table: "InstructorAvailabilities",
                newName: "TimespanOfSlotMinutes");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "InstructorAvailabilities",
                newName: "EndTime");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorAvailabilities_RoomId_StartDate_EndDate",
                table: "InstructorAvailabilities",
                newName: "IX_InstructorAvailabilities_RoomId_StartTime_EndTime");
        }
    }
}
