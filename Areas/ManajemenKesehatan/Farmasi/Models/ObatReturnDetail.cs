using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("ObatReturnDetail", Schema = "public")]
    public class ObatReturnDetail : UserActivity
    {
        [Key]
        public Guid ObatReturnDetailId { get; set; }
        public Guid? ObatReturnId { get; set; }
        public Guid? ObatId { get; set; }
        public string? NamaObat { get; set; }
        public int? Qty { get; set; }
        public string? NoBatch { get; set; }
        public bool? IsMasihTersegel { get; set; }
        public bool? IsObatUtuh { get; set; }
        public string? Keterangan { get; set; }
    }
}
