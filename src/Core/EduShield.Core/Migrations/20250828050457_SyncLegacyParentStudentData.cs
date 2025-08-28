using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShield.Core.Migrations
{
    /// <inheritdoc />
    public partial class SyncLegacyParentStudentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sync existing Student.ParentId relationships to ParentStudent table
            migrationBuilder.Sql(@"
                INSERT INTO ""ParentStudents"" (""ParentId"", ""StudentId"", ""Relationship"", ""IsPrimaryContact"", ""IsAuthorizedToPickup"", ""IsEmergencyContact"", ""IsActive"", ""Notes"", ""CreatedAt"", ""UpdatedAt"")
                SELECT 
                    s.""ParentId"",
                    s.""Id"",
                    'Parent' as ""Relationship"",
                    true as ""IsPrimaryContact"",
                    true as ""IsAuthorizedToPickup"",
                    true as ""IsEmergencyContact"",
                    true as ""IsActive"",
                    'Migrated from legacy parent relationship' as ""Notes"",
                    s.""CreatedAt"",
                    NOW() as ""UpdatedAt""
                FROM ""Students"" s
                WHERE s.""ParentId"" IS NOT NULL
                AND NOT EXISTS (
                    SELECT 1 FROM ""ParentStudents"" ps 
                    WHERE ps.""ParentId"" = s.""ParentId"" 
                    AND ps.""StudentId"" = s.""Id""
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the migrated relationships (only those with the specific notes)
            migrationBuilder.Sql(@"
                DELETE FROM ""ParentStudents"" 
                WHERE ""Notes"" = 'Migrated from legacy parent relationship';
            ");
        }
    }
}
