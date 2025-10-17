namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class SupplierViewModel
    {
        public string SupplierName { get; set; }
        public string ContactPerson { get; set; }
        public Guid? TermOfPaymentId { get; set; }
        public string? TermOfPaymentName { get; set; }
        public int Ppn { get; set; }
        public string Address { get; set; }
        public string? City { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public string? Note { get; set; }
        public bool? IsPKS { get; set; }
        public bool? IsActive { get; set; }
    }
}
