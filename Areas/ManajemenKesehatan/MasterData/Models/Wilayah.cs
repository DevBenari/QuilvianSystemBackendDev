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

    }

    [Table("MstKabupaten", Schema = "dbo")]
    public class Kabupaten
    {
        [Key]
        public Guid KabupatenId { get; set; }
        public string KabupatenCode { get; set; }
        public string KabupatenName { get; set; }

        // Foreign key to ProvinsiId (Guid)
        public Guid ProvinsiId { get; set; }  
    }

    [Table("MstKecamatan", Schema = "dbo")]
    public class Kecamatan
    {
        [Key]
        public Guid KecamatanId { get; set; }
        public string KecamatanCode { get; set; }
        public string KecamatanName { get; set; }

        // Foreign key to KabupatenId (Guid)
        public Guid KabupatenId { get; set; }  
    }

    [Table("MstKelurahan", Schema = "dbo")]
    public class Kelurahan
    {
        [Key]
        public Guid KelurahanId { get; set; }
        public string KelurahanCode { get; set; }
        public string KelurahanName { get; set; }

        // Foreign key to KecamatanId (Guid)
        public Guid KecamatanId { get; set; }  
    }
}
