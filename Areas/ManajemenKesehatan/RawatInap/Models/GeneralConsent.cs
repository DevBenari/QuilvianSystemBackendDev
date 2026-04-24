using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class GeneralConsent : UserActivity
    {
        [Key]
        public Guid GeneralConsentId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? HubunganPasien { get; set; }
        public string? NamaPenandaTangan { get; set; }
        public string? AlamatPenandaTangan { get; set; } 

        public string? TipeKamarRawat { get; set; }
        public string? KamarRawat { get; set; }

        public DateTime? TanggalTTD { get; set; }
        public bool? IsMenerimaPanduanRawatInap { get; set; }

        public Guid? KepalaRuanganId { get; set; }
        public string? PathTTDKepalaRuangan { get; set; }

        public string? PathTTDPenandaTangan { get; set; }

        public string? Keterangan { get; set; }
    }
}
