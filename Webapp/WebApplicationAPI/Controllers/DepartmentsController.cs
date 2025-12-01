using Microsoft.AspNetCore.Mvc;
using WebApplicationAPI.DTOs;
using WebApplicationAPI.Extensions;
using WebApplicationAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationAPI.Controllers
{
    /// <summary>
    /// API Controller for Department CRUD operations
    /// Provides RESTful endpoints for managing departments
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DepartmentsController : ControllerBase
    {
        private readonly DepartmentsService _departmentsService;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(DepartmentsService departmentsService, ILogger<DepartmentsController> logger)
        {
            _departmentsService = departmentsService ?? throw new ArgumentNullException(nameof(departmentsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all departments
        /// </summary>
        /// <returns>List of all departments</returns>
        /// <response code="200">Returns the list of departments</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), 200)]
        public ActionResult<IEnumerable<DepartmentDto>> getAllDepartments()
        {
            try
            {
                var departments = _departmentsService.GetAllDepartments();
                return Ok(departments.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all departments");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Gets a department by ID
        /// </summary>
        /// <param name="id">The department ID</param>
        /// <returns>The department with the specified ID</returns>
        /// <response code="200">Returns the department</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the department is not found</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DepartmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<DepartmentDto> GetDepartmentById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Department ID must be a positive number");
            }

            try
            {
                var department = _departmentsService.GetDepartmentById(id);
                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }

                return Ok(department.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting department with ID {DepartmentId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Gets a department by name
        /// </summary>
        /// <param name="name">The department name</param>
        /// <returns>The department with the specified name</returns>
        /// <response code="200">Returns the department</response>
        /// <response code="400">If the name is invalid</response>
        /// <response code="404">If the department is not found</response>
        [HttpGet("by-name/{name}")]
        [ProducesResponseType(typeof(DepartmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<DepartmentDto> GetDepartmentByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Department name cannot be empty");
            }

            try
            {
                var department = _departmentsService.GetDepartmentByName(name);
                if (department == null)
                {
                    return NotFound($"Department with name '{name}' not found");
                }

                return Ok(department.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting department with name {DepartmentName}", name);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Searches departments by name keyword
        /// </summary>
        /// <param name="keyword">The keyword to search for</param>
        /// <returns>List of departments matching the keyword</returns>
        /// <response code="200">Returns the list of matching departments</response>
        /// <response code="400">If the keyword is invalid</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), 200)]
        [ProducesResponseType(400)]
        public ActionResult<IEnumerable<DepartmentDto>> SearchDepartments([FromQuery, Required] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("Search keyword cannot be empty");
            }

            try
            {
                var departments = _departmentsService.SearchDepartmentsByName(keyword);
                return Ok(departments.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching departments with keyword {Keyword}", keyword);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Creates a new department
        /// </summary>
        /// <param name="createDto">The department data to create</param>
        /// <returns>The created department</returns>
        /// <response code="201">Returns the created department</response>
        /// <response code="400">If the department data is invalid</response>
        /// <response code="409">If a department with the same name already exists</response>
        [HttpPost]
        [ProducesResponseType(typeof(DepartmentDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public ActionResult<DepartmentDto> CreateDepartment([FromBody] CreateDepartmentDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var department = createDto.ToEntity();
                _departmentsService.AddDepartment(department);
                
                var createdDepartment = _departmentsService.GetDepartmentByName(department.Name);
                return CreatedAtAction(nameof(GetDepartmentById), 
                                     new { id = createdDepartment!.Id }, 
                                     createdDepartment.ToDto());
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating department {DepartmentName}", createDto.Name);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Updates an existing department
        /// </summary>
        /// <param name="id">The department ID to update</param>
        /// <param name="updateDto">The updated department data</param>
        /// <returns>The updated department</returns>
        /// <response code="200">Returns the updated department</response>
        /// <response code="400">If the department data is invalid</response>
        /// <response code="404">If the department is not found</response>
        /// <response code="409">If a department with the same name already exists</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(DepartmentDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public ActionResult<DepartmentDto> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest("Department ID must be a positive number");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if department exists
                var existingDepartment = _departmentsService.GetDepartmentById(id);
                if (existingDepartment == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }

                // Check for duplicate name (if name is changing)
                if (!existingDepartment.Name.Equals(updateDto.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var duplicateDepartment = _departmentsService.GetDepartmentByName(updateDto.Name);
                    if (duplicateDepartment != null)
                    {
                        return Conflict($"A department with name '{updateDto.Name}' already exists");
                    }
                }

                // Update the existing tracked entity instead of creating a new one
                existingDepartment.Name = updateDto.Name;
                existingDepartment.Description = updateDto.Description ?? string.Empty;
                _departmentsService.UpdateDepartment(existingDepartment);
                
                return Ok(existingDepartment.ToDto());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating department with ID {DepartmentId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Deletes a department
        /// </summary>
        /// <param name="id">The department ID to delete</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">Department successfully deleted</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the department is not found</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult DeleteDepartment(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Department ID must be a positive number");
            }

            try
            {
                var existingDepartment = _departmentsService.GetDepartmentById(id);
                if (existingDepartment == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }

                _departmentsService.DeleteDepartment(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting department with ID {DepartmentId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}