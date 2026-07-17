using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAccountActiveToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccountActive",
                table: "Users",
                type: "bit",
                nullable: true);

            // New accounts are inactive by default, but existing accounts (the admin
            // and anyone already using the system) must keep working — activate them all.
            migrationBuilder.Sql("UPDATE [Users] SET [IsAccountActive] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccountActive",
                table: "Users");
        }
    }
}
