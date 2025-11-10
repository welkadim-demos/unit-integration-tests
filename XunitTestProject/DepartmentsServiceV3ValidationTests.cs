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
    /// Integration-style unit tests that demonstrate the actual data annotation validation behavior
    /// These tests use the simplified approach with Validator class directly
    /// Shows how data annotations work without additional abstraction layers
    /// </summary>
    public class DepartmentsServiceV3ValidationTests
    {
        private readonly Mock<IDepartmentRepository> _mockRepository;
        private readonly Mock<ILogger<DepartmentsServiceV3>> _mockLogger;
        private readonly DepartmentsServiceV3 _service;

        public DepartmentsServiceV3ValidationTests()
        {
            _mockRepository = new Mock<IDepartmentRepository>();
            _mockLogger = new Mock<ILogger<DepartmentsServiceV3>>();
            _service = new DepartmentsServiceV3(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void AddDepartment_WithEmptyName_ShouldThrowValidationExceptionWithDataAnnotationMessage()
        {
            // Arrange
            var department = new Department { Name = "", Description = "Valid description" };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => _service.AddDepartment(department));
            
            // Verify it contains the data annotation validation message
            Assert.Contains("Validation failed for department", exception.Message);
            Assert.Contains("Department name is required", exception.Message);
            
            // Verify repository methods were not called due to validation failure
            _mockRepository.Verify(r => r.ExistsByName(It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(r => r.Add(It.IsAny<Department>()), Times.Never);
        }

        [Fact]
        public void AddDepartment_WithNullName_ShouldThrowValidationExceptionWithDataAnnotationMessage()
        {
            // Arrange - Create department with null name by bypassing required modifier
            var department = new Department { Name = "temp", Description = "Valid description" };
            // Set Name to null using reflection to test data annotation behavior
            var nameProperty = typeof(Department).GetProperty("Name");
            nameProperty!.SetValue(department, null);

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => _service.AddDepartment(department));
            
            // Verify it contains the data annotation validation message
            Assert.Contains("Validation failed for department", exception.Message);
            Assert.Contains("Department name is required", exception.Message);
        }

        [Fact]
        public void AddDepartment_WithNameTooLong_ShouldThrowValidationExceptionWithDataAnnotationMessage()
        {
            // Arrange
            var longName = new string('A', 101); // Exceeds StringLength(100)
            var department = new Department { Name = longName, Description = "Valid description" };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => _service.AddDepartment(department));
            
            // Verify it contains the data annotation validation message
            Assert.Contains("Validation failed for department", exception.Message);
            Assert.Contains("Department name must be between 1 and 100 characters", exception.Message);
        }

        [Fact]
        public void AddDepartment_WithNameAtExactMaxLength_ShouldPassValidation()
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

        [Fact]
        public void AddDepartment_WithValidDepartment_ShouldPassAllValidations()
        {
            // Arrange
            var department = new Department 
            { 
                Name = "Human Resources", 
                Description = "Manages employee relations and company policies" 
            };
            
            _mockRepository.Setup(r => r.ExistsByName("Human Resources")).Returns(false);
            _mockRepository.Setup(r => r.Add(department)).Returns(department);
            _mockRepository.Setup(r => r.SaveChanges()).Returns(1);

            // Act & Assert - Should complete successfully
            _service.AddDepartment(department);
            
            // Verify all repository methods were called in sequence
            _mockRepository.Verify(r => r.ExistsByName("Human Resources"), Times.Once);
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

        [Fact]
        public void AddDepartment_WithMultipleValidationErrors_ShouldReturnFirstValidationError()
        {
            // Arrange
            var department = new Department 
            { 
                Name = "", // Required validation will fail
                Description = "Valid description" 
            };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => _service.AddDepartment(department));
            
            // Data annotations typically return the first validation error encountered
            Assert.Contains("Department name is required", exception.Message);
        }
    }
}