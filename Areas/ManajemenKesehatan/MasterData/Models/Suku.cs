using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class Suku : UserActivity
    {
        [Key]
        public Guid SukuId { get; set; }
        public string KodeSuku { get; set; }
        public string NamaSuku { get; set; }

    }
}
