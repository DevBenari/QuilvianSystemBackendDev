using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilang.Models
{
    [Table("Fin_AyatSilang", Schema = "public")]
    public class AyatSilang : UserActivity
    {
        public Guid AyatSilangId { get; set; }

        public string NoReferensi { get; set; }

        public string NoAyatSilang { get; set; }

        public Guid AsuransiId { get; set; }

        public Guid BankId { get; set; }

        public decimal TotalPembayaran { get; set; }

        public DateTime TglPembayaran { get; set; }

        public Guid UserProcess { get; set; }
        public bool? IsSudahTerpakai { get; set; }
        public string Keterangan { get; set; }
    }
}
