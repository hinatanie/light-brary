namespace light_brary.Data
{
    internal static class LibrarySeedCatalog
    {
        internal static readonly (string Name, string Description)[] Categories =
        [
            ("Công nghệ thông tin", "Lập trình, thuật toán, thiết kế phần mềm và hệ thống dữ liệu."),
            ("Kỹ năng sống", "Thói quen, tư duy, giao tiếp và phát triển bản thân."),
            ("Văn học", "Tiểu thuyết và các tác phẩm văn học kinh điển thế giới."),
            ("Kinh tế", "Kinh doanh, khởi nghiệp, quản trị và tài chính cá nhân."),
            ("Lịch sử & triết học", "Lịch sử nhân loại, tư tưởng và những góc nhìn về cuộc sống.")
        ];

        // ISBN và năm xuất bản ứng với ấn bản tiếng Anh; giữ nguyên tên sách gốc.
        internal static readonly (string Title, string Author, string Isbn, int Year, string Category)[] Books =
        [
            ("Clean Code", "Robert C. Martin", "9780132350884", 2008, "Công nghệ thông tin"),
            ("The Pragmatic Programmer", "David Thomas, Andrew Hunt", "9780135957059", 2019, "Công nghệ thông tin"),
            ("Design Patterns", "Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides", "9780201633610", 1994, "Công nghệ thông tin"),
            ("Introduction to Algorithms", "Thomas H. Cormen, Charles E. Leiserson, Ronald L. Rivest, Clifford Stein", "9780262046305", 2022, "Công nghệ thông tin"),
            ("Refactoring", "Martin Fowler", "9780134757599", 2018, "Công nghệ thông tin"),
            ("Designing Data-Intensive Applications", "Martin Kleppmann", "9781449373320", 2017, "Công nghệ thông tin"),
            ("Atomic Habits", "James Clear", "9780735211292", 2018, "Kỹ năng sống"),
            ("How to Win Friends and Influence People", "Dale Carnegie", "9780671027032", 1998, "Kỹ năng sống"),
            ("The 7 Habits of Highly Effective People", "Stephen R. Covey", "9781982137274", 2020, "Kỹ năng sống"),
            ("Mindset", "Carol S. Dweck", "9780345472328", 2007, "Kỹ năng sống"),
            ("Deep Work", "Cal Newport", "9781455586691", 2016, "Kỹ năng sống"),
            ("The Power of Habit", "Charles Duhigg", "9780812981605", 2014, "Kỹ năng sống"),
            ("The Alchemist", "Paulo Coelho", "9780062315007", 2014, "Văn học"),
            ("The Little Prince", "Antoine de Saint-Exupéry", "9780156012195", 2000, "Văn học"),
            ("1984", "George Orwell", "9780451524935", 1950, "Văn học"),
            ("To Kill a Mockingbird", "Harper Lee", "9780061120084", 2006, "Văn học"),
            ("Pride and Prejudice", "Jane Austen", "9780141439518", 2002, "Văn học"),
            ("The Great Gatsby", "F. Scott Fitzgerald", "9780743273565", 2004, "Văn học"),
            ("The Lean Startup", "Eric Ries", "9780307887894", 2011, "Kinh tế"),
            ("Zero to One", "Peter Thiel, Blake Masters", "9780804139298", 2014, "Kinh tế"),
            ("The Intelligent Investor", "Benjamin Graham", "9780060555665", 2003, "Kinh tế"),
            ("Rich Dad Poor Dad", "Robert T. Kiyosaki", "9781612680194", 2017, "Kinh tế"),
            ("The Psychology of Money", "Morgan Housel", "9780857197689", 2020, "Kinh tế"),
            ("Good to Great", "Jim Collins", "9780066620992", 2001, "Kinh tế"),
            ("Sapiens: A Brief History of Humankind", "Yuval Noah Harari", "9780062316097", 2015, "Lịch sử & triết học"),
            ("Homo Deus: A Brief History of Tomorrow", "Yuval Noah Harari", "9780062464316", 2017, "Lịch sử & triết học"),
            ("A Brief History of Time", "Stephen Hawking", "9780553380163", 1998, "Lịch sử & triết học"),
            ("Man's Search for Meaning", "Viktor E. Frankl", "9780807014271", 2006, "Lịch sử & triết học"),
            ("Meditations", "Marcus Aurelius", "9780812968255", 2003, "Lịch sử & triết học"),
            ("The Story of Philosophy", "Will Durant", "9780671739164", 1991, "Lịch sử & triết học")
        ];
    }
}
