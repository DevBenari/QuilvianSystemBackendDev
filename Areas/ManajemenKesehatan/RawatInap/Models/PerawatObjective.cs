using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [Table("PerawatObejctive", Schema = "public")]
    public class PerawatObjective : UserActivity
    {
        [Key]
        public Guid ObjNurseId { get; set; }
        public string? NamaObjective { get; set; }
        public string? Keterangan { get; set; }
    }
}
