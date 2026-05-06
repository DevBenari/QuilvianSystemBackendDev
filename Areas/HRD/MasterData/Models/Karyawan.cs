using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("Hrd_Karyawan", Schema = "public")]
    public class Karyawan : UserActivity
    {
        [Key]
        public Guid KaryawanId { get; set; }
        public Guid? UserActiveId { get; set; }
        public Guid? DepartementId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public Guid? JabatanId { get; set; }

        public string? NoIdentitas { get; set; }
        public string? KodeKaryawan { get; set; }
        public string? NoRekening { get; set; }
        public string? NoKaryawan { get; set; }
        public string? BankId { get; set; }

        public DateTime? TanggalKontrak { get; set; }
        public DateTime? TanggalAwalKerja { get; set; }
        public DateTime? TanggalAkhirKerja { get; set; }

        public string? NoHandphone { get; set; }
        public string? Email { get; set; }
        public string? Alamat { get; set; }
        public string? FotoPath { get;  set; }
        public string? FotoName { get;  set; }
    }
}
