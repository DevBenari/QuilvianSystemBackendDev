using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class PengkajianPerawat : UserActivity
    {
        [Key]
        public Guid PengkajianPerawatId { get; set; }
        public Guid? KunjunganId { get; set; }               
        public Guid? PendaftaranPasienBaruId { get; set; }   
        public Guid? DokterId { get; set; }                  
        public string? SumberData { get; set; }              
        public string? HubunganDenganPasien { get; set; }    
        public DateTime? TglMasuk { get; set; }              
        public DateTime? TglPengkajianPerawat { get; set; }  
        public string? MasalahPsikologi { get; set; }        
        public bool? IsHubunganSosial { get; set; }          
        public string? TempatTinggal { get; set; }           
        public string? GangguanFungsional { get; set; }      
        public string? NilaiKepercayaan { get; set; } 
        public DateTime? MensPertama {  get; set; }
        public DateTime? MensTerakhir {  get; set; }
        public decimal? Minum {  get; set; }
        public string? TipeImunisasi { get; set; }
        public DateTime? TanggalImunisasiLanjutan {  get; set; }
        public string? Keterangan { get; set; }
    }
}
