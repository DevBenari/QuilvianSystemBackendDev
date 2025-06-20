using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ResepViewModel
    {
        public Guid? KunjunganId { get; set; }
        public List<ResepDetailViewModel>? DaftarObat { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? NamaAsuransi { get; set; }
        public Guid? PasienId { get; set; }
        public string? NamaPasien { get; set; }
        public Guid? PoliklinikId { get; set; }
        public string? NamaPoliklinik { get; set; }
        public Guid? DokterId { get; set; }
        public string? NamaDokter { get; set; }
        public int? AntrianResep { get; set; }
        public string? AntrianRegistrasi { get; set; }
        public string? StatusPembuatanResep { get; set; }
        public bool? IsLunas { get; set; }
        public Guid? RacikanId { get; set; }
        public string? IsRacikan { get; set; } // "Ya" or "Tidak"
        public DateOnly? TanggalPembuatanResep { get; set; }
    }
}
