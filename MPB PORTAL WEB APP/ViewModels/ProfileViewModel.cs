using System.ComponentModel.DataAnnotations;

namespace MPB_PORTAL_WEB_APP.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Role { get; set; }
    }
}
