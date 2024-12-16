namespace QuilvianSystem.Areas.Administration.ViewModels
{
    public class CreatePromoViewModel
    {
        public Guid PromoId { get; set; }
        public string KodePromo { get; set; }
        public string NamaPromo { get; set; }
        public string? Keterangan { get; set; }
    }
}
