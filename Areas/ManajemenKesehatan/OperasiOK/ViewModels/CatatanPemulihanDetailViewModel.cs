namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels
{
    public class CatatanPemulihanDetailViewModel
    {
        public Guid? CatatanPemulihanId { get; set; }
        public DateTime? WaktuPengawasan { get; set; }
        public string? PengawasanTDPostOP { get; set; }
        public decimal? BilaSistole { get; set; }
        public string? PengawasanTerapi { get; set; }
        public string? IntruksiKhusus { get; set; }
        public string? IntruksiSedasi { get; set; }
        public decimal? NilaiNumeric { get; set; }
        public decimal? NilaiRespirasi { get; set; }
        public decimal? NilaiSirkulasi { get; set; }
        public decimal? NilaiKesadaran { get; set; }
        public decimal? NilaiWarnaKulit { get; set; }
        public decimal? JumlahScoreAldrete { get; set; }
        public bool? IsAldreteDewasa { get; set; }
        public decimal? BromageScore { get; set; }
        public string? Keterangan { get; set; }
    }
}
