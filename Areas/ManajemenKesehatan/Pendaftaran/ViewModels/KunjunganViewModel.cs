using System.Globalization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels
{
    public class KunjunganViewModel
    {
        public Guid? AsuransiId { get; set; }
        public Guid? AsuransiExcessId { get; set; }
        public Guid? PoliklinikId { get; set; }
        public Guid? AsuransiPasienExcessId { get; set; }
        public Guid? AsuransiPasienId { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? PasienId { get; set; }
        //public bool? IsFinished { get; set; } 
        public string? DokterPerujuk { get; set; }
        public string? RSPerujuk { get; set; }
        public string? NoRekamMedis { get; set; }
        public string? TipePasien { get; set; }
        public string? TipePembayaran { get; set; }
        public string? JenisKunjungan { get; set; }
        public string? AsalKunjungan { get; set; }
        public string? CaraMasukRS { get; set; }
        public string? KondisiKeluar { get; set; }
        public bool? IsTriage { get; set; }
        public bool? IsCTTPasienIGD { get; set; }
        public decimal? DepositRanap { get; set; }
        public string? NoHandphone { get; set; }
        public string? Email { get; set; }
        public string? KategoriPendaftaran { get; set; }
    }
}
