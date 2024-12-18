
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Models
{
    [Table("PdfPasienBayi", Schema = "dbo")]
    public class PendaftaranPasienBayi : UserActivity
    {
    }

}
