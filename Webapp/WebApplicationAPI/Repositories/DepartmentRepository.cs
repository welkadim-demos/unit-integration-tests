using Microsoft.EntityFrameworkCore;
using WebApplicationAPI.Entities;
using WebApplicationAPI.Model;

namespace WebApplicationAPI.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the Department repository
    /// Provides concrete data access operations for Department entity
    /// </summary>
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _dbContext;

        public DepartmentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Adds a new department to the database
        /// </summary>
        /// <param name="department">The department to add</param>
        /// <returns>The added department with generated ID</returns>
        public Department Add(Department department)
        {
            ArgumentNullException.ThrowIfNull(department);

            var entityEntry = _dbContext.Departments.Add(department);
            return entityEntry.Entity;
        }

        /// <summary>
        /// Updates an existing department in the database
        /// </summary>
        /// <param name="department">The department to update</param>
        /// <returns>The updated department</returns>
        public Department Update(Department department)
        {
            ArgumentNullException.ThrowIfNull(department);

            var entityEntry = _dbContext.Departments.Update(department);
            return entityEntry.Entity;
        }

        /// <summary>
        /// Deletes a department by ID
        /// </summary>
        /// <param name="id">The ID of the department to delete</param>
        /// <returns>True if deleted successfully, false if not found</returns>
        public bool Delete(int id)
        {
            var department = GetById(id);
            if (department == null)
            {
                return false;
            }

            _dbContext.Departments.Remove(department);
            return true;
        }

        /// <summary>
        /// Gets a department by ID
        /// </summary>
        /// <param name="id">The ID of the department</param>
        /// <returns>The department if found, null otherwise</returns>
        public Department? GetById(int id)
        {
            return _dbContext.Departments.Find(id);
        }

        /// <summary>
        /// Gets a department by name (case-insensitive)
        /// </summary>
        /// <param name="name">The name of the department</param>
        /// <returns>The department if found, null otherwise</returns>
        public Department? GetByName(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return _dbContext.Departments
                .FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Gets all departments ordered by name
        /// </summary>
        /// <returns>List of all departments</returns>
        public IList<Department> GetAll()
        {
            return _dbContext.Departments
                .OrderBy(d => d.Name)
                .ToList();
        }

        /// <summary>
        /// Searches departments by name keyword (case-insensitive)
        /// </summary>
        /// <param name="keyword">The keyword to search for</param>
        /// <returns>List of departments containing the keyword</returns>
        public IList<Department> SearchByName(string keyword)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(keyword);

            return _dbContext.Departments
                .Where(d => d.Name.Contains(keyword))
                .OrderBy(d => d.Name)
                .ToList();
        }

        /// <summary>
        /// Checks if a department with the given name exists (case-insensitive)
        /// </summary>
        /// <param name="name">The department name to check</param>
        /// <returns>True if exists, false otherwise</returns>
        public bool ExistsByName(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return _dbContext.Departments
                .Any(d => d.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Checks if a department with the given name exists, excluding a specific ID
        /// Used for update scenarios to check for duplicates
        /// </summary>
        /// <param name="name">The department name to check</param>
        /// <param name="excludeId">The ID to exclude from the check</param>
        /// <returns>True if exists, false otherwise</returns>
        public bool ExistsByName(string name, int excludeId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return _dbContext.Departments
                .Any(d => d.Name.ToLower() == name.ToLower() && d.Id != excludeId);
        }

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>Number of entities affected</returns>
        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }
    }
}