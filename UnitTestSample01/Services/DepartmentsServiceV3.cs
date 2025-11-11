using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using UnitTestSample01.Entities;
using UnitTestSample01.Repositories;

namespace UnitTestSample01.Services
{
    /// <summary>
    /// Version 3 of DepartmentsService using Repository Pattern and Data Annotations
    /// Demonstrates clean architecture with validation through data annotations
    /// Business logic is separated from data access logic
    /// Validation is handled through data annotations for consistency and maintainability
    /// Uses Validator class directly for simple and straightforward validation
    /// </summary>
    public class DepartmentsServiceV3
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ILogger<DepartmentsServiceV3> _logger;

        public DepartmentsServiceV3(IDepartmentRepository departmentRepository,
                                   ILogger<DepartmentsServiceV3> logger)
        {
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a new department with data annotation validation and business rule validation
        /// Uses repository pattern for clean separation of concerns
        /// Uses data annotations for input validation
        /// </summary>
        /// <param name="department">The department to add</param>
        /// <exception cref="ArgumentNullException">Thrown when department is null</exception>
        /// <exception cref="ValidationException">Thrown when data annotation validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
        public void AddDepartment(Department department)
        {
            _logger.LogInformation("Starting AddDepartment operation");

            // Input validation using data annotations
            ValidateInputUsingDataAnnotations(department);

            // Business rules validation
            ValidateBusinessRulesForAdd(department);

            try
            {
                // Repository operations
                _departmentRepository.Add(department);
                var affectedRows = _departmentRepository.SaveChanges();

                if (affectedRows == 0)
                {
                    _logger.LogWarning("No rows were affected when adding department {DepartmentName}", department.Name);
                    throw new InvalidOperationException("Failed to add department to database.");
                }

                _logger.LogInformation("Successfully added department {DepartmentName} with ID {DepartmentId}", 
                                     department.Name, department.Id);
            }
            catch (Exception ex) when (!(ex is ValidationException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Unexpected error occurred while adding department {DepartmentName}", 
                               department?.Name ?? "Unknown");
                throw new InvalidOperationException("An unexpected error occurred while adding the department.", ex);
            }
        }

        /// <summary>
        /// Validates input using data annotations with Validator class
        /// </summary>
        /// <param name="department">The department to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when department is null</exception>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        private void ValidateInputUsingDataAnnotations(Department department)
        {
            _logger.LogDebug("Validating department input using data annotations");

            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            var validationContext = new ValidationContext(department);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(department, validationContext, validationResults, validateAllProperties: true);

            if (!isValid)
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                var message = $"Validation failed for department: {errors}";
                
                _logger.LogWarning("Data annotation validation failed for department: {ValidationError}", message);
                throw new ValidationException(message);
            }

            _logger.LogDebug("Department input validation passed");
        }

        /// <summary>
        /// Validates business rules for AddDepartment operation
        /// </summary>
        /// <param name="department">The department to validate</param>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
        private void ValidateBusinessRulesForAdd(Department department)
        {
            _logger.LogDebug("Validating business rules for department {DepartmentName}", department.Name);

            // Check for duplicate department name
            if (_departmentRepository.ExistsByName(department.Name))
            {
                _logger.LogWarning("Attempt to add duplicate department name: {DepartmentName}", department.Name);
                throw new InvalidOperationException($"A department with name '{department.Name}' already exists.");
            }

            // Additional business rules can be added here
            // For example:
            // - Check if user has permission to add departments
            // - Validate department code format
            // - Check organizational limits
            // - Validate against external systems

            _logger.LogDebug("Business rules validation passed for department {DepartmentName}", department.Name);
        }

        /// <summary>
        /// Gets all departments
        /// </summary>
        /// <returns>List of all departments</returns>
        public IList<Department> GetAllDepartments()
        {
            _logger.LogDebug("Getting all departments");
            return _departmentRepository.GetAll();
        }

        /// <summary>
        /// Gets a department by ID
        /// </summary>
        /// <param name="id">The department ID</param>
        /// <returns>The department if found, null otherwise</returns>
        public Department? GetDepartmentById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Department ID must be greater than zero.", nameof(id));
            }

            _logger.LogDebug("Getting department by ID: {DepartmentId}", id);
            return _departmentRepository.GetById(id);
        }

        /// <summary>
        /// Gets a department by name
        /// </summary>
        /// <param name="name">The department name</param>
        /// <returns>The department if found, null otherwise</returns>
        public Department? GetDepartmentByName(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            
            _logger.LogDebug("Getting department by name: {DepartmentName}", name);
            return _departmentRepository.GetByName(name);
        }

        /// <summary>
        /// Searches departments by name keyword
        /// </summary>
        /// <param name="keyword">The keyword to search for</param>
        /// <returns>List of departments matching the keyword</returns>
        public IList<Department> SearchDepartmentsByName(string keyword)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(keyword, nameof(keyword));
            
            _logger.LogDebug("Searching departments by keyword: {Keyword}", keyword);
            return _departmentRepository.SearchByName(keyword);
        }
    }
}