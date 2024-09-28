using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookSpot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addcompanytoidentityuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComapanyId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ComapanyId",
                table: "AspNetUsers",
                column: "ComapanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Companies_ComapanyId",
                table: "AspNetUsers",
                column: "ComapanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Companies_ComapanyId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ComapanyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ComapanyId",
                table: "AspNetUsers");
        }
    }
}
