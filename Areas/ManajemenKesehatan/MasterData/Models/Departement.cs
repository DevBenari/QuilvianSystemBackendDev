using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{

    [Table("MstDepartement", Schema = "dbo")]
    public class Departement : UserActivity
    {
        [Key]
        public Guid DepartementId { get; set; }
        public string KodeDepartement { get; set; }
        public string NamaDepartement { get; set; }
        public string KepalaDepartement { get; set; }
        public string Lokasi { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public DateTime? JamBuka { get; set; }
        public DateTime? JamTutup { get; set; }
        public string Layanan { get; set; }
    }
}
