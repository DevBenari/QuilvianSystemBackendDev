namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class KaryawanViewModel
    {
        public Guid? UserActiveId { get; set; }
        public Guid? DepartementId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public Guid? JabatanId { get; set; }
        public string? NoIdentitas { get; set; }
        public string? KodeKaryawan { get; set; }
        public string? NoRekening { get; set; }
        public string? BankId { get; set; }
        public DateTime? TanggalKontrak { get; set; }
        public DateTime? TanggalAwalKerja { get; set; }
        public DateTime? TanggalAkhirKerja { get; set; }
        public string? NoHandphone { get; set; }
        public string? Email { get; set; }
        public string? Alamat { get; set; }
        public bool? IsKaryawanMedis { get; set; } = false;
    }
}
