using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models
{
    public class InfeksiTD : UserActivity
    {
        [Key]
        public Guid InfeksiTransfusiId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId {  get; set; }
        public DateTime? TglTransfusi { get; set; }
        public string? JenisTransfusi { get; set; }
        public decimal? Jumlah {  get; set; }
        public DateTime? TglPencatatan {  get; set; }
        public string? Keterangan { get; set; }
    }
}
