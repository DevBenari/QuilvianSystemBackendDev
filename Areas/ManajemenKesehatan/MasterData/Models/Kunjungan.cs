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
        public Guid? PasienId { get; set; }
        public string NoRekamMedis { get; set; }
        public string? TipePasien { get; set; }
        public string TipePembayaran { get; set; }
        public bool? IsFinished { get; set; } = false;
        public string JenisKunjungan { get; set; }
        public string? Antrian { get; set; }
        public bool? IsScreening { get; set; }
        public bool? IsPresent { get; set; }
        public bool? IsFinishedKasir { get; set; }
        public string? StatusPengkajian { get; set; }
        public string? AsalKunjungan { get; set; }
        public DateTime? TglMasuk {  get; set; }
        public string? CaraMasukRS { get; set; }
        public string? KondisiKeluar {  get; set; }
        public bool? IsTriage {  get; set; }
        public bool? IsCTTPasienIGD { get; set; }
        // ttg rawat inap
        //public DateTime? TglMasukRanap { get; set; }
        //public DateTime? TglKeluarRanap { get; set; }
        //public Guid? DokterDPJId { get; set; }
        //public Guid? KamarId { get; set; }
        //public Guid? BedId { get; set; }
        //public bool? StatusRanap { get; set; }
        //public string? AlasanKeluar { get; set; }
        //public Guid? ReferensiKunjunganId { get; set; }
    }

}
