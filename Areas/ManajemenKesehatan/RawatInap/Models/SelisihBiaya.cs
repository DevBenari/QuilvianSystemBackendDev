using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class SelisihBiaya : UserActivity
    {
        [Key]
        public Guid SelisihBiayaId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? NamaPasien { get; set; }
        public string? AlamatPasien { get; set; }
        public string? NoRM { get; set; }
        public string? Kelas { get; set; }
        public string? NamaPenandaTangan { get; set; }
        public string? AlamatPenandaTangan { get; set; }
        public string? PekerjaanPenandaTangan { get; set; }
        public string? NoPengenalPenandaTangan { get; set; }
        public string? TipeTandaPengenal { get; set; }
        public string? NoHpPenandaTangan { get; set; }
        public string? NoTelpKantorPenandaTangan { get; set; }
        public string? HubunganPasien { get; set; }
        public DateTime? TanggalTTD { get; set; }
        public Guid? PetugasId { get; set; }
        public string? PathTTDPetugas { get; set; }
        public string? PathTTDPenandaTangan { get; set; }
        public string? Keterangan { get; set; }
    }
}
