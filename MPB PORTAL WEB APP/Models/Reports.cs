using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace MPB_PORTAL_WEB_APP.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfReport { get; set; }

        public string? FilePath { get; set; } // Make FilePath nullable (remove = string.Empty)

        [Required]
        [StringLength(255)]
        public string Report_Made_By { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "submitted";

        [NotMapped] // This ensures it is not stored in the database
        public IFormFile? File { get; set; } // Optional file upload
    }
}
