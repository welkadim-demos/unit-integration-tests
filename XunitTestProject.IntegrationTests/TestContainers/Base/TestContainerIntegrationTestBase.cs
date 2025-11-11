using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnitTestSample01.Model;
using UnitTestSample01.Services;
using XunitTestProject.IntegrationTests.TestContainers.Collections;
using XunitTestProject.IntegrationTests.TestContainers.Fixtures;

namespace XunitTestProject.IntegrationTests.TestContainers.Base
{
    /// <summary>
    /// Base class for Test Container integration tests that provides common setup and utilities
    /// Demonstrates containerized database testing patterns
    /// </summary>
    [Collection("TestContainer collection")]
    public abstract class TestContainerIntegrationTestBase : IDisposable
    {
        protected readonly TestContainerDatabaseFixture _databaseFixture;
        protected readonly IServiceScope _scope;
        protected readonly AppDbContext _context;
        protected readonly ILogger<DepartmentsService> _logger;
        protected readonly DepartmentsService _departmentsService;

        protected TestContainerIntegrationTestBase(TestContainerDatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            _scope = _databaseFixture.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Create logger for DepartmentsService
            var loggerFactory = _scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<DepartmentsService>();
            
            // Create the service under test
            _departmentsService = new DepartmentsService(_context, _logger);
            
            // Clean database before each test
            CleanDatabase();
        }

        /// <summary>
        /// Cleans the database to ensure test isolation
        /// </summary>
        protected virtual void CleanDatabase()
        {
            _databaseFixture.CleanDatabase();
        }

        /// <summary>
        /// Seeds the database with test data
        /// </summary>
        protected virtual void SeedTestData()
        {
            _databaseFixture.SeedTestData();
        }

        /// <summary>
        /// Creates a new department for testing with container-specific naming
        /// </summary>
        /// <param name="name">Department name</param>
        /// <param name="description">Department description</param>
        /// <returns>New Department instance</returns>
        protected UnitTestSample01.Entities.Department CreateTestDepartment(string name, string? description = null)
        {
            return new UnitTestSample01.Entities.Department
            {
                Name = name,
                Description = description ?? $"Container test description for {name}"
            };
        }

        /// <summary>
        /// Asserts that the database contains exactly the specified number of departments
        /// </summary>
        /// <param name="expectedCount">Expected number of departments</param>
        protected void AssertDepartmentCount(int expectedCount)
        {
            var actualCount = _context.Departments.Count();
            Assert.Equal(expectedCount, actualCount);
        }

        /// <summary>
        /// Asserts that a department with the given name exists in the database
        /// </summary>
        /// <param name="name">Department name to check</param>
        /// <returns>The found department</returns>
        protected UnitTestSample01.Entities.Department AssertDepartmentExists(string name)
        {
            var department = _context.Departments.FirstOrDefault(d => d.Name == name);
            Assert.NotNull(department);
            return department;
        }

        /// <summary>
        /// Asserts that no department with the given name exists in the database
        /// </summary>
        /// <param name="name">Department name to check</param>
        protected void AssertDepartmentDoesNotExist(string name)
        {
            var department = _context.Departments.FirstOrDefault(d => d.Name == name);
            Assert.Null(department);
        }

        /// <summary>
        /// Gets container information for debugging and verification
        /// </summary>
        /// <returns>Container connection details</returns>
        protected (string Hostname, ushort Port, string Image) GetContainerInfo()
        {
            return _databaseFixture.ContainerInfo;
        }

        /// <summary>
        /// Verifies that the container is healthy and responsive
        /// </summary>
        protected async Task AssertContainerHealthyAsync()
        {
            var isHealthy = await _databaseFixture.IsContainerHealthyAsync();
            Assert.True(isHealthy, "Test container should be healthy and responsive");
        }

        public virtual void Dispose()
        {
            _scope?.Dispose();
        }
    }
}