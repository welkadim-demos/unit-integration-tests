using Microsoft.EntityFrameworkCore;
using UnitTestSample01.Entities;
using XunitTestProject.IntegrationTests.TestContainers.Base;
using XunitTestProject.IntegrationTests.TestContainers.Fixtures;

namespace XunitTestProject.IntegrationTests.TestContainers
{
    /// <summary>
    /// Integration tests for DepartmentsService using SQL Server Test Containers
    /// Demonstrates containerized database testing as an alternative to LocalDB
    /// Tests the complete interaction between service, Entity Framework, and containerized SQL Server
    /// </summary>
    public class TestContainerDepartmentsServiceIntegrationTests : TestContainerIntegrationTestBase
    {
        public TestContainerDepartmentsServiceIntegrationTests(TestContainerDatabaseFixture databaseFixture) 
            : base(databaseFixture)
        {
        }

        #region Container Health Verification Tests

        [Fact]
        public async Task Container_ShouldBeHealthyAndResponsive()
        {
            // Act & Assert
            await AssertContainerHealthyAsync();
            
            // Verify container details
            var (hostname, port, image) = GetContainerInfo();
            Assert.NotEmpty(hostname);
            Assert.True(port > 0);
            Assert.Contains("mssql/server", image);
        }

        [Fact]
        public void Container_ShouldProvideValidConnectionString()
        {
            // Act
            var connectionString = _databaseFixture.ConnectionString;

            // Assert
            Assert.NotNull(connectionString);
            Assert.Contains("Server=", connectionString);
            Assert.Contains("Database=master", connectionString);
            Assert.Contains("User Id=sa", connectionString);
        }

        #endregion

        #region AddDepartment Integration Tests (Container Version)

        [Fact]
        public void AddDepartment_WithValidDataInContainer_ShouldPersistToDatabase()
        {
            // Arrange
            var department = CreateTestDepartment("Container Marketing", "Marketing running in SQL Server container");

            // Act
            _departmentsService.AddDepartment(department);

            // Assert
            AssertDepartmentCount(1);
            var savedDepartment = AssertDepartmentExists("Container Marketing");
            Assert.Equal("Marketing running in SQL Server container", savedDepartment.Description);
            Assert.True(savedDepartment.Id > 0);
        }

        [Fact]
        public void AddDepartment_WithMultipleDepartmentsInContainer_ShouldPersistAllToDatabase()
        {
            // Arrange
            var dept1 = CreateTestDepartment("Container Sales", "Sales operations in container");
            var dept2 = CreateTestDepartment("Container Support", "Customer support in container");
            var dept3 = CreateTestDepartment("Container Research", "R&D in container");

            // Act
            _departmentsService.AddDepartment(dept1);
            _departmentsService.AddDepartment(dept2);
            _departmentsService.AddDepartment(dept3);

            // Assert
            AssertDepartmentCount(3);
            AssertDepartmentExists("Container Sales");
            AssertDepartmentExists("Container Support");
            AssertDepartmentExists("Container Research");
        }

        [Fact]
        public void AddDepartment_WithDuplicateNameInContainer_ShouldThrowAndNotPersist()
        {
            // Arrange
            var originalDept = CreateTestDepartment("Container Operations", "Original operations in container");
            _departmentsService.AddDepartment(originalDept);
            
            var duplicateDept = CreateTestDepartment("Container Operations", "Duplicate operations in container");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _departmentsService.AddDepartment(duplicateDept));
            
            // Verify only original department exists
            AssertDepartmentCount(1);
            var savedDept = AssertDepartmentExists("Container Operations");
            Assert.Equal("Original operations in container", savedDept.Description);
        }

        #endregion

        #region Container-Specific Transaction Tests

        [Fact]
        public void AddDepartment_ContainerTransactionRollback_ShouldNotPersistOnException()
        {
            // Arrange - Add a department first
            var validDept = CreateTestDepartment("Container Legal", "Legal affairs in container");
            _departmentsService.AddDepartment(validDept);
            AssertDepartmentCount(1);

            // Try to add duplicate (should fail)
            var duplicateDept = CreateTestDepartment("Container Legal", "Another legal dept in container");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _departmentsService.AddDepartment(duplicateDept));
            
            // Verify database state remains unchanged in container
            AssertDepartmentCount(1);
            var originalDept = AssertDepartmentExists("Container Legal");
            Assert.Equal("Legal affairs in container", originalDept.Description);
        }

        #endregion

        #region Container Data Seeding Tests

        [Fact]
        public void SeedTestData_InContainer_ShouldCreateContainerSpecificData()
        {
            // Act
            SeedTestData();

            // Assert
            AssertDepartmentCount(3);
            AssertDepartmentExists("Container HR");
            AssertDepartmentExists("Container IT");
            AssertDepartmentExists("Container Finance");

            // Verify container-specific descriptions
            var hrDept = AssertDepartmentExists("Container HR");
            Assert.Equal("Human Resources running in container", hrDept.Description);
        }

        #endregion

        #region Container Performance Tests

        [Fact]
        public async Task Container_DatabaseOperations_ShouldPerformWell()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var departments = new List<Department>();
            
            for (int i = 1; i <= 10; i++)
            {
                departments.Add(CreateTestDepartment($"Container Dept {i:00}", $"Department {i} in container"));
            }

            // Act - Measure bulk operations in container
            foreach (var dept in departments)
            {
                _departmentsService.AddDepartment(dept);
            }
            
            stopwatch.Stop();

            // Assert - Should complete within reasonable time
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Container operations took {stopwatch.ElapsedMilliseconds}ms, should be under 5000ms");
            AssertDepartmentCount(10);

            // Verify container is still healthy after bulk operations
            await AssertContainerHealthyAsync();
        }

        #endregion

        #region Container-Specific Query Tests

        [Fact]
        public void GetAllDepartments_FromContainer_ShouldReturnAllContainerData()
        {
            // Arrange
            SeedTestData();

            // Act
            var departments = _departmentsService.GetAllDepartments();

            // Assert
            Assert.Equal(3, departments.Count);
            Assert.All(departments, dept => Assert.Contains("Container", dept.Name));
            Assert.All(departments, dept => Assert.Contains("container", dept.Description!.ToLower()));
        }

        [Fact]
        public void SearchDepartmentsByName_InContainer_ShouldRespectContainerCollation()
        {
            // Arrange
            var dept1 = CreateTestDepartment("Container Data Analytics", "Analytics in container");
            var dept2 = CreateTestDepartment("Container Business Intelligence", "BI in container");
            _departmentsService.AddDepartment(dept1);
            _departmentsService.AddDepartment(dept2);

            // Act - Test case insensitive search in SQL Server container
            var results = _departmentsService.SearchDepartmentsByName("container");

            // Assert - Should find both departments (case insensitive)
            Assert.Equal(2, results.Count);
            Assert.All(results, dept => Assert.Contains("Container", dept.Name));
        }

        #endregion

        #region Container Isolation Tests

        [Fact]
        public void Container_ShouldProvideCompleteIsolation()
        {
            // Arrange
            var department = CreateTestDepartment("Isolation Test", "Testing container isolation");
            
            // Act
            _departmentsService.AddDepartment(department);
            
            // Assert - This test runs in its own container context
            AssertDepartmentCount(1);
            AssertDepartmentExists("Isolation Test");
            
            // Verify we can connect to our container
            Assert.True(_context.Database.CanConnect());
            
            // Verify container details are accessible
            var (hostname, port, image) = GetContainerInfo();
            Assert.NotEmpty(hostname);
            Assert.True(port > 1024); // Should be a mapped port
        }

        #endregion
    }
}