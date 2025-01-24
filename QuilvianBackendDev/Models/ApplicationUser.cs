using Microsoft.AspNetCore.Identity;

namespace QuilvianBackendDev.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NamaDepan { get; set; }
        public string NamaBelakang { get; set; }
    }

}
