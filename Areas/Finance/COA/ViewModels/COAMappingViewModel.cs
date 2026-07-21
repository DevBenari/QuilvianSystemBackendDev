using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.ViewModels
{
    public class COAMappingViewModel
    {
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
