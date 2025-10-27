using System.Globalization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KunjunganViewModel
    {
        public Guid? AsuransiId { get; set; }
        public Guid? PoliklinikId { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? PasienId { get; set; }
        //public bool? IsFinished { get; set; } 
        public string NoRekamMedis { get; set; }
        public string? TipePasien { get; set; }
        public string TipePembayaran { get; set; }
        public string? JenisKunjungan { get; set; }
        public string? AsalKunjungan { get; set; }

        //public bool? IsScreening { get; set; } 

        // ttg rawat inap
        //public string? TglMasukRanap { get; set; }
        //public DateTime? TglMasukRanapParsed =>
        //    DateTime.TryParseExact(TglMasukRanap, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
        //        ? DateTime.SpecifyKind(result, DateTimeKind.Utc)
        //        : null;
        //public string? TglKeluarRanap { get; set; }
        //public DateTime? TglKeluarRanapParsed =>
        //    DateTime.TryParseExact(TglKeluarRanap, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
        //        ? DateTime.SpecifyKind(result, DateTimeKind.Utc)
        //        : null;
        //public Guid? DokterDPJId { get; set; }
        //public Guid? KamarId { get; set; }
        //public Guid? BedId { get; set; }
        //public bool? StatusRanap { get; set; }
        //public string? AlasanKeluar { get; set; }
        //public Guid? ReferensiKunjunganId { get; set; }
    }
}
