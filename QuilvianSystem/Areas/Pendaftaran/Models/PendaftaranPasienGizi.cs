
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Models
{
    [Table("PdfPasienGizi", Schema = "dbo")]
    public class PendaftaranPasienGizi : UserActivity
    {
    }

}
