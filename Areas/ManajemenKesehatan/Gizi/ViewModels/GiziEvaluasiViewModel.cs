namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.ViewModels
{
    public class GiziEvaluasiViewModel
    {
        public Guid? AssessmentGiziId { get; set; }
        public DateTime? TglEvaluasi { get; set; }
        public decimal? MakananPokok { get; set; }
        public decimal? LHTinggiLemak { get; set; }
        public decimal? LHRendahLemak { get; set; }
        public decimal? LaukNabati { get; set; }
        public decimal? Sayur { get; set; }
        public decimal? Buah { get; set; }
        public decimal? SusuDiabetes { get; set; }
        public decimal? SusuBiasa { get; set; }
        public decimal? JumlahKalori { get; set; }

        public string? IdentifikasiMasalah { get; set; }
        public string? TindakLanjut { get; set; }

        public string? CatatanPerawat { get; set; }
    }
}
