using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class Laboratorium : UserActivity
    {
        [Key]
        public Guid LaboratoriumId { get; set; }

    }
}
