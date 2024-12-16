using Microsoft.AspNetCore.Identity;

namespace QuilvianSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NamaDepan { get; set; }
        public string NamaBelakang { get; set; }
    }

}
