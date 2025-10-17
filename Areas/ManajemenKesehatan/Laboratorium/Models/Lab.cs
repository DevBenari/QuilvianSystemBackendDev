using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    [Table("MstLab", Schema = "public")]
    public class Lab : UserActivity
    {
        [Key]
        public Guid? LabId { get; set; }
        public string? NamaLab { get; set; }
        public string? KodeKategori { get; set; }
        public string? Keterangan {  get; set; }
    }
}
