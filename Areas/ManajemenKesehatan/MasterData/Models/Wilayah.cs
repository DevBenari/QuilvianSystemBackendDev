using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstProvinsi", Schema = "dbo")]
    public class Provinsi
    {
        [Key]
        public Guid ProvinsiId { get; set; }
        public string ProvinsiCode { get; set; }
        public string ProvinsiName { get; set; }

        //foreign key to NegaraId
        public Guid NegaraId { get; set; }
        [ForeignKey("NegaraId")]
        public Negara Negara { get; set; }

        // Relationship with Kabupaten
        public ICollection<KabupatenKota> Kabupaten { get; set; }
    }

    [Table("MstKabupatenKota", Schema = "dbo")]
    public class KabupatenKota
    {
        [Key]
        public Guid KabupatenKotaId { get; set; }
        public string KabupatenKotaCode { get; set; }
        public string KabupatenKotaName { get; set; }

        // Foreign key to ProvinsiId (Guid)
        public Guid ProvinsiId { get; set; }  // Changed to match ProvinsiId type (Guid)
        [ForeignKey("ProvinsiId")]
        public Provinsi Provinsi { get; set; } // Relationship with Provinsi

        // Relationship with Kecamatan
        public ICollection<Kecamatan> Kecamatans { get; set; }
    }

    [Table("MstKecamatan", Schema = "dbo")]
    public class Kecamatan
    {
        [Key]
        public Guid KecamatanId { get; set; }
        public string KecamatanCode { get; set; }
        public string KecamatanName { get; set; }

        // Foreign key to KabupatenId (Guid)
        public Guid KabupatenKotaId { get; set; }  // Changed to match KabupatenId type (Guid)
        [ForeignKey("KabupatenKotaId")]
        public KabupatenKota Kabupatenkota { get; set; }  // Relationship with Kabupaten

        // Relationship with Kelurahan
        public ICollection<Kelurahan> Kelurahans { get; set; }
    }

    [Table("MstKelurahan", Schema = "dbo")]
    public class Kelurahan
    {
        [Key]
        public Guid KelurahanId { get; set; }
        public string KelurahanCode { get; set; }
        public string KelurahanName { get; set; }

        // Foreign key to KecamatanId (Guid)
        public Guid KecamatanId { get; set; }  // Changed to match KecamatanId type (Guid)
        [ForeignKey("KecamatanId")]
        public Kecamatan Kecamatan { get; set; }  // Relationship with Kecamatan
    }
}
