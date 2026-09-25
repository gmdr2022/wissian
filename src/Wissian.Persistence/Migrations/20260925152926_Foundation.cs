using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wissian.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Foundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wissian");

            migrationBuilder.CreateTable(
                name: "audit_events",
                schema: "wissian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    AuthorityVersion = table.Column<long>(type: "bigint", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "identity_users",
                schema: "wissian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "wissian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "identity_claims",
                schema: "wissian",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_identity_claims_identity_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "wissian",
                        principalTable: "identity_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_logins",
                schema: "wissian",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_identity_logins_identity_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "wissian",
                        principalTable: "identity_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_tokens",
                schema: "wissian",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_identity_tokens_identity_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "wissian",
                        principalTable: "identity_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "memberships",
                schema: "wissian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memberships", x => new { x.OrganizationId, x.Id });
                    table.UniqueConstraint("AK_memberships_OrganizationId_Id_UserId", x => new { x.OrganizationId, x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_memberships_identity_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "wissian",
                        principalTable: "identity_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memberships_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "wissian",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_modules",
                schema: "wissian",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_modules", x => new { x.OrganizationId, x.ModuleId });
                    table.ForeignKey(
                        name: "FK_organization_modules_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "wissian",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membership_permissions",
                schema: "wissian",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_permissions", x => new { x.OrganizationId, x.MembershipId, x.Permission });
                    table.ForeignKey(
                        name: "FK_membership_permissions_memberships_OrganizationId_Membershi~",
                        columns: x => new { x.OrganizationId, x.MembershipId },
                        principalSchema: "wissian",
                        principalTable: "memberships",
                        principalColumns: new[] { "OrganizationId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "wissian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    MembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    MembershipVersion = table.Column<long>(type: "bigint", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.CheckConstraint("sessions_context_complete", "(\"OrganizationId\" IS NULL AND \"MembershipId\" IS NULL AND \"MembershipVersion\" IS NULL) OR (\"OrganizationId\" IS NOT NULL AND \"MembershipId\" IS NOT NULL AND \"MembershipVersion\" IS NOT NULL AND \"MembershipVersion\" > 0)");
                    table.CheckConstraint("sessions_lifetime", "\"ExpiresAtUtc\" > \"CreatedAtUtc\"");
                    table.ForeignKey(
                        name: "FK_sessions_identity_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "wissian",
                        principalTable: "identity_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sessions_memberships_OrganizationId_MembershipId_UserId",
                        columns: x => new { x.OrganizationId, x.MembershipId, x.UserId },
                        principalSchema: "wissian",
                        principalTable: "memberships",
                        principalColumns: new[] { "OrganizationId", "Id", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_OrganizationId_OccurredAtUtc",
                schema: "wissian",
                table: "audit_events",
                columns: new[] { "OrganizationId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_identity_claims_UserId",
                schema: "wissian",
                table: "identity_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_identity_logins_UserId",
                schema: "wissian",
                table: "identity_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "wissian",
                table: "identity_users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "wissian",
                table: "identity_users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_memberships_OrganizationId_UserId",
                schema: "wissian",
                table: "memberships",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_memberships_UserId",
                schema: "wissian",
                table: "memberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_Slug",
                schema: "wissian",
                table: "organizations",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_OrganizationId_MembershipId_UserId",
                schema: "wissian",
                table: "sessions",
                columns: new[] { "OrganizationId", "MembershipId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId",
                schema: "wissian",
                table: "sessions",
                column: "UserId");

            migrationBuilder.Sql("""
                REVOKE ALL ON SCHEMA wissian FROM PUBLIC;
                GRANT USAGE ON SCHEMA wissian TO wissian_runtime;
                CREATE FUNCTION wissian.current_actor() RETURNS uuid
                    LANGUAGE sql STABLE AS $$ SELECT nullif(current_setting('wissian.actor_id', true), '')::uuid $$;
                CREATE FUNCTION wissian.current_organization() RETURNS uuid
                    LANGUAGE sql STABLE AS $$ SELECT nullif(current_setting('wissian.organization_id', true), '')::uuid $$;
                REVOKE ALL ON FUNCTION wissian.current_actor(), wissian.current_organization() FROM PUBLIC;
                GRANT EXECUTE ON FUNCTION wissian.current_actor(), wissian.current_organization() TO wissian_runtime;

                ALTER TABLE wissian.memberships ENABLE ROW LEVEL SECURITY;
                CREATE POLICY actor_memberships ON wissian.memberships FOR SELECT TO wissian_runtime
                    USING ("UserId" = wissian.current_actor()
                        AND (wissian.current_organization() IS NULL OR "OrganizationId" = wissian.current_organization()));

                ALTER TABLE wissian.organizations ENABLE ROW LEVEL SECURITY;
                CREATE POLICY member_organizations ON wissian.organizations FOR SELECT TO wissian_runtime
                    USING ((wissian.current_organization() IS NULL OR "Id" = wissian.current_organization())
                        AND EXISTS (SELECT 1 FROM wissian.memberships m
                            WHERE m."OrganizationId" = "organizations"."Id"
                            AND m."UserId" = wissian.current_actor() AND m."IsActive"));

                ALTER TABLE wissian.organization_modules ENABLE ROW LEVEL SECURITY;
                CREATE POLICY scoped_modules ON wissian.organization_modules FOR SELECT TO wissian_runtime
                    USING ("OrganizationId" = wissian.current_organization()
                        AND EXISTS (SELECT 1 FROM wissian.memberships m
                            WHERE m."OrganizationId" = "organization_modules"."OrganizationId"
                            AND m."UserId" = wissian.current_actor() AND m."IsActive"));

                ALTER TABLE wissian.membership_permissions ENABLE ROW LEVEL SECURITY;
                CREATE POLICY own_permissions ON wissian.membership_permissions FOR SELECT TO wissian_runtime
                    USING ("OrganizationId" = wissian.current_organization()
                        AND EXISTS (SELECT 1 FROM wissian.memberships m
                            WHERE m."OrganizationId" = "membership_permissions"."OrganizationId"
                            AND m."Id" = "membership_permissions"."MembershipId"
                            AND m."UserId" = wissian.current_actor() AND m."IsActive"));

                ALTER TABLE wissian.sessions ENABLE ROW LEVEL SECURITY;
                CREATE POLICY own_sessions ON wissian.sessions TO wissian_runtime
                    USING ("UserId" = wissian.current_actor())
                    WITH CHECK ("UserId" = wissian.current_actor());

                ALTER TABLE wissian.audit_events ENABLE ROW LEVEL SECURITY;
                CREATE POLICY actor_audit ON wissian.audit_events TO wissian_runtime
                    USING ("ActorId" = wissian.current_actor()
                        AND "OrganizationId" IS NOT DISTINCT FROM wissian.current_organization())
                    WITH CHECK ("ActorId" = wissian.current_actor()
                        AND "OrganizationId" IS NOT DISTINCT FROM wissian.current_organization());

                GRANT SELECT ON wissian.organizations, wissian.memberships,
                    wissian.organization_modules, wissian.membership_permissions TO wissian_runtime;
                GRANT SELECT, INSERT, UPDATE ON wissian.sessions TO wissian_runtime;
                GRANT SELECT, INSERT ON wissian.audit_events TO wissian_runtime;
                -- Identity is global authentication metadata, not an organization data module.
                GRANT SELECT, INSERT, UPDATE, DELETE ON wissian.identity_users, wissian.identity_claims,
                    wissian.identity_logins, wissian.identity_tokens TO wissian_runtime;
                GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA wissian TO wissian_runtime;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Não executar Down destrutivo; recuperar backup em outro destino ou aplicar correção aditiva.");
        }
    }
}
