using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_brary.code.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeUserBorrowHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowRecords_AspNetUsers_UserId",
                table: "BorrowRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_FineDetails_BorrowDetails_BorrowDetailId",
                table: "FineDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BorrowRecords_BorrowRecordId",
                table: "Fines");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowRecords_AspNetUsers_UserId",
                table: "BorrowRecords",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FineDetails_BorrowDetails_BorrowDetailId",
                table: "FineDetails",
                column: "BorrowDetailId",
                principalTable: "BorrowDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BorrowRecords_BorrowRecordId",
                table: "Fines",
                column: "BorrowRecordId",
                principalTable: "BorrowRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowRecords_AspNetUsers_UserId",
                table: "BorrowRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_FineDetails_BorrowDetails_BorrowDetailId",
                table: "FineDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BorrowRecords_BorrowRecordId",
                table: "Fines");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowRecords_AspNetUsers_UserId",
                table: "BorrowRecords",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FineDetails_BorrowDetails_BorrowDetailId",
                table: "FineDetails",
                column: "BorrowDetailId",
                principalTable: "BorrowDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BorrowRecords_BorrowRecordId",
                table: "Fines",
                column: "BorrowRecordId",
                principalTable: "BorrowRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
