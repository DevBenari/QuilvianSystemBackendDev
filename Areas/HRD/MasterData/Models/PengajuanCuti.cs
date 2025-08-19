using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_MstPengajuanCuti", Schema = "public")]
    public class PengajuanCuti : UserActivity
    {
        [Key]
        public Guid PengajuanCutiId { get; set; }
        public Guid UserActiveId { get; set; }
        public Guid JenisCutiId { get; set; }

        [Column(TypeName = "date")]
        public DateTime MulaiCuti { get; set; }

        [Column(TypeName = "date")]
        public DateTime SelesaiCuti { get; set; }
        public int JumlahCutiDiambil { get; set; }
        public int SisaKuotaCuti { get; set; }
        public string AlasanCuti { get; set; }
        public string PICPengganti { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? TglPersetujuan { get; set; }
        public string CatatanApprovedBy { get; set; }
        public Guid? Approved2By { get; set; }
        public DateTime? TglPersetujuan2 { get; set; }
        public string CatatanApproved2By { get; set; }
        public string LampiranPendukung { get; set; }
    }
}
