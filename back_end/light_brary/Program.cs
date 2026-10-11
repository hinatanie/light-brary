using System.Text;
using light_brary.Services;
using light_brary.Services.Authentication;
using light_brary.Data;
using light_brary.Repositories;
using light_brary.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

/* Cấu hình JWT Authentication */
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(light_brary.Entities.Role.ADMIN));
    options.AddPolicy("LibrarianOrAdmin", p => p.RequireRole(light_brary.Entities.Role.ADMIN, light_brary.Entities.Role.LIBRARIAN));
    options.AddPolicy("UserOrAbove", p => p.RequireRole(light_brary.Entities.Role.ADMIN, light_brary.Entities.Role.LIBRARIAN, light_brary.Entities.Role.USER));
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
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserRoleRepository>();

/* Đăng ký các Authentication */
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // https://localhost:<port>/swagger
}

app.UseExceptionHandlingMiddleware();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Custom Middleware: Tự bóc tách Bearer token và nạp User vào HttpContext.User
app.UseJwtAuthenticationMiddleware();
app.UseAuthorization();

app.MapControllers();

/* Tự động áp dụng Migration (tạo bảng nếu chưa có) & Khởi tạo tài khoản Admin ban đầu */
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    var passwordHasher = services.GetRequiredService<PasswordHasher>();
    await DataSeeder.SeedAsync(context, passwordHasher);
}

app.Run();
