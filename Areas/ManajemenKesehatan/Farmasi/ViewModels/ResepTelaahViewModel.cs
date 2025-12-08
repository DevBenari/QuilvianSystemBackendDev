namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ResepTelaahViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? ResepId { get; set; }

        // Boolean parameter telaah
        public bool IsAdministratif { get; set; }
        public bool IsNamaObatdanKetersediaan { get; set; }
        public bool IsDosisdanJumlahObat { get; set; }
        public bool IsAturandanCaraPenggunaan { get; set; }
        public bool IsTepatDosis { get; set; }
        public bool IsTepatWaktu { get; set; }
        public bool IsDuplikasi { get; set; }
        public bool IsPolifarmasi { get; set; }
        public bool IsAlergi { get; set; }
        public bool IsKontradiksi { get; set; }
        public bool IsInteraksiObat { get; set; }

        // Keterangan tambahan
        public string? Keterangan { get; set; }
    }
}
