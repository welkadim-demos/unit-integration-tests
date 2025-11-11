using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using UnitTestSample01.Entities;
using UnitTestSample01.Repositories;
using UnitTestSample01.Services;
using Xunit;

namespace XunitTestProject
{
    /// <summary>
    /// Unit tests for DepartmentsServiceV3.AddDepartment method
    /// Demonstrates testing with Repository Pattern and simplified Data Annotations validation
    /// Uses Validator class directly for straightforward validation testing
    /// </summary>
    public class DepartmentsServiceV3SimplifiedUnitTests
    {
        private readonly Mock<IDepartmentRepository> _mockRepository;
        private readonly Mock<ILogger<DepartmentsServiceV3>> _mockLogger;
        private readonly DepartmentsServiceV3 _service;

        public DepartmentsServiceV3SimplifiedUnitTests()
        {
            _mockRepository = new Mock<IDepartmentRepository>();
            _mockLogger = new Mock<ILogger<DepartmentsServiceV3>>();
            _service = new DepartmentsServiceV3(_mockRepository.Object, _mockLogger.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DepartmentsServiceV3(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new DepartmentsServiceV3(_mockRepository.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_service);
        }

        #endregion

        #region AddDepartment - Data Annotation Validation Tests

        [Fact]
        public void AddDepartment_WithNullDepartment_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _service.AddDepartment(null!));
        }

        [Fact]
        public void AddDepartment_WithEmptyName_ShouldThrowValidationException()
        {
            // Arrange
            var department = new Department { Name = "", Description = "Valid description" };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => _service.AddDepartment(department));
            Assert.Contains("Validation failed for department", exception.Message);
            Assert.Contains("Department name is required", exception.Message);

            // Verify repository was not called due to validation failure
            _mockRepository.Verify(r => r.ExistsByName(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void AddDepartment_WithNameTooLong_ShouldThrowValidationException()
        {
            // Arrange
            var longName = new string('A', 101); // Exceeds StringLength(100)
            var department = new Department { Name = longName, Description = "Valid description" };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => _service.AddDepartment(department));
            Assert.Contains("Validation failed for department", exception.Message);
            Assert.Contains("Department name must be between 1 and 100 characters", exception.Message);

            // Verify repository was not called due to validation failure
            _mockRepository.Verify(r => r.ExistsByName(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void AddDepartment_WithValidDataAnnotations_ShouldPassValidation()
        {
            // Arrange
            var department = new Department { Name = "HR Department", Description = "Human Resources" };
            
            _mockRepository.Setup(r => r.ExistsByName("HR Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act
            _service.AddDepartment(department);

            // Assert - Repository methods should be called after validation passes
            _mockRepository.Verify(r => r.ExistsByName("HR Department"), Times.Once);
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

            // Verify validation passed but repository existence check failed
            _mockRepository.Verify(r => r.ExistsByName("Existing Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(It.IsAny<Department>()), Times.Never);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Never);
        }

        [Fact]
        public void AddDepartment_WithValidDepartment_ShouldCallAllServices()
        {
            // Arrange
            var department = new Department { Name = "IT Department", Description = "Information Technology" };
            
            _mockRepository.Setup(r => r.ExistsByName("IT Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act
            _service.AddDepartment(department);

            // Assert - Verify all services were called in correct order
            _mockRepository.Verify(r => r.ExistsByName("IT Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_WhenSaveChangesReturnsZero_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var department = new Department { Name = "Finance Department", Description = "Financial Operations" };
            
            _mockRepository.Setup(r => r.ExistsByName("Finance Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(0); // Simulate no changes saved

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _service.AddDepartment(department));
            Assert.Contains("Failed to add department to database", exception.Message);

            // Verify all methods were called
            _mockRepository.Verify(r => r.ExistsByName("Finance Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_WhenRepositoryThrowsException_ShouldWrapInInvalidOperationException()
        {
            // Arrange
            var department = new Department { Name = "Legal Department", Description = "Legal Affairs" };
            
            _mockRepository.Setup(r => r.ExistsByName("Legal Department")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Throws(new Exception("Database connection error"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _service.AddDepartment(department));
            Assert.Contains("An unexpected error occurred while adding the department", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Database connection error", exception.InnerException.Message);

            // Verify services were called
            _mockRepository.Verify(r => r.ExistsByName("Legal Department"), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        #endregion

        #region AddDepartment - Integration Flow Tests

        [Theory]
        [InlineData("Marketing")]
        [InlineData("Sales & Customer Success")]
        [InlineData("Research & Development")]
        [InlineData("Quality Assurance")]
        [InlineData("Product Management")]
        public void AddDepartment_WithVariousValidNames_ShouldSucceed(string departmentName)
        {
            // Arrange
            var department = new Department { Name = departmentName, Description = $"Description for {departmentName}" };
            
            _mockRepository.Setup(r => r.ExistsByName(departmentName)).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should not throw
            _service.AddDepartment(department);

            // Verify all services were called
            _mockRepository.Verify(r => r.ExistsByName(departmentName), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddDepartment_WithNameAtExactMaxLength_ShouldSucceed()
        {
            // Arrange
            var maxLengthName = new string('A', 100); // Exactly at StringLength(100)
            var department = new Department { Name = maxLengthName, Description = "Valid description" };
            
            _mockRepository.Setup(r => r.ExistsByName(maxLengthName)).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should not throw
            _service.AddDepartment(department);
            
            // Verify repository methods were called (validation passed)
            _mockRepository.Verify(r => r.ExistsByName(maxLengthName), Times.Once);
            _mockRepository.Verify(r => r.Add(department), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Theory]
        [InlineData("A")] // Minimum length
        [InlineData("IT")] // Short but valid
        [InlineData("Research and Development Department")] // Longer but valid
        public void AddDepartment_WithValidNameLengths_ShouldPassValidation(string departmentName)
        {
            // Arrange
            var department = new Department 
            { 
                Name = departmentName, 
                Description = $"Description for {departmentName}" 
            };
            
            _mockRepository.Setup(r => r.ExistsByName(departmentName)).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should complete successfully
            _service.AddDepartment(department);
            
            // Verify repository was called (validation passed)
            _mockRepository.Verify(r => r.ExistsByName(departmentName), Times.Once);
        }

        #endregion

        #region GetDepartmentById Tests

        [Fact]
        public void GetDepartmentById_WithZeroId_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.GetDepartmentById(0));
            Assert.Contains("Department ID must be greater than zero", exception.Message);
            Assert.Equal("id", exception.ParamName);
        }

        [Fact]
        public void GetDepartmentById_WithNegativeId_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.GetDepartmentById(-1));
            Assert.Contains("Department ID must be greater than zero", exception.Message);
            Assert.Equal("id", exception.ParamName);
        }

        [Fact]
        public void GetDepartmentById_WithValidId_ShouldCallRepository()
        {
            // Arrange
            var expectedDepartment = new Department { Id = 1, Name = "Test Dept" };
            _mockRepository.Setup(r => r.GetById(1)).Returns(expectedDepartment);

            // Act
            var result = _service.GetDepartmentById(1);

            // Assert
            Assert.Equal(expectedDepartment, result);
            _mockRepository.Verify(r => r.GetById(1), Times.Once);
        }

        #endregion
    }
}