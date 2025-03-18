using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{

    public class DokterPoli : UserActivity
    {
        [Key]
        public Guid DokterPoliId { get; set; }
        public Guid DokterId { get; set; }
        public Guid? PoliId { get; set; }
    }
}
