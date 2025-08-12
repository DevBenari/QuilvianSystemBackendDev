using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class ICDPlanning : UserActivity
    {
        [Key]
        public Guid ICDPlanningId { get; set; }
        public string? NamaPlanning { get; set; }
        public string? KategoriPlanning { get; set; }
        [Column(TypeName = "text")]
        public string? Keterangan { get; set; }
        [Column(TypeName = "text")]
        public string? DeskripsiDetail { get; set; }
    }
}
