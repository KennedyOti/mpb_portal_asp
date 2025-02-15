using System.ComponentModel.DataAnnotations;

namespace MPB_PORTAL_WEB_APP.Models
{
    public class Pages
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(255)]
        public string MetaTitle { get; set; }

        public string MetaDescription { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
