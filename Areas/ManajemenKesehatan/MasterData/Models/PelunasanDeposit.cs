using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class PelunasanDeposit : UserActivity
    {
        [Key]
        public Guid PelunasanDepositId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public decimal? Urutan { get; set; }
        public decimal? NoRevisi { get; set; }
        public DateTime? TanggalTTD { get; set; }
        public string? NamaPenandaTangan { get; set; }
        public string? AlamatPenandaTangan { get; set; }
        public string? TelpPenandaTangan { get; set; }
        public DateTime? TanggalJatuhTempo { get; set; }
        public string? TTDPenandaTanganPath { get; set; }  
        public Guid? PetugasId { get; set; }
        public string? PathTTDPetugas { get; set; }
        public string? Keterangan { get; set; }
    }
}
