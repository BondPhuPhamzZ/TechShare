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
            // state EnsureCreated() tries to create the same file again. Check
            // the server first and only attach/create when the database is truly
            // missing; this keeps the startup path idempotent.
            if (context.Database.CanConnect())
            {
                return;
            }

            var source = context.Database.GetDbConnection().ConnectionString;
            var databaseName = GetDatabaseName(source);
            if (DatabaseExistsOnServer(source, databaseName))
            {
                throw new InvalidOperationException(
                    $"SQL Server database '{databaseName}' exists but cannot be opened with the configured connection.");
            }

            if (AttachExistingLocalDbFile(context))
            {
                if (context.Database.CanConnect())
                {
                    return;
                }

                throw new InvalidOperationException(
                    $"SQL Server database '{databaseName}' was found or attached, but the application cannot connect to it.");
            }

            try
            {
                context.Database.EnsureCreated();
            }
            catch (SqlException ex) when (IsExistingDatabaseFileError(ex))
            {
                // Another process may have created/attached it between the
                // existence check and EnsureCreated. Reuse it if it is now
                // reachable; never issue a second CREATE DATABASE blindly.
                if (context.Database.CanConnect())
                {
                    return;
                }

                throw;
            }
        }

        private static bool AttachExistingLocalDbFile(TechShareDbContext context)
        {
            var source = context.Database.GetDbConnection().ConnectionString;
            var builder = new SqlConnectionStringBuilder(source);
            var databaseName = GetDatabaseName(source);

            var mdfPath = builder.AttachDBFilename;
            if (string.IsNullOrWhiteSpace(mdfPath))
            {
                mdfPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    $"{databaseName}.mdf");
            }

            var masterBuilder = new SqlConnectionStringBuilder(source)
            {
                InitialCatalog = "master",
                ConnectTimeout = 5
            };
            masterBuilder.Remove("AttachDbFilename");

            using var connection = new SqlConnection(masterBuilder.ConnectionString);
            connection.Open();

            using (var existsCommand = connection.CreateCommand())
            {
                existsCommand.CommandText = "SELECT DB_ID(@databaseName);";
                existsCommand.Parameters.AddWithValue("@databaseName", databaseName);
                if (existsCommand.ExecuteScalar() is not null and not DBNull)
                {
                    return true;
                }
            }

            if (!File.Exists(mdfPath))
            {
                // There is no orphaned file to attach; the normal EnsureCreated
                // path will create a new LocalDB database.
                return false;
            }

            var safeDatabaseName = databaseName.Replace("]", "]]", StringComparison.Ordinal);
            // SQL Server does not allow a parameter in the FILENAME clause of
            // CREATE DATABASE. Keep the database name parameterized for the
            // existence check, and escape the local path before embedding it
            // as a Unicode SQL string literal.
            var safeMdfPath = mdfPath.Replace("'", "''", StringComparison.Ordinal);
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{safeDatabaseName}] ON (FILENAME = N'{safeMdfPath}') FOR ATTACH_REBUILD_LOG;";
            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlException ex) when (IsExistingDatabaseFileError(ex))
            {
                // A concurrent startup may have attached it first. Treat that
                // case as success and let the caller verify the normal context
                // connection.
                using var verifyCommand = connection.CreateCommand();
                verifyCommand.CommandText = "SELECT DB_ID(@databaseName);";
                verifyCommand.Parameters.AddWithValue("@databaseName", databaseName);
                return verifyCommand.ExecuteScalar() is not null and not DBNull;
            }
        }

        private static string GetDatabaseName(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return string.IsNullOrWhiteSpace(builder.InitialCatalog)
                ? "TechShareDb"
                : builder.InitialCatalog;
        }

        private static bool DatabaseExistsOnServer(string connectionString, string databaseName)
        {
            var masterBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master",
                ConnectTimeout = 5
            };
            masterBuilder.Remove("AttachDbFilename");

            using var connection = new SqlConnection(masterBuilder.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT DB_ID(@databaseName);";
            command.Parameters.AddWithValue("@databaseName", databaseName);
            return command.ExecuteScalar() is not null and not DBNull;
        }

        private static bool IsExistingDatabaseFileError(SqlException exception)
        {
            return exception.Number == 5170 ||
                   exception.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase);
        }
    }
}
