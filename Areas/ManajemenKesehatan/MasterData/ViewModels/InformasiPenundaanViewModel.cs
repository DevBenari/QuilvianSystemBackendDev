namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class InformasiPenundaanViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public DateTime? TanggalInfoTunda { get; set; }
        public Guid? Keterangan { get; set; }
        public Guid? PerawatId { get; set; }
    }
}
