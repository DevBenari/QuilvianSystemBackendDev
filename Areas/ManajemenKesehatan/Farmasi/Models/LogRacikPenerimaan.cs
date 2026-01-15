using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    public class LogRacikPenerimaan : UserActivity
    {
        [Key]
        public Guid LogPeracikanPenerimaanId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? ResepId { get; set; }
        public Guid? UserActiveFarmasiId { get; set; }
        public string? NamaFarmasi { get; set; }
        public DateTime? TglPeracikan { get; set; }
        public Guid? UserActivePerawatId { get; set; }
        public string? NamaPerawat { get; set; }
        public string? ShiftPengambilan { get; set; }
        public DateTime? TglPengambilanObat { get; set; }
        public string? Keterangan { get; set; }
    }
}
