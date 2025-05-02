using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstObat", Schema = "public")]
    public class Obat : UserActivity
    {
        [Key]
        public Guid ObatId { get; set; }
        public string? ObatCode { get; set; }
        public string ObatName { get; set; }
        public Guid BentukObatId { get; set; }
        public decimal HargaJual { get; set; }
        public decimal? HargaAwal { get; set; }
        public bool? IsActive { get; set; }
        public int Stock { get; set; }
        public string? Note { get; set; }

        //public string KategoriObat { get; set; }
        //public string Asuransi { get; set; }
        //public string KandunganObat { get; set; }
        //public string TipeHarga { get; set; }

    }
}
