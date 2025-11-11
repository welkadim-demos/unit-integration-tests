using Microsoft.Extensions.Logging;
using UnitTestSample01.Entities;
using UnitTestSample01.Repositories;

namespace UnitTestSample01.Services
{
    /// <summary>
    /// Version 2 of DepartmentsService using Repository Pattern
    /// Demonstrates clean architecture with separation of concerns
    /// Business logic is separated from data access logic
    /// Improved testability through dependency injection of repository interface
    /// </summary>
    public class DepartmentsServiceV2
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ILogger<DepartmentsServiceV2> _logger;

        public DepartmentsServiceV2(IDepartmentRepository departmentRepository, 
                                   ILogger<DepartmentsServiceV2> logger)
        {
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a new department with comprehensive business validation
        /// Uses repository pattern for clean separation of concerns
        /// </summary>
        /// <param name="department">The department to add</param>
        /// <exception cref="ArgumentNullException">Thrown when department is null</exception>
        /// <exception cref="ArgumentException">Thrown when department data is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when business rules are violated</exception>
        public void AddDepartment(Department department)
        {
            _logger.LogInformation("Starting AddDepartment operation");

            // Input validation
            ValidateInputForAdd(department);

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
            catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Unexpected error occurred while adding department {DepartmentName}", department.Name);
                throw new InvalidOperationException("An unexpected error occurred while adding the department.", ex);
            }
        }

        /// <summary>
        /// Validates input parameters for AddDepartment operation
        /// </summary>
        /// <param name="department">The department to validate</param>
        private void ValidateInputForAdd(Department department)
        {
            // Null check
            ArgumentNullException.ThrowIfNull(department, nameof(department));

            // Name validation
            if (string.IsNullOrWhiteSpace(department.Name))
            {
                _logger.LogWarning("AddDepartment called with null or empty department name");
                throw new ArgumentException("Department name cannot be null or empty.", nameof(department));
            }

            // Name length validation
            if (department.Name.Length > 100)
            {
                _logger.LogWarning("AddDepartment called with department name longer than 100 characters: {Length}", 
                                 department.Name.Length);
                throw new ArgumentException("Department name cannot exceed 100 characters.", nameof(department));
            }

            // Description validation (if provided)
            if (!string.IsNullOrWhiteSpace(department.Description) && department.Description.Length > 500)
            {
                _logger.LogWarning("AddDepartment called with department description longer than 500 characters: {Length}", 
                                 department.Description.Length);
                throw new ArgumentException("Department description cannot exceed 500 characters.", nameof(department));
            }
        }

        /// <summary>
        /// Validates business rules for AddDepartment operation
        /// </summary>
        /// <param name="department">The department to validate</param>
        private void ValidateBusinessRulesForAdd(Department department)
        {
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