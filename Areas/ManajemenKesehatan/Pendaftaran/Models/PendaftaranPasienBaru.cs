using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("PdfPasienBaru", Schema = "dbo")]
    public class PendaftaranPasienBaru : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienBaruId { get; set; }

        // Informasi Utama
        public string KodePasien { get; set; }
        public string NoRekamMedis { get; set; }
        public DateTime TanggalDibuat { get; set; }
        public string DibuatOleh { get; set; }

        // Informasi Pasien
        public string Title { get; set; }
        public string NamaLengkap { get; set; }
        public string Identitas { get; set; }
        public string NoIdentitas { get; set; } // KTP atau Passport
        public string TempatLahir { get; set; }
        public DateTime TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string Status { get; set; }
        public string Agama { get; set; }
        public string PendidikanTerakhir { get; set; }

        // Informasi Alamat
        public string AlamatIdentitas { get; set; }
        public string AlamatDomisili { get; set; }
        public string Negara { get; set; }
        public string Provinsi { get; set; }
        public string Kota { get; set; }
        public string Kabupaten { get; set; }
        public string Kelurahan { get; set; }
        public string Kecamatan { get; set; }
        public string KodePos { get; set; }
        public string Email { get; set; }
        public string Notelpon1 { get; set; }
        public string Notelpon2 { get; set; }
        public string Notelpon3 { get; set; }

        // Informasi Grafis
        public string? Kewarganegaraan { get; set; }
        public string? Suku { get; set; }
        public string? StatusKewarganegaraan { get; set; }

        // Informasi Pekerjaan
        public string Pekerjaan { get; set; } // Nomor telepon pasien
        public string NamaPerusahaan { get; set; }
        public string AlamatPerusahaan { get; set; }
        public string NoPerusahaan { get; set; }

        // Informasi Kesehatan
        public string GolonganDarah { get; set; }
        public string Alergi { get; set; }
        public string RiwayatPenyakit { get; set; }
        public string RiwayatOperasi { get; set; }
        public string RiwayatPenyakitKeluarga { get; set; }

        // Informasi Keluarga
        public string NomorKeluargaTerdekat { get; set; }
        public string HubunganKeluarga { get; set; }
        public string AlamatKeluarga { get; set; }
        public string KelurahanKeluarga { get; set; }
        public string KabupatenKeluarga { get; set; }
        public string NomorTeleponKeluarga { get; set; }
        public string NamaAyah { get; set; }
        public string NamaIbu { get; set; }
        public string NamaSutri { get; set; }
        public string NomorKtpSutri { get; set; }

        // Informasi Darurat
        public string NamaKontakDarurat { get; set; } // Data karyawan rumah sakit yang input
        public string Hubkel { get; set; }
        public string IdentitasDarurat { get; set; }
        public string AlamatDarurat { get; set; }
        public string NoDarurat { get; set; }

        // Informasi Darurat
        public string NamaOrtu { get; set; } // Data karyawan rumah sakit yang input
        public string IdentitasOrtu { get; set; }
        public string PekerjaanOrtu { get; set; }
        public string HubkelAnak { get; set; }
        public string InformasiSekolah { get; set; }

        // Informasi Darurat
        public string Foto { get; set; }
        public string QrCode { get; set; }

    }

}
