using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TenderAnalytics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    identifier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenders",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expected_amount = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    cpv_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    procuring_entity_identifier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    procuring_entity_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    tender_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    award_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.id);
                    table.ForeignKey(
                        name: "FK_contracts_tenders_tender_id",
                        column: x => x.tender_id,
                        principalTable: "tenders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contract_suppliers",
                columns: table => new
                {
                    contract_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_suppliers", x => new { x.contract_id, x.supplier_id });
                    table.ForeignKey(
                        name: "FK_contract_suppliers_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contract_suppliers_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contract_suppliers_supplier_id",
                table: "contract_suppliers",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_award_id",
                table: "contracts",
                column: "award_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_tender_id",
                table: "contracts",
                column: "tender_id");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_normalized_name",
                table: "suppliers",
                column: "normalized_name");

            migrationBuilder.CreateIndex(
                name: "ux_suppliers_identifier",
                table: "suppliers",
                column: "identifier",
                unique: true,
                filter: "\"identifier\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tenders_cpv_status_date_created",
                table: "tenders",
                columns: new[] { "cpv_code", "status", "date_created" });

            migrationBuilder.CreateIndex(
                name: "ix_tenders_procuring_entity",
                table: "tenders",
                columns: new[] { "procuring_entity_identifier", "procuring_entity_name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_suppliers");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "tenders");
        }
    }
}
