using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.ManajemenKesehatan.Tindakan.Models
{
    [Table("TndPasienRehabilitas", Schema = "dbo")]
    public class TindakanPasienRehabilitas : UserActivity
    {

        [Key]
        public Guid PendaftaranPasienRehabilitasId { get; set; }

        // Informasi Registrasi
        public string NoRegistrasi { get; set; }
        public string NoRekamMedis { get; set; }
        public string TipePasien { get; set; } // Tipe Pasien
        public string Penjamin { get; set; }

        // Tanggal Registrasi
        public DateTime TanggalRegistrasi { get; set; }

        // Rujukan
        public string Dirujuk { get; set; }

        // Konsultasi dan Status Permintaan
        public string Konsul { get; set; } // Radio button Konsul
        public string LuarRS { get; set; } // Radio button Luar RS
        public string AtasPermintaanSendiri { get; set; } // Radio button Atas Permintaan Sendiri

        // Dokter dan Rumah Sakit
        public string NamaDokter { get; set; }
        public string RumahSakit { get; set; } // RSU/RS/RB (Puskesmas, Dr/Drg, Maramedik, Dukun terlatih, Kasus Polisi, Keluarga)

        // Kode Member
        public string KodeMember { get; set; } // Voucher Potongan, RS MMC Dokter, RS MMC Tunai (10%), VIP BKM Tanpa Part

        // Tipe Pemeriksaan
        public string TipePemeriksaan { get; set; } // Patologi Klinik, Patologi Anatomi, Mikrobiologi

        // Diagnosa Awal
        public string DiagnosaAwal { get; set; }
    }

}
