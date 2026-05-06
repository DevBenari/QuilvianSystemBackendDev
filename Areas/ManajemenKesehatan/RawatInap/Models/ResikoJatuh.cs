using System.ComponentModel.DataAnnotations;
using Npgsql.Internal.TypeHandlers.NumericHandlers;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class ResikoJatuh : UserActivity
    {
        [Key]
        public Guid ResikoJatuhId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public decimal? ScoreResikoJatuh { get; set; }
        public string? HasilResikoJatuh { get; set; }
        public string? ShiftPenilaian {  get; set; }
        public string? Keterangan { get; set; }
    }
}
