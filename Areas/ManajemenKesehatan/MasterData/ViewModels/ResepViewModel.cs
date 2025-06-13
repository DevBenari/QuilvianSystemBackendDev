using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ResepViewModel
    {
        public Guid? KunjunganId { get; set; }
        public List<DetailResepViewModel>? DaftarObat { get; set; }
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
        public bool? StatusPengambilan { get; set; } = false;
        public bool? IsCanceled { get; set; } = false;
        public DateOnly? TanggalPembuatanResep { get; set; }
    }
}
