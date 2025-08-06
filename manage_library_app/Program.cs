using manage_library_app;
using manage_library_app.Models.Entities;
using manage_library_app.Services.Implements;
using manage_library_app.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Lấy Connection String từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowingService, BorrowingService>();

// Cấu hình JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Tùy chọn, nên để true trong thực tế
        ValidateAudience = false, // Tùy chọn, nên để true trong thực tế
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };

    // Tùy chỉnh các sự kiện của JwtBearer
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // Tùy chỉnh response cho lỗi 401 (chưa có token hoặc token không hợp lệ)
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var errorResponse = new
            {
                success = false,
                message = "Bạn chưa đăng nhập hoặc token không hợp lệ.",
                response = (object)null
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        },

        OnAuthenticationFailed = context =>
        {
            // Tùy chỉnh response cho lỗi xác thực (ví dụ: token đã hết hạn)
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var errorResponse = new
            {
                success = false,
                message = "Token không hợp lệ.",
                response = new { errorDetails = context.Exception.Message }
            };
            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Cấu hình URL routing
/*
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Tạo các vai trò mặc định nếu chưa có
    var roles = new[] { "Admin", "Librarian", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Add account admin if not exists in database
    var adminEmail = builder.Configuration["AdminSettings:Email"];
    var adminPassword = builder.Configuration["AdminSettings:Password"];

    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Admin User" };
            var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createAdminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                // Ghi log lỗi nếu không tạo được admin
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError("Failed to create default admin user: {Errors}", string.Join(", ", createAdminResult.Errors.Select(e => e.Description)));
            }
        }
    }
}

app.Run();
/*
 * dotnet ef migrations add InitialCreate: sẽ tạo một thư mục Migrations chứa các file code mô tả cấu trúc database. 
 * dotnet ef database update: sẽ áp dụng các migration để tạo database thực tế dựa trên cấu trúc đã định nghĩa trong DbContext.
 * ISBN (hay International Standard Book Number) là mã số tiêu chuẩn quốc tế cho sách. Đây là dãy số độc nhất, gồm 10 hoặc 13 chữ số, được phân cách bằng dấu cách hoặc dấu gạch ngang. Mỗi cuốn sách được xuất bản để bán thương mại đều được cấp mã ISBN riêng biệt.
 * DueDate: Đây là thuộc tính quan trọng để tính toán xem một cuốn sách có bị quá hạn hay không. Ngày đến hạn sẽ được thiết lập khi sách được mượn.
 * Fines: Thuộc tính này cho phép bạn lưu trữ số tiền phạt (nếu có) khi sách bị trả muộn hoặc hư hỏng. Đây là một phần không thể thiếu của một hệ thống quản lý thư viện.
*/
/*
LibraryManagement.Api/
├── Controllers/
│   ├── AuthController.cs     // Xử lý các endpoint đăng ký, đăng nhập
│   └── BooksController.cs    // Xử lý các endpoint về sách
│   └── ...                   // Các controller khác
│
├── Models/
│   ├── DTOs/
│   │   ├── Auth/
│   │   │   ├── RegisterRequest.cs
│   │   │   └── LoginRequest.cs
│   │   ├── BookDto.cs
│   │   └── ...
│   ├── Entities/
│   │   ├── ApplicationUser.cs
│   │   ├── Book.cs
│   │   └── ...
│
├── Services/
│   ├── AuthService.cs        // Logic nghiệp vụ cho việc xác thực
│   └── BookService.cs        // Logic nghiệp vụ cho việc quản lý sách
│   └── ...
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Migrations/
│
├── Program.cs
└── appsettings.json 
*/