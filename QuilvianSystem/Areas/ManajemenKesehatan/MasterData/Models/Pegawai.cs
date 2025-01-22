using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MasterPegawai", Schema = "dbo")]
    public class Pegawai : UserActivity
    {
        [Key]
        public Guid UserActiveId { get; set; }
        public string UserActiveCode { get; set; }

        // Informasi Penjamin dan Identitas
        public string NoPenjamin { get; set; }
        public string Title { get; set; }
        public string NoRekamMedis { get; set; }
        public string NamaLengkap { get; set; }
        public string JenisIdentitas { get; set; }
        public string NoIdentitas { get; set; } // KTP atau Passport
        public string NIK { get; set; }

        // Tempat dan Tanggal Lahir
        public string TempatLahir { get; set; }
        public DateTime TanggalLahir { get; set; }

        // Informasi Pribadi
        public string JenisKelamin { get; set; }
        public string Agama { get; set; }
        public string Suku { get; set; }
        public string Kewarganegaraan { get; set; }
        public string PendidikanTerakhir { get; set; }

        // Informasi Alamat
        public string AlamatDomisili { get; set; }
        public string InformasiAlamat { get; set; }
        public string Kelurahan { get; set; }
        public string Kecamatan { get; set; }

        // Nomor Telepon
        public string NomorHP { get; set; } // Nomor telepon pasien
        public string Email { get; set; }

        // Informasi Pekerjaan
        public string Pekerjaan { get; set; }
        public string NamaKantor { get; set; }
        public string AlamatKantor { get; set; }
        public string NomorTeleponKantor { get; set; }

        // Informasi Medis
        public string Departemen { get; set; }

        // Informasi Keluarga
        public string NomorKeluargaTerdekat { get; set; }
        public string HubunganKeluarga { get; set; }
        public string AlamatKeluarga { get; set; }
        public string KelurahanKeluarga { get; set; }
        public string KabupatenKeluarga { get; set; }
        public string NomorTeleponKeluarga { get; set; }
        public string NamaKeluarga { get; set; }
        public string NomorKtpKeluarga { get; set; }
        public string NamaAyah { get; set; }
        public string NamaIbu { get; set; }
        public string NamaSutri { get; set; }

        // Informasi Lain
        public string DataKaryawanInput { get; set; } // Data karyawan rumah sakit yang input
        public string Foto { get; set; } // Path atau URL foto
        public bool IsActive { get; set; }
    }
}
