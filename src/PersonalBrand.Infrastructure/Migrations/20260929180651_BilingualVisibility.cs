using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalBrand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BilingualVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Workshops",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TranslationGroupId",
                table: "Sessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TranslationGroupId",
                table: "Lessons",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Articles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Keep existing rows and IDs intact. Missing translations become hidden drafts
            // for an editor to complete; never invent translated public content.
            foreach (var table in new[] { "Articles", "Courses", "Projects", "Workshops" })
            {
                migrationBuilder.Sql($"""
                    ;WITH duplicates AS (
                        SELECT *, ROW_NUMBER() OVER (PARTITION BY TranslationGroupId, Language ORDER BY Id) AS n
                        FROM [{table}] WHERE TranslationGroupId IS NOT NULL
                    ) UPDATE duplicates SET TranslationGroupId = NEWID() WHERE n > 1;
                    UPDATE [{table}] SET TranslationGroupId = NEWID() WHERE TranslationGroupId IS NULL;
                    DECLARE @columns nvarchar(max), @expressions nvarchar(max), @sql nvarchar(max);
                    SELECT @columns = STRING_AGG(CAST(QUOTENAME(name) AS nvarchar(max)), ',') WITHIN GROUP (ORDER BY column_id),
                           @expressions = STRING_AGG(CAST(CASE name
                               WHEN 'Id' THEN 'NEWID()'
                               WHEN 'Language' THEN 'CASE source.Language WHEN ''en'' THEN ''fa'' ELSE ''en'' END'
                               WHEN 'Slug' THEN '''translation-'' + REPLACE(CONVERT(varchar(36), NEWID()), ''-'', '''')'
                               WHEN 'Title' THEN 'CASE source.Language WHEN ''fa'' THEN ''English translation required'' ELSE ''Persian translation required'' END'
                               WHEN 'Body' THEN ''''''
                               WHEN 'Status' THEN '0'
                               WHEN 'IsVisible' THEN '0'
                               WHEN 'PublishedAt' THEN 'NULL'
                               ELSE 'source.' + QUOTENAME(name) END AS nvarchar(max)), ',') WITHIN GROUP (ORDER BY column_id)
                    FROM sys.columns WHERE object_id = OBJECT_ID('[{table}]');
                    SET @sql = 'INSERT INTO [{table}] (' + @columns + ') SELECT ' + @expressions +
                        ' FROM [{table}] source WHERE NOT EXISTS (SELECT 1 FROM [{table}] target WHERE target.TranslationGroupId = source.TranslationGroupId AND target.Language <> source.Language)';
                    EXEC sp_executesql @sql;
                    """);
            }
            BackfillChildren(migrationBuilder, "Lessons", "Courses", "CourseId", "Order", "ContentBody", "IsPublished");
            BackfillChildren(migrationBuilder, "Sessions", "Workshops", "WorkshopId", "SessionNumber", "Description", "IsVisible");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_TranslationGroupId_Language",
                table: "Workshops",
                columns: new[] { "TranslationGroupId", "Language" },
                unique: true,
                filter: "[TranslationGroupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TranslationGroupId_Language",
                table: "Projects",
                columns: new[] { "TranslationGroupId", "Language" },
                unique: true,
                filter: "[TranslationGroupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TranslationGroupId_Language",
                table: "Courses",
                columns: new[] { "TranslationGroupId", "Language" },
                unique: true,
                filter: "[TranslationGroupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_TranslationGroupId_Language",
                table: "Articles",
                columns: new[] { "TranslationGroupId", "Language" },
                unique: true,
                filter: "[TranslationGroupId] IS NOT NULL");
        }

        private static void BackfillChildren(MigrationBuilder migrationBuilder, string table, string parents, string parentId, string order, string body, string visible)
        {
            migrationBuilder.Sql($"""
                UPDATE child SET TranslationGroupId = NEWID()
                FROM [{table}] child JOIN [{parents}] parent ON parent.Id = child.[{parentId}]
                WHERE parent.Language = 'en' AND child.TranslationGroupId IS NULL;
                UPDATE fa SET TranslationGroupId = en.TranslationGroupId
                FROM [{table}] fa
                JOIN [{parents}] fp ON fp.Id = fa.[{parentId}] AND fp.Language = 'fa'
                JOIN [{parents}] ep ON ep.TranslationGroupId = fp.TranslationGroupId AND ep.Language = 'en'
                JOIN [{table}] en ON en.[{parentId}] = ep.Id AND en.[{order}] = fa.[{order}]
                WHERE fa.TranslationGroupId IS NULL;
                UPDATE [{table}] SET TranslationGroupId = NEWID() WHERE TranslationGroupId IS NULL;
                DECLARE @columns nvarchar(max), @expressions nvarchar(max), @sql nvarchar(max);
                SELECT @columns = STRING_AGG(CAST(QUOTENAME(name) AS nvarchar(max)), ',') WITHIN GROUP (ORDER BY column_id),
                    @expressions = STRING_AGG(CAST(CASE name
                        WHEN 'Id' THEN 'NEWID()'
                        WHEN '{parentId}' THEN 'targetParent.Id'
                        WHEN 'Title' THEN 'CASE targetParent.Language WHEN ''en'' THEN ''English translation required'' ELSE ''Persian translation required'' END'
                        WHEN '{body}' THEN ''''''
                        WHEN '{visible}' THEN '0'
                        ELSE 'source.' + QUOTENAME(name) END AS nvarchar(max)), ',') WITHIN GROUP (ORDER BY column_id)
                FROM sys.columns WHERE object_id = OBJECT_ID('[{table}]');
                SET @sql = 'INSERT INTO [{table}] (' + @columns + ') SELECT ' + @expressions +
                    ' FROM [{table}] source JOIN [{parents}] parent ON parent.Id = source.[{parentId}] JOIN [{parents}] targetParent ON targetParent.TranslationGroupId = parent.TranslationGroupId AND targetParent.Language <> parent.Language WHERE NOT EXISTS (SELECT 1 FROM [{table}] target WHERE target.[{parentId}] = targetParent.Id AND target.TranslationGroupId = source.TranslationGroupId)';
                EXEC sp_executesql @sql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workshops_TranslationGroupId_Language",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TranslationGroupId_Language",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TranslationGroupId_Language",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Articles_TranslationGroupId_Language",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "TranslationGroupId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TranslationGroupId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Articles");
        }
    }
}
