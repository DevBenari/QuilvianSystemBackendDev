using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    public class DiskonDetail : UserActivity
    {
        [Key]
        public Guid DetailDiskonId { get; set; }
        public Guid? DiskonId { get; set; }
        public Guid? LayananId { get; set; }
        public string? KodeLayanan { get; set; }
        public string? KategoriLayanan { get; set; } 
        public decimal? MaxQty { get; set; }
        public decimal? MaxHarga { get; set; }
        public string? Keterangan { get; set; }
    }
}
