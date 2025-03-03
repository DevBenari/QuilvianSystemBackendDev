using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstDokter", Schema = "public")]
    public class Dokter : UserActivity
    {
        [Key]
        public Guid DokterId { get; set; }
        public string KdDokter { get; set; }
        public string NmDokter { get; set; }
        public string Sip { get; set; }
        public string Str { get; set; }
        public DateTime? TglSip { get; set; }
        public DateTime? TglStr { get; set; }
        public string Nik { get; set; }
        public string Email { get; set; }
        public string Nohp { get; set; }
        public string Alamat { get; set; }
        public bool? IsAsuransi { get; set; }

        public string? FotoDokter { get; set; }
        public string? JudulFileFoto { get; set; }
        public string? FotoPath { get; set; }

        [Column(TypeName = "VARBINARY(MAX)")]
        public byte[]? ImageBytes { get; set; }

        //relasi ke dokter poli
        public ICollection<DokterPoli> DokterPolis { get; set; }

    }
}
