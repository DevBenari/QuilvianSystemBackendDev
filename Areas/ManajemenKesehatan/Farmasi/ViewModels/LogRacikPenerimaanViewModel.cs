namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class LogRacikPenerimaanViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? ResepId { get; set; }
        public Guid? UserActiveFarmasiId { get; set; }
        public string? NamaFarmasi { get; set; }
        public string? TglPeracikan { get; set; }
        public Guid? UserActivePerawatId { get; set; }
        public string? NamaPerawat { get; set; }
        public string? TglPengambilanObat { get; set; }
        public string? Keterangan { get; set; }
    }
}
