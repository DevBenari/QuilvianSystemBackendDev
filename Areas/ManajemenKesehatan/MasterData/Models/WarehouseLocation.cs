using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstWarehouseLocation", Schema = "public")]
    public class WarehouseLocation : UserActivity
    {
        [Key]
        public Guid WarehouseLocationId { get; set; }
        public string WarehouseLocationCode { get; set; }
        public string WarehouseLocationName { get; set; }
        public Guid? WarehouseManagerId { get; set; }
        public string? WarehouseManagerName { get; set; }
        public string? Address { get; set; }

    }
}
