using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Hemodialisa.Models
{
    [NotMapped]
    public class LaporanNaCLEntry
    {
        public string? Jam { get; set; }
        public decimal? JumlahNaCl { get; set; }
    }
}
