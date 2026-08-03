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

            if (context.Categories.Any())
            {
                return;
            }

            var users = new User[]
            {
                new User { Username = "admin", FullName = "Nguyễn Văn Admin", Email = "admin@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), PhoneNumber = "0123456789", Role = "Admin", IsVerified = true },
                new User { Username = "khach", FullName = "Trần Thị Khách", Email = "khach@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), PhoneNumber = "0987654321", Role = "User", IsVerified = true },
                new User { Username = "hang", FullName = "Lê Văn Hàng", Email = "hang@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), PhoneNumber = "0999888777", Role = "User", IsVerified = true }
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

            // === Devices ===
            context.Devices.AddRange(
                // Category 1: Laptop & PC
                new Device{ Name="Macbook Pro M1 2020", Description="Máy nguyên bản, pin trâu, cực kỳ mượt mà để code đồ án Web.", PricePerDay = 250000, DepositAmount = 10000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600&h=600&fit=crop" },
                new Device{ Name="Dell XPS 15 9500", Description="Màn hình 4K siêu sắc nét, phù hợp thiết kế đồ họa.", PricePerDay = 300000, DepositAmount = 15000000, StockQuantity = 2, Status = DeviceStatus.SanSang, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1593640408182-31c70c8268f5?w=600&h=600&fit=crop" },
                
                // Category 2: Máy ảnh & Quay phim
                new Device{ Name="Sony A6400 + Lens Kit", Description="Máy ảnh quay vlog cực nét, lấy nét tự động siêu nhanh. Tặng kèm túi chống sốc.", PricePerDay = 200000, DepositAmount = 5000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 2, ImageUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=600&h=600&fit=crop" },

                // Category 3: Phụ kiện (Âm thanh/Ánh sáng)
                new Device{ Name="Micro thu âm Rode Wireless GO II", Description="Thu âm 2 người cùng lúc, lọc ồn siêu tốt.", PricePerDay = 150000, DepositAmount = 4000000, StockQuantity = 2, Status = DeviceStatus.SanSang, CategoryId = 3, ImageUrl = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=600&h=600&fit=crop" }
            );
            context.SaveChanges();
        }
    }
}
