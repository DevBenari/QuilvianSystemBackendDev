namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ResepTemplateViewModel
    {
        public Guid? ObatId { get; set; }
        public string? Judul { get; set; }
        public Guid? DokterId { get; set; }
        public int? Qty { get; set; }
        public string? Signa { get; set; }
        public string? SignaTambahan { get; set; }
        public string? InteraturObat { get; set; }
    }
}
