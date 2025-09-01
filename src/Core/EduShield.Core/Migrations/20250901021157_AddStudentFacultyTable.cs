using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShield.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentFacultyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentFaculties_Faculty_FacultyId",
                table: "StudentFaculties");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFaculties_Students_StudentId",
                table: "StudentFaculties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentFaculties",
                table: "StudentFaculties");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "StudentFaculties");

            migrationBuilder.RenameTable(
                name: "StudentFaculties",
                newName: "StudentFaculty");

            migrationBuilder.RenameIndex(
                name: "IX_StudentFaculties_FacultyId",
                table: "StudentFaculty",
                newName: "IX_StudentFaculty_FacultyId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "StudentFaculty",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "StudentFaculty",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Semester",
                table: "StudentFaculty",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "StudentFaculty",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentFaculty",
                table: "StudentFaculty",
                columns: new[] { "StudentId", "FacultyId" });

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFaculty_Faculty_FacultyId",
                table: "StudentFaculty",
                column: "FacultyId",
                principalTable: "Faculty",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFaculty_Students_StudentId",
                table: "StudentFaculty",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentFaculty_Faculty_FacultyId",
                table: "StudentFaculty");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFaculty_Students_StudentId",
                table: "StudentFaculty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentFaculty",
                table: "StudentFaculty");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "StudentFaculty");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "StudentFaculty");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "StudentFaculty");

            migrationBuilder.RenameTable(
                name: "StudentFaculty",
                newName: "StudentFaculties");

            migrationBuilder.RenameIndex(
                name: "IX_StudentFaculty_FacultyId",
                table: "StudentFaculties",
                newName: "IX_StudentFaculties_FacultyId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "StudentFaculties",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "StudentFaculties",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentFaculties",
                table: "StudentFaculties",
                columns: new[] { "StudentId", "FacultyId" });

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFaculties_Faculty_FacultyId",
                table: "StudentFaculties",
                column: "FacultyId",
                principalTable: "Faculty",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFaculties_Students_StudentId",
                table: "StudentFaculties",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
