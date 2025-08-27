using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models
{
    public class PengajuanResign : UserActivity
    {
        [Key]
        public Guid ResignId { get; set; }

        public Guid UserActiveId { get; set; }

        public Guid DepartementId { get; set; }
        public Guid PositionId { get; set; }

        public DateTime TglEfektifResign { get; set; }

        public float NoticePeriod { get; set; }

        public string? AlasanUtama { get; set; }

        public string? AlasanTambahan { get; set; }

        public Guid Approved1 { get; set; }

        public Guid Approved2 { get; set; }

        public bool isTerimaPenawaran { get; set; }
        public string StatusResign { get; set; } = string.Empty;
    }
}
