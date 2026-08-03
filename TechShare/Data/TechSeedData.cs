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
                new Device{ Name="Macbook Pro M1 2020", Description="Máy nguyên bản, pin trâu, cực kỳ mượt mà để code đồ án Web.", PricePerDay = 250000, DepositAmount = 10000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 1, ImageUrl = "https://cdn.tgdd.vn/Products/Images/44/231244/macbook-air-m1-2020-gray-600x600.jpg" },
                new Device{ Name="Dell XPS 15 9500", Description="Màn hình 4K siêu sắc nét, phù hợp thiết kế đồ họa.", PricePerDay = 300000, DepositAmount = 15000000, StockQuantity = 2, Status = DeviceStatus.SanSang, CategoryId = 1, ImageUrl = "https://cdn.tgdd.vn/Products/Images/44/281735/dell-xps-15-9520-i7-71000670-thumb-600x600.jpg" },
                new Device{ Name="Lenovo ThinkPad X1 Carbon Gen 9", Description="Bàn phím gõ cực sướng, mỏng nhẹ, pin cả ngày.", PricePerDay = 280000, DepositAmount = 12000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 1, ImageUrl = "https://cdn.tgdd.vn/Products/Images/44/279188/lenovo-thinkpad-x1-carbon-gen-9-i7-20xw00g8vn-thumb-600x600.jpg" },
                new Device{ Name="Asus ROG Zephyrus G14", Description="Laptop gaming cấu hình khủng, chiến mọi tựa game.", PricePerDay = 350000, DepositAmount = 18000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 1, ImageUrl = "https://cdn.tgdd.vn/Products/Images/44/286044/asus-rog-zephyrus-g14-ga402rj-r7-l8030w-600x600.jpg" },
                new Device{ Name="Surface Pro 8", Description="Lai giữa tablet và laptop, siêu tiện lợi cho văn phòng.", PricePerDay = 200000, DepositAmount = 9000000, StockQuantity = 3, Status = DeviceStatus.SanSang, CategoryId = 1, ImageUrl = "https://cdn.tgdd.vn/Products/Images/44/263590/surface-pro-8-i5-8p2-00015-600x600.jpg" },
                new Device{ Name="PC Gaming Core i7 RTX 3060", Description="Thùng PC cấu hình cao cho ae thuê cày game/render ở nhà.", PricePerDay = 400000, DepositAmount = 20000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 1, ImageUrl = "https://cdn.tgdd.vn/Products/Images/5698/312061/pc-asus-rog-strix-g10ce-i5-51140f099w-thumb-1-600x600.jpg" },
                
                // Category 2: Máy ảnh & Quay phim
                new Device{ Name="Sony A6400 + Lens Kit", Description="Máy ảnh quay vlog cực nét, lấy nét tự động siêu nhanh. Tặng kèm túi chống sốc.", PricePerDay = 200000, DepositAmount = 5000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 2, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/222621/sony-alpha-a6400-body-1-600x600.jpg" },
                new Device{ Name="Canon EOS R5", Description="Quay 8K RAW siêu khủng, chụp thể thao bắt nét nhanh.", PricePerDay = 800000, DepositAmount = 40000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 2, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/238870/canon-eos-r5-body-1-600x600.jpg" },
                new Device{ Name="Fujifilm X-T4", Description="Màu giả lập film cực đẹp, chống rung ngon nghẻ.", PricePerDay = 350000, DepositAmount = 15000000, StockQuantity = 2, Status = DeviceStatus.SanSang, CategoryId = 2, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/235882/fujifilm-x-t4-body-1-600x600.jpg" },
                new Device{ Name="GoPro Hero 10 Black", Description="Camera hành trình chống rung mượt nhất hiện tại.", PricePerDay = 150000, DepositAmount = 4000000, StockQuantity = 4, Status = DeviceStatus.SanSang, CategoryId = 2, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/289524/gopro-hero-10-black-thumb-1-600x600.jpg" },
                new Device{ Name="DJI Osmo Pocket 2", Description="Camera gimbal mini bỏ túi tiện lợi quay vlog.", PricePerDay = 100000, DepositAmount = 3000000, StockQuantity = 2, Status = DeviceStatus.SanSang, CategoryId = 2, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/238865/dji-pocket-2-1-600x600.jpg" },
                new Device{ Name="Sony Alpha A7 IV", Description="Đỉnh cao quay chụp FullFrame, hệ màu chuẩn xác.", PricePerDay = 600000, DepositAmount = 30000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 2, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/292676/sony-alpha-a7c-ii-body-thumb-600x600.jpg" },
                new Device{ Name="Lens Canon EF 50mm f/1.8", Description="Lens chân dung thần thánh, xóa phông mịt mù.", PricePerDay = 50000, DepositAmount = 1500000, StockQuantity = 5, Status = DeviceStatus.SanSang, CategoryId = 2, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/238878/canon-ef-50mm-f-18-stm-1-600x600.jpg" },

                // Category 3: Phụ kiện (Âm thanh/Ánh sáng)
                new Device{ Name="Dây cáp kết nối máy chiếu HDMI 5 mét", Description="Dây dài 5m, bọc dù chống đứt, tín hiệu ổn định. Cho thuê số lượng nhiều để làm sự kiện.", PricePerDay = 20000, DepositAmount = 100000, StockQuantity = 5, Status = DeviceStatus.SanSang, CategoryId = 3, ImageUrl = "https://cdn.tgdd.vn/Products/Images/58/289564/cap-hdmi-2-0-day-du-3m-xmobile-ds261-2-600x600.jpg" },
                new Device{ Name="Micro thu âm Rode Wireless GO II", Description="Thu âm 2 người cùng lúc, lọc ồn siêu tốt.", PricePerDay = 150000, DepositAmount = 4000000, StockQuantity = 2, Status = DeviceStatus.SanSang, CategoryId = 3, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/289525/rode-wireless-go-ii-thumb-1-600x600.jpg" },
                new Device{ Name="Đèn quay phim Godox SL60W", Description="Ánh sáng chuẩn studio, kèm sẵn softbox.", PricePerDay = 80000, DepositAmount = 1500000, StockQuantity = 3, Status = DeviceStatus.SanSang, CategoryId = 3, ImageUrl = "https://cdn.tgdd.vn/Products/Images/54/293527/den-led-video-godox-sl60w-thumb-600x600.jpg" },
                new Device{ Name="Gimbal DJI RS 3", Description="Chống rung đỉnh cao cho máy ảnh lớn.", PricePerDay = 250000, DepositAmount = 7000000, StockQuantity = 2, Status = DeviceStatus.SanSang, CategoryId = 3, ImageUrl = "https://cdn.tgdd.vn/Products/Images/4728/289529/dji-rs-3-thumb-1-600x600.jpg" },
                new Device{ Name="Loa Bluetooth JBL PartyBox 310", Description="Quẩy tung nóc cho tiệc BBQ ngoài trời.", PricePerDay = 300000, DepositAmount = 8000000, StockQuantity = 1, Status = DeviceStatus.SanSang, CategoryId = 3, ImageUrl = "https://cdn.tgdd.vn/Products/Images/2162/236113/loa-bluetooth-jbl-partybox-310-1-600x600.jpg" },
                new Device{ Name="Tai nghe kiểm âm Sony MDR-7506", Description="Chuẩn cho dân dựng phim, mix nhạc.", PricePerDay = 70000, DepositAmount = 1500000, StockQuantity = 4, Status = DeviceStatus.SanSang, CategoryId = 3, ImageUrl = "https://cdn.tgdd.vn/Products/Images/54/281313/tai-nghe-chup-tai-sony-mdr-7506-thumb-600x600.jpg" },
                new Device{ Name="Chân đế Tripod Benro T880EX", Description="Chân đế chắc chắn, phù hợp quay chụp đa góc.", PricePerDay = 30000, DepositAmount = 500000, StockQuantity = 6, Status = DeviceStatus.SanSang, CategoryId = 3, ImageUrl = "https://cdn.tgdd.vn/Products/Images/54/281315/chan-de-may-anh-benro-t880ex-thumb-600x600.jpg" }
            );
            context.SaveChanges();
        }
    }
}
