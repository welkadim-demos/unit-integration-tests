using UnitTestSample01.Entities;

namespace UnitTestSample01.Repositories
{
    /// <summary>
    /// Repository interface for Department entity operations
    /// Provides abstraction layer for data access operations
    /// Enables better testability and separation of concerns
    /// </summary>
    public interface IDepartmentRepository
    {
        /// <summary>
        /// Adds a new department to the data store
        /// </summary>
        /// <param name="department">The department to add</param>
        /// <returns>The added department with generated ID</returns>
        Department Add(Department department);

        /// <summary>
        /// Updates an existing department in the data store
        /// </summary>
        /// <param name="department">The department to update</param>
        /// <returns>The updated department</returns>
        Department Update(Department department);

        /// <summary>
        /// Deletes a department by ID
        /// </summary>
        /// <param name="id">The ID of the department to delete</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        bool Delete(int id);

        /// <summary>
        /// Gets a department by ID
        /// </summary>
        /// <param name="id">The ID of the department</param>
        /// <returns>The department if found, null otherwise</returns>
        Department? GetById(int id);

        /// <summary>
        /// Gets a department by name
        /// </summary>
        /// <param name="name">The name of the department</param>
        /// <returns>The department if found, null otherwise</returns>
        Department? GetByName(string name);

        /// <summary>
        /// Gets all departments
        /// </summary>
        /// <returns>List of all departments</returns>
        IList<Department> GetAll();

        /// <summary>
        /// Searches departments by name keyword
        /// </summary>
        /// <param name="keyword">The keyword to search for</param>
        /// <returns>List of departments matching the keyword</returns>
        IList<Department> SearchByName(string keyword);

        /// <summary>
        /// Checks if a department with the given name exists
        /// </summary>
        /// <param name="name">The department name to check</param>
        /// <returns>True if exists, false otherwise</returns>
        bool ExistsByName(string name);

        /// <summary>
        /// Checks if a department with the given name exists, excluding a specific ID
        /// Used for update scenarios to check for duplicates
        /// </summary>
        /// <param name="name">The department name to check</param>
        /// <param name="excludeId">The ID to exclude from the check</param>
        /// <returns>True if exists, false otherwise</returns>
        bool ExistsByName(string name, int excludeId);

        /// <summary>
        /// Saves all pending changes to the data store
        /// </summary>
        /// <returns>Number of entities affected</returns>
        int SaveChanges();
    }
}