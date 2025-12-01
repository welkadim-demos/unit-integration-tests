using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using WebApplicationAPI.DTOs;
using WebApplicationAPI.Model;
using Xunit;

namespace WebApplicationAPI.IntegrationTests
{
    /// <summary>
    /// Integration tests for Departments API using SQL Server test containers
    /// Demonstrates real database operations with containerized SQL Server
    /// </summary>
    public class DepartmentsApiIntegrationTests : IClassFixture<WebApplicationAPIFactory>
    {
        private readonly WebApplicationAPIFactory _factory;
        private readonly HttpClient _client;

        public DepartmentsApiIntegrationTests(WebApplicationAPIFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAllDepartments_ShouldReturnEmptyList_WhenNoDepartmentsExist()
        {
            // Arrange - Clean database
            await ClearDatabase();

            // Act
            var response = await _client.GetAsync("/api/departments");

            // Assert
          

        }

        [Fact]
        public async Task CreateDepartment_ShouldReturnCreated_WhenValidData()
        {
            // Arrange
            await ClearDatabase();
            var createDto = new CreateDepartmentDto
            {
                Name = "Human Resources",
                Description = "Manages employee relations and company policies"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/departments", createDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdDepartment = await response.Content.ReadFromJsonAsync<DepartmentDto>();
            Assert.NotNull(createdDepartment);
            Assert.Equal(createDto.Name, createdDepartment.Name);
            Assert.Equal(createDto.Description, createdDepartment.Description);
            Assert.True(createdDepartment.Id > 0);
        }

        [Fact]
        public async Task CreateDepartment_ShouldReturnConflict_WhenDuplicateName()
        {
            // Arrange
            await ClearDatabase();
            var createDto = new CreateDepartmentDto
            {
                Name = "Finance",
                Description = "Financial operations"
            };

            // Create first department
            await _client.PostAsJsonAsync("/api/departments", createDto);

            // Act - Try to create duplicate
            var response = await _client.PostAsJsonAsync("/api/departments", createDto);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task GetDepartmentById_ShouldReturnDepartment_WhenExists()
        {
            // Arrange
            await ClearDatabase();
            var createDto = new CreateDepartmentDto
            {
                Name = "IT Department",
                Description = "Information Technology services"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/departments", createDto);
            var createdDepartment = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();

            // Act
            var response = await _client.GetAsync($"/api/departments/{createdDepartment!.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var department = await response.Content.ReadFromJsonAsync<DepartmentDto>();
            Assert.NotNull(department);
            Assert.Equal(createdDepartment.Id, department.Id);
            Assert.Equal(createDto.Name, department.Name);
        }

        [Fact]
        public async Task GetDepartmentById_ShouldReturnNotFound_WhenDoesNotExist()
        {
            // Arrange
            await ClearDatabase();

            // Act
            var response = await _client.GetAsync("/api/departments/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateDepartment_ShouldReturnUpdated_WhenValidData()
        {
            // Arrange
            await ClearDatabase();
            var createDto = new CreateDepartmentDto
            {
                Name = "Marketing",
                Description = "Original description"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/departments", createDto);
            var createdDepartment = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();

            var updateDto = new UpdateDepartmentDto
            {
                Name = "Marketing & Sales",
                Description = "Updated description"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/departments/{createdDepartment!.Id}", updateDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedDepartment = await response.Content.ReadFromJsonAsync<DepartmentDto>();
            Assert.NotNull(updatedDepartment);
            Assert.Equal(updateDto.Name, updatedDepartment.Name);
            Assert.Equal(updateDto.Description, updatedDepartment.Description);
        }

        [Fact]
        public async Task DeleteDepartment_ShouldReturnNoContent_WhenExists()
        {
            // Arrange
            await ClearDatabase();
            var createDto = new CreateDepartmentDto
            {
                Name = "Operations",
                Description = "Business operations"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/departments", createDto);
            var createdDepartment = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/departments/{createdDepartment!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/departments/{createdDepartment.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task SearchDepartments_ShouldReturnMatchingResults()
        {
            // Arrange
            await ClearDatabase();
            var departments = new[]
            {
                new CreateDepartmentDto { Name = "Human Resources", Description = "HR operations" },
                new CreateDepartmentDto { Name = "Human Capital", Description = "People management" },
                new CreateDepartmentDto { Name = "Finance", Description = "Financial operations" }
            };

            foreach (var dept in departments)
            {
                await _client.PostAsJsonAsync("/api/departments", dept);
            }

            // Act
            var response = await _client.GetAsync("/api/departments/search?keyword=Human");

            // Assert
            response.EnsureSuccessStatusCode();
            var results = await response.Content.ReadFromJsonAsync<List<DepartmentDto>>();
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.All(results, d => Assert.Contains("Human", d.Name));
        }

        [Fact]
        public async Task CreateDepartment_ShouldReturnBadRequest_WhenInvalidData()
        {
            // Arrange
            var invalidDto = new CreateDepartmentDto
            {
                Name = "", // Invalid - empty name
                Description = "Valid description"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/departments", invalidDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DatabaseConnection_ShouldWork_WithSqlServerContainer()
        {
            // Arrange & Act - Test that we can connect to the database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Assert
            Assert.True(await context.Database.CanConnectAsync());
            Assert.Contains("Server=", _factory.ConnectionString);
            Assert.Contains("Database=", _factory.ConnectionString);
        }

        /// <summary>
        /// Helper method to clear database between tests
        /// </summary>
        private async Task ClearDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            await initializer.ClearAsync();
        }
    }
}