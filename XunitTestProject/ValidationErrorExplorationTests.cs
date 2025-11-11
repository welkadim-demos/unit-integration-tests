using System.ComponentModel.DataAnnotations;
using UnitTestSample01.Entities;
using Xunit;
using Xunit.Abstractions;

namespace XunitTestProject
{
    /// <summary>
    /// Test to explore actual data annotation error messages
    /// </summary>
    public class ValidationErrorExplorationTests
    {
        private readonly ITestOutputHelper _output;

        public ValidationErrorExplorationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ExploreValidationErrors_WithEmptyName_ShouldShowActualMessage()
        {
            // Arrange
            var department = new Department { Name = "", Description = "Valid description" };
            var validationContext = new ValidationContext(department);
            var validationResults = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(department, validationContext, validationResults, validateAllProperties: true);

            // Assert and Output
            Assert.False(isValid);
            _output.WriteLine($"Number of validation errors: {validationResults.Count}");
            foreach (var result in validationResults)
            {
                _output.WriteLine($"Error: {result.ErrorMessage}");
                _output.WriteLine($"Member Names: {string.Join(", ", result.MemberNames)}");
            }
        }

        [Fact]
        public void ExploreValidationErrors_WithNameTooLong_ShouldShowActualMessage()
        {
            // Arrange
            var longName = new string('A', 101);
            var department = new Department { Name = longName, Description = "Valid description" };
            var validationContext = new ValidationContext(department);
            var validationResults = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(department, validationContext, validationResults, validateAllProperties: true);

            // Assert and Output
            Assert.False(isValid);
            _output.WriteLine($"Number of validation errors: {validationResults.Count}");
            foreach (var result in validationResults)
            {
                _output.WriteLine($"Error: {result.ErrorMessage}");
                _output.WriteLine($"Member Names: {string.Join(", ", result.MemberNames)}");
            }
        }
    }
}