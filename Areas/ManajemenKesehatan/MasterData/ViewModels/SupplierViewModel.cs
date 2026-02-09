namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class SupplierViewModel
    {
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string? ContactPerson { get; set; }
        public string? TermOfPayment { get; set; }
        public string? LeadTime { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public bool? IsPKS { get; set; }
        public bool? IsActive { get; set; }

        public Guid? BankId { get; set; }
        public int? NoRekening { get; set; }
        public string? AccountHolderName { get; set; }

        public bool? IsFullPaid { get; set; }
        public bool? IsBloodBankSupplier { get; set; }

        public string? PaymentMethod { get; set; }
        public decimal? PPN { get; set; }
        public string? Note { get; set; }
    }
}
