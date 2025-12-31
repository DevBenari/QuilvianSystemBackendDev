using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{

    [Table("MstTindakan", Schema = "public")]
    public class Tindakan : UserActivity
    {
        [Key]
        public Guid TindakanId { get; set; }
        public string KodeTindakan { get; set; }
        public string NamaTindakan { get; set; }
        public string? UnitAsal {  get; set; }
        public bool? IsRawatInap {  get; set; }
        
    }

    [Table("MstTindakanAsuransi", Schema = "public")]
    public class TindakanAsuransi : UserActivity
    {
        [Key]
        public Guid TindakanAsuransiId { get; set; }
        public Guid TindakanId { get; set; }
        public Guid AsuransiId { get; set; }
        public decimal? Diskon { get; set; }

    }

    [Table("MstTindakanPoli", Schema = "public")]
    public class TindakanPoli : UserActivity
    {
        [Key]
        public Guid TindakanPoliId { get; set; }
        public Guid TindakanId { get; set; }
        public Guid PoliId { get; set; }

    }
}
