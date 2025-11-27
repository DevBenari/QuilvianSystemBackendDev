using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class DarahDetail : UserActivity
    {
        [Key]
        public Guid? DarahDetailId { get; set; }
        public Guid? GolonganDarahId { get; set; }
        public Guid? DarahId { get; set; }
        public string? Rhesus {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
