using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShield.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveComputedPropertiesFromStudentFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountDue",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "FineAmount",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "StudentFees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountDue",
                table: "StudentFees",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "StudentFees",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FineAmount",
                table: "StudentFees",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "StudentFees",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
