namespace QuilvianSystem.Areas.AccountingAndFinancial.ViewModels
{
    public class CreateBankCabangViewModel
    {
        public Guid BankCabangId { get; set; }
        public string KodeBankCabang { get; set; }
        public string NamaCabang { get; set; }
        public Guid? BankId { get; set; }
    }
}
