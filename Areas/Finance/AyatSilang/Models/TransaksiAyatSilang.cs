using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Models
{
    [Table("Fin_TransaksiAyatSilang", Schema = "public")]
    public class TransaksiAyatSilang : UserActivity
    {
        [Key]
        public Guid TransAyatSilangId { get; set; }

        public Guid AyatSilangId { get; set; }

        public DateTime TglTransaksiMasuk { get; set; }

        public decimal SaldoKredit { get; set; }

        public DateTime TglTransaksiKeluar { get; set; }

        public decimal SaldoDebet { get; set; }

        public string Keterangan { get; set; }
    }
}