
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Models
{
    [Table("PdfPasienMcu", Schema = "dbo")]
    public class PendaftaranPasienMcu : UserActivity
    {
    }

}
