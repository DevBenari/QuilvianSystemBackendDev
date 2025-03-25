using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstKunjungan", Schema = "public")]
    public class Kunjungan : UserActivity
    {
        [Key]
        public Guid KunjunganID { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? PoliklinikId { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? TindakanId { get; set; }
        public Guid? PasienId { get; set; }

        public string NoRekamMedis {  get; set; }
        public string TipePasien { get; set; }
        public string TipePembayaran { get; set; }
        public string Antrian { get; set; }
        public string JumlahKunjungan { get; set; }
    }
}
