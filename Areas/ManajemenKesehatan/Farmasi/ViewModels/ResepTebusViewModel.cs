namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ResepTebusViewModel
    {
        public string? NamaPenebus { get; set; }
        public Guid? GudangUnitId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public string? JenisLayanan { get; set; }
        public decimal? TotalHargaResep { get; set; }
        public DateTime? TanggalLunas { get; set; }
        public Guid? PetugasFarmasiId { get; set; }
        public string? NoResepLuar { get; set; }
        public string? AsalFaskes { get; set; }
        public string? NoHpPenebus { get; set; }
        public List<ResepTebusDetailViewModel>? DaftarObat { get; set; }
        public string? StatusPembuatanResep { get; set; }
    }
}
