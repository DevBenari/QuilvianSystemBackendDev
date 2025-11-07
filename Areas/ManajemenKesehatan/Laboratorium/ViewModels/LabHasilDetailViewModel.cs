namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class LabHasilDetailViewModel
    {
        public Guid? HasilLabId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? KelasId { get; set; }
        public DateTime? TanggalSelesai { get; set; }
        public string? NoPhotoLab { get; set; }
        public IFormFile? PhotoLab { get; set; }
        public string? HasilLabManual { get; set; }
        public string? HasilLabAI { get; set; }
        public string? JumlahFilm { get; set; }
        public string? Keterangan { get; set; }
    }
}
