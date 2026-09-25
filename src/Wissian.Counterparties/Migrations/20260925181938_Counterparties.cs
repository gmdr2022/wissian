using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wissian.Counterparties.Migrations
{
    /// <inheritdoc />
    public partial class Counterparties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wissian_counterparties");

            migrationBuilder.CreateTable(
                name: "counterparties",
                schema: "wissian_counterparties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Relation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counterparties", x => new { x.OrganizationId, x.Id });
                    table.CheckConstraint("counterparty_kind", "\"Kind\" IN ('organization', 'person')");
                    table.CheckConstraint("counterparty_relation", "\"Relation\" IN ('customer', 'supplier', 'both', 'other')");
                    table.CheckConstraint("counterparty_version", "\"Version\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "command_receipts",
                schema: "wissian_counterparties",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CounterpartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    AppliedVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_receipts", x => new { x.OrganizationId, x.ActorId, x.CommandId });
                    table.ForeignKey(
                        name: "FK_command_receipts_counterparties_OrganizationId_Counterparty~",
                        columns: x => new { x.OrganizationId, x.CounterpartyId },
                        principalSchema: "wissian_counterparties",
                        principalTable: "counterparties",
                        principalColumns: new[] { "OrganizationId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_command_receipts_OrganizationId_CounterpartyId",
                schema: "wissian_counterparties",
                table: "command_receipts",
                columns: new[] { "OrganizationId", "CounterpartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_counterparties_OrganizationId_IsArchived_Name_Id",
                schema: "wissian_counterparties",
                table: "counterparties",
                columns: new[] { "OrganizationId", "IsArchived", "Name", "Id" });

            migrationBuilder.Sql("""
                REVOKE ALL ON SCHEMA wissian_counterparties FROM PUBLIC;
                GRANT USAGE ON SCHEMA wissian_counterparties TO wissian_runtime;
                GRANT SELECT ON wissian_counterparties.schema_history TO wissian_runtime;
                ALTER TABLE wissian_counterparties.counterparties ADD CONSTRAINT counterparties_organization_fk
                    FOREIGN KEY ("OrganizationId") REFERENCES wissian.organizations ("Id") ON DELETE RESTRICT;
                CREATE FUNCTION wissian_counterparties.allowed(required_permission text) RETURNS boolean
                LANGUAGE sql STABLE SET search_path = pg_catalog AS $$
                    SELECT EXISTS (
                        SELECT 1 FROM wissian.sessions s
                        JOIN wissian.identity_users u ON u."Id" = s."UserId" AND u."IsActive"
                        JOIN wissian.memberships m ON m."OrganizationId" = s."OrganizationId"
                            AND m."Id" = s."MembershipId" AND m."UserId" = u."Id" AND m."IsActive"
                            AND m."Version" = s."MembershipVersion" AND m."CreatedAtUtc" <= CURRENT_TIMESTAMP
                        JOIN wissian.organizations o ON o."Id" = m."OrganizationId" AND o."IsActive"
                            AND o."CreatedAtUtc" <= CURRENT_TIMESTAMP
                        JOIN wissian.organization_modules x ON x."OrganizationId" = o."Id"
                            AND x."ModuleId" = 'counterparties' AND x."IsEligible" AND x."IsEnabled"
                        JOIN wissian.membership_permissions p ON p."OrganizationId" = o."Id"
                            AND p."MembershipId" = m."Id" AND p."IsActive" AND p."Permission" = required_permission
                        WHERE s."Id" = wissian.current_session() AND s."UserId" = wissian.current_actor()
                            AND s."OrganizationId" = wissian.current_organization()
                            AND s."RevokedAtUtc" IS NULL AND s."ExpiresAtUtc" > CURRENT_TIMESTAMP
                            AND s."CreatedAtUtc" <= CURRENT_TIMESTAMP
                    )
                $$;
                REVOKE ALL ON FUNCTION wissian_counterparties.allowed(text) FROM PUBLIC;
                GRANT EXECUTE ON FUNCTION wissian_counterparties.allowed(text) TO wissian_runtime;
                ALTER TABLE wissian_counterparties.counterparties ENABLE ROW LEVEL SECURITY;
                ALTER TABLE wissian_counterparties.command_receipts ENABLE ROW LEVEL SECURITY;
                CREATE POLICY counterparties_read ON wissian_counterparties.counterparties FOR SELECT TO wissian_runtime
                    USING ("OrganizationId" = wissian.current_organization() AND wissian_counterparties.allowed('counterparties.read'));
                CREATE POLICY counterparties_insert ON wissian_counterparties.counterparties FOR INSERT TO wissian_runtime
                    WITH CHECK ("OrganizationId" = wissian.current_organization() AND wissian_counterparties.allowed('counterparties.read')
                        AND wissian_counterparties.allowed('counterparties.write'));
                CREATE POLICY counterparties_update ON wissian_counterparties.counterparties FOR UPDATE TO wissian_runtime
                    USING ("OrganizationId" = wissian.current_organization() AND wissian_counterparties.allowed('counterparties.write'))
                    WITH CHECK ("OrganizationId" = wissian.current_organization() AND wissian_counterparties.allowed('counterparties.read')
                        AND wissian_counterparties.allowed('counterparties.write'));
                CREATE POLICY receipts_read ON wissian_counterparties.command_receipts FOR SELECT TO wissian_runtime
                    USING ("OrganizationId" = wissian.current_organization() AND "ActorId" = wissian.current_actor()
                        AND wissian_counterparties.allowed('counterparties.read') AND wissian_counterparties.allowed('counterparties.write'));
                CREATE POLICY receipts_insert ON wissian_counterparties.command_receipts FOR INSERT TO wissian_runtime
                    WITH CHECK ("OrganizationId" = wissian.current_organization() AND "ActorId" = wissian.current_actor()
                        AND wissian_counterparties.allowed('counterparties.read') AND wissian_counterparties.allowed('counterparties.write'));
                GRANT SELECT, INSERT ON wissian_counterparties.counterparties TO wissian_runtime;
                GRANT UPDATE ("Name", "Kind", "Relation", "Reference", "IsArchived", "Version", "UpdatedAtUtc")
                    ON wissian_counterparties.counterparties TO wissian_runtime;
                GRANT SELECT, INSERT ON wissian_counterparties.command_receipts TO wissian_runtime;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Preservar cadastros e referências. Corrigir aditivamente.");
        }
    }
}
