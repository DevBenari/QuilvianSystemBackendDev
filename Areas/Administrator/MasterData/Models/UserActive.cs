using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models
{
    [Table("MstUserActive", Schema = "public")]
    public class UserActive : UserActivity
    {
        [Key]
        public Guid UserActiveId { get; set; }
        public string UserActiveCode { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string Handphone { get; set; }
        public string Email { get; set; }
        [MaxLength(64)]
        public string? PinPegawai { get; set; }
        public string? NoSTR { get; set; }
        public string? StatusPegawai { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? DepartemenId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? TipeUserId { get; set; }
        public Guid? InstalasiUnitId { get; set; }


        //karyawan
        //public Guid KaryawanId { get; set; }
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
        public string? Alamat { get; set; }
        public string? FotoPath { get; set; }
        public string? FotoName { get; set; }

        // navigation
        public Departement? Departemen { get; set; }
        public InstalasiUnit? InstalasiUnit { get; set; }
        public Position? Position { get; set; }
        public TipeUser? TipeUser { get; set; }
        public Jabatan? Jabatan { get; set; }
    }
}
