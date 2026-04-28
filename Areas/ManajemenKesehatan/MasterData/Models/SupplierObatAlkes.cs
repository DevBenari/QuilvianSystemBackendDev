using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class SupplierObatAlkes : UserActivity
    {
        [Key]
        public Guid SupplierObatAlkesId { get; set; }

        public Guid? ObatAlkesId { get; set; }
        public Guid? SupplierId { get; set; }

        public decimal? MinOrder { get; set; }
        public decimal? HargaBeli { get; set; }

        public bool? IsUtama { get; set; }

        public string? Keterangan { get; set; }

        // Navigation
        public ObatAlkes? ObatAlkes { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
