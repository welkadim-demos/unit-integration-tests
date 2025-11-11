using System.ComponentModel.DataAnnotations;

namespace WebApplicationAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object for Department responses
    /// </summary>
    public class DepartmentDto
    {
        public int Id { get; set; }
        
        public required string Name { get; set; }
        
        public string? Description { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for creating a new Department
    /// </summary>
    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Department name must be between 1 and 100 characters")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for updating an existing Department
    /// </summary>
    public class UpdateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Department name must be between 1 and 100 characters")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for search requests
    /// </summary>
    public class DepartmentSearchDto
    {
        [Required(ErrorMessage = "Search keyword is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Search keyword must be between 1 and 100 characters")]
        public required string Keyword { get; set; }
    }
}