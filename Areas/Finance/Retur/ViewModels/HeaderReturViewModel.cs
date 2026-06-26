using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Retur.ViewModels
{
    public class HeaderReturViewModel
    {
        [Required]
        public Guid SupplierId { get; set; }

        public Guid? GudangId { get; set; }

        public bool IsTerkonfirmasi { get; set; }

        public DateTime TglRetur { get; set; }

        public string? Keterangan { get; set; }
    }
}