using Microsoft.AspNetCore.Http;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Models
{
    [Table("Fin_DokAyatSilang", Schema = "public")]
    public class DokAyatSilang : UserActivity
    {
        public Guid DokAyatSilangId { get; set; }

        public Guid AyatSilangId { get; set; }

        public string NamaDokumen { get; set; }
        public string? FilePath { get; set; }
        //public IFormFile FileAyatSilang { get; set; }

        public DateTime TglPenyimpanan { get; set; }

        public string Keterangan { get; set; }
    }
}