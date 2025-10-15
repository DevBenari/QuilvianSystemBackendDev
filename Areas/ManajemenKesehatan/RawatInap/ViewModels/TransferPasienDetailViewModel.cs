namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class TransferPasienDetailViewModel
    {
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? TransferPasienId { get; set; }
        public Guid? LabId { get; set; }
        public string? PenggunaanAlat { get; set; }
        public string? TglPasang { get; set; } // Tgl ketika alat digunakan/dipasang
        public string? TglPemeriksaanLab { get; set; }
        public decimal? JumlahPemeriksaanLab { get; set; }
        public string? Keterangan { get; set; }
    }
}
