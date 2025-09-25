namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class ObservasiCairanViewModel
    {
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }
        public Guid UserActiveId { get; set; }
        public DateTime TglObservasi { get; set; }

        public string CairanMasuk { get; set; }
        public string CairanKeluar { get; set; }

        public decimal CairanSisa { get; set; }
        public decimal JumlahUrin { get; set; }

        public Guid TTDId { get; set; }
        public string PathTtd { get; set; }

        public string Keterangan { get; set; }
    }
}
