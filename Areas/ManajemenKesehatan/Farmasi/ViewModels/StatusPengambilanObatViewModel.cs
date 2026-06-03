namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class StatusPengambilanObatViewModel
    {
        public bool? Status { get; set; }

        // Contoh:
        // ["PAGI"]
        // ["SIANG"]
        // ["MALAM"]
        // ["PAGI", "SIANG", "MALAM"]
        public List<string>? WaktuPengambilan { get; set; }

        public DateTime? TanggalPengambilan { get; set; }

        public Guid? PerawatId { get; set; }
        public Guid? ApotekerId { get; set; }
    }
}
