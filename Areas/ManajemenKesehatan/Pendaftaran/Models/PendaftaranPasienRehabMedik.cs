using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("PdfPasienRehabMedik", Schema = "public")]
    public class PendaftaranPasienRehabMedik : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienRehabMedikId { get; set; }
        public string KodePdfPasienRehabMedik { get; set; }
        public Guid? PasienId { get; set; }
        public string NoRekamMedis { get; set; }
        public DateTime? TanggalLahir { get; set; }
        public DateTime? TanggalPendaftaran { get; set; }
        public string NamaPasien { get; set; }
        public string AlamatPasien { get; set; }
        public string NoTelpPasien { get; set; }
        public string JenisKelamin { get; set; }
        public string Email { get; set; }
        public string? Title { get; set; }
        public string Provinsi { get; set; }
        public string KabupatenKota { get; set; }
        public string Kecamatan { get; set; }
        public string TipePasien { get; set; }
        public string Asuransi { get; set; }
        public string DokterPemeriksa { get; set; }
        public string KodeMember { get; set; }
        public string TipePemeriksaan { get; set; }
        public string DiagnosaAwal { get; set; }

        // informasi jika rujukan
        public string TipeRujukan { get; set; } // konsul, luar RS, permintaan sendiri
        public string? JenisKonsul { get; set; }
        public string NamaRSRujukan { get; set; }
    }
}
