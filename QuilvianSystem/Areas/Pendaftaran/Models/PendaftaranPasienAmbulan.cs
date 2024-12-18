
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Models
{
    [Table("PdfPasienAmbulan", Schema = "dbo")]
    public class PendaftaranPasienAmbulan : UserActivity
    {
    }

}
