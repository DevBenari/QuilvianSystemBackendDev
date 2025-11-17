namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class InfeksiADPViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public bool? IsInfusVenaPerifer { get; set; }
        public bool? IsCVP { get; set; }
        public bool? IsKateterDarah { get; set; }
        public string? HasilLabLeokosit { get; set; }
        public string? HasilLabHB { get; set; }
        public DateTime? TglPencatatan { get; set; }
        public string? Keterangan { get; set; }
    }
}
