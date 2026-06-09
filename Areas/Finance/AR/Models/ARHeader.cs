using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AR.Models
{
    [Table("FIN_ARHeader", Schema = "public")]
    public class ARHeader : UserActivity
    {
        public Guid ARHeaderId { get; set; }
        public Guid AsuransiId { get; set; }
        public string JenisAR { get; set; }
        public string NoInvoice { get; set; }
        public string Tipe_Kunjungan { get; set; }
        public DateTime TglPembuatanInvoice { get; set; }

        // numeric biasanya dipetakan ke int atau decimal
        // kalau "DueDate" maksudnya hari jatuh tempo, pakai int
        public int DueDate { get; set; }

        public decimal TotalInvoice { get; set; }

        public DateTime? TglKirim { get; set; }
        public DateTime? TglTerima { get; set; }
        public DateTime? TglTagihan { get; set; }
        public DateTime? TglJatuhTempo { get; set; }

        public bool IsDocumentComplited { get; set; }
        public bool IsCanceled { get; set; }
        public bool IsLunas { get; set; } = false;
        public decimal? SisaPembayaran { get; set; }

        public string Keterangan { get; set; }
    }
}
