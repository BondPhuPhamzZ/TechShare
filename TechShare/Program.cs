using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TechShare.Data;

namespace TechShare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Avoid the Windows Event Log provider in a normal user session.
            // Writing to that provider without administrator rights can turn a
            // harmless request exception into a second unhandled exception.
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

// Cấu hình Xác thực bằng Cookie (Cookie Authentication) siêu nhẹ cho Đồ án
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Duy trì đăng nhập 7 ngày
    });

            // Add cấu hình Database kết nối với Server. Khi máy không có LocalDB
            // (thường gặp khi chạy file .exe độc lập), dùng SQLite cục bộ để app
            // vẫn khởi động được và phục vụ việc demo/kiểm thử.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection.");
            var canUseSqlServer = CanReachSqlServer(connectionString);
            builder.Services.AddDbContext<TechShareDbContext>(options =>
            {
                if (canUseSqlServer)
                {
                    options.UseSqlServer(connectionString);
                    return;
                }

                options.UseSqlite($"Data Source={GetFallbackSqlitePath()}");
            });

            // Add Custom Services
            builder.Services.AddScoped<TechShare.Services.IPasswordHasherService, TechShare.Services.PasswordHasherService>();
            builder.Services.AddScoped<TechShare.Services.IVNPayService, TechShare.Services.VNPayService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Đảm bảo thứ tự: Authentication phải nằm trước Authorization
            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Add Seed Data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<TechShareDbContext>();
                TechSeedData.Initialize(context);
            }

            app.Run();
        }

        private static bool CanReachSqlServer(string connectionString)
        {
            try
            {
                var master = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master",
                    ConnectTimeout = 2
                };
                master.Remove("AttachDbFilename");
                using var connection = new SqlConnection(master.ConnectionString);
                connection.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetFallbackSqlitePath()
        {
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TechShare", "TechShare.db"),
                Path.Combine(Path.GetTempPath(), "TechShare", "TechShare.db")
            };

            foreach (var candidate in candidates)
            {
                try
                {
                    var directory = Path.GetDirectoryName(candidate);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    using (File.Open(candidate, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                    }
                    return candidate;
                }
                catch (UnauthorizedAccessException)
                {
                    // Try the next user-writable location.
                }
            }

            return Path.Combine(Path.GetTempPath(), "TechShare.db");
        }
    }
}
