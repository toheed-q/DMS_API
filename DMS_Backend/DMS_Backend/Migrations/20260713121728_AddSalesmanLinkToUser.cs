using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesmanLinkToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesmanId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SalesmanId",
                table: "Users",
                column: "SalesmanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Salesmen_SalesmanId",
                table: "Users",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "SalesmanId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Salesmen_SalesmanId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SalesmanId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SalesmanId",
                table: "Users");
        }
    }
}
