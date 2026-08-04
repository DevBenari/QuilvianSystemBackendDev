using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPemeriksaanAsuransi", Schema = "public")]
    public class PemeriksaanLabAsuransi : UserActivity
    {
        [Key]
        public Guid PemeriksaanLabAsuransiId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? AsuransiId { get; set; }
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

        // navigation 
        public Asuransi? Asuransi { get; set; }
        public LabPemeriksaan? PemeriksaanLab { get; set; }
    }
}
