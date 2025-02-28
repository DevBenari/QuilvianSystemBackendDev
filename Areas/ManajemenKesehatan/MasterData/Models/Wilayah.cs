using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstProvinsi", Schema = "public")]
    public class Provinsi : UserActivity
    {
        [Key]
        public Guid ProvinsiId { get; set; }
        public string KodeProvinsi { get; set; }
        public string NamaProvinsi { get; set; }
        public Guid? NegaraId { get; set; }
        public ICollection<KabupatenKota> KabupatenKotas { get; set; }

        //Relation        
        [ForeignKey("NegaraId")]
        public Negara? Negara { get; set; }        
    }

    [Table("MstKabupatenKota", Schema = "public")]
    public class KabupatenKota : UserActivity
    {
        [Key]
        public Guid KabupatenKotaId { get; set; }
        public string KodeKabupatenKota { get; set; }
        public string NamaKabupatenKota { get; set; }
        public Guid? ProvinsiId { get; set; }
        public ICollection<Kecamatan> Kecamatan { get; set; }

        //Relation
        [ForeignKey("ProvinsiId")]
        public Provinsi? Provinsi { get; set; }           
    }

    [Table("MstKecamatan", Schema = "public")]
    public class Kecamatan : UserActivity
    {
        [Key]
        public Guid KecamatanId { get; set; }
        public string KodeKecamatan { get; set; }
        public string NamaKecamatan { get; set; }
        public Guid? KabupatenKotaId { get; set; }
        public ICollection<Kelurahan> Kelurahan { get; set; }

        //Relation
        [ForeignKey("KabupatenKotaId")]
        public KabupatenKota? KabupatenKota { get; set; }        
    }

    [Table("MstKelurahan", Schema = "public")]
    public class Kelurahan : UserActivity
    {
        [Key]
        public Guid KelurahanId { get; set; }
        public string KodeKelurahan { get; set; }
        public string NamaKelurahan { get; set; }
        public Guid? KecamatanId { get; set; }

    }
}
