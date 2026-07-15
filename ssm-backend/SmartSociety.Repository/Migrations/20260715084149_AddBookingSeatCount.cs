using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSociety.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSeatCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_FacilityId",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "SeatsBooked",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FacilityId_StartTime_EndTime",
                table: "Bookings",
                columns: new[] { "FacilityId", "StartTime", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_FacilityId_StartTime_EndTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SeatsBooked",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FacilityId",
                table: "Bookings",
                column: "FacilityId");
        }
    }
}
