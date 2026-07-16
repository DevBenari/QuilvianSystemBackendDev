using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pajak.ViewModels
{
    public class PajakViewModel
    {
        [Required(ErrorMessage = "Kode pajak wajib diisi.")]
        [MaxLength(30)]
        public string KodePajak { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama pajak wajib diisi.")]
        [MaxLength(150)]
        public string NamaPajak { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jenis pajak wajib diisi.")]
        [MaxLength(50)]
        public string JenisPajak { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Tarif pajak harus berada pada rentang 0 sampai 100 persen.")]
        public decimal TarifPersen { get; set; }

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PajakStatusViewModel
    {
        public bool IsActive { get; set; }
    }
}
