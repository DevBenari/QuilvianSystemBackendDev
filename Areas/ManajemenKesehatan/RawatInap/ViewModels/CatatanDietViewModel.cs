namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class CatatanDietViewModel
    {
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }

        public string? Diet { get; set; }
        public string? StatusDiet { get; set; }   // Pasien Baru / Pasien Puasa / Perubahan Diet
        public string? Keterangan { get; set; }
        public string? TglCatatanDiet { get; set; }
        public string? Diagnosa { get; set; }

    }
}
