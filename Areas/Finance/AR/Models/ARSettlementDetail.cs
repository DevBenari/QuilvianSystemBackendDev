using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AR.Models
{
    [Table("FIN_ARSettlementDetail", Schema = "public")]
    public class ARSettlementDetail : UserActivity
    {
        [Key]
        public Guid DetailSettlementARId { get; set; }

        public Guid SettlementARId { get; set; }

        public string NoRegistrasi { get; set; } = string.Empty;

        public string NoBill { get; set; } = string.Empty;

        public string NoInvoice { get; set; } = string.Empty;

        public DateTime TglTransaksi { get; set; }

        public decimal JumlahUang { get; set; }

        public decimal Saldo { get; set; }

        public int PembayaranKe { get; set; }

        public bool IsCanceled { get; set; }

        public string User { get; set; } = string.Empty;

        public string TipeSettlement { get; set; } = string.Empty;

        public string Keterangan { get; set; } = string.Empty;
    }
}
