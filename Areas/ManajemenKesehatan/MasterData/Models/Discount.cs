using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDiscount", Schema = "public")]
    public class Discount : UserActivity
    {
        [Key]
        public Guid DiscountId { get; set; }
        public string KodeDiscount { get; set; }
        public int DiscountValue { get; set; }
        public string? Note { get; set; }
    }
}
