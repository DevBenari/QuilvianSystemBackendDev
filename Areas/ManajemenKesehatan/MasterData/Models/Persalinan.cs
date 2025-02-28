using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstPersalinan", Schema = "public")]
    public class Persalinan
    {
        public Guid PersalinanId { get; set; }
        public string KodePersalinan { get; set; }

        public string NamaPersalinan { get; set; }
        public DateTime? TanggalPersalinan { get; set; }
        public string TipePersalinan { get; set; }
        public string TindakanPersalinan { get; set; }
        public string SubTindakanPersalinan { get; set; }
        public string KomplikasiPersalinan { get; set; }
        public string NamaKamar { get; set; }
        public string NoKamar { get; set; }
        public string KategoriKamar { get; set; }
        public string CatatanPersalinan { get; set; }

        //informasi Nakes
        public string DokterPersalinan { get; set; }
        public string BidanPersalinan { get; set; }
        public string AnastesiPersalinan { get; set; }
        public string ObservasiPersalinan { get; set; }

        // informasi bayi
        public string NamaBayi { get; set; }
        public string JenisKelaminBayi { get; set; }
        public DateTime? TTLBayi { get; set; }
        public string BeratBayi { get; set; }
        public string PanjangBayi { get; set; }
        public string NamaAyah { get; set; }
        public string NamaIbu { get; set; }
        public string StatusBayi { get; set; }

    }
}
