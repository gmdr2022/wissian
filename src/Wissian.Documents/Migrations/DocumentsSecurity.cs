using Microsoft.EntityFrameworkCore.Migrations;

namespace Wissian.Documents.Migrations;

public partial class Documents
{
    private static void Protect(MigrationBuilder migration)
    {
        migration.Sql("""
            REVOKE ALL ON SCHEMA wissian_documents FROM PUBLIC;
            GRANT USAGE ON SCHEMA wissian_documents TO wissian_runtime;
            GRANT SELECT ON wissian_documents.schema_history TO wissian_runtime;
            ALTER TABLE wissian_documents.documents ADD CONSTRAINT documents_organization_fk
                FOREIGN KEY ("OrganizationId") REFERENCES wissian.organizations ("Id") ON DELETE RESTRICT;
            CREATE FUNCTION wissian_documents.allowed(required_permission text) RETURNS boolean
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
                        AND x."ModuleId" = 'documents' AND x."IsEligible" AND x."IsEnabled"
                    JOIN wissian.membership_permissions p ON p."OrganizationId" = o."Id"
                        AND p."MembershipId" = m."Id" AND p."IsActive" AND p."Permission" = required_permission
                    WHERE s."Id" = wissian.current_session() AND s."UserId" = wissian.current_actor()
                        AND s."OrganizationId" = wissian.current_organization()
                        AND s."RevokedAtUtc" IS NULL AND s."ExpiresAtUtc" > CURRENT_TIMESTAMP
                        AND s."CreatedAtUtc" <= CURRENT_TIMESTAMP
                )
            $$;
            REVOKE ALL ON FUNCTION wissian_documents.allowed(text) FROM PUBLIC;
            GRANT EXECUTE ON FUNCTION wissian_documents.allowed(text) TO wissian_runtime;
            ALTER TABLE wissian_documents.documents ENABLE ROW LEVEL SECURITY;
            CREATE POLICY documents_read ON wissian_documents.documents FOR SELECT TO wissian_runtime
                USING ("OrganizationId" = wissian.current_organization() AND wissian_documents.allowed('documents.read')
                    AND ("Classification" = 'internal' OR wissian_documents.allowed('documents.restricted')));
            CREATE POLICY documents_insert ON wissian_documents.documents FOR INSERT TO wissian_runtime
                WITH CHECK ("OrganizationId" = wissian.current_organization() AND wissian_documents.allowed('documents.read')
                    AND wissian_documents.allowed('documents.write')
                    AND ("Classification" = 'internal' OR wissian_documents.allowed('documents.restricted')));
            CREATE POLICY documents_update ON wissian_documents.documents FOR UPDATE TO wissian_runtime
                USING ("OrganizationId" = wissian.current_organization() AND wissian_documents.allowed('documents.write'))
                WITH CHECK ("OrganizationId" = wissian.current_organization() AND wissian_documents.allowed('documents.read')
                    AND wissian_documents.allowed('documents.write')
                    AND ("Classification" = 'internal' OR wissian_documents.allowed('documents.restricted')));
            GRANT SELECT, INSERT ON wissian_documents.documents TO wissian_runtime;
            GRANT UPDATE ("Title", "Classification", "Source", "IsArchived", "Version", "LatestRevision", "UpdatedAtUtc")
                ON wissian_documents.documents TO wissian_runtime;
            """);
        foreach (var table in new[] { "revisions", "originals", "command_receipts" })
        {
            // Identifiers here are compiled constants. Document visibility applies before counts/bytes/receipts.
            var visible = $"\"OrganizationId\" = wissian.current_organization() AND EXISTS (SELECT 1 FROM wissian_documents.documents d WHERE d.\"OrganizationId\" = {table}.\"OrganizationId\" AND d.\"Id\" = {table}.\"DocumentId\")";
            var select = visible;
            var insert = visible + " AND wissian_documents.allowed('documents.write')";
            if (table == "command_receipts") select = insert += " AND \"ActorId\" = wissian.current_actor()";
            if (table == "originals") select += " AND (wissian_documents.allowed('documents.write') OR EXISTS (SELECT 1 FROM wissian_documents.revisions r WHERE r.\"OrganizationId\" = originals.\"OrganizationId\" AND r.\"DocumentId\" = originals.\"DocumentId\" AND r.\"Number\" = originals.\"Number\" AND r.\"ScanState\" = 'accepted' AND r.\"ScannedAtUtc\" BETWEEN CURRENT_TIMESTAMP - INTERVAL '24 hours' AND CURRENT_TIMESTAMP))";
            migration.Sql($"""
                ALTER TABLE wissian_documents.{table} ENABLE ROW LEVEL SECURITY;
                CREATE POLICY {table}_read ON wissian_documents.{table} FOR SELECT TO wissian_runtime USING ({select});
                CREATE POLICY {table}_insert ON wissian_documents.{table} FOR INSERT TO wissian_runtime WITH CHECK ({insert});
                GRANT SELECT, INSERT ON wissian_documents.{table} TO wissian_runtime;
                """);
            if (table == "revisions") migration.Sql($"""
                CREATE POLICY revisions_scan ON wissian_documents.revisions FOR UPDATE TO wissian_runtime
                    USING ({insert}) WITH CHECK ({insert});
                GRANT UPDATE ("ScanState", "Scanner", "ScannedAtUtc") ON wissian_documents.revisions TO wissian_runtime;
                """);
        }
        migration.Sql("""
            -- Quota enforcement sees every classification but returns no totals. Serialize all inserts
            -- for the organization so concurrent actors cannot each consume the same remaining space.
            CREATE FUNCTION wissian_documents.enforce_limits() RETURNS trigger
            LANGUAGE plpgsql SECURITY DEFINER SET search_path = pg_catalog AS $$
            BEGIN
                IF NEW."OrganizationId" IS DISTINCT FROM wissian.current_organization()
                    OR NOT wissian_documents.allowed('documents.write') OR NOT wissian_documents.allowed('documents.read') THEN
                    RAISE EXCEPTION 'Document authority required' USING ERRCODE = '42501';
                END IF;
                PERFORM pg_advisory_xact_lock(hashtextextended('wissian_documents:' || NEW."OrganizationId"::text, 0));
                IF TG_TABLE_NAME = 'documents' THEN
                    IF (SELECT count(*) FROM wissian_documents.documents WHERE "OrganizationId" = NEW."OrganizationId") >= 1000 THEN
                        RAISE EXCEPTION 'Document quota exceeded' USING ERRCODE = '23514', CONSTRAINT = 'document_quota';
                    END IF;
                ELSE
                    IF COALESCE((SELECT sum(octet_length("Bytes")) FROM wissian_documents.originals
                        WHERE "OrganizationId" = NEW."OrganizationId"), 0) + octet_length(NEW."Bytes") > 104857600 THEN
                        RAISE EXCEPTION 'Document quota exceeded' USING ERRCODE = '23514', CONSTRAINT = 'document_quota';
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM wissian_documents.revisions r WHERE r."OrganizationId" = NEW."OrganizationId"
                        AND r."DocumentId" = NEW."DocumentId" AND r."Number" = NEW."Number"
                        AND r."Length" = octet_length(NEW."Bytes") AND r."Sha256" = upper(encode(sha256(NEW."Bytes"), 'hex'))) THEN
                        RAISE EXCEPTION 'Original integrity mismatch' USING ERRCODE = '23514', CONSTRAINT = 'original_integrity';
                    END IF;
                END IF;
                RETURN NEW;
            END $$;
            REVOKE ALL ON FUNCTION wissian_documents.enforce_limits() FROM PUBLIC;
            CREATE TRIGGER documents_limit BEFORE INSERT ON wissian_documents.documents
                FOR EACH ROW EXECUTE FUNCTION wissian_documents.enforce_limits();
            CREATE TRIGGER originals_integrity BEFORE INSERT ON wissian_documents.originals
                FOR EACH ROW EXECUTE FUNCTION wissian_documents.enforce_limits();
            """);
    }
}
