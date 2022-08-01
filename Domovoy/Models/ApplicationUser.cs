using Microsoft.AspNetCore.Identity;

namespace Domovoy.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
