using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models
{
    [Table("Hrd_PengajuanRekrutmen", Schema = "public")]
    public class PengajuanRekrutmen : UserActivity
    {
        [Key]
        public Guid PengajuanRekrutmenId { get; set; }

        public Guid UserActiveId { get; set; }
        public Guid DepartementId { get; set; }

        public DateTime? TglPengajuan { get; set; }
        public string? LokasiPenempatan { get; set; }

        public int? JumlahDibutuhkan { get; set; }
        public string? Jobtype { get; set; }
        public string? SalaryRange { get; set; }
        public string? JenisKontrak { get; set; }
        public DateTime? TglPerkiraan { get; set; }
        public string? StatusPrioritas { get; set; }

        public string? AlasanPengajuanRekrutmen { get; set; }
        public string? DeskripsiDetail { get; set; }
        public string? DampakPengajuan { get; set; }

        public decimal? EstimasiKerugian { get; set; }

        public string? DeskripsiPekerjaan { get; set; }
        public string? KualifikasiUtama { get; set; }
        public string? KualifikasiTambahan { get; set; }

        public string? MinimalPengalaman { get; set; }
        public string? MinimalPendidikan { get; set; }

        public string? Keterangan { get; set; }
    }
}
