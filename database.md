-- 1. TẠO CƠ SỞ DỮ LIỆU
CREATE DATABASE LibraryDb;
GO

USE LibraryDb;
GO

-- =====================================================================
-- 2. CÁC BẢNG QUẢN TRỊ TÀI KHOẢN (TẬN DỤNG CẤU TRÚC ASP.NET CORE IDENTITY)
-- =====================================================================

-- Bảng Roles (Vai trò: Admin, Librarian, Member...)
CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256) NULL,
);

-- Bảng Users (Độc giả, Thủ thư, Quản trị viên)
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    PasswordHash NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(50) NULL,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    -- Các trường mở rộng tùy chỉnh
    FullName NVARCHAR(150) NOT NULL,
    Address NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Bảng liên kết User - Role
CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_Roles FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
);

-- =====================================================================
-- 3. CÁC BẢNG DANH MỤC & SÁCH
-- =====================================================================

-- Bảng Categories (Thể loại sách)
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL
);

-- Bảng Books (Đầu sách)
CREATE TABLE Books (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(250) NOT NULL,
    Author NVARCHAR(150) NOT NULL,
    ISBN NVARCHAR(50) NULL,
    PublishYear INT NULL,
    CategoryId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Books_Categories FOREIGN KEY (CategoryId) REFERENCES Categories (Id) ON DELETE RESTRICT
);

-- Bảng BookCopies (Bản sao vật lý từng cuốn sách trên kệ)
-- Status: 0 = Available (Có sẵn), 1 = Borrowed (Đang mượn), 2 = Lost (Mất/Hỏng)
CREATE TABLE BookCopies (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BookId INT NOT NULL,
    Barcode NVARCHAR(50) NOT NULL UNIQUE,       -- Mã vạch dán trên gáy sách
    Status TINYINT NOT NULL DEFAULT 0,          
    ConditionNote NVARCHAR(255) NULL,           -- Ghi chú tình trạng sách (mới, rách nhẹ...)
    CONSTRAINT FK_BookCopies_Books FOREIGN KEY (BookId) REFERENCES Books (Id) ON DELETE CASCADE
);

-- =====================================================================
-- 4. CÁC BẢNG QUẢN LÝ MƯỢN TRẢ
-- =====================================================================

-- Bảng BorrowRecords (Phiếu mượn)
-- Status: 0 = Borrowing (Đang mượn), 1 = Returned (Đã trả xong), 2 = Overdue (Quá hạn)
CREATE TABLE BorrowRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,              -- Người mượn
    BorrowDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DueDate DATETIME2 NOT NULL,                 -- Hạn phải trả
    Status TINYINT NOT NULL DEFAULT 0,
    Note NVARCHAR(255) NULL,
    CONSTRAINT FK_BorrowRecords_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE NO ACTION
);

-- Bảng BorrowDetails (Chi tiết các cuốn sách trong phiếu mượn)
-- Status: 0 = Borrowing (Đang mượn), 1 = Returned (Đã trả), 2 = Lost (Báo mất)
CREATE TABLE BorrowDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BorrowRecordId INT NOT NULL,
    BookCopyId INT NOT NULL,                    -- Chỉ đích danh cuốn sách vật lý nào
    ReturnDate DATETIME2 NULL,                  -- Ngày thực tế mang trả cuốn này
    Status TINYINT NOT NULL DEFAULT 0,
    FineAmount DECIMAL(18,2) NOT NULL DEFAULT 0, -- Phí phạt quá hạn / đền bù nếu có
    CONSTRAINT FK_BorrowDetails_BorrowRecords FOREIGN KEY (BorrowRecordId) REFERENCES BorrowRecords (Id) ON DELETE CASCADE,
    CONSTRAINT FK_BorrowDetails_BookCopies FOREIGN KEY (BookCopyId) REFERENCES BookCopies (Id) ON DELETE NO ACTION
);

-- TẠO CHỈ MỤC (INDEX) TỐI ƯU TRUY VẤN
CREATE INDEX IX_Books_CategoryId ON Books(CategoryId);
CREATE INDEX IX_BookCopies_BookId ON BookCopies(BookId);
CREATE INDEX IX_BookCopies_Barcode ON BookCopies(Barcode);
CREATE INDEX IX_BorrowRecords_UserId ON BorrowRecords(UserId);
CREATE INDEX IX_BorrowDetails_BorrowRecordId ON BorrowDetails(BorrowRecordId);
CREATE INDEX IX_BorrowDetails_BookCopyId ON BorrowDetails(BookCopyId);
GO