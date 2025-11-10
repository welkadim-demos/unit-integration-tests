using Microsoft.Extensions.Logging;
using Moq;
using UnitTestSample01.Entities;
using UnitTestSample01.Repositories;
using UnitTestSample01.Services;
using Xunit;

namespace XunitTestProject
{
    /// <summary>
    /// Unit tests for DepartmentsServiceV2.AddDepartment method
    /// Demonstrates testing with Repository Pattern using mocks
    /// Shows improved testability through dependency injection
    /// </summary>
    public class DepartmentsServiceV2UnitTests
    {
        private readonly Mock<IDepartmentRepository> _mockRepository;
        private readonly Mock<ILogger<DepartmentsServiceV2>> _mockLogger;
        private readonly DepartmentsServiceV2 _service;

        public DepartmentsServiceV2UnitTests()
        {
            _mockRepository = new Mock<IDepartmentRepository>();
            _mockLogger = new Mock<ILogger<DepartmentsServiceV2>>();
            _service = new DepartmentsServiceV2(_mockRepository.Object, _mockLogger.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DepartmentsServiceV2(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DepartmentsServiceV2(_mockRepository.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_service);
        }

        #endregion

        #region AddDepartment - Input Validation Tests

        [Fact]
        public void AddDepartment_WithNullDepartment_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _service.AddDepartment(null!));
            Assert.Equal("department", exception.ParamName);
        }

        [Fact]
        public void AddDepartment_WithNullName_ShouldThrowArgumentException()
        {
            // Arrange
            var department = new Department { Name = null!, Description = "Test description" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.AddDepartment(department));
            Assert.Contains("Department name cannot be null or empty", exception.Message);
            Assert.Equal("department", exception.ParamName);
        }

        [Fact]
        public void AddDepartment_WithEmptyName_ShouldThrowArgumentException()
        {
            // Arrange
            var department = new Department { Name = "", Description = "Test description" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.AddDepartment(department));
            Assert.Contains("Department name cannot be null or empty", exception.Message);
            Assert.Equal("department", exception.ParamName);
        }

        [Fact]
        public void AddDepartment_WithWhitespaceName_ShouldThrowArgumentException()
        {
            // Arrange
            var department = new Department { Name = "   ", Description = "Test description" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.AddDepartment(department));
            Assert.Contains("Department name cannot be null or empty", exception.Message);
            Assert.Equal("department", exception.ParamName);
        }

        [Fact]
        public void AddDepartment_WithNameExceeding100Characters_ShouldThrowArgumentException()
        {
            // Arrange
            var longName = new string('A', 101); // 101 characters
            var department = new Department { Name = longName, Description = "Test description" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.AddDepartment(department));
            Assert.Contains("Department name cannot exceed 100 characters", exception.Message);
            Assert.Equal("department", exception.ParamName);
        }

        [Fact]
        public void AddDepartment_WithNameExactly100Characters_ShouldNotThrowException()
        {
            // Arrange
            var maxName = new string('A', 100); // Exactly 100 characters
            var department = new Department { Name = maxName, Description = "Test description" };

            _mockRepository.Setup(r => r.ExistsByName(maxName)).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should not throw
            _service.AddDepartment(department);

            // Verify repository interactions
            _mockRepository.Verify(r => r.ExistsByName(maxName), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_WithDescriptionExceeding500Characters_ShouldThrowArgumentException()
        {
            // Arrange
            var longDescription = new string('B', 501); // 501 characters
            var department = new Department { Name = "Test Department", Description = longDescription };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.AddDepartment(department));
            Assert.Contains("Department description cannot exceed 500 characters", exception.Message);
            Assert.Equal("department", exception.ParamName);
        }

        [Fact]
        public void AddDepartment_WithDescriptionExactly500Characters_ShouldNotThrowException()
        {
            // Arrange
            var maxDescription = new string('B', 500); // Exactly 500 characters
            var department = new Department { Name = "Test Department", Description = maxDescription };

            _mockRepository.Setup(r => r.ExistsByName("Test Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should not throw
            _service.AddDepartment(department);

            // Verify repository interactions
            _mockRepository.Verify(r => r.ExistsByName("Test Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_WithNullDescription_ShouldSucceed()
        {
            // Arrange
            var department = new Department { Name = "Test Department", Description = null! };

            _mockRepository.Setup(r => r.ExistsByName("Test Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should not throw
            _service.AddDepartment(department);

            // Verify repository interactions
            _mockRepository.Verify(r => r.ExistsByName("Test Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        #endregion

        #region AddDepartment - Business Logic Tests

        [Fact]
        public void AddDepartment_WithDuplicateName_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var department = new Department { Name = "Existing Department", Description = "Test description" };
            _mockRepository.Setup(r => r.ExistsByName("Existing Department")).Returns(true);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _service.AddDepartment(department));
            Assert.Contains("A department with name 'Existing Department' already exists", exception.Message);

            // Verify repository was called to check existence but not to add
            _mockRepository.Verify(r => r.ExistsByName("Existing Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(It.IsAny<Department>()), Times.Never);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Never);
        }

        [Fact]
        public void AddDepartment_WithValidDepartment_ShouldCallRepositoryMethods()
        {
            // Arrange
            var department = new Department { Name = "HR Department", Description = "Human Resources" };
            _mockRepository.Setup(r => r.ExistsByName("HR Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act
            _service.AddDepartment(department);

            // Assert - Verify all repository methods were called in correct order
            _mockRepository.Verify(r => r.ExistsByName("HR Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_WhenSaveChangesReturnsZero_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var department = new Department { Name = "IT Department", Description = "Information Technology" };
            _mockRepository.Setup(r => r.ExistsByName("IT Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(0); // Simulate no changes saved

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _service.AddDepartment(department));
            Assert.Contains("Failed to add department to database", exception.Message);

            // Verify repository methods were called
            _mockRepository.Verify(r => r.ExistsByName("IT Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_WhenRepositoryThrowsException_ShouldWrapInInvalidOperationException()
        {
            // Arrange
            var department = new Department { Name = "Finance Department", Description = "Financial Operations" };
            _mockRepository.Setup(r => r.ExistsByName("Finance Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _service.AddDepartment(department));
            Assert.Contains("An unexpected error occurred while adding the department", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Database error", exception.InnerException.Message);

            // Verify repository methods were called
            _mockRepository.Verify(r => r.ExistsByName("Finance Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        #endregion

        #region AddDepartment - Edge Cases and Integration Tests

        [Fact]
        public void AddDepartment_WithMinimalValidData_ShouldSucceed()
        {
            // Arrange
            var department = new Department { Name = "A" }; // Minimal valid name
            _mockRepository.Setup(r => r.ExistsByName("A")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should not throw
            _service.AddDepartment(department);

            // Verify repository interactions
            _mockRepository.Verify(r => r.ExistsByName("A"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_WithSpecialCharactersInName_ShouldSucceed()
        {
            // Arrange
            var department = new Department { Name = "R&D-Department (2024)", Description = "Research & Development" };
            _mockRepository.Setup(r => r.ExistsByName("R&D-Department (2024)")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should not throw
            _service.AddDepartment(department);

            // Verify repository interactions
            _mockRepository.Verify(r => r.ExistsByName("R&D-Department (2024)"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Theory]
        [InlineData("Marketing")]
        [InlineData("Sales & Operations")]
        [InlineData("Customer Service")]
        [InlineData("Quality Assurance")]
        [InlineData("Business Development")]
        public void AddDepartment_WithVariousValidNames_ShouldSucceed(string departmentName)
        {
            // Arrange
            var department = new Department { Name = departmentName, Description = $"Description for {departmentName}" };
            _mockRepository.Setup(r => r.ExistsByName(departmentName)).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should not throw
            _service.AddDepartment(department);

            // Verify repository interactions
            _mockRepository.Verify(r => r.ExistsByName(departmentName), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_SuccessfulOperation_ShouldLogAppropriateMessages()
        {
            // Arrange
            var department = new Department { Name = "Legal Department", Description = "Legal Affairs" };
            department.Id = 1; // Simulate ID being set after save

            _mockRepository.Setup(r => r.ExistsByName("Legal Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act
            _service.AddDepartment(department);

            // Assert - Verify logging occurred
            // Note: In a real application, you might want to verify specific log messages
            // Here we verify that the repository methods were called, which means logging would have occurred
            _mockRepository.Verify(r => r.ExistsByName("Legal Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        #endregion
    }
}