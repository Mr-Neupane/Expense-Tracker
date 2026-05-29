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

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_bank_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
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
                        name: "FK_bank_transactions_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
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
                        name: "FK_coa_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
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
                        name: "FK_income_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
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
                        name: "FK_ledger_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
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
                        name: "FK_liability_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
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
                        name: "FK_transactions_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
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
                        name: "FK_expenses_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expenses_ledger_ledger_id",
                        column: x => x.ledger_id,
                        principalSchema: "accounting",
                        principalTable: "ledger",
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
                        name: "FK_transaction_details_AspNetUsers_rec_by_id",
                        column: x => x.rec_by_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_details_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalSchema: "accounting",
                        principalTable: "transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_role_id",
                table: "AspNetRoleClaims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_user_id",
                table: "AspNetUserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_user_id",
                table: "AspNetUserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_role_id",
                table: "AspNetUserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "normalized_user_name",
                unique: true);

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
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

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
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ledger",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "transactions",
                schema: "accounting");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
