using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
            EnsureDatabaseAvailable(context);

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

        private static void EnsureDatabaseAvailable(TechShareDbContext context)
        {
            if (context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
            {
                context.Database.EnsureCreated();
                return;
            }

            // A previous LocalDB run can leave TechShareDb.mdf on disk while the
            // database is no longer attached to the LocalDB instance. In that
            // state EnsureCreated() tries to create the same file again and the
            // application terminates with SQL error 5170 (0xe0434352 at the
            // Windows process level). Attach the existing file first.
            try
            {
                if (context.Database.CanConnect())
                {
                    return;
                }

                AttachExistingLocalDbFile(context);
                context.Database.EnsureCreated();
            }
            catch (SqlException ex) when (IsExistingDatabaseFileError(ex))
            {
                AttachExistingLocalDbFile(context);
                context.Database.EnsureCreated();
            }
        }

        private static void AttachExistingLocalDbFile(TechShareDbContext context)
        {
            var source = context.Database.GetDbConnection().ConnectionString;
            var builder = new SqlConnectionStringBuilder(source);
            var databaseName = string.IsNullOrWhiteSpace(builder.InitialCatalog)
                ? "TechShareDb"
                : builder.InitialCatalog;

            var mdfPath = builder.AttachDBFilename;
            if (string.IsNullOrWhiteSpace(mdfPath))
            {
                mdfPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    $"{databaseName}.mdf");
            }

            if (!File.Exists(mdfPath))
            {
                // There is no orphaned file to attach; the normal EnsureCreated
                // path will create a new LocalDB database.
                return;
            }

            var masterBuilder = new SqlConnectionStringBuilder(source)
            {
                InitialCatalog = "master",
                ConnectTimeout = 5
            };
            masterBuilder.Remove("AttachDbFilename");

            using var connection = new SqlConnection(masterBuilder.ConnectionString);
            connection.Open();

            var safeDatabaseName = databaseName.Replace("]", "]]", StringComparison.Ordinal);
            // SQL Server does not allow a parameter in the FILENAME clause of
            // CREATE DATABASE. Keep the database name parameterized for the
            // existence check, and escape the local path before embedding it
            // as a Unicode SQL string literal.
            var safeMdfPath = mdfPath.Replace("'", "''", StringComparison.Ordinal);
            using var command = connection.CreateCommand();
            command.CommandText = $"IF DB_ID(@databaseName) IS NULL " +
                                  $"CREATE DATABASE [{safeDatabaseName}] ON (FILENAME = N'{safeMdfPath}') FOR ATTACH_REBUILD_LOG;";
            command.Parameters.AddWithValue("@databaseName", databaseName);
            command.ExecuteNonQuery();
        }

        private static bool IsExistingDatabaseFileError(SqlException exception)
        {
            return exception.Number == 5170 ||
                   exception.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase);
        }
    }
}
