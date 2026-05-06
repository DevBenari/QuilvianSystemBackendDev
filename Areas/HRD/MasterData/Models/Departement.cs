using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Models
{
    [Table("MstDepartement", Schema = "public")]
    public class Departement : UserActivity
    {
        [Key]
        public Guid DepartementId { get; set; }
        [Required]
        public string KodeDepartement { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public string? NamaDepartement { get; set; }
        public string? KepalaDepartement { get; set; }
        public string? Lokasi { get; set; }
        public string? Telepon { get; set; }
        public string? Email { get; set; }
        public string? JamBuka { get; set; }
        public string? JamTutup { get; set; }
        public string? Layanan { get; set; }
        public ICollection<Position> Positions { get; set; }
    }

    [Table("MstPosition", Schema = "public")]
    public class Position : UserActivity
    {
        [Key]
        public Guid PositionId { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public Guid? DepartementId { get; set; }

        //Relationship
        [ForeignKey("DepartementId")]
        public Departement? Departement { get; set; }
    }
}
