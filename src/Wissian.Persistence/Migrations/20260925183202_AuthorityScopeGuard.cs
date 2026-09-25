using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace Wissian.Persistence.Migrations;

public partial class AuthorityScopeGuard : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // A guessed organization URL cannot lock another organization's authority rows.
        migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION wissian.lock_current_authority() RETURNS void
                    LANGUAGE plpgsql SECURITY DEFINER SET search_path = pg_catalog AS $$
                BEGIN
                    PERFORM 1 FROM wissian.identity_users u WHERE u."Id" = wissian.current_actor() FOR SHARE;
                    PERFORM 1 FROM wissian.sessions s WHERE s."Id" = wissian.current_session()
                        AND s."UserId" = wissian.current_actor() FOR UPDATE;
                    IF NOT EXISTS (SELECT 1 FROM wissian.sessions s
                        WHERE s."Id" = wissian.current_session() AND s."UserId" = wissian.current_actor()
                            AND s."OrganizationId" = wissian.current_organization()
                            AND s."RevokedAtUtc" IS NULL AND s."ExpiresAtUtc" > CURRENT_TIMESTAMP)
                    THEN RETURN; END IF;
                    PERFORM 1 FROM wissian.organizations o WHERE o."Id" = wissian.current_organization() FOR SHARE;
                    PERFORM 1 FROM wissian.memberships m WHERE m."OrganizationId" = wissian.current_organization()
                        AND m."UserId" = wissian.current_actor() ORDER BY m."Id" FOR SHARE;
                    PERFORM 1 FROM wissian.organization_modules m WHERE m."OrganizationId" = wissian.current_organization()
                        ORDER BY m."ModuleId" FOR SHARE;
                    PERFORM 1 FROM wissian.membership_permissions p JOIN wissian.memberships m
                        ON m."OrganizationId" = p."OrganizationId" AND m."Id" = p."MembershipId"
                        WHERE p."OrganizationId" = wissian.current_organization() AND m."UserId" = wissian.current_actor()
                        ORDER BY p."Permission" FOR SHARE OF p;
                END $$;
                """);
    }
    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Preservar a fronteira de autoridade; corrigir aditivamente.");
}
