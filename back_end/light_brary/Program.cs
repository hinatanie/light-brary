using light_brary.Services;
using light_brary.Data;
using light_brary.Repositories;
using light_brary.Middleware;
using light_brary.Constants;
using light_brary.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Dán accessToken từ POST /api/auth/login, không thêm tiền tố Bearer."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

/* Cấu hình Database */
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

/* Cấu hình Redis */
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:Redis.");
    options.InstanceName = "LightBrary:";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(RoleNames.ADMIN));
    options.AddPolicy("LibrarianOrAdmin", p => p.RequireRole(RoleNames.ADMIN, RoleNames.LIBRARIAN));
});

// dependency ịnection

builder.Services.AddScoped<BookCopyService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<BorrowRecordService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<BookCopyRepository>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<BorrowRecordRepository>();
builder.Services.AddScoped<BorrowDetailRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<RoleRepository>();
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// middleware o day ne

app.UseExceptionHandlingMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<ValidateUserMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

/* Tự động áp dụng Migration và khởi tạo dữ liệu */
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await using var transaction = await context.Database.BeginTransactionAsync();
    await DataSeeder.SeedAsync(userManager, roleManager, builder.Configuration);
    if (builder.Configuration.GetValue("Seed:SampleData", app.Environment.IsDevelopment()))
    {
        await LibraryDataSeeder.SeedAsync(context, userManager, builder.Configuration);
    }
    await transaction.CommitAsync();
}

app.Run();
