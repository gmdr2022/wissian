using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wissian.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OperationAuthority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No write privilege on authority tables is granted to the runtime. This narrow
            // definer function can only lock rows in its current actor/session/organization scope.
            migrationBuilder.Sql("""
                CREATE FUNCTION wissian.current_session() RETURNS uuid
                    LANGUAGE sql STABLE AS $$ SELECT nullif(current_setting('wissian.session_id', true), '')::uuid $$;
                REVOKE ALL ON FUNCTION wissian.current_session() FROM PUBLIC;
                GRANT EXECUTE ON FUNCTION wissian.current_session() TO wissian_runtime;
                CREATE FUNCTION wissian.lock_current_authority() RETURNS void
                    LANGUAGE plpgsql SECURITY DEFINER SET search_path = pg_catalog AS $$
                BEGIN
                    PERFORM 1 FROM wissian.identity_users u WHERE u."Id" = wissian.current_actor() FOR SHARE;
                    PERFORM 1 FROM wissian.sessions s WHERE s."Id" = wissian.current_session()
                        AND s."UserId" = wissian.current_actor() FOR UPDATE;
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
                REVOKE ALL ON FUNCTION wissian.lock_current_authority() FROM PUBLIC;
                GRANT EXECUTE ON FUNCTION wissian.lock_current_authority() TO wissian_runtime;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Autoridade em uso: corrigir aditivamente, sem Down destrutivo.");
        }
    }
}
