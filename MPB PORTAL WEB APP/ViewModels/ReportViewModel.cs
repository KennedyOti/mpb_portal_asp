using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MPB_PORTAL_WEB_APP.ViewModels
{
    public class ReportViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfReport { get; set; }

        [Required]  // Add this to ensure file is required during creation
        public IFormFile File { get; set; }  // Optional file upload during edit

        [StringLength(255)]
        public string? FilePath { get; set; }  // Make nullable to avoid required validation

        [StringLength(50)]
        public string Status { get; set; } = "submitted";
    }
}
