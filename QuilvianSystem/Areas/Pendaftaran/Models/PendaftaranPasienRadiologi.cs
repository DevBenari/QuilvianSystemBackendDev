
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Models
{
    [Table("PdfPasienRadiologi", Schema = "dbo")]
    public class PendaftaranPasienRadiologi : UserActivity
    {
    }

}
