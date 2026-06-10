using TechShare.Models;
using TechShare.Enums;

namespace TechShare.Data
{
    // ========== SEED DATA ==========
    public static class DbInitializer
    {
        public static void Initialize(TechShareDbContext context)
        {
            if (context.Categories.Any())
            {
                return;   
            }

            // === Users ===
            context.Users.AddRange(
                new User{ Username="chu_may_vip@gmail.com", PasswordHash="123456", FullName="Phạm Gia Phú", PhoneNumber="0123456789", StudentId="SV001", ReputationScore = 5.0f },
                new User{ Username="khach_thue", PasswordHash="123456", FullName="Dương Chí Kiệt", PhoneNumber="0987654321", StudentId="SV002", ReputationScore = 4.5f }
            );
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
                new Device{ 
                    Name="Macbook Pro M1 2020", 
                    Description="Máy nguyên bản, pin trâu, cực kỳ mượt mà để code đồ án Web.", 
                    PricePerDay = 250000, 
                    DepositAmount = 10000000, 
                    StockQuantity = 1, 
                    CategoryId = 1, 
                    OwnerId = 1,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/44/231244/macbook-air-m1-2020-gray-600x600.jpg" 
                },
                new Device{ 
                    Name="Sony A6400 + Lens Kit", 
                    Description="Máy ảnh quay vlog cực nét, lấy nét tự động siêu nhanh. Tặng kèm túi chống sốc.", 
                    PricePerDay = 200000, 
                    DepositAmount = 5000000, 
                    StockQuantity = 1, 
                    CategoryId = 2, 
                    OwnerId = 1,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/222621/sony-alpha-a6400-body-1-600x600.jpg"
                },
                new Device{ 
                    Name="Dây cáp kết nối máy chiếu HDMI 5 mét", 
                    Description="Dây dài 5m, bọc dù chống đứt, tín hiệu ổn định. Cho thuê số lượng nhiều để làm sự kiện.", 
                    PricePerDay = 20000, 
                    DepositAmount = 100000, 
                    StockQuantity = 5, 
                    CategoryId = 3, 
                    OwnerId = 1,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/58/289564/cap-hdmi-2-0-day-du-3m-xmobile-ds261-2-600x600.jpg"
                }
            );
            context.SaveChanges();
        }
    }
}
