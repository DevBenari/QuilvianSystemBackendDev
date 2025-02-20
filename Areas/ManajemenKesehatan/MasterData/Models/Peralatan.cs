using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPeralatan", Schema = "dbo")]
    public class Peralatan : UserActivity
    {
        [Key]
        public Guid PeralatanId { get; set; }
        public string KodePeralatan { get; set; }
        public string NamaPeralatan { get; set; }
        public string Manufacturer { get; set; }
        public string Purchase_date { get; set; }
        public string Maintenance_status { get; set; }
        public string Operational_status { get; set; }
        public string Department_name { get; set; }
        public string Location { get; set; }

        // RElasi
        public Guid KategoriPeralatanId { get; set; }

        [ForeignKey("KategoriPeralatanId")]
        public KategoriPeralatan KategoriPeralatans { get; set; }

    }
}
