namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class MembershipViewModel
    {
        public string? NamaMembership { get; set; }
        public string? Keterangan { get; set; }
        public decimal? BiayaMembership { get; set; }
        public bool? IsAktif { get; set; } = true;
        public string? Durasi { get; set; }
    }
}
