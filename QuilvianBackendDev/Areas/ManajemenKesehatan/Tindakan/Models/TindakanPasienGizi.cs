using QuilvianBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianBackendDev.Areas.ManajemenKesehatan.Tindakan.Models
{
    [Table("TndPasienGizi", Schema = "dbo")]
    public class TindakanPasienGizi : UserActivity
    {
    }

}
