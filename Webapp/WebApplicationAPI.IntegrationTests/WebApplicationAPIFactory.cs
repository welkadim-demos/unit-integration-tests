using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;
using WebApplicationAPI.Model;

namespace WebApplicationAPI.IntegrationTests
{
    /// <summary>
    /// Custom WebApplicationFactory for integration tests using SQL Server test containers
    /// Provides isolated database environment for each test run
    /// </summary>
    public class WebApplicationAPIFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _msSqlContainer;

        public WebApplicationAPIFactory()
        {
            // Configure MS SQL Server test container
            _msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("YourStrong!Password123")
                //.WithCleanUp(true)
                .Build();
        }

        /// <summary>
        /// Gets the connection string for the test container
        /// </summary>
        public string ConnectionString => _msSqlContainer.GetConnectionString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext configuration
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Add DbContext with SQL Server test container
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(_msSqlContainer.GetConnectionString());
                    options.EnableSensitiveDataLogging();
                    options.EnableServiceProviderCaching(false);
                    options.EnableDetailedErrors();
                });

                // Ensure the database is created and migrations are applied
                services.AddScoped<DbInitializer>();
            });

            builder.UseEnvironment("Testing");
            
            // Configure logging for better test debugging
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });
        }

        /// <summary>
        /// Initialize the test container before tests run
        /// </summary>
        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync();
            
            // Create a scope to initialize the database
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Ensure database is created and apply migrations
            //await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            
            // Apply any pending migrations
            //await context.Database.MigrateAsync();
        }

        /// <summary>
        /// Clean up resources after tests complete
        /// </summary>
        public new async Task DisposeAsync()
        {
            await _msSqlContainer.StopAsync();
            await _msSqlContainer.DisposeAsync();
            await base.DisposeAsync();
        }
    }

    /// <summary>
    /// Helper class for database initialization in tests
    /// </summary>
    public class DbInitializer2
    {
        private readonly AppDbContext _context;

        public DbInitializer2(AppDbContext context)
        {
          _context = context;
        }

        /// <summary>
        /// Seeds the database with test data
        /// </summary>
        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            
            // Add any seed data here if needed for tests
            if (!_context.Departments.Any())
            {
                var departments = new[]
                {
                    new WebApplicationAPI.Entities.Department
                    {
                        Name = "Test Department 1",
                        Description = "Test department for integration tests"
                    },
                    new WebApplicationAPI.Entities.Department
                    {
                        Name = "Test Department 2", 
                        Description = "Another test department"
                    }
                };

                _context.Departments.AddRange(departments);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Clears all data from the database
        /// </summary>
        public async Task ClearAsync()
        {
            _context.Departments.RemoveRange(_context.Departments);
            await _context.SaveChangesAsync();
        }
    }
}

