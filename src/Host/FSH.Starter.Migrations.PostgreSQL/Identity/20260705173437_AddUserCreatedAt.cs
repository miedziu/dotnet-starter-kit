using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Identity
{
    /// <inheritdoc />
    public partial class AddUserCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "identity",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<short?>(
                name: "VoivodeshipId",
                schema: "identity",
                table: "Users",
                type: "smallint",
                nullable: true,
                comment: "Voivodeship ID (administrative division)");

            migrationBuilder.AddColumn<short?>(
                name: "DistrictId",
                schema: "identity",
                table: "Users",
                type: "smallint",
                nullable: true,
                comment: "District ID (administrative division)");

            migrationBuilder.AddColumn<short?>(
                name: "CommuneId",
                schema: "identity",
                table: "Users",
                type: "smallint",
                nullable: true,
                comment: "Commune ID (administrative division)");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_NewReferredUserId",
                schema: "identity",
                table: "Referrals",
                column: "NewReferredUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Referrals_NewReferredUserId",
                schema: "identity",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "CommuneId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VoivodeshipId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "identity",
                table: "Users");
        }
    }
}