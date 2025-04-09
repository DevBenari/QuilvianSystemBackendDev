using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstObat", Schema = "public")]
    public class Obat : UserActivity
    {
        [Key]
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductExtCode { get; set; }
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public Guid? KategoryObatId { get; set; }
        public string? NamaKategoriObat { get; set; }
        public Guid? MeasurementId { get; set; }
        public string? MeasurementName { get; set; }
        public Guid? WarehouseLocationId { get; set; }
        public string? WarehouseLocationName { get; set; }
        public string? DiscountId { get; set; }
        public string? DiscountValue { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? ExpiredDate { get; set; }
        public string? DosageStrength { get; set; }
        public string? DosageVolume { get; set; }
        public string? DosageForm { get; set; }

        public int Stock { get; set; }
        public decimal Cogs { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public string StorageLocation { get; set; }
        public string RackNumber { get; set; }
        public bool? IsSupplierUtama { get; set; }
        public bool? IsActive { get; set; }
        public string? Note { get; set; }
    }
}
