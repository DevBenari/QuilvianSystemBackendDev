namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels
{
    public class RiwayatBendaMedisPasienViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? SumberDataId { get; set; }
        public string? NamaSumberData { get; set; }
        public string? NamaBendaMedis { get; set; }
        public string? LokasiBendaMedis { get; set; }
        public bool? IsPermanen { get; set; }
        public string? Keterangan { get; set; }
    }
}
