using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.ManajemenKesehatan.Tindakan.Models
{
    [Table("TndPasienBayi", Schema = "dbo")]
    public class TindakanPasienBayi : UserActivity
    {
    }

}
