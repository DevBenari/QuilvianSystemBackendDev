using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class DiskonDetail : UserActivity
    {
        [Key]
        public Guid DetailDiskonId { get; set; }
        public Guid? DiskonId { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? KelasId { get; set; }
        public Guid? LayananId { get; set; }
        public string? KodeLayanan { get; set; }
        public string? KategoriLayanan { get; set; } 
        public decimal? HargaItem { get; set; }
        public string? Keterangan { get; set; }
    }
}
