using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstAsuransiPasien", Schema = "public")]
    public class AsuransiPasien : UserActivity
    {
        [Key]
        public Guid AsuransiPasienId { get; set; }
        public Guid? PasienId { get; set; }
        public string? NoPolis { get; set; }
        public Guid? AsuransiId { get; set; }
        public bool? IsUtama { get; set; }
        public string? Umur { get; set; }
        public bool? IsExcess {  get; set; }

        //public ICollection<Kunjungan> KunjunganAsuransiPasiens { get; set; } = new List<Kunjungan>();
        //public ICollection<Kunjungan> KunjunganAsuransiPasienExcesses { get; set; } = new List<Kunjungan>();
    }
}
