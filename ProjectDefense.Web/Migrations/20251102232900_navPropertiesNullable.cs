using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectDefense.Web.Migrations
{
    /// <inheritdoc />
    public partial class navPropertiesNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProwadzacyId",
                table: "DostepnosciProwadzacych",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProwadzacyId",
                table: "DostepnosciProwadzacych",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
