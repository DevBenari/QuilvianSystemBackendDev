using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{

    [Table("MstTindakan", Schema = "public")]
    public class Tindakan : UserActivity
    {
        [Key]
        public Guid TindakanId { get; set; }
        public string KodeTindakan { get; set; }
        public string NamaTindakan { get; set; }
        public string? UnitAsal {  get; set; }
        public bool? IsRawatInap {  get; set; }

        // nav
        //public virtual ICollection<TarifKelas>? TarifKelass { get; set; } = new List<TarifKelas>();
        //public virtual ICollection<TindakanPoli>? TindakanPolis { get; set; } = new List<TindakanPoli>();
        //public virtual ICollection<TindakanAsuransi>? TindakanAsuransis { get; set; } = new List<TindakanAsuransi>();
    }

    [Table("MstTindakanAsuransi", Schema = "public")]
    public class TindakanAsuransi : UserActivity
    {
        [Key]
        public Guid TindakanAsuransiId { get; set; }
        public Guid TindakanId { get; set; }
       //public virtual Tindakan? Tindakan { get; set; } = null!;
        public Guid AsuransiId { get; set; }
       //public virtual Asuransi? Asuransi { get; set; } = null!;
        // ============================
        // MARKUP
        // ============================
        public decimal? MarkupDokter { get; set; }
        public decimal? MarkupRs { get; set; }
        public decimal? MarkupJp { get; set; }
        public decimal? MarkupBahp { get; set; }
        public decimal? MarkupLainnya { get; set; }
        public decimal? MarkupTotal { get; set; }

        public bool IsMarkupBerlaku { get; set; } = true;

        // Tahun Bulan (misal: 2025-01)
        public DateTime? MarkupDari { get; set; }
        public DateTime? MarkupSampai { get; set; }


        // ============================
        // DISKON
        // ============================
        public decimal? DiskonDokter { get; set; }
        public decimal? DiskonRs { get; set; }
        public decimal? DiskonJp { get; set; }
        public decimal? DiskonBahp { get; set; }
        public decimal? DiskonTotal { get; set; }

        public bool IsDiskonBerlaku { get; set; } = true;

        // Tahun Bulan (misal: 2025-01)
        public DateTime? DiskonDari { get; set; }
        public DateTime? DiskonSampai { get; set; }

    }

    [Table("MstTindakanPoli", Schema = "public")]
    public class TindakanPoli : UserActivity
    {
        [Key]
        public Guid TindakanPoliId { get; set; }
        public Guid TindakanId { get; set; }
        //public virtual Tindakan? Tindakan { get; set; }
        public Guid PoliklinikId { get; set; }
       // public virtual Poliklinik? Poliklinik { get; set; }

    }
}
