using light_brary.Services;
using light_brary.Data;
using light_brary.Repositories;
using light_brary.Middleware;
using light_brary.Constants;
using light_brary.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

/* Cấu hình CORS cho Frontend */
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://127.0.0.1:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

/* Cấu hình Swagger */
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* Cấu hình Database */
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(RoleNames.ADMIN));
    options.AddPolicy("LibrarianOrAdmin", p => p.RequireRole(RoleNames.ADMIN, RoleNames.LIBRARIAN));
    options.AddPolicy("UserOrAbove", p => p.RequireRole(RoleNames.ADMIN, RoleNames.LIBRARIAN, RoleNames.USER));
});

/* Đăng ký các Serivce */
builder.Services.AddScoped<BookCopyService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<BorrowRecordService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();

/* Đăng ký các Repository */
builder.Services.AddScoped<BookCopyRepository>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<BorrowRecordRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<UserRepository>();

// Đăng ký Identity
builder.Services.AddIdentityApiEndpoints<User>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>();

/* Đăng ký các Authentication */
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // https://localhost:<port>/swagger
}

app.UseExceptionHandlingMiddleware();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

/* Tự động áp dụng Migration (tạo bảng nếu chưa có) & Khởi tạo tài khoản Admin ban đầu */
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await DataSeeder.SeedAsync(userManager, roleManager, builder.Configuration);
}

app.Run();
