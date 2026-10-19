using light_brary.Constants;
using light_brary.Entities;
using light_brary.Enums;
using light_brary.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace light_brary.Data
{
    public static class LibraryDataSeeder
    {
        public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager,
            IConfiguration configuration)
        {
            await SeedUsersAsync(userManager, configuration);
            await SeedBooksAsync(context);
        }

        private static async Task SeedUsersAsync(UserManager<User> userManager, IConfiguration configuration)
        {
            var password = configuration["Seed:SamplePassword"] ?? configuration["Seed:AdminPassword"];
            var phoneNumbers = (await userManager.Users.Select(user => user.PhoneNumber).ToListAsync()).ToHashSet();
            var primaryStudents = new (string StudentId, string FullName)[]
            {
                ("3123410249", "Lâm Thái Yến Nhi"),
                ("3123410063", "Trần Đình Khánh Dư"),
                ("3123410076", "Nguyễn Hải Đăng"),
                ("3123410216", "Lương Quang Minh")
            };

            var additionalStudentNames = new[]
            {
                "Nguyễn Văn An", "Trần Thị Bích", "Lê Minh Châu", "Phạm Quốc Dũng",
                "Hoàng Thị Mai", "Huỳnh Gia Huy", "Phan Ngọc Linh", "Vũ Đức Long",
                "Võ Hoàng Nam", "Đặng Thị Ngọc", "Bùi Minh Phúc", "Đỗ Quỳnh Anh",
                "Hồ Thanh Sơn", "Ngô Bảo Trâm", "Dương Anh Tuấn", "Lý Thu Uyên",
                "Trương Hải Vân", "Đinh Thành Vinh", "Mai Như Ý", "Cao Nhật Minh"
            };

            var nextStudentId = primaryStudents.Select(s => long.Parse(s.StudentId)).Max() + 1;
            var students = primaryStudents
                .Concat(additionalStudentNames.Select((name, index) => (StudentId: (nextStudentId + index).ToString("D10"), FullName: name)))
                .ToArray();

            var streets = new[] { "Nguyễn Trãi", "An Dương Vương", "Trần Hưng Đạo", "Nguyễn Văn Cừ", "Lê Hồng Phong" };

            if (!(await userManager.GetUsersInRoleAsync(RoleNames.LIBRARIAN)).Any())
            {
                await CreateUserAsync("thuthu", "Nguyễn Thị Thanh Hà", "0902000000",
                    "273 An Dương Vương, TP. Hồ Chí Minh", RoleNames.LIBRARIAN);
            }

            for (var index = 0; index < students.Length; index++)
            {
                var student = students[index];
                await CreateUserAsync(student.StudentId, student.FullName, $"090100{index + 1:0000}",
                    $"{20 + index * 3} {streets[index % streets.Length]}, TP. Hồ Chí Minh", RoleNames.USER);
            }

            async Task CreateUserAsync(string userName, string fullName, string phone, string address, string role)
            {
                if (await userManager.FindByNameAsync(userName) is not null) return;

                if (string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException("Cần cấu hình Seed:SamplePassword hoặc Seed:AdminPassword bằng User Secrets hoặc biến môi trường để tạo tài khoản mẫu.");

                while (phoneNumbers.Contains(phone))
                    phone = (long.Parse(phone) + 1).ToString("D10");

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = userName,
                    FullName = fullName,
                    PhoneNumber = phone,
                    Address = address
                };
                (await userManager.CreateAsync(user, password)).ThrowIfFailed();
                (await userManager.AddToRoleAsync(user, role)).ThrowIfFailed();
                phoneNumbers.Add(phone);
            }
        }

        private static async Task SeedBooksAsync(AppDbContext context)
        {
            var categories = await context.Categories.ToDictionaryAsync(category => category.Name);
            var books = await context.Books.Include(book => book.BookCopies).ToListAsync();
            var booksByIsbn = books.ToDictionary(book => book.ISBN.Replace("-", ""));

            foreach (var (name, description) in LibrarySeedCatalog.Categories)
            {
                if (categories.ContainsKey(name)) continue;
                var category = new Category { Name = name, Description = description };
                context.Categories.Add(category);
                categories.Add(name, category);
            }

            for (var index = 0; index < LibrarySeedCatalog.Books.Length; index++)
            {
                var (title, author, isbn, year, categoryName) = LibrarySeedCatalog.Books[index];
                if (!booksByIsbn.TryGetValue(isbn, out var book))
                {
                    book = new Book
                    {
                        Title = title,
                        Author = author,
                        ISBN = isbn,
                        PublishYear = year,
                        ImageUrl = $"https://covers.openlibrary.org/b/isbn/{isbn}-L.jpg",
                        Category = categories[categoryName]
                    };
                    context.Books.Add(book);
                    booksByIsbn.Add(isbn, book);
                }

                // Bổ sung đến 2 hoặc 3 bản sao, giữ nguyên tình trạng và dữ liệu đã có.
                var targetCopies = 2 + index % 2;
                var copyNumber = 1;
                while (book.BookCopies.Count < targetCopies)
                {
                    var barcode = $"LB-{isbn}-{copyNumber++:00}";
                    if (book.BookCopies.Any(copy => copy.Barcode == barcode)) continue;
                    var copy = new BookCopy
                    {
                        Barcode = barcode,
                        Status = BookCopyStatus.Available,
                        ConditionNote = $"Sách nguyên vẹn, sẵn sàng cho mượn. Kệ {index / 6 + 1:00}.",
                        Book = book
                    };
                    book.BookCopies.Add(copy);
                    context.BookCopies.Add(copy);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
