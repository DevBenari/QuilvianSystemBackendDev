using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("ObservasiCairanWsd", Schema = "public")]
    public class ObservasiCairanWsd : UserActivity
    {
        [Key]
        public Guid ObservasiCairanWSDId { get; set; }
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }
        public Guid UserActiveId { get; set; }

        public DateTime TglAwalObservasiWSD { get; set; }
        public DateTime TglAkhirObservasiWSD { get; set; }

        public decimal CairanSisaWSDSebelumnya { get; set; }
        public decimal CairanWSDBertambah { get; set; }
        public decimal CairanSisaWSDTabung { get; set; }

        public Guid TtdId { get; set; }
        public string PathTtd { get; set; }
        public string Keterangan { get; set; }
    }
}
