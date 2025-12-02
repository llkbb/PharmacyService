using Microsoft.AspNetCore.Identity;

namespace PharmacyChain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
