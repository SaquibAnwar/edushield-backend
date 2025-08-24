using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShield.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentPerformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExamType = table.Column<int>(type: "integer", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EncryptedScore = table.Column<string>(type: "text", nullable: false),
                    Comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ExamTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPerformances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPerformances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentPerformances_StudentId_Subject_ExamType_ExamDate",
                table: "StudentPerformances",
                columns: new[] { "StudentId", "Subject", "ExamType", "ExamDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentPerformances");
        }
    }
}
