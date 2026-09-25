using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wissian.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ContextSelectionReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_sessions_Id_UserId",
                schema: "wissian",
                table: "sessions",
                columns: new[] { "Id", "UserId" });

            migrationBuilder.CreateTable(
                name: "context_receipts",
                schema: "wissian",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipVersion = table.Column<long>(type: "bigint", nullable: false),
                    RequestHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    AppliedVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_context_receipts", x => new { x.SessionId, x.CommandId });
                    table.CheckConstraint("context_receipts_versions", "\"AppliedVersion\" > 0 AND \"MembershipVersion\" > 0");
                    table.ForeignKey(
                        name: "FK_context_receipts_memberships_OrganizationId_MembershipId_Us~",
                        columns: x => new { x.OrganizationId, x.MembershipId, x.UserId },
                        principalSchema: "wissian",
                        principalTable: "memberships",
                        principalColumns: new[] { "OrganizationId", "Id", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_context_receipts_sessions_SessionId_UserId",
                        columns: x => new { x.SessionId, x.UserId },
                        principalSchema: "wissian",
                        principalTable: "sessions",
                        principalColumns: new[] { "Id", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_context_receipts_OrganizationId_MembershipId_UserId",
                schema: "wissian",
                table: "context_receipts",
                columns: new[] { "OrganizationId", "MembershipId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_context_receipts_SessionId_UserId",
                schema: "wissian",
                table: "context_receipts",
                columns: new[] { "SessionId", "UserId" });

            var currentAuthority = """
                "UserId" = wissian.current_actor()
                AND "OrganizationId" = wissian.current_organization()
                AND EXISTS (SELECT 1 FROM wissian.identity_users u
                    WHERE u."Id" = wissian.current_actor() AND u."IsActive")
                AND EXISTS (SELECT 1 FROM wissian.memberships m
                    JOIN wissian.organizations o ON o."Id" = m."OrganizationId"
                    WHERE m."OrganizationId" = "context_receipts"."OrganizationId"
                    AND m."Id" = "context_receipts"."MembershipId"
                    AND m."UserId" = wissian.current_actor() AND m."IsActive" AND o."IsActive"
                    AND m."Version" = "context_receipts"."MembershipVersion")
                AND EXISTS (SELECT 1 FROM wissian.sessions s WHERE s."Id" = "context_receipts"."SessionId"
                    AND s."UserId" = wissian.current_actor() AND s."RevokedAtUtc" IS NULL
                    AND s."ExpiresAtUtc" > CURRENT_TIMESTAMP)
                """;
            migrationBuilder.Sql($"""
                ALTER TABLE wissian.context_receipts ENABLE ROW LEVEL SECURITY;
                CREATE POLICY scoped_context_receipts ON wissian.context_receipts TO wissian_runtime
                USING ({currentAuthority})
                WITH CHECK ({currentAuthority}
                    AND EXISTS (SELECT 1 FROM wissian.sessions s WHERE s."Id" = "context_receipts"."SessionId"
                        AND s."OrganizationId" = "context_receipts"."OrganizationId"
                        AND s."MembershipId" = "context_receipts"."MembershipId"
                        AND s."MembershipVersion" = "context_receipts"."MembershipVersion"
                        AND s."Version" = "context_receipts"."AppliedVersion"));
                GRANT SELECT, INSERT ON wissian.context_receipts TO wissian_runtime;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Recibos são histórico; corrigir aditivamente sem Down destrutivo.");
        }
    }
}
