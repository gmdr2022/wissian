using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wissian.Documents.Migrations
{
    /// <inheritdoc />
    public partial class Documents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wissian_documents");

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "wissian_documents",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Classification = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    LatestRevision = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => new { x.OrganizationId, x.Id });
                    table.CheckConstraint("document_classification", "\"Classification\" IN ('internal', 'restricted')");
                    table.CheckConstraint("document_version", "\"Version\" > 0 AND \"LatestRevision\" BETWEEN 1 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "command_receipts",
                schema: "wissian_documents",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    AppliedVersion = table.Column<long>(type: "bigint", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    ScanState = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_receipts", x => new { x.OrganizationId, x.ActorId, x.CommandId });
                    table.ForeignKey(
                        name: "FK_command_receipts_documents_OrganizationId_DocumentId",
                        columns: x => new { x.OrganizationId, x.DocumentId },
                        principalSchema: "wissian_documents",
                        principalTable: "documents",
                        principalColumns: new[] { "OrganizationId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "revisions",
                schema: "wissian_documents",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    MediaType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Sha256 = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    ScanState = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Scanner = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ScannedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revisions", x => new { x.OrganizationId, x.DocumentId, x.Number });
                    table.CheckConstraint("revision_length", "\"Length\" BETWEEN 1 AND 10485760");
                    table.CheckConstraint("revision_number", "\"Number\" BETWEEN 1 AND 100");
                    table.CheckConstraint("revision_scan_state", "\"ScanState\" IN ('accepted', 'quarantined', 'blocked')");
                    table.ForeignKey(
                        name: "FK_revisions_documents_OrganizationId_DocumentId",
                        columns: x => new { x.OrganizationId, x.DocumentId },
                        principalSchema: "wissian_documents",
                        principalTable: "documents",
                        principalColumns: new[] { "OrganizationId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "originals",
                schema: "wissian_documents",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Bytes = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_originals", x => new { x.OrganizationId, x.DocumentId, x.Number });
                    table.CheckConstraint("original_length", "octet_length(\"Bytes\") BETWEEN 1 AND 10485760");
                    table.ForeignKey(
                        name: "FK_originals_revisions_OrganizationId_DocumentId_Number",
                        columns: x => new { x.OrganizationId, x.DocumentId, x.Number },
                        principalSchema: "wissian_documents",
                        principalTable: "revisions",
                        principalColumns: new[] { "OrganizationId", "DocumentId", "Number" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_command_receipts_OrganizationId_DocumentId",
                schema: "wissian_documents",
                table: "command_receipts",
                columns: new[] { "OrganizationId", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_OrganizationId_IsArchived_Title_Id",
                schema: "wissian_documents",
                table: "documents",
                columns: new[] { "OrganizationId", "IsArchived", "Title", "Id" });
            Protect(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Preservar originais e versões. Corrigir aditivamente.");
        }
    }
}
