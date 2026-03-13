using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    public class DiskonPersentase : UserActivity
    {
        [Key]
        public Guid DiskonPercentaseId { get; set; }
        public decimal? NominalPersentase {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
