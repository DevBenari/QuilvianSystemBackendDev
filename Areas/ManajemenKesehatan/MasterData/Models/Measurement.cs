using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstMeasurement", Schema = "public")]

    public class Measurement : UserActivity
    {
        [Key]
        public Guid MeasurementId { get; set; }
        public string KodeMeasurement { get; set; }
        public string NamaMeasurement { get; set; }
        public string MeasurementExtCode { get; set; }
        public string? Note { get; set; }

    }
}
