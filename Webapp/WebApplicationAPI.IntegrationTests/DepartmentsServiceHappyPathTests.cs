using Microsoft.Extensions.DependencyInjection;
using WebApplicationAPI.Entities;
using WebApplicationAPI.Model;
using WebApplicationAPI.Services;
using Xunit;

namespace WebApplicationAPI.IntegrationTests
{
    /// <summary>
    /// Minimal integration test for DepartmentsService happy path using SQL Server test containers
    /// Demonstrates that department ID is properly set as identity when adding new items
    /// </summary>
    public class DepartmentsServiceHappyPathTests : IClassFixture<WebApplicationAPIFactory>
    {
        private readonly WebApplicationAPIFactory _factory;

        public DepartmentsServiceHappyPathTests(WebApplicationAPIFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AddDepartment_ShouldSetIdentityId_WhenValidDepartment()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<DepartmentsService>();

            // Clear any existing data
            context.Departments.RemoveRange(context.Departments);
            await context.SaveChangesAsync();

            var newDepartment = new Department
            {
                Name = "Test Department",
                Description = "Test Description"
            };

            // Act
            service.AddDepartment(newDepartment);

            // Assert
            Assert.True(newDepartment.Id > 0, "Department ID should be set to greater than 0 after adding");
            Assert.True(newDepartment.Id >= 1, "Department ID should be set to 1 or greater as identity");
            
            // Verify it's actually in the database
            var savedDepartment = await context.Departments.FindAsync(newDepartment.Id);
            Assert.NotNull(savedDepartment);
            Assert.Equal("Test Department", savedDepartment.Name);
            Assert.Equal("Test Description", savedDepartment.Description);
        }

        [Fact]
        public async Task AddMultipleDepartments_ShouldIncrementIdentityIds_WhenValidDepartments()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<DepartmentsService>();

            // Clear any existing data
            context.Departments.RemoveRange(context.Departments);
            await context.SaveChangesAsync();

            var department1 = new Department { Name = "First Dept", Description = "First" };
            var department2 = new Department { Name = "Second Dept", Description = "Second" };

            // Act
            service.AddDepartment(department1);
            service.AddDepartment(department2);

            // Assert
            Assert.True(department1.Id >= 1, "First department ID should be 1 or greater");
            Assert.True(department2.Id > department1.Id, "Second department ID should be greater than first");
            Assert.Equal(department1.Id + 1, department2.Id);
        }
    }
}