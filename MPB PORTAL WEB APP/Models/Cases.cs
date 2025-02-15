using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace MPB_PORTAL_WEB_APP.Models
{
    public class Cases
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; }

        [Required]
        public string AssignedStaffId { get; set; }

        [ForeignKey("AssignedStaffId")]
        public Users AssignedStaff { get; set; }  // Navigation property for Staff Name

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "pending";

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public string? Attachments { get; set; }

        [NotMapped]
        public IFormFileCollection? Files { get; set; }
    }
}
