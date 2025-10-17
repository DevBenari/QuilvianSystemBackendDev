using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{

    public class IndikatorScore : UserActivity
    {
        [Key]
        public Guid IndikatorScoreId { get; set; }              
        public string? NamaIndikatorScore { get; set; } 
        public decimal? ScoreIndikator { get; set; }            
        public string? Keterangan { get; set; }                 
    }
}
