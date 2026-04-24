using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [MinLength(8, ErrorMessage = "Password baru minimal 8 karakter.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Konfirmasi password tidak cocok.")]
        public string ConfirmPassword { get; set; }
    }
}
