using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Hemodialisa.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Hemodialisa.ViewModels
{
    public class HemodialisaHasilViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? NamaAsuransi { get; set; }
        public int? NoMesin { get; set; }
        public int? HemodialisaKe { get; set; }
        public string? TipeDializer { get; set; }
        public TimeSpan? JamMulai { get; set; }
        public TimeSpan? JamAkhir { get; set; }

        public string? AksesVaskuler { get; set; }
        public string? JenisHemodialisa { get; set; }
        public string? Dialisat { get; set; }

        public decimal? SirkulasiHeparin { get; set; }

        public decimal? HeparinAwal { get; set; }
        public decimal? HeparinMaintenance { get; set; }
        public decimal? HeparinContinue { get; set; }
        public decimal? HeparinIntermitten { get; set; }

        public string? PenyulitHD { get; set; }

        // ================================
        // FILE UPLOADS (Stored as string path)
        // ================================
        public string? TTDAksesVaskuler { get; set; }
        public string? TTDPPJA { get; set; }

        // ================================
        // OTHER GUIDS
        // ================================
        public Guid? AksesVaskulerId { get; set; }
        public Guid? DPPIAId { get; set; }
        public Guid? VerifikatorId { get; set; }

        // ================================
        // Status Gizi
        // ================================
        public decimal? ScoreTotalGizi { get; set; }
        public string? StatusGizi { get; set; }

        public string? Keterangan { get; set; }

        // ================================
        // Dictionary fields
        // ================================
        public Dictionary<string, decimal>? UF { get; set; }

        // Dictionary LaporanNaCl → value pakai TimeOnly
        public Dictionary<string, LaporanNaCLEntry>? LaporanNaCl { get; set; }
    }
}
