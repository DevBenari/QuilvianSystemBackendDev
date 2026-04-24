using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class PenerimaanDarah : UserActivity
    {
        [Key]
        public Guid PenerimaanDarahId { get; set; }
        public string? KodePenerimaan {  get; set; }
        public DateTime? TglPenerimaan { get; set; }
        public DateTime? TglFaktur {  get; set; }
        public string? NoFaktur { get; set; }
        public string? NoPO {  get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? PenerimaId {  get; set; }
        public Guid? DarahDetailId { get; set; }
        public decimal? JumlahKantong {  get; set; }
        public string? Keterangan { get; set; }
    }

}
