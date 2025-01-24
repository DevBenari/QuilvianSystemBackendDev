using QuilvianBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianBackendDev.Areas.ManajemenKesehatan.Tindakan.Models
{
    [Table("TndPasienBayi", Schema = "dbo")]
    public class TindakanPasienBayi : UserActivity
    {
    }

}
