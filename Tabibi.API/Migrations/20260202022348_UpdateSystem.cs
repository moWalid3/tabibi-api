using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabibi.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PricePaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "core",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "core",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "core",
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_DoctorId",
                schema: "core",
                table: "Bookings",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PatientId",
                schema: "core",
                table: "Bookings",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_CreatedAt",
                schema: "core",
                table: "Bookings",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                schema: "core",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                schema: "core",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                schema: "core",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_BookingId",
                schema: "core",
                table: "Prescriptions",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                schema: "core",
                table: "Reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_DoctorId",
                schema: "core",
                table: "Reviews",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PatientId",
                schema: "core",
                table: "Reviews",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "core");

            migrationBuilder.DropTable(
                name: "PrescriptionItems",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Prescriptions",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "core");
        }
    }
}
