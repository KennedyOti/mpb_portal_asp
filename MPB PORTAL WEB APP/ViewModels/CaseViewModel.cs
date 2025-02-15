using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MPB_PORTAL_WEB_APP.ViewModels
{
    public class CaseViewModel
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string ClientName { get; set; } = string.Empty; // Add default value

        [Required]
        public string Description { get; set; }

        [Required]
        public string AssignedStaffId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "pending";

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public IFormFileCollection? Files { get; set; }

        public List<string>? ExistingAttachments { get; set; }
    }
}
