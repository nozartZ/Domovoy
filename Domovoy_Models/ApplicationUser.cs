using Microsoft.AspNetCore.Identity;

namespace Domovoy_Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
