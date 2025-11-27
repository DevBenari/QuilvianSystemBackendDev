using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstVitalSign", Schema = "public")]
    public class VitalSign : UserActivity
    {
        [Key]
        public Guid VitalSignId { get; set; }
        public Guid? KunjunganId { get; set; }
        public decimal? Suhu { get; set; }
        public int? HR { get; set; }
        public int? RR { get; set; }
        public int? TekananDarahSystolic { get; set; }
        public int? TekananDarahDiastolic { get; set; }
        public decimal? SaturasiOksigen { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }
        public decimal? LingkarKepalaBayi { get; set; }
        public decimal? Nadi { get; set; }
        public Guid? RanapId { get; set; }
        public decimal? PenggunaanOksigen { get; set; }
        public string? OksigenTambahan { get; set; }
        public decimal? MAP {  get; set; }
        public string? HasilMAP { get; set; }
        public decimal? SkorEWS { get; set; }
        public string? FrekuensiMonitoring {  get; set; }
        public string? Kesadaran {  get; set; }
        public bool? IsNadiTeraba {  get; set; }
        public bool? IsRegularNadi {  get; set; }
        public decimal? Respirasi {  get; set; }
        public string? PolaNapas {  get; set; }
        public decimal? BBPreHD { get; set; }
        public decimal? BBPostHD { get;set; }
        public decimal? PenguranganBBHD { get; set; }
        public decimal? PenambahanBBHD { get; set; }
        public decimal? BBKering {  get; set; }
        public decimal? LingkarLenganAtas {  get; set; }
        public Guid? PengkajianScoreId { get; set; }
        public decimal? ScoreGizi {  get; set; }
    }
}
