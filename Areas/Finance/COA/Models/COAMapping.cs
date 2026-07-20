using QuilvianSystemBackendDev.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.Models
{
    [Table("Fin_COAMapping", Schema = "public")]
    public class COAMapping : UserActivity
    {
        [Key]
        public Guid COAMappingId { get; set; }

        /// <summary>
        /// Id transaksi (Obat/Layanan/dll)
        /// </summary>
        public Guid TransaksiId { get; set; }

        /// <summary>
        /// Nama transaksi, diambil berdasarkan TransaksiId
        /// </summary>
        [MaxLength(200)]
        public string? NamaTransaksi { get; set; }

        /// <summary>
        /// Referensi ke Master COA
        /// </summary>
        public Guid COAId { get; set; }

        /// <summary>
        /// Nama COA, diambil berdasarkan COAId
        /// </summary>
        [MaxLength(200)]
        public string? NamaCOA { get; set; }

        /// <summary>
        /// Keterangan
        /// </summary>
        [MaxLength(500)]
        public string? Keterangan { get; set; }
    }
}