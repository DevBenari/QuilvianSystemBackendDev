using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.ViewModels
{
    public class PengajuanTiketingViewModel
    {
        public Guid TicketId { get; set; }

        public Guid UserActiveId { get; set; }

        public Guid DepartementId { get; set; }

        public Guid JenisTicketId { get; set; }

        [MaxLength(50)]
        public string? NoAntrian { get; set; }

        [MaxLength(200)]
        public string JudulTicketing { get; set; } = string.Empty;

        public string? Deskripsi { get; set; }

        [MaxLength(50)]
        public string? Prioritas { get; set; }

        [MaxLength(100)]
        public string? Ruangan { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TglDibutuhkan { get; set; }

        public decimal? EstimasiBudget { get; set; }

        // Simpan path / nama file, bukan file binary langsung
        public string? Lampiran { get; set; }

        public Guid? ApprovedBy1 { get; set; }

        public Guid? ApprovedBy2 { get; set; }
        public string Status { get; set; }
    }
}
