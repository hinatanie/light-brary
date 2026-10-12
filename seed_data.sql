-- ==============================================================================
-- LIGHTBRARY SQL SCRIPT: THỂ LOẠI (CATEGORIES), SÁCH (BOOKS) & BẢN SAO (BOOKCOPIES)
-- ==============================================================================

BEGIN;

-- 1. THÊM CÁC THỂ LOẠI SÁCH (CATEGORIES)
INSERT INTO "Categories" ("Id", "Name", "Description")
VALUES
    (gen_random_uuid(), 'Công nghệ thông tin', 'Sách về lập trình, kiến trúc hệ thống, AI và công nghệ phần mềm.'),
    (gen_random_uuid(), 'Kỹ năng sống', 'Sách rèn luyện tư duy, giao tiếp, tâm lý và phát triển năng lực cá nhân.'),
    (gen_random_uuid(), 'Văn học', 'Các tác phẩm văn học kinh điển, tiểu thuyết nổi tiếng thế giới và Việt Nam.'),
    (gen_random_uuid(), 'Kinh tế', 'Kiến thức quản trị kinh doanh, tài chính, đầu tư và khởi nghiệp đổi mới sáng tạo.'),
    (gen_random_uuid(), 'Lịch sử & Triết học', 'Lịch sử văn minh nhân loại, tư tưởng triết học và văn hóa các dân tộc.')
ON CONFLICT ("Name") DO NOTHING;

-- 2. THÊM CÁC ĐẦU SÁCH (BOOKS) VÀ BẢN SAO (BOOKCOPIES)
DO $$
DECLARE
    cat_it UUID;
    cat_selfhelp UUID;
    cat_lit UUID;
    cat_biz UUID;
    cat_hist UUID;

    b_dnt UUID := gen_random_uuid();
    b_ngk UUID := gen_random_uuid();
    b_cln UUID := gen_random_uuid();
    b_sys UUID := gen_random_uuid();
    b_sta UUID := gen_random_uuid();
    b_sap UUID := gen_random_uuid();
    b_tdu UUID := gen_random_uuid();
    b_dme UUID := gen_random_uuid();
BEGIN
    SELECT "Id" INTO cat_it FROM "Categories" WHERE "Name" = 'Công nghệ thông tin' LIMIT 1;
    SELECT "Id" INTO cat_selfhelp FROM "Categories" WHERE "Name" = 'Kỹ năng sống' LIMIT 1;
    SELECT "Id" INTO cat_lit FROM "Categories" WHERE "Name" = 'Văn học' LIMIT 1;
    SELECT "Id" INTO cat_biz FROM "Categories" WHERE "Name" = 'Kinh tế' LIMIT 1;
    SELECT "Id" INTO cat_hist FROM "Categories" WHERE "Name" = 'Lịch sử & Triết học' LIMIT 1;

    -- Cuốn 1: Đắc Nhân Tâm (Kỹ năng sống)
    IF NOT EXISTS (SELECT 1 FROM "Books" WHERE "ISBN" = '978-604-58-9032-5') THEN
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "ImageUrl", "PublishYear", "CategoryId", "CreatedAt")
        VALUES (b_dnt, 'Đắc Nhân Tâm', 'Dale Carnegie', '978-604-58-9032-5', 'https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?auto=format&fit=crop&w=600&q=80', 2021, cat_selfhelp, NOW());

        INSERT INTO "BookCopies" ("Id", "Barcode", "Status", "ConditionNote", "BookId")
        VALUES
            (gen_random_uuid(), 'BC-DNT-01', 0, 'Sách mới 100%, có sẵn trên kệ trung tâm.', b_dnt),
            (gen_random_uuid(), 'BC-DNT-02', 0, 'Tình trạng tốt, sẵn sàng mượn.', b_dnt),
            (gen_random_uuid(), 'BC-DNT-03', 0, 'Bản sao lưu trữ tại thư viện.', b_dnt);
    END IF;

    -- Cuốn 2: Nhà Giả Kim (Văn học)
    IF NOT EXISTS (SELECT 1 FROM "Books" WHERE "ISBN" = '978-604-56-2608-5') THEN
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "ImageUrl", "PublishYear", "CategoryId", "CreatedAt")
        VALUES (b_ngk, 'Nhà Giả Kim', 'Paulo Coelho', '978-604-56-2608-5', 'https://images.unsplash.com/photo-1543002588-bfa74002ed7e?auto=format&fit=crop&w=600&q=80', 2020, cat_lit, NOW());

        INSERT INTO "BookCopies" ("Id", "Barcode", "Status", "ConditionNote", "BookId")
        VALUES
            (gen_random_uuid(), 'BC-NGK-01', 0, 'Sách mới, bìa mềm nguyên vẹn.', b_ngk),
            (gen_random_uuid(), 'BC-NGK-02', 0, 'Tình trạng tốt, sẵn sàng mượn.', b_ngk),
            (gen_random_uuid(), 'BC-NGK-03', 0, 'Bản sao dự phòng.', b_ngk);
    END IF;

    -- Cuốn 3: Clean Code (CNTT)
    IF NOT EXISTS (SELECT 1 FROM "Books" WHERE "ISBN" = '978-013-235-088-4') THEN
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "ImageUrl", "PublishYear", "CategoryId", "CreatedAt")
        VALUES (b_cln, 'Clean Code', 'Robert C. Martin', '978-013-235-088-4', 'https://images.unsplash.com/photo-1532012164546-f432f2e3777a?auto=format&fit=crop&w=600&q=80', 2008, cat_it, NOW());

        INSERT INTO "BookCopies" ("Id", "Barcode", "Status", "ConditionNote", "BookId")
        VALUES
            (gen_random_uuid(), 'BC-CLN-01', 0, 'Sách chuyên ngành IT, bìa cứng.', b_cln),
            (gen_random_uuid(), 'BC-CLN-02', 0, 'Tình trạng rất tốt.', b_cln),
            (gen_random_uuid(), 'BC-CLN-03', 0, 'Bản sao tham khảo tại chỗ.', b_cln);
    END IF;

    -- Cuốn 4: Thiết Kế Hệ Thống Quy Mô Lớn (CNTT)
    IF NOT EXISTS (SELECT 1 FROM "Books" WHERE "ISBN" = '978-173-604-911-2') THEN
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "ImageUrl", "PublishYear", "CategoryId", "CreatedAt")
        VALUES (b_sys, 'Thiết Kế Hệ Thống Quy Mô Lớn (System Design)', 'Alex Xu', '978-173-604-911-2', 'https://images.unsplash.com/photo-1512820790803-83ca734da794?auto=format&fit=crop&w=600&q=80', 2022, cat_it, NOW());

        INSERT INTO "BookCopies" ("Id", "Barcode", "Status", "ConditionNote", "BookId")
        VALUES
            (gen_random_uuid(), 'BC-SYS-01', 0, 'Sách thiết kế kiến trúc phân tán.', b_sys),
            (gen_random_uuid(), 'BC-SYS-02', 0, 'Sách mới, có hình minh họa chi tiết.', b_sys),
            (gen_random_uuid(), 'BC-SYS-03', 0, 'Bản sao kệ sách CNTT.', b_sys);
    END IF;

    -- Cuốn 5: Khởi Nghiệp Tinh Gọn (Kinh tế)
    IF NOT EXISTS (SELECT 1 FROM "Books" WHERE "ISBN" = '978-604-90-2814-4') THEN
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "ImageUrl", "PublishYear", "CategoryId", "CreatedAt")
        VALUES (b_sta, 'Khởi Nghiệp Tinh Gọn (The Lean Startup)', 'Eric Ries', '978-604-90-2814-4', 'https://images.unsplash.com/photo-1589829085413-56de8ae18c73?auto=format&fit=crop&w=600&q=80', 2019, cat_biz, NOW());

        INSERT INTO "BookCopies" ("Id", "Barcode", "Status", "ConditionNote", "BookId")
        VALUES
            (gen_random_uuid(), 'BC-STA-01', 0, 'Sách khởi nghiệp kinh doanh.', b_sta),
            (gen_random_uuid(), 'BC-STA-02', 0, 'Tình trạng tốt.', b_sta),
            (gen_random_uuid(), 'BC-STA-03', 0, 'Bản sao dự phòng.', b_sta);
    END IF;

    -- Cuốn 6: Sapiens (Lịch sử)
    IF NOT EXISTS (SELECT 1 FROM "Books" WHERE "ISBN" = '978-604-77-3860-1') THEN
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "ImageUrl", "PublishYear", "CategoryId", "CreatedAt")
        VALUES (b_sap, 'Sapiens: Lược Sử Loài Người', 'Yuval Noah Harari', '978-604-77-3860-1', 'https://images.unsplash.com/photo-1497633762265-9d179a990aa6?auto=format&fit=crop&w=600&q=80', 2017, cat_hist, NOW());

        INSERT INTO "BookCopies" ("Id", "Barcode", "Status", "ConditionNote", "BookId")
        VALUES
            (gen_random_uuid(), 'BC-SAP-01', 0, 'Tác phẩm lịch sử văn minh nổi tiếng.', b_sap),
            (gen_random_uuid(), 'BC-SAP-02', 0, 'Tình trạng hoàn hảo.', b_sap),
            (gen_random_uuid(), 'BC-SAP-03', 0, 'Bản sao lưu trữ.', b_sap);
    END IF;

    -- Cuốn 7: Tư Duy Nhanh Và Chậm (Kỹ năng sống)
    IF NOT EXISTS (SELECT 1 FROM "Books" WHERE "ISBN" = '978-604-77-1647-0') THEN
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "ImageUrl", "PublishYear", "CategoryId", "CreatedAt")
        VALUES (b_tdu, 'Tư Duy Nhanh Và Chậm (Thinking, Fast and Slow)', 'Daniel Kahneman', '978-604-77-1647-0', 'https://images.unsplash.com/photo-1495446815901-a7297e633e8d?auto=format&fit=crop&w=600&q=80', 2018, cat_selfhelp, NOW());

        INSERT INTO "BookCopies" ("Id", "Barcode", "Status", "ConditionNote", "BookId")
        VALUES
            (gen_random_uuid(), 'BC-TDU-01', 0, 'Sách tâm lý hành vi & kinh tế học.', b_tdu),
            (gen_random_uuid(), 'BC-TDU-02', 0, 'Tình trạng tốt.', b_tdu),
            (gen_random_uuid(), 'BC-TDU-03', 0, 'Bản sao dự phòng.', b_tdu);
    END IF;

    -- Cuốn 8: Dế Mèn Phiêu Lưu Ký (Văn học)
    IF NOT EXISTS (SELECT 1 FROM "Books" WHERE "ISBN" = '978-604-22-1890-0') THEN
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "ImageUrl", "PublishYear", "CategoryId", "CreatedAt")
        VALUES (b_dme, 'Dế Mèn Phiêu Lưu Ký', 'Tô Hoài', '978-604-22-1890-0', 'https://images.unsplash.com/photo-1516979187457-637abb4f9353?auto=format&fit=crop&w=600&q=80', 2023, cat_lit, NOW());

        INSERT INTO "BookCopies" ("Id", "Barcode", "Status", "ConditionNote", "BookId")
        VALUES
            (gen_random_uuid(), 'BC-DME-01', 0, 'Tác phẩm văn học thiếu nhi kinh điển.', b_dme),
            (gen_random_uuid(), 'BC-DME-02', 0, 'Sách mới in tái bản có tranh minh họa.', b_dme),
            (gen_random_uuid(), 'BC-DME-03', 0, 'Bản sao lưu trữ.', b_dme);
    END IF;
END $$;

COMMIT;
