using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class Item : UserActivity
    {
        [Key]
        public Guid ItemId { get; set; } // Generate Otomatis
        public string? KodeItem { get; set; } // Contoh: OBT = Obat, ALK = Alat Kesehatan
        public string? NamaItem { get; set; } // Nama Obat/Produk/Alkes
        public string? GenericName { get; set; } // Untuk Obat
        public Guid? KategoriItemId { get; set; } // Relasi ke master kategori item
        public Guid? BentukSatuanId { get; set; } // Relasi ke master bentuk satuan
        public string? Keterangan { get; set; } // Catatan tambahan atau deskripsi
    }
}
