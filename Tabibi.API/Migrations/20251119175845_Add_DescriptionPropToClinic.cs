using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabibi.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_DescriptionPropToClinic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "core",
                table: "Clinics",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "core",
                table: "Clinics");
        }
    }
}
