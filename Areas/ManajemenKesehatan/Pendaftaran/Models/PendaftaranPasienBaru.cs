using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("PdfPasienBaru", Schema = "public")]
    //[Index(nameof(NoRekamMedis))]
    public class PendaftaranPasienBaru : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienBaruId { get; set; }
        // Informasi Utama
        public string? KodePasien { get; set; }
        public string? NoRekamMedis { get; set; }
        public string? NoRekamMedisAsal { get; set; }
        public string? TipePasien { get; set; }
        public string? TipePendaftaran { get; set; } // Rawat Jalan, Rawat Inap, Darurat, dll.
        // Informasi Pasien
        public Guid? TitleId { get; set; }
        public string NamaLengkap { get; set; }
        public Guid IdentitasId { get; set; }
        public string NoIdentitas { get; set; } // KTP atau Passport
        public string? TempatLahir { get; set; }
        public DateTime? TanggalLahir { get; set; }
        public string? JenisKelamin { get; set; }
        public string? StatusPerkawinan { get; set; }
        public Guid? AgamaId { get; set; }
        public string? NamaAgama {  get; set; }
        public Guid? PendidikanTerakhirId { get; set; }
        public string? CatatanKhusus {  get; set; }
        public string? TinggalBersama { get; set; }
        public Guid? KaryawanId { get; set; }
        public string? NoKaryawan { get; set; }

        // Informasi Alamat
        public string? AlamatIdentitas { get; set; }
        public string? AlamatDomisili { get; set; }
        public Guid? NegaraId { get; set; }
        public Guid? ProvinsiId { get; set; }
        public Guid? KotaId { get; set; }
        public Guid? KecKabId { get; set; }
        public Guid? KelurahanId { get; set; }
        public string? KodePos { get; set; }
        public string? Email { get; set; }
        public string? NoPasien { get; set; }
        public string? NoWali1 { get; set; }
        public string? NoWali2 { get; set; }

        // Informasi Grafis
        public string? Kewarganegaraan { get; set; }
        public string? Suku { get; set; }
        public string? StatusKewarganegaraan { get; set; }

        // Informasi Pekerjaan
        public Guid? PekerjaanId { get; set; }
        public string? NamaPerusahaan { get; set; }
        public string? AlamatPerusahaan { get; set; }
        public string? NoTeleponPerusahaan { get; set; }

        // Informasi Kesehatan
        public Guid? GolonganDarahId { get; set; }
        public string? Alergi { get; set; }
        public string? RiwayatPenyakit { get; set; }
        public string? RiwayatOperasi { get; set; }
        public string? RiwayatPenyakitKeluarga { get; set; }

        // Informasi Darurat
        public string? NamaWali1 { get; set; }
        public string? NamaWali2 { get; set; }
        public string? HubunganKeluarga1 { get; set; }
        public string? HubunganPasien { get; set; }
        public string? AlamatDarurat { get; set; }
        public string? NoTeleponDarurat { get; set; }

        // Informasi Pasien Anak
        public string? NamaOrangTua { get; set; } // Data karyawan rumah sakit yang input
        public string? IdentitasOrangTua { get; set; }
        public string? PekerjaanWali { get; set; }
        public string? HubunganKeluarga2 { get; set; }
        public string? HubunganKeluarga3 { get; set; } // Data karyawan rumah sakit yang input
        public string? NamaKontakDarurat { get; set; } // Nama kontak darurat yang bisa dihubungi

        // Informasi Membership
        public Guid? MembershipId { get; set; }

        // Informasi Tambahan
        public string? FotoName { get; set; }
        public string? FotoPath { get; set; }
        public string? QrCode { get; set; }
        public byte[]? QrCodeImage { get; set; }


        // Navigation Property
        public ICollection<AlatPemakaian> AlatPemakaians { get; set; } = new List<AlatPemakaian>();
    }

}
