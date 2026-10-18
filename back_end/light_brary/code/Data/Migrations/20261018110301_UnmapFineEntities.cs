using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_brary.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnmapFineEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chỉ bỏ ánh xạ EF; giữ nguyên bảng Fines và FineDetails cùng dữ liệu hiện có.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Các bảng vẫn tồn tại nên khôi phục model không cần thay đổi schema.
        }
    }
}
