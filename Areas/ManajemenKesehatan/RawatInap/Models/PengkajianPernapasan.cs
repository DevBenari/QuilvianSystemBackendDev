using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class PengkajianPernapasan : UserActivity
    {
        [Key]
        public Guid PernapasanId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PengkajianPerawatId { get; set; }
        public bool? IsSulitBernapas { get; set; }
        public decimal? PemakaianO2 { get; set; }
        public string? AlatO2 { get; set; }
        public bool? IsBatukProduktive { get; set; }
        public string? PolaPernapasan { get; set; }
        public string? MasalahPernapasan { get; set; }
        public bool? IsGerakanDadaSimetris {  get; set; }
        public string? IramaNapas {  get; set; }
        public bool? IsPolaNapasTeratur {  get; set; }
        public bool? IsAdaRetraksiDada {  get; set; }
        public bool? IsAdaSesakNapas{ get; set; }
        public string? Obstruksi {  get; set; }
        public bool? IsJalanNapasPaten {  get; set; }
        public string? SuaraNapas { get; set; }
        public string? Keterangan { get; set; }
    }    
}
