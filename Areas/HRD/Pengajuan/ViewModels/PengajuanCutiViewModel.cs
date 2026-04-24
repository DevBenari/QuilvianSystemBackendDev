using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.ViewModels
{
    public class PengajuanCutiViewModel
    {
        public Guid PengajuanCutiId { get; set; }
        public Guid UserActiveId { get; set; }
        public Guid JenisCutiId { get; set; }
        public Guid? DepartemenId { get; set; }

        [Column(TypeName = "date")]
        public DateTime MulaiCuti { get; set; }

        [Column(TypeName = "date")]
        public DateTime SelesaiCuti { get; set; }
        public int JumlahCutiDiambil { get; set; }
        public int SisaKuotaCuti { get; set; }
        public string AlasanCuti { get; set; }
        public string PICPengganti { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? TglPersetujuan { get; set; }
        public string CatatanApprovedBy { get; set; }
        public Guid? Approved2By { get; set; }
        public string Approved2ByName { get; set; }
        public DateTime? TglPersetujuan2 { get; set; }
        public string CatatanApproved2By { get; set; }
        public string LampiranPendukung { get; set; }
        public string Status { get; set; }

        [ForeignKey("ApprovedBy")]
        public UserActive ApprovedByUser { get; set; }

        [ForeignKey("Approved2By")]
        public UserActive Approved2ByUser { get; set; }
    }
}
