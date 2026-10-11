using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_brary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinesAndFineDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Returned (1) và Lost (2) của phiếu cũ đều trở thành Closed (1).
            migrationBuilder.Sql("UPDATE \"BorrowRecords\" SET \"Status\" = 1 WHERE \"Status\" = 2;");

            migrationBuilder.CreateTable(
                name: "Fines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fines_BorrowRecords_BorrowRecordId",
                        column: x => x.BorrowRecordId,
                        principalTable: "BorrowRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FineDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FineId = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FineDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FineDetails_BorrowDetails_BorrowDetailId",
                        column: x => x.BorrowDetailId,
                        principalTable: "BorrowDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FineDetails_Fines_FineId",
                        column: x => x.FineId,
                        principalTable: "Fines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FineDetails_BorrowDetailId",
                table: "FineDetails",
                column: "BorrowDetailId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FineDetails_FineId",
                table: "FineDetails",
                column: "FineId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_BorrowRecordId",
                table: "Fines",
                column: "BorrowRecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FineDetails");

            migrationBuilder.DropTable(
                name: "Fines");

            // Khi rollback, suy ra phiếu mất sách từ các chi tiết còn lưu.
            migrationBuilder.Sql("""
                UPDATE "BorrowRecords" AS record
                SET "Status" = 2
                WHERE record."Status" = 1
                  AND EXISTS (
                      SELECT 1 FROM "BorrowDetails" AS detail
                      WHERE detail."BorrowRecordId" = record."Id"
                        AND detail."Status" = 2
                  );
                """);
        }
    }
}
