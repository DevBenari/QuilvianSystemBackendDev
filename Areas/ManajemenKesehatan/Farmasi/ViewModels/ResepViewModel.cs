using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ResepViewModel
    {
        public Guid? KunjunganId { get; set; }
        public List<BillingViewModel>? Billing { get; set; }
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
        public Guid? DiskonId { get; set; }
        public bool? IsResepPulang { get; set; }
    }
}
