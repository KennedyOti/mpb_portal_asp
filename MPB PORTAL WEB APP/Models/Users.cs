using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MPB_PORTAL_WEB_APP.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }

        [NotMapped] // Prevents Role from being stored in the database
        public string Role { get; set; }
    }
}
