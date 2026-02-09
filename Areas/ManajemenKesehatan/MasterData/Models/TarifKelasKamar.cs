using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class TarifKelasKamar : UserActivity
    {
        [Key]
        public Guid? TarifKelasKamarId {get; set;}
        public Guid? TarifId { get; set;}
        public Guid? KamarId { get; set;}
    }
}
