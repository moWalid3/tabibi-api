using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabibi.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_StatusPropToDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "identity",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                schema: "identity",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);
        }
    }
}
