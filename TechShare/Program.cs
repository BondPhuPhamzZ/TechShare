using Microsoft.EntityFrameworkCore;
using TechShare.Data;

namespace TechShare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            // Add cấu hình Database kết nối với Server
            builder.Services.AddDbContext<TechShareDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Custom Services
            builder.Services.AddScoped<TechShare.Services.IPasswordHasherService, TechShare.Services.PasswordHasherService>();

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
    }
}
