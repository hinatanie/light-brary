using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_brary.Data.Migrations
{
    /// <inheritdoc />
    public partial class newest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCopies_Books_BookId",
                table: "BookCopies");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowDetails_BookCopies_BookCopyId",
                table: "BorrowDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "BookCopyId",
                table: "BorrowDetails",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCopies_Books_BookId",
                table: "BookCopies",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowDetails_BookCopies_BookCopyId",
                table: "BorrowDetails",
                column: "BookCopyId",
                principalTable: "BookCopies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCopies_Books_BookId",
                table: "BookCopies");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowDetails_BookCopies_BookCopyId",
                table: "BorrowDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "BookCopyId",
                table: "BorrowDetails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCopies_Books_BookId",
                table: "BookCopies",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowDetails_BookCopies_BookCopyId",
                table: "BorrowDetails",
                column: "BookCopyId",
                principalTable: "BookCopies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
