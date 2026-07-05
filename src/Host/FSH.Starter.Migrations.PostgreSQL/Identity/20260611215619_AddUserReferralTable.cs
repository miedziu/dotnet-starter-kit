using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Identity
{
    /// <inheritdoc />
    public partial class AddUserReferralTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Referrals table with composite primary key (ReferrerUserId, NewReferredUserId)
            migrationBuilder.CreateTable(
                name: "Referrals",
                schema: "identity",
                columns: table => new
                {
                    ReferrerUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NewReferredUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => new { x.ReferrerUserId, x.NewReferredUserId });
                    table.ForeignKey(
                        name: "FK_Referrals_Users_NewReferredUserId",
                        column: x => x.NewReferredUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Referrals_Users_ReferrerUserId",
                        column: x => x.ReferrerUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Referrals",
                schema: "identity");
        }
    }
}
