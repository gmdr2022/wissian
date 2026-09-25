using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wissian.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RuntimeSchemaRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("GRANT SELECT ON wissian.schema_history TO wissian_runtime;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Reverter compatibilidade por migração aditiva, sem alterar o histórico aplicado.");
        }
    }
}
