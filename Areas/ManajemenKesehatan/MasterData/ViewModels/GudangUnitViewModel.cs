namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class GudangUnitViewModel
    {
        public Guid? GudangId { get; set; }
        public Guid? ObatId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public string? NamaGudangUnit { get; set; }
        public string? KodeGudangUnit { get; set; }
        public string? Keterangan { get; set; }
    }
}
