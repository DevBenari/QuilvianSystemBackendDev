using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class TindakanKunjungan : UserActivity
    {
        [Key]
        public Guid TindakanKunjunganId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? PoliklinikId { get; set; }
        public Guid? KelasId { get; set; }
        public string? NamaKelas { get; set; }
        public Guid? TarifKelasId { get; set; }
        public string? TindakanPoliId { get; set; }
        public int? Quantity { get; set; }
        public decimal? Total { get; set; }
        public string? Disposition { get; set; }
        public string? NamaPegawai { get; set; }
    }
}
