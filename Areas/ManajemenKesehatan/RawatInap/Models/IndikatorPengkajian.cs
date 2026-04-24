using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class IndikatorPengkajian : UserActivity
    {
        [Key]
        public Guid? IndikatorPengkajianId { get; set; }     
        public Guid? IndikatorId { get; set; }               
        public Guid? IndikatorScoreId { get; set; }          
        public Guid? KategoriIndikatorId { get; set; }       
        public string? Keterangan { get; set; }            
    }
}
