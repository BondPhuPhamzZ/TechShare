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
                var newDevices = new List<Device>
                {
                    new Device{ Name="Macbook Pro M2 2022", Description="Máy cực mạnh, pin siêu trâu.", PricePerDay=300000, DepositAmount=15000000, StockQuantity=2, Status=DeviceStatus.SanSang, CategoryId=1, ImageUrl="https://cdn.tgdd.vn/Products/Images/44/282827/apple-macbook-pro-13-inch-m2-2022-xam-600x600.jpg" },
                    new Device{ Name="Sony A7 Mark III", Description="Chụp ảnh thiếu sáng xuất sắc.", PricePerDay=400000, DepositAmount=20000000, StockQuantity=1, Status=DeviceStatus.SanSang, CategoryId=2, ImageUrl="https://cdn.tgdd.vn/Products/Images/4728/198759/sony-alpha-a7m3-body-600x600.jpg" },
                    new Device{ Name="Micro Rode Wireless GO II", Description="Thu âm cực nét, không độ trễ.", PricePerDay=150000, DepositAmount=3000000, StockQuantity=3, Status=DeviceStatus.SanSang, CategoryId=3, ImageUrl="https://cdn.tgdd.vn/Products/Images/4728/238717/rode-wireless-go-ii-600x600.jpg" },
                    new Device{ Name="Đèn trợ sáng Godox SL60W", Description="Đèn chiếu sáng chuyên dụng cho studio.", PricePerDay=80000, DepositAmount=1000000, StockQuantity=4, Status=DeviceStatus.SanSang, CategoryId=3, ImageUrl="https://cdn.tgdd.vn/Products/Images/4728/238721/godox-sl60w-600x600.jpg" },
                    new Device{ Name="Bàn phím cơ Keychron K2", Description="Bàn phím cơ không dây layout 75%.", PricePerDay=50000, DepositAmount=1500000, StockQuantity=5, Status=DeviceStatus.SanSang, CategoryId=3, ImageUrl="https://cdn.tgdd.vn/Products/Images/86/238734/ban-phim-co-keychron-k2-600x600.jpg" },
                    new Device{ Name="Chuột Logitech MX Master 3", Description="Chuột làm việc đa thiết bị tốt nhất.", PricePerDay=60000, DepositAmount=2000000, StockQuantity=3, Status=DeviceStatus.SanSang, CategoryId=3, ImageUrl="https://cdn.tgdd.vn/Products/Images/86/228148/chuot-bluetooth-logitech-mx-master-3-den-600x600.jpg" },
                    new Device{ Name="Máy ảnh Canon EOS R5", Description="Quay 8K, chụp 45MP siêu nét.", PricePerDay=800000, DepositAmount=40000000, StockQuantity=1, Status=DeviceStatus.SanSang, CategoryId=2, ImageUrl="https://cdn.tgdd.vn/Products/Images/4728/228151/canon-eos-r5-body-600x600.jpg" },
                    new Device{ Name="Lens Canon RF 50mm f/1.8", Description="Lens chân dung cơ bản, xóa phông mịn.", PricePerDay=100000, DepositAmount=3000000, StockQuantity=2, Status=DeviceStatus.SanSang, CategoryId=2, ImageUrl="https://cdn.tgdd.vn/Products/Images/4728/238740/lens-canon-rf-50mm-f1-8-600x600.jpg" },
                    new Device{ Name="Dell XPS 15 9500", Description="Laptop Windows màn hình 4K siêu nét.", PricePerDay=350000, DepositAmount=18000000, StockQuantity=1, Status=DeviceStatus.SanSang, CategoryId=1, ImageUrl="https://cdn.tgdd.vn/Products/Images/44/228155/dell-xps-15-9500-600x600.jpg" },
                    new Device{ Name="Màn hình Dell UltraSharp 27", Description="Màn hình chuẩn màu đồ họa.", PricePerDay=120000, DepositAmount=5000000, StockQuantity=2, Status=DeviceStatus.SanSang, CategoryId=1, ImageUrl="https://cdn.tgdd.vn/Products/Images/5697/238755/man-hinh-dell-ultrasharp-27-inch-600x600.jpg" }
                };
                context.Devices.AddRange(newDevices);
                context.SaveChanges();
            }

            // --- Thêm Reviews mẫu ---
            if (!context.Rentals.Any(r => r.Review != null))
            {
                var device = context.Devices.FirstOrDefault();
                var user = context.Users.FirstOrDefault(u => u.Role == "User");
                if (device != null && user != null)
                {
                    // Tạo 5 đơn thuê hoàn tất để có review
                    for (int i = 1; i <= 5; i++)
                    {
                        var rental = new Rental
                        {
                            RenterId = user.Id,
                            DeviceId = device.Id,
                            Quantity = 1,
                            StartDate = DateTime.Now.AddDays(-10 - i),
                            EndDate = DateTime.Now.AddDays(-8 - i),
                            Status = RentalStatus.HoanTat,
                            DeliveryMethod = DeliveryMethod.TuLay,
                            TotalPrice = device.PricePerDay * 2,
                            ShipTime = DateTime.Now.AddDays(-10 - i),
                            IsReviewed = true
                        };
                        context.Rentals.Add(rental);
                        context.SaveChanges(); // Để lấy RentalId
                        
                        var review = new Review
                        {
                            RentalId = rental.Id,
                            ReviewerId = user.Id,
                            Rating = (i % 2 == 0) ? 5 : 4,
                            Comment = $"Thiết bị rất tuyệt vời, dùng ổn định, chất lượng xứng đáng. Đây là bình luận mẫu số {i}.",
                            CreatedAt = DateTime.Now.AddDays(-7 - i)
                        };
                        context.Reviews.Add(review);
                    }
                    context.SaveChanges();
                }
            }

            // Removed hardcoded device add to avoid duplication, handled by dynamic add above.
        }
    }
}
