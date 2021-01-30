using Microsoft.EntityFrameworkCore.Migrations;

namespace AutonoFit.Data.Migrations
{
    public partial class DebuggedIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2de9dd41-3db2-4a86-b8f0-812f56201d17");

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Client",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "2c31d2e1-e9e2-4756-b3c1-a7dd7547b0a3", "7c48f4fd-0cf0-448d-bc31-020ff8adf08a", "Client", "CLIENT" });

            migrationBuilder.CreateIndex(
                name: "IX_Client_IdentityUserId",
                table: "Client",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Client_AspNetUsers_IdentityUserId",
                table: "Client",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Client_AspNetUsers_IdentityUserId",
                table: "Client");

            migrationBuilder.DropIndex(
                name: "IX_Client_IdentityUserId",
                table: "Client");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c31d2e1-e9e2-4756-b3c1-a7dd7547b0a3");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Client");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "2de9dd41-3db2-4a86-b8f0-812f56201d17", "13a428f2-3d7e-491a-8efb-1a7aeea0b9e7", "Client", "CLIENT" });
        }
    }
}
