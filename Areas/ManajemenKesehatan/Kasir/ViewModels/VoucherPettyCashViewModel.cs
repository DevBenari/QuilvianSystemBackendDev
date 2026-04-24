namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels
{
    public class VoucherPettyCashViewModel
    {
        public string? KodeVoucherPC { get; set; }
        public Guid LayananId { get; set; }
        public Guid KasirId { get; set; }
        public string? ShiftSesi { get; set; }
        public string? NamaPenerima { get; set; }
        public DateTime? TanggalPengajuan { get; set; }
        public string? KategoriVoucher { get; set; }
        public decimal? NominalVoucher { get; set; }
        public string? BuktiNota { get; set; }
        public string? StatusVoucher { get; set; }
        public string? Keterangan { get; set; }
    }
}