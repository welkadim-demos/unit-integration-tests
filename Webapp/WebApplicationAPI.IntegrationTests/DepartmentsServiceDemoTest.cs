using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApplicationAPI.Entities;
using WebApplicationAPI.Model;
using WebApplicationAPI.Services;
using Xunit;
using Xunit.Abstractions;

namespace WebApplicationAPI.IntegrationTests
{
    /// <summary>
    /// Demo test for DepartmentsService showing identity ID assignment with test containers
    /// Minimal code demonstrating SQL Server test container integration
    /// </summary>
    public class DepartmentsServiceDemoTest : IClassFixture<WebApplicationAPIFactory>
    {
        private readonly WebApplicationAPIFactory _factory;
        private readonly ITestOutputHelper _output;

        public DepartmentsServiceDemoTest(WebApplicationAPIFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task Demo_AddDepartment_ShouldAssignIdentityId_GreaterThanZero()
        {
            // Arrange - Setup test scope and clean database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<DepartmentsService>();

            // Clean database for isolated test
            context.Departments.RemoveRange(context.Departments);
            await context.SaveChangesAsync();
            
            var department = new Department
            {
                Name = "Demo Department",
                Description = "Testing identity assignment"
            };

            _output.WriteLine($"Before adding: Department ID = {department.Id}");

            // Act - Add department using service
            service.AddDepartment(department);

            // Assert - Verify identity ID was assigned
            _output.WriteLine($"After adding: Department ID = {department.Id}");
            _output.WriteLine($"Connection String: {_factory.ConnectionString}");
            
            Assert.True(department.Id > 0, "Department ID must be greater than 0");
            Assert.True(department.Id >= 1, "Identity ID should start from 1");
            
            // Verify in database
            var savedDept = await context.Departments.FindAsync(department.Id);
            Assert.NotNull(savedDept);
            Assert.Equal("Demo Department", savedDept.Name);
            
            _output.WriteLine("✅ Test completed successfully!");
            _output.WriteLine($"✅ Department saved with ID: {department.Id}");
        }
    }
}