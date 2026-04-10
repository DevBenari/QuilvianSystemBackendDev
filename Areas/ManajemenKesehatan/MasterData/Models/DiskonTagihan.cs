using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class DiskonTagihan : UserActivity
    {
        [Key] 
        public Guid? DiskonTagihanId { get; set; }
        public Guid? DiskonId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? NamaDiskon {  get; set; }
        public decimal? ValueDiskon { get; set; }
        public string? Keterangan { get; set; }
    }
}
