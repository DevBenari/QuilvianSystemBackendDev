using Newtonsoft.Json;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class ObatViewModel
    {
        public string ObatName { get; set; }
        public string? JumlahSatuan { get; set; }
        public Guid? SatuanId { get; set; }
        public Guid BentukObatId { get; set; }
        public decimal HargaJual { get; set; }
        public decimal? HargaAwal { get; set; }
        public bool? IsActive { get; set; }
        public int Stock { get; set; }
        public string? Note { get; set; }

        //public string KategoriObat { get; set; }
        //public string Asuransi { get; set; }
        //public string KandunganObat { get; set; }
        //public string TipeHarga { get; set; } 
    }

    public class ObatKandunganViewModel
    {
        public Guid ObatId { get; set; }
        public Guid KandunganId { get; set; }
    }
    public class ObatAsuransiViewModel
    {
        public Guid ObatId { get; set; }
        public Guid AsuransiId { get; set; }
    }
}
