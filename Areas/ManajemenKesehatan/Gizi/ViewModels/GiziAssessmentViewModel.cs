namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.ViewModels
{
    public class GiziAssessmentViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }

        // STRING DATA
        public string? Anthropometri { get; set; }
        public string? Biokimia { get; set; }
        public string? Klinis { get; set; }
        public string? RiwayatGizi { get; set; }
        public string? RiwayatPersonal { get; set; }
        public string? DiagnosisGizi { get; set; }
        public string? IntervensiGizi { get; set; }
        public string? JenisDiet { get; set; }
        public string? BentukMakanan { get; set; }
        public string? Frekuensi { get; set; }
        public string? RutePemberian { get; set; }

        // NUMERIC DATA
        public decimal? Energi { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Karbohidrat { get; set; }
        public decimal? Lemak { get; set; }

        // TEXT FIELDS
        public string? EdukasiAwal { get; set; }
        public string? Keterangan { get; set; }

        // DATETIME
        public DateTime? TglPencatatan { get; set; }

        // details evaluasi gizi
        public List<GiziEvaluasiViewModel>? EvaluasiGizi { get; set; }
    }
}
