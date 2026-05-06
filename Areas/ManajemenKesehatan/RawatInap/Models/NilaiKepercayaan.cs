using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class NilaiKepercayaan : UserActivity
    {
        [Key]
        public Guid NilaiKepercayaanId { get; set; }
        public Guid? PasienId { get; set; }
        public decimal? Urutan {  get; set; }
        public decimal? NoRevisi { get; set; }
        public DateTime? TanggalTTD { get; set; }
        public string? NamaPenandaTangan { get; set; }
        public DateTime? TanggalLahirPenandaTangan { get; set; }
        public string? UmurPenandaTangan { get; set; }
        public string? GenderPenandaTangan { get; set; }
        public string? AlamatPenandaTangan { get; set; }
        public string? HubDenganPasien { get; set; }
        public string? AgamaPasien { get; set; }
        public string? GenderPasien { get; set; }
        public string? PathLabelPasien { get; set; }
        public string? NilaiBertentangan { get; set; }
        public string? TTDPenandaTanganPath {  get; set; }
    }
}
