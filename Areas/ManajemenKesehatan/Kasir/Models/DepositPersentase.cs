using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models
{
    public class DepositPersentase : UserActivity
    {
        [Key]
        public Guid PersentaseDeposidId { get; set; }
        public decimal? LimitPersentase {  get; set; }
        public DateTime? AwalPeriode { get; set; }
        public DateTime? AkhirPeriode { get; set; }
        public string? Keterangan {  get; set; }
    }
}
