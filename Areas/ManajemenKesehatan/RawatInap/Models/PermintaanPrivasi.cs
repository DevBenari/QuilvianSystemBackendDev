using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    public class PermintaanPrivasi:UserActivity
    {
        [Key]
        public Guid PermintaanPrivasiId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public decimal? Urutan {  get; set; }
        public decimal? NoRevisi {  get; set; }
        public string? AksesDiperbolehkan { get; set; }
        public string? PermintaanKhusus {  get; set; }
        public bool? IsTransportasiPrivasi { get; set; }
        public DateTime? TanggalPermintaan {  get; set; }
        public Guid? KepalaRuanganId { get; set; }
        public string? PathKepalaRuangan { get; set; }
        public string? PathTTDPenandaTangan { get; set; }
        public string? Keterangan {  get; set; }
    }
}
