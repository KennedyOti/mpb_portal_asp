using System;
using System.ComponentModel.DataAnnotations;

namespace MPB_PORTAL_WEB_APP.Models
{
    public class CountyActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ActivityName { get; set; } = string.Empty;

        [StringLength(255)]
        public string Organization { get; set; } = string.Empty;

        [Required]
        public string ActivityDescription { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ActualBeneficiaries { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ExpectedBeneficiaries { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "in_progress";
    }
}
