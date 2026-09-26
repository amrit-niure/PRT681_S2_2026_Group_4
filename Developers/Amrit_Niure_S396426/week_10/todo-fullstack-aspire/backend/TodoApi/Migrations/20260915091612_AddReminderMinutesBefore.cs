using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderMinutesBefore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReminderMinutesBefore",
                table: "TodoItems",
                type: "integer",
                nullable: true);

            // Preserve the old "remind me on the due date" behavior for tasks that
            // already had a due date before this feature existed.
            migrationBuilder.Sql(
                """UPDATE "TodoItems" SET "ReminderMinutesBefore" = 0 WHERE "DueDate" IS NOT NULL""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderMinutesBefore",
                table: "TodoItems");
        }
    }
}
