using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("ObservasiCairan", Schema = "public")]
    public class ObservasiCairan : UserActivity
    {
        [Key]
        public Guid ObservasiCairanId { get; set; }
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }
        public Guid UserActiveId { get; set; }
        public DateTime TglObservasi { get; set; }

        public string CairanMasuk { get; set; }
        public string CairanKeluar { get; set; }

        public decimal CairanSisa { get; set; }
        public decimal JumlahUrin { get; set; }

        public Guid TTDId { get; set; }
        public string PathTtd { get; set; }

        public string Keterangan { get; set; }
    }
}
