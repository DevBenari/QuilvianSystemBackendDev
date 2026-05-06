namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class AsuransiPasienViewModel
    {
        public Guid? PasienId { get; set; }
        public string? NoPolis { get; set; }
        public Guid? AsuransiId {get; set; }
        public string Umur { get; set; }
        public bool? IsUtama { get; set; }
        public bool? IsExcess {  get; set; }
    }
}
