# Response Mapping Implementation Plan

**Goal:** Đưa mapping entity sang response vào `FromEntity()` trong response tương ứng, giữ nguyên cấu trúc và giá trị API.

**Architecture:** Dùng static method theo convention hiện có ở `UserResponse`, `BorrowRecordResponse`, `BorrowDetailResponse`. Service vẫn truy vấn dữ liệu, lấy role và xử lý nghiệp vụ; mapper không truy vấn database.

**Tech Stack:** C#, ASP.NET Core Identity, EF Core, .NET 10; không thêm dependency.

**Spec:** Yêu cầu trong hội thoại: áp dụng `FromEntity()` cho các response để kiểm soát mapping tập trung.

## Constraints

- Giữ nguyên thay đổi chưa commit của người dùng; chưa commit/push.
- Không thay đổi validation, phân quyền, transaction hoặc hợp đồng JSON.
- Giữ mapper đã có; chuyển mapper trong service sang DTO, không tạo class mapper riêng.
- `ReturnBooksResponse` là kết quả tổng hợp; chỉ gọi mapper của từng item.

## Task 1: Chuyển mapping và xác minh

- [x] Chụp kết quả JSON trước thay đổi cho book, copy, category, role, summary và action; kiểm tra navigation rỗng, enum và dữ liệu lồng nhau.
- [x] Thêm `FromEntity()` vào `BookResponse`, `BookCopyResponse`, `CategoryResponse`, `RoleResponse`, `BorrowRecordSummaryResponse`, `BorrowDetailActionResponse`.
- [x] Cập nhật `BookService`, `BookCopyService`, `CategoryService`, `RoleService`, `BorrowRecordService`; bỏ mapper riêng và tái sử dụng mapper action khi trả sách.
- [x] Chạy `dotnet build Library.slnx --nologo --no-restore`, so sánh snapshot JSON trước/sau và kiểm tra truy vấn role trên PostgreSQL tạm.
- [x] Review diff, kiểm tra không còn mapping response trong service ngoại trừ kết quả tổng hợp trả sách.

## Verification

- Backend build: 0 warnings, 0 errors.
- 22 serialized JSON snapshots: before/after identical.
- 28 PostgreSQL-backed HTTP checks passed, including role projections and borrow/return/lost mappings.
- Independent scoped review: no actionable findings.
- Tests used temporary files outside the repository; no dependency added, no commit/push.
