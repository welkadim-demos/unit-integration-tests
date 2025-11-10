using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Testcontainers.MsSql;
using UnitTestSample01.Model;

namespace XunitTestProject.IntegrationTests.TestContainers.Fixtures
{
    /// <summary>
    /// Database fixture for integration tests using SQL Server Test Containers
    /// Demonstrates containerized database testing as an alternative to LocalDB
    /// </summary>
    public class TestContainerDatabaseFixture : IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MsSqlContainer _sqlContainer;
        private readonly string _connectionString;
        
        public TestContainerDatabaseFixture()
        {
            // Create SQL Server container with specific configuration
            _sqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("StrongPassword123!")
                .Build();

            // Start the container synchronously in constructor
            // Note: This is acceptable for test fixtures
            _sqlContainer.StartAsync().GetAwaiter().GetResult();
            
            _connectionString = _sqlContainer.GetConnectionString();

            // Setup dependency injection container
            var services = new ServiceCollection();
            
            // Configure DbContext with SQL Server Test Container
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_connectionString)
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors());

            // Configure logging for debugging
            services.AddLogging(builder =>
                builder.AddConsole()
                       .AddDebug()
                       .SetMinimumLevel(LogLevel.Information)
                       .AddFilter("Testcontainers", LogLevel.Information)
                       .AddFilter("Docker", LogLevel.Information));

            _serviceProvider = services.BuildServiceProvider();

            // Create and migrate database
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Apply migrations to create database schema
            context.Database.Migrate();
        }

        /// <summary>
        /// Creates a new scope and returns the DbContext
        /// Each test should use a new scope to ensure proper isolation
        /// </summary>
        /// <returns>A scoped DbContext instance</returns>
        public IServiceScope CreateScope()
        {
            return _serviceProvider.CreateScope();
        }

        /// <summary>
        /// Gets a new DbContext instance within a scope
        /// </summary>
        /// <returns>AppDbContext instance</returns>
        public AppDbContext CreateContext()
        {
            var scope = CreateScope();
            return scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }

        /// <summary>
        /// Gets the connection string used by this fixture
        /// </summary>
        public string ConnectionString => _connectionString;

        /// <summary>
        /// Gets the SQL Server container instance (for advanced scenarios)
        /// </summary>
        public MsSqlContainer SqlContainer => _sqlContainer;

        /// <summary>
        /// Gets container information for debugging
        /// </summary>
        public (string Hostname, ushort Port, string Image) ContainerInfo => (
            _sqlContainer.Hostname,
            _sqlContainer.GetMappedPublicPort(1433),
            _sqlContainer.Image.FullName
        );

        /// <summary>
        /// Seeds the database with test data
        /// </summary>
        public void SeedTestData()
        {
            using var scope = CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Clear existing data
            context.Departments.RemoveRange(context.Departments);
            context.SaveChanges();

            // Add seed data
            var departments = new[]
            {
                new UnitTestSample01.Entities.Department 
                { 
                    Name = "Container HR", 
                    Description = "Human Resources running in container" 
                },
                new UnitTestSample01.Entities.Department 
                { 
                    Name = "Container IT", 
                    Description = "Information Technology running in container" 
                },
                new UnitTestSample01.Entities.Department 
                { 
                    Name = "Container Finance", 
                    Description = "Finance operations running in container" 
                }
            };

            context.Departments.AddRange(departments);
            context.SaveChanges();
        }

        /// <summary>
        /// Cleans the database by removing all data
        /// </summary>
        public void CleanDatabase()
        {
            using var scope = CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Remove all departments
            context.Departments.RemoveRange(context.Departments);
            context.SaveChanges();
        }

        /// <summary>
        /// Checks if the container is healthy and responsive
        /// </summary>
        public async Task<bool> IsContainerHealthyAsync()
        {
            try
            {
                using var scope = CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            // Clean up database and stop container
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureDeletedAsync();
            }
            catch (Exception)
            {
                // Ignore cleanup errors during disposal
            }

            // Stop and dispose the SQL Server container
            if (_sqlContainer != null)
            {
                await _sqlContainer.StopAsync();
                await _sqlContainer.DisposeAsync();
            }

            // Dispose service provider
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        // Implement IDisposable for backward compatibility
        public void Dispose()
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}