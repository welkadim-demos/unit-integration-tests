using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestSample01.Entities
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Department name must be between 1 and 100 characters.")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Department description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
