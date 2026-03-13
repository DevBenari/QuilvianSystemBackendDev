using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class PaketLayananAsuransi : UserActivity
    {
        [Key]
        public Guid PaketLayananAsuransiId { get; set; }
        public Guid? PaketLayananId { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? CorporateId { get; set; }
        public DateTime? TglPembuatan { get; set; }
        public string? Keterangan { get; set; }
    }
}
