using Newtonsoft.Json;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokter", Schema = "public")]
    public class Dokter : UserActivity
    {
        public Guid DokterId { get; set; }
        public string KdDokter { get; set; }
        public string NmDokter { get; set; }
        public string? Sip { get; set; }
        public string? Str { get; set; }
        public string? Spesialis { get; set; }
        public string? TglSip { get; set; }
        public string? TglStr { get; set; }
        public string? Nik { get; set; }
        public string? Email { get; set; }
        public string? Nohp { get; set; }
        public string? Alamat { get; set; }
        public bool? IsAsuransi { get; set; }
        public string? FotoName { get; set; }
        public string? FotoPath { get; set; }
        public bool? IsActive { get; set; }
        public Guid? UserActiveId { get; set; }

        // Navigation properties
        //public virtual ICollection<DokterPoli>? DokterPolis { get; set; }
        //public virtual ICollection<DokterAsuransi>? DokterAsuransis { get; set; } // Add this property  
        //public virtual ICollection<JadwalPraktek>? JadwalPrakteks { get; set; }
    }
}
