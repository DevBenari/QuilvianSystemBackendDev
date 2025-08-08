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
        public string Address { get; set; }
        public string Handphone { get; set; }
        public string Email { get; set; }
        [MaxLength(64)]
        public string? PinPegawai { get; set; }
        public Guid? ProvinsiId { get; set; }
        public Guid? KabupatenKotaId { get; set; }
        public Guid? KecamatanId { get; set; }
        public Guid? KelurahanId { get; set; }
        public string? Kewarganegaraan { get; set; }
        public Guid? AgamaId { get; set; }
        public bool? IsPerawat { get; set; }
        public string? NoSTR { get; set; }
        public string? StatusPegawai { get; set; }
        public string? JenisPegawai { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? DepartemenId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? TipeUserId { get; set; }
        public DateTime? TglMasuk { get; set; }
        public DateTime? TglKeluar { get; set; }
        public DateTime? TglAwalKontrak { get; set; }
        public DateTime? TglAkhirKontrak { get; set; }

        // untuk foto
        public string? FotoName { get; set; }
        public string? FotoPath { get; set; }
    }
}
