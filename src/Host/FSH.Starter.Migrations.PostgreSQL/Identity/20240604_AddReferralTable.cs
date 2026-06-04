using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Identity;

/// <inheritdoc />
public partial class AddReferralTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Referrals",
            schema: "identity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ReferralCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                ReferrerUserId = table.Column<string>(type: "text", nullable: false),
                ReferredUserId = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Referrals", x => x.Id);
                table.ForeignKey(
                    name: "FK_Referrals_Users_ReferrerUserId",
                    column: x => x.ReferrerUserId,
                    principalSchema: "identity",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Referrals_Users_ReferredUserId",
                    column: x => x.ReferredUserId,
                    principalSchema: "identity",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_ReferralCode",
            schema: "identity",
            table: "Referrals",
            column: "ReferralCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_ReferrerUserId",
            schema: "identity",
            table: "Referrals",
            column: "ReferrerUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_ReferredUserId",
            schema: "identity",
            table: "Referrals",
            column: "ReferredUserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Referrals",
            schema: "identity");
    }
}