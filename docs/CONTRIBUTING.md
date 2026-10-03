# QUY TẮC LÀM VIỆC VỚI SOURCE CODE
## Library Management API
## 1. Cấu trúc source code

src/
├── Library.Api/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── Library.Application/
│   ├── DTOs/
│   ├── Exceptions/
│   ├── Services/
│   └── Validators/
│
├── Library.Domain/
│   ├── Entities/
│   └── Enums/
│
└── Library.Infrastructure/
    ├── Authentication/
    ├── Persistence/
    │   ├── Configurations/
    │   └── Migrations/
    └── Repositories/
# 5. Quy định Library.Application
Library.Application chứa logic xử lý use case của hệ thống.
Library.Application/
├── DTOs/
├── Exceptions/
├── Services/
└── Validators/
Ví dụ các use case:
Đăng ký tài khoản
Đăng nhập
Tìm kiếm sách
Gửi yêu cầu mượn
Hủy yêu cầu mượn
Xử lý yêu cầu mượn

# 6. Quy định DTO
DTO được đặt tại:
Library.Application/DTOs/
DTO dùng để truyền dữ liệu giữa API và Application.

# 7. Quy định Services
Service chịu trách nhiệm thực hiện use case và business logic.

# 8. Quy định Validators
Validation đặt tại:

Validator kiểm tra dữ liệu đầu vào trước khi thực hiện nghiệp vụ.
Ví dụ:
RegisterRequestValidator.cs
CreateBookRequestValidator.cs
CreateBorrowRequestValidator.cs
Các kiểm tra điển hình:
Email không được rỗng
Password phải đủ độ dài
ISBN đúng định dạng
Ngày trả không nhỏ hơn ngày mượn
Số lượng sách phải lớn hơn 0
Không đưa tất cả validation vào Controller.
# 9. Quy định Exceptions
Exception nghiệp vụ đặt tại:
Library.Application/Exceptions/
Ví dụ:
NotFoundException.cs
ValidationException.cs
BusinessException.cs
UnauthorizedException.cs
ConflictException.cs

Service có thể:
throw new NotFoundException("Book not found");
Controller không cần try/catch ở từng endpoint ,Middleware xử lý lỗi tập trung.
# 10. Quy định Library.Infrastructure
Infrastructure chứa những phần liên quan đến công nghệ bên ngoài và triển khai kỹ thuật.
Library.Infrastructure/
├── Authentication/
├── Persistence/
└── Repositories/
Ví dụ:
Database
Entity Framework Core
MySQL
Repository
JWT
Password hashing
# 11. Quy định Persistence
Database code đặt tại:
Library.Infrastructure/Persistence/
Ví dụ:
Persistence/
├── LibraryDbContext.cs
├── Configurations/
└── Migrations/
LibraryDbContext.cs chịu trách nhiệm kết nối Entities với database.

# 12. Quy định Entity Configurations
Entity Framework configuration đặt tại:
Persistence/Configurations/
Ví dụ:
BookConfiguration.cs
UserConfiguration.cs
LoanConfiguration.cs
BorrowRequestConfiguration.cs

# 13. Quy định Migration
Migration phải nằm tại:
Library.Infrastructure/Persistence/Migrations/
Ví dụ:
20260926120000_InitialCreate.cs
20260927100000_AddBooksTable.cs
20260928150000_AddLoanStatus.cs
Tên migration phải mô tả thay đổi.

Ví dụ: dotnet ef migrations add AddPenaltyTable

Không chỉnh sửa migration cũ tùy tiện sau khi migration đó đã được nhóm sử dụng chung.

# 14. Quy định Repositories
Repository implementation đặt tại:
Library.Infrastructure/Repositories/
Ví dụ:
BookRepository.cs
UserRepository.cs
LoanRepository.cs
BorrowRequestRepository.cs
Repository chịu trách nhiệm truy cập dữ liệu.

# 15. Quy định Authentication
Các implementation liên quan đến authentication đặt tại:
Library.Infrastructure/Authentication/
Ví dụ:
JwtTokenService.cs
PasswordHasher.cs
JwtTokenService chịu trách nhiệm:
	Tạo JWT
	Thiết lập claim
	Thiết lập expiration
	Ký token
PasswordHasher chịu trách nhiệm:
	Hash password
	Verify password
# 16. Quy định Library.Api
Library.Api là entry point của hệ thống.
Library.Api/
├── Controllers/
├── Middleware/
├── Program.cs
├── appsettings.json
└── Properties/
Api chịu trách nhiệm:
	Nhận HTTP request
	Routing
	Authentication/Authorization
	Gọi Application Service
	Trả HTTP response
	Middleware
	Dependency Injection configuration

# 17. Quy định Controllers
Controller đặt trong: `Library.Api/Controllers/`
Tên: `<Entity/Resource>Controller.cs`
Ví dụ:
	AuthController.cs
	BooksController.cs
	BorrowRequestsController.cs
Controller phải kế thừa:
	ControllerBase
và sử dụng:
`[ApiController]`

# 18. Quy tắc URL API
API phải version hóa.
Sử dụng: /api/v1/...
Ví dụ:
```
POST /api/v1/auth/login
GET /api/v1/books
GET /api/v1/books/{bookId}
```

URL sử dụng danh từ, không dùng động từ nếu không cần thiết.
Nên:
GET /api/v1/books
Không nên:
GET /api/v1/getBooks
Nên:
POST /api/v1/books
Không nên:
POST /api/v1/createBook

---
# 19. Quy tắc HTTP Method
Sử dụng đúng ý nghĩa HTTP:

|   |   |
|---|---|
|Method|Mục đích|
|GET|Lấy dữ liệu|
|POST|Tạo resource mới hoặc gửi command|
|PUT|Cập nhật toàn bộ resource|
|PATCH|Cập nhật một phần hoặc thay đổi trạng thái|
|DELETE|Xóa resource|

# 20. Quy tắc HTTP Status Code
Sử dụng status code đúng ngữ nghĩa.
200 OK
    Request thành công.
201 Created
    Tạo resource thành công.
204 No Content
    Thành công nhưng không cần response body.
400 Bad Request
    Request không hợp lệ.
401 Unauthorized
    Chưa đăng nhập hoặc token không hợp lệ.
403 Forbidden
    Đã đăng nhập nhưng không có quyền.
404 Not Found
    Resource không tồn tại.
409 Conflict
    Xung đột dữ liệu hoặc trạng thái.
500 Internal Server Error
    Lỗi hệ thống không mong muốn.
Không trả:
200 OK
cho tất cả trường hợp rồi ghi:
```
{
    "success": false,
    "message": "Book not found"
}
```
nếu thực tế phải là:
404 Not Found

# 21. Quy định Middleware
Middleware đặt tại: `Library.Api/Middleware/`
Ví dụ:
ExceptionHandlingMiddleware.cs
RequestLoggingMiddleware.cs
Exception toàn hệ thống nên được xử lý tập trung tại middleware.
Luồng:
```
Request
   ↓
Middleware
   ↓
Controller
   ↓
Service
   ↓
Repository
   ↓
Database
```

Nếu xảy ra Exception:
Service
   ↓
throw Exception
   ↓
ExceptionHandlingMiddleware
   ↓
HTTP Response

# 22. Quy định Program.cs
Program.cs chỉ dùng để cấu hình và khởi động application.
Ví dụ:
	AddControllers
	AddDbContext
	AddAuthentication
	AddAuthorization
	AddServices
	AddRepositories
	AddValidators
	AddSwagger
	UseMiddleware
	MapControllers

# 23. Quy định appsettings
Cấu hình chung: `appsettings.json`
Cấu hình development:
appsettings.Development.json

```
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Jwt": {
    "Issuer": "...",
    "Audience": "...",
    "ExpirationMinutes": 60
  }
}
```
Không commit secret thực tế lên GitHub như:
	Database password
	JWT secret
	API key
	Private key
	Secret local nên sử dụng environment variable hoặc .NET User Secrets.

# 24. Quy tắc đặt tên
Theo convention của C#/.NET:
### Class
```
PascalCase:
BookService
LoanRepository
BorrowRequestController
```
### Method
```
PascalCase:
GetBookByIdAsync()
CreateBorrowRequestAsync()
UpdateBookAsync()
```
### Property
```
PascalCase:
public string FullName { get; set; }
```
### Local variable
```
camelCase:
var currentUser = ...
var borrowRequest = ...
```
### Private field
Sử dụng _camelCase:
```
private readonly LibraryDbContext _dbContext;
private readonly BookService _bookService;
```
### Interface
Bắt đầu bằng I:
```
IBookRepository
IUserRepository
ITokenService
```

---
# 25. Quy tắc phương thức Async
Các thao tác I/O như:
	Database
	File
	Network
	External API
phải ưu tiên asynchronous.

Tên method async kết thúc bằng: Async
Ví dụ:
	GetByIdAsync()
	CreateAsync()
	UpdateAsync()
	SaveChangesAsync()
Không đặt:
	`GetById()`
nếu method thực sự trả:
	`Task<Book>``

# 26. Quy tắc namespace
Namespace phải khớp với project/folder.
Ví dụ:
```
namespace Library.Domain.Entities;
namespace Library.Application.DTOs.Books;
namespace Library.Application.Services;
namespace Library.Infrastructure.Repositories;
namespace Library.Infrastructure.Persistence;
namespace Library.Api.Controllers;
```
Không sử dụng namespace cũ như:
namespace server;

# 27. Quy tắc file
Mỗi file C# thông thường chỉ nên chứa một class/interface/enum chính.
Ví dụ:
```
Book.cs
BookService.cs
BookRepository.cs
BooksController.cs
```
Không tạo:
```
Models.cs
```
rồi nhét:
```
class Book
class User
class Loan
class Penalty
```
vào cùng một file.
Tên file phải trùng với tên class chính:
BookService.cs
public class BookService

# 28. Quy trình tạo một chức năng mới
Ví dụ nhóm cần thêm chức năng:
Độc giả gửi yêu cầu mượn sách.
Không bắt đầu bằng cách viết tất cả vào Controller.
Luồng phát triển:
1. Domain
   ↓
Kiểm tra Entity/Enum cần thiết
2. Application/DTOs
   ↓
Tạo request/response model
3. Application/Validators
   ↓
Tạo validation
4. Application/Services
   ↓
Viết business logic
5. Infrastructure/Repositories
   ↓
Viết truy vấn database
6. Api/Controllers
   ↓
Expose HTTP endpoint
7. Test API

Luồng runtime:
POST /api/v1/borrow-requests
        ↓
BorrowRequestsController
        ↓
BorrowRequestService
        ↓
BorrowRequestRepository
        ↓
LibraryDbContext
        ↓
Database
# 29. Không commit bin/ và obj/
Hai thư mục:
bin/
obj/
được .NET tự sinh khi build.
Không commit chúng lên GitHub.
.gitignore cần có:
bin/
obj/
Tương tự không commit:
.vs/
*.user
*.suo

# 30. Branch Naming
Công thức:
```
<type>/<short-description>
```
Các loại nhánh chính:
```
feat/
fix/
hotfix/
refactor/
docs/
test/
chore/
```
Ví dụ cho chính project này:
```
feat/user-authentication
feat/book-management
refactor/book-service
docs/update-api-specification
```
# 31. Conventional Commits
Format:
```
<type>(<scope>): <description>
```
Các type:
	feat
	fix
	refactor
	docs
	test
	chore
	style
Scope nên phản ánh module nghiệp vụ.
Ví dụ:
	auth
	book
	database
	api
Ví dụ commit:
	feat(auth): add login endpoint
	feat(book): add book creation service
	feat(borrow): implement borrow request workflow
	fix(loan): prevent returning completed loan twice
	refactor(book): move database query to repository
	docs(readme): add development setup guide
	chore(database): add initial migration
Mỗi commit nên đại diện cho một thay đổi logic rõ ràng.
# 32. Workflow làm việc nhóm
Trước khi bắt đầu:
	git checkout main
	git pull origin main
Tạo branch:
	git checkout -b feat/book-management
Trong quá trình code:
	git add .
	git commit -m "feat(book): add book entity"
	git add .
	git commit -m "feat(book): add book repository"
	git add .
	git commit -m "feat(book): add book service"
	git add .
	git commit -m "feat(book): add book endpoints"
Push:
	git push origin feat/book-management
Sau đó tạo Pull Request:
	feat/book-management
	        ↓
	main
Code phải được review trước khi merge.

# 33. Quy tắc trước khi tạo Pull Request
Trước khi push/PR phải đảm bảo:
	dotnet restore
	dotnet build Library.slnx
	Build phải:
	Build succeeded.
API liên quan phải được chạy thử:
	dotnet run --project src/Library.Api
Không tạo PR khi project không compile.

# 34. Quy tắc xử lý conflict
Không sửa conflict trực tiếp một cách tùy ý nếu không hiểu code của thành viên khác.
Trước tiên cập nhật main:
	git checkout main
	git pull origin main
quay về branch:
	git checkout feat/book-management
merge:
	git merge main
Sau đó giải quyết conflict, build lại:
	dotnet build Library.slnx
rồi commit:
	git add .
	git commit -m "chore: resolve merge conflicts"

# 35. Quy tắc không sửa code ngoài phạm vi task
Nếu đang làm:
	feat/borrow-request
không tự ý sửa:
	Authentication
	Book management
	Penalty
	User management
nếu không liên quan trực tiếp.
Điều này giúp PR nhỏ, dễ review và giảm conflict.

# 36. Không commit code thử nghiệm
Không để lại:
```
	Console.WriteLine("abc");
	Console.WriteLine("test");
	var a = ...
	var test = ...
```
không sử dụng.
Không để commented code:
```
	// var oldCode = ...
	// _context.Users.Add(...);
	// ...
```
Nếu code không còn dùng thì xóa. Git đã lưu lịch sử.
# 38. Quy tắc về lỗi và Response
Không trả raw exception ra client.
```
{
    "error": "Npgsql.PostgresException: relation ..."
}
```
Response lỗi nên có cấu trúc thống nhất, ví dụ:
```
{
  "status": 404,
  "message": "Book not found"
}
Validation:
{
  "status": 400,
  "message": "Validation failed",
  "errors": {
    "title": [
      "Title is required"
    ]
  }
}
```
# 39. Quy tắc security
Không bao giờ:
```
Lưu password dạng plain text
Log password
Log JWT
Commit JWT secret
Commit database password
```

Authentication và authorization phải được xử lý tại API/Application/Infrastructure đúng trách nhiệm.
# 40. Quy tắc thay đổi Database
Khi thay đổi Entity có ảnh hưởng database:
Thay đổi Entity
       ↓
Cập nhật Configuration
       ↓
Tạo Migration
       ↓
Kiểm tra Migration
       ↓
Update Database
       ↓
Commit Migration cùng feature
Ví dụ:
```
dotnet ef migrations add AddBookCopyTable \
--project src/Library.Infrastructure \
--startup-project src/Library.Api \
--output-dir Persistence/Migrations
```
Sau đó:
```
dotnet ef database update \
--project src/Library.Infrastructure \
--startup-project src/Library.Api
```

# Quy trình chuẩn khi một thành viên nhận một task
Ví dụ nhận task:
Implement borrow request creation
Thành viên thực hiện:
git checkout main
git pull origin main
        ↓
git checkout -b feat/borrow-request
        ↓
Domain
BorrowRequest / Enum cần thiết
        ↓
Application
DTO → Validator → Service
        ↓
Infrastructure
Repository / Persistence
        ↓
Api
Controller / Endpoint
        ↓
dotnet build Library.slnx
        ↓
Test endpoint
        ↓
git add .
git commit ...
        ↓
git push origin feat/borrow-request
        ↓
Pull Request
        ↓
Code Review
        ↓
Merge main
Đây nên được coi là quy ước chung của toàn bộ nhóm, thay vì mỗi thành viên tự tổ chức code theo cách riêng.