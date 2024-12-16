
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Models
{
    [Table("AccBank", Schema = "dbo")]
    public class Bank : UserActivity
    {
        [Key]
        public Guid BankId { get; set; }
        public string KodeBank { get; set; }
        public string NamaBank { get; set; }
        public ICollection<BankCabang> BankCabang { get; set; }
    }

    [Table("AccBankCabang", Schema = "dbo")]
    public class BankCabang : UserActivity
    {
        [Key]
        public Guid BankCabangId { get; set; }
        public string KodeBankCabang { get; set; }
        public string NamaCabang { get; set; }
        public Guid? BankId { get; set; }

        //Relationship
        [ForeignKey("BankId")]
        public Bank? Bank { get; set; }
    }
}
