using WebApplicationAPI.DTOs;
using WebApplicationAPI.Entities;

namespace WebApplicationAPI.Extensions
{
    /// <summary>
    /// Extension methods for mapping between DTOs and Entities
    /// </summary>
    public static class DepartmentMappingExtensions
    {
        /// <summary>
        /// Maps Department entity to DepartmentDto
        /// </summary>
        public static DepartmentDto ToDto(this Department department)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            };
        }

        /// <summary>
        /// Maps CreateDepartmentDto to Department entity
        /// </summary>
        public static Department ToEntity(this CreateDepartmentDto dto)
        {
            return new Department
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty
            };
        }

        /// <summary>
        /// Maps UpdateDepartmentDto to Department entity
        /// </summary>
        public static Department ToEntity(this UpdateDepartmentDto dto, int id)
        {
            return new Department
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description ?? string.Empty
            };
        }

        /// <summary>
        /// Maps collection of Department entities to DepartmentDto collection
        /// </summary>
        public static IEnumerable<DepartmentDto> ToDto(this IEnumerable<Department> departments)
        {
            return departments.Select(d => d.ToDto());
        }
    }
}