using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Retur.ViewModels
{
    public class DepositReturViewModel
    {
        [Required]
        public Guid PoId { get; set; }

        [Required]
        public Guid SupplierId { get; set; }

        [Required]
        public Guid ReceiveOrderId { get; set; }

        [Required]
        public Guid HeaderReturId { get; set; }

        public DateTime TglInsertDeposit { get; set; }

        [MaxLength(50)]
        public string? StatusDeposit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Jumlah deposit tidak boleh kurang dari 0.")]
        public decimal JumlahDeposit { get; set; }

        public string? Keterangan { get; set; }
    }
}