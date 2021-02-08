using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AutonoFit.Migrations
{
    public partial class UpdatedProgramModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2da45c9-0df7-457a-aea0-5589fcd293cf");

            migrationBuilder.AddColumn<DateTime>(
                name: "programStart",
                table: "ClientProgram",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "fa0b38a2-f53d-44af-b19c-5fbbeca5424f", "b7f5d7e2-9f6e-45a8-8455-28ea27834615", "Client", "CLIENT" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fa0b38a2-f53d-44af-b19c-5fbbeca5424f");

            migrationBuilder.DropColumn(
                name: "programStart",
                table: "ClientProgram");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "c2da45c9-0df7-457a-aea0-5589fcd293cf", "77fa7989-7f68-46bb-a20c-12432888fabe", "Client", "CLIENT" });
        }
    }
}
