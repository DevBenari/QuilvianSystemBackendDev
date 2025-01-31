using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Tindakan.Models
{
    [Table("TndPasienAmbulan", Schema = "dbo")]
    public class TindakanPasienAmbulan : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienAmbulanId { get; set; }
        public Guid PendaftaranPasienId { get; set; }
        // Informasi Registrasi
        public string NoRegistrasi { get; set; }
        public string NoRekamMedis { get; set; }

        // Departemen (Read Only)
        public string Departemen { get; set; } // Read-only departemen

        // Komponen (Gambar pada kolom tambahan)
        public string Komponen { get; set; } // Selection komponen

        // Informasi Tambahan
        public decimal JumlahKelebihanJarak { get; set; } // Jumlah kelebihan jarak
        public decimal KelebihanWaktu { get; set; } // Kelebihan waktu
        public int JumlahParamedis { get; set; } // Jumlah paramedis
        public string Notes { get; set; } // Notes tambahan

        // Antar Jemput (Ya/Tidak)
        public bool AntarJemput { get; set; } // Selection Antar Jemput (Ya, Tidak)

        //Relationship        
        [ForeignKey("PendaftaranPasienId")]
        public PendaftaranPasien? PendaftaranPasien { get; set; }
    }

}
