using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class LevelViewModel
    {
        public Guid LevelId { get; set; }
        public string KodeLevel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxSalary { get; set; }

        public string Keterangan { get; set; }
    }
}
