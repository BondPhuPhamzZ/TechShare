using TechShare.Models;
using TechShare.Enums;
using System.Linq;

namespace TechShare.Data
{
    // ========== SEED DATA ==========
    public static class TechSeedData
    {
        public static void Initialize(TechShareDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Categories.Any())
            {
                var users = new User[]
                {
                    new User { Username = "admin", FullName = "Nguyễn Văn Admin", Email = "admin@gmail.com", PasswordHash = "123", PhoneNumber = "0123456789", Role = "Admin" },
                    new User { Username = "khach", FullName = "Trần Thị Khách", Email = "khach@gmail.com", PasswordHash = "123", PhoneNumber = "0987654321", Role = "User" },
                    new User { Username = "hang", FullName = "Lê Văn Hàng", Email = "hang@gmail.com", PasswordHash = "123", PhoneNumber = "0999888777", Role = "User" }
                };
                context.Users.AddRange(users);
                context.SaveChanges();

                // === Categories ===
                context.Categories.AddRange(
                    new Category{ Name="Laptop & PC" },
                    new Category{ Name="Máy ảnh & Quay phim" },
                    new Category{ Name="Phụ kiện (Âm thanh/Ánh sáng)" }
                );
                context.SaveChanges();
            }

            if (context.Devices.Count() < 10)
            {
                for (int i = 1; i <= 10; i++)
                {
                    context.Devices.Add(new Device{ 
                        Name = $"Thiết bị công nghệ ảo siêu cấp VIP Pro {i}", 
                        Description = "Mô tả mẫu cho thiết bị được tự động tạo ra để test phân trang.", 
                        PricePerDay = 150000 + (i * 10000), 
                        DepositAmount = 2000000 + (i * 100000), 
                        StockQuantity = i + 2, 
                        Status = DeviceStatus.SanSang,
                        CategoryId = (i % 3) + 1, 
                        ImageUrl = "https://ui-avatars.com/api/?name=Thiet+Bi&background=random&size=300"
                    });
                }
                context.SaveChanges();
            }

            // Removed hardcoded device add to avoid duplication, handled by dynamic add above.
        }
    }
}
