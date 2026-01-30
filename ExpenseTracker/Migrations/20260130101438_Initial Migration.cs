using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpenseTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bank");

            migrationBuilder.EnsureSchema(
                name: "accounting");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bank",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_name = table.Column<string>(type: "text", nullable: false),
                    account_number = table.Column<string>(type: "text", nullable: false),
                    bank_contact_number = table.Column<long>(type: "bigint", nullable: false),
                    ledger_id = table.Column<int>(type: "integer", nullable: false),
                    remaining_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    bank_address = table.Column<string>(type: "text", nullable: false),
                    account_open_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank", x => x.id);
                    table.ForeignKey(
                        name: "FK_bank_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bank_transactions",
                schema: "bank",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_id = table.Column<int>(type: "integer", nullable: false),
                    txn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    transaction_id = table.Column<int>(type: "integer", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_bank_transactions_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coa",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coa", x => x.id);
                    table.ForeignKey(
                        name: "FK_coa_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "income",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ledger_id = table.Column<int>(type: "integer", nullable: false),
                    dr_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    cr_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    txn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income", x => x.id);
                    table.ForeignKey(
                        name: "FK_income_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ledger",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    ledger_name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    sub_parent_id = table.Column<int>(type: "integer", nullable: true),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger", x => x.id);
                    table.ForeignKey(
                        name: "FK_ledger_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "liability",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ledger_id = table.Column<int>(type: "integer", nullable: false),
                    dr_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    cr_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    txn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liability", x => x.id);
                    table.ForeignKey(
                        name: "FK_liability_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    txn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    voucher_type = table.Column<int>(type: "integer", nullable: false),
                    voucher_no = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_reversed = table.Column<bool>(type: "boolean", nullable: false),
                    reversed_id = table.Column<int>(type: "integer", nullable: true),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_transactions_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ledger_id = table.Column<int>(type: "integer", nullable: false),
                    dr_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    cr_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    txn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.id);
                    table.ForeignKey(
                        name: "FK_expenses_ledger_ledger_id",
                        column: x => x.ledger_id,
                        principalSchema: "accounting",
                        principalTable: "ledger",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expenses_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_details",
                schema: "accounting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transaction_id = table.Column<int>(type: "integer", nullable: false),
                    ledger_id = table.Column<int>(type: "integer", nullable: false),
                    dr_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    cr_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    dr_cr = table.Column<char>(type: "character(1)", nullable: false),
                    rec_status = table.Column<char>(type: "character(1)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rec_by_id = table.Column<int>(type: "integer", nullable: false),
                    rec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_details_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalSchema: "accounting",
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_details_users_rec_by_id",
                        column: x => x.rec_by_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bank_rec_by_id",
                schema: "bank",
                table: "bank",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_rec_by_id",
                schema: "bank",
                table: "bank_transactions",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_coa_rec_by_id",
                schema: "accounting",
                table: "coa",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_ledger_id",
                schema: "accounting",
                table: "expenses",
                column: "ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_rec_by_id",
                schema: "accounting",
                table: "expenses",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_income_rec_by_id",
                schema: "accounting",
                table: "income",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_rec_by_id",
                schema: "accounting",
                table: "ledger",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_liability_rec_by_id",
                schema: "accounting",
                table: "liability",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_details_rec_by_id",
                schema: "accounting",
                table: "transaction_details",
                column: "rec_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_details_transaction_id",
                schema: "accounting",
                table: "transaction_details",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_rec_by_id",
                schema: "accounting",
                table: "transactions",
                column: "rec_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank",
                schema: "bank");

            migrationBuilder.DropTable(
                name: "bank_transactions",
                schema: "bank");

            migrationBuilder.DropTable(
                name: "coa",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "expenses",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "income",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "liability",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "transaction_details",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "ledger",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "transactions",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
