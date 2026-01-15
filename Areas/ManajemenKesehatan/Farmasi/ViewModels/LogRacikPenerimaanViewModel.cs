using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class LogRacikPenerimaanViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? ResepId { get; set; }
        public Guid? UserActiveFarmasiId { get; set; }
        public string? NamaFarmasi { get; set; }
        public DateTime? TglPeracikan { get; set; }
        public Guid? UserActivePerawatId { get; set; }
        public string? NamaPerawat { get; set; }
        [Required]
        public DateTime? TglPengambilanObat { get; set; }
        public string? Keterangan { get; set; }
    }
}
