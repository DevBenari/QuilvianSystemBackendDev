using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Tindakan.Models
{
    [Table("TndPasienLaboratorium", Schema = "dbo")]
    public class TindakanPasienLaboratorium : UserActivity
    {
        public Guid PendaftaranLaboratoriumId { get; set; }

        // Informasi Registrasi
        public string NoRegistrasi { get; set; }
        public string NoRekamMedis { get; set; }
        public string TipePasien { get; set; } // Selection seperti Pasien Umum, BPJS
        public string Penjamin { get; set; }

        // Informasi Rujukan dan Konsultasi
        public DateTime TanggalRegistrasi { get; set; }
        public bool Dirujuk { get; set; }
        public bool Konsul { get; set; } // Radio button: Ya/Tidak
        public bool LuarRS { get; set; } // Radio button: Ya/Tidak
        public bool AtasPermintaanSendiri { get; set; } // Radio button: Ya/Tidak

        // Informasi Dokter dan RS
        public string NamaDokter { get; set; } // Selection list dokter
        public string RumahSakit { get; set; } // Selection: Puskesmas, Dokter, dll.

        // Informasi Member dan Pemeriksaan
        public string KodeMember { get; set; } // Selection: Voucher, RS MMC, dll.
        public string TipePemeriksaan { get; set; } // Selection: Patologi Klinik, Mikrobiologi, dll.
        public string DiagnosaAwal { get; set; }
        public DateTime TanggalSampling { get; set; }
        public string NamaPemeriksaan { get; set; }

        // Tindakan Laboratorium
        public string NamaTindakan { get; set; }
        public int Jumlah { get; set; }
        public string Action { get; set; } // Status seperti Selesai/Proses
    }

}
