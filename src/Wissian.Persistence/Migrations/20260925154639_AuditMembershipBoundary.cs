using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wissian.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditMembershipBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER POLICY actor_audit ON wissian.audit_events
                USING (
                    "ActorId" = wissian.current_actor()
                    AND "OrganizationId" IS NOT DISTINCT FROM wissian.current_organization()
                    AND ("OrganizationId" IS NULL OR EXISTS (
                        SELECT 1 FROM wissian.memberships m
                        JOIN wissian.organizations o ON o."Id" = m."OrganizationId"
                        WHERE m."OrganizationId" = "audit_events"."OrganizationId"
                        AND m."UserId" = wissian.current_actor() AND m."IsActive" AND o."IsActive")))
                WITH CHECK (
                    "ActorId" = wissian.current_actor()
                    AND "OrganizationId" IS NOT DISTINCT FROM wissian.current_organization()
                    AND ("OrganizationId" IS NULL OR EXISTS (
                        SELECT 1 FROM wissian.memberships m
                        JOIN wissian.organizations o ON o."Id" = m."OrganizationId"
                        WHERE m."OrganizationId" = "audit_events"."OrganizationId"
                        AND m."UserId" = wissian.current_actor() AND m."IsActive" AND o."IsActive")));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Não remover a fronteira de auditoria; corrigir aditivamente.");
        }
    }
}
