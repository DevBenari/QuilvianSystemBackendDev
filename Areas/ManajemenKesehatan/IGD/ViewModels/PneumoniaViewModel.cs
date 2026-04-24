namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class PneumoniaViewModel
    {
        public Guid? KunjunganId { get; set; }  // Unit/Kamar
        public Guid? PasienId { get; set; }     // Pasien
        public bool? IsFotoThorax { get; set; }        // Ada Foto Thorax?
        public bool? IsHAP { get; set; }               // Hospital Acquired Pneumonia
        public string? HasilFotoThorax { get; set; }   // Hasil Foto Thorax
        public Guid? DokterHAPId { get; set; }         // Dokter yang menyatakan HAP
        public bool? IsVAP { get; set; }               // Ventilator Acquired Pneumonia
        public Guid? DokterVAPId { get; set; }         // Dokter yang menyatakan VAP
        public bool? IsVentilatorTerpasang { get; set; } // Ventilator terpasang?
        public DateTime? TglAwalVT { get; set; }       // Ventilator Terpasang - Awal
        public DateTime? TglAkhirVT { get; set; }      // Ventilator Terpasang - Akhir
        public int? HariKe { get; set; }               // TglAkhir - TglAwal (jika dihitung otomatis)
        public string? HasilThoraxSebelumVT { get; set; } // Hasil Thorax sebelum ventilator
        public string? HasilThoraxSesudahVT { get; set; } // Hasil Thorax setelah ventilator
        public DateTime? TglPencatatan { get; set; }   // Waktu catatan dibuat
        public string? Keterangan { get; set; }        // Catatan tambahan
    }
}
