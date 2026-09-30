using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalBrand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChildTranslationUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sessions_WorkshopId_TranslationGroupId",
                table: "Sessions",
                columns: new[] { "WorkshopId", "TranslationGroupId" },
                unique: true,
                filter: "[TranslationGroupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_CourseId_TranslationGroupId",
                table: "Lessons",
                columns: new[] { "CourseId", "TranslationGroupId" },
                unique: true,
                filter: "[TranslationGroupId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_WorkshopId_TranslationGroupId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_CourseId_TranslationGroupId",
                table: "Lessons");
        }
    }
}
