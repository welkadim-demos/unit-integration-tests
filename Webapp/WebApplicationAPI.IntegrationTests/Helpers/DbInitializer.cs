using Microsoft.EntityFrameworkCore;
using WebApplicationAPI.Model;

namespace WebApplicationAPI.IntegrationTests
{
    /// <summary>
    /// Helper class for database initialization and cleanup in integration tests
    /// </summary>
    public class DbInitializer
    {
        private readonly AppDbContext _context;

        public DbInitializer(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initialize the database schema and apply migrations
        /// </summary>
        public async Task InitializeAsync()
        {
            // Ensure database is created and migrations are applied
            await _context.Database.MigrateAsync();
        }

        /// <summary>
        /// Clear all data from the database for clean test state
        /// </summary>
        public async Task ClearAsync()
        {
            // Remove all departments
            var departments = await _context.Departments.ToListAsync();
            _context.Departments.RemoveRange(departments);
            await _context.SaveChangesAsync();

            // Reset identity seed if using SQL Server
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('Departments', RESEED, 0)");
            }
        }

        /// <summary>
        /// Seed the database with test data if needed
        /// </summary>
        public async Task SeedTestDataAsync()
        {
            if (!await _context.Departments.AnyAsync())
            {
                var testDepartments = new[]
                {
                    new Entities.Department { Name = "Human Resources", Description = "HR operations and employee management" },
                    new Entities.Department { Name = "Information Technology", Description = "IT infrastructure and software development" },
                    new Entities.Department { Name = "Finance", Description = "Financial planning and accounting" }
                };

                await _context.Departments.AddRangeAsync(testDepartments);
                await _context.SaveChangesAsync();
            }
        }
    }
}