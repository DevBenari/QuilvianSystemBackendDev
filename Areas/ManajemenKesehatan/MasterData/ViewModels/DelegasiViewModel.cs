using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DelegasiViewModel
    {
        public Guid? UserDelegasiId { get; set; } // ID pengguna yang didelegasikan
        public Guid? UserActiveId { get; set; } // ID pengguna yang aktif
        public DelegasiTugas? Tugas { get; set; } // Deskripsi tugas yang didelegasikan
        public string? Pin { get; set; } // Pin untuk verifikasi delegasi
    }
}
