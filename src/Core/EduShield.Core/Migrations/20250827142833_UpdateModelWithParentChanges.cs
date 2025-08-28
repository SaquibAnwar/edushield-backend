using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShield.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelWithParentChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Parents_ParentId1",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_ParentId1",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentId1",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId1",
                table: "Students",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ParentId1",
                table: "Students",
                column: "ParentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Parents_ParentId1",
                table: "Students",
                column: "ParentId1",
                principalTable: "Parents",
                principalColumn: "Id");
        }
    }
}
