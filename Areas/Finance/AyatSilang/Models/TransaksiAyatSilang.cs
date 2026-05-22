using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilang.Models
{
    [Table("Fin_TransaksiAyatSilang", Schema = "public")]
    public class TransaksiAyatSilang : UserActivity
    {
        public Guid TransAyatSilangId { get; set; }

        public Guid AyatSilangId { get; set; }

        public DateTime TglTransaksiMasuk { get; set; }

        public decimal SaldoKredit { get; set; }

        public DateTime TglTransaksiKeluar { get; set; }

        public decimal SaldoDebet { get; set; }

        public string Keterangan { get; set; }
    }
}