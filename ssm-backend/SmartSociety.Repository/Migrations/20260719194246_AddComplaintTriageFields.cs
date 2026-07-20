using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSociety.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintTriageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Complaints",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DraftAdminResponse",
                table: "Complaints",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PossibleDuplicateIdsCsv",
                table: "Complaints",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Complaints",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TriageProcessed",
                table: "Complaints",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "DraftAdminResponse",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "PossibleDuplicateIdsCsv",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "TriageProcessed",
                table: "Complaints");
        }
    }
}
