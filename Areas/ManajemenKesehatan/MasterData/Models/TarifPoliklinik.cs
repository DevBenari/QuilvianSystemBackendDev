using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class TarifPoliklinik
    {
        public Guid TarifPoliId { get; set; }
        

        // Relasi ke Poliklinik
        [ForeignKey("PoliId")]
        public Poliklinik? Poliklinik { get; set; }
        public string NamaPoliklinik { get; set; }

        // Relasi ke SubPoli
        [ForeignKey("SubPoliId")]
        public SubPoli? SubPoli { get; set; }
    }
}
