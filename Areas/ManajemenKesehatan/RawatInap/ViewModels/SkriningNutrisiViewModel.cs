namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class SkriningNutrisiViewModel
    {
        public Guid? KunjunganId { get; set; }

        // Indikator/parameter
        public bool? IsIMT85 { get; set; }                      // IMT ≈ 18.5 kg/m2
        public bool? IsWeightLoss3mo { get; set; }              // Penurunan BB dalam 3 bulan terakhir
        public bool? IsLowFoodIntake1wk { get; set; }           // Asupan makan ↓ dalam 1 minggu terakhir
        public bool? IsPasienKurus { get; set; }                // Pasien tampak kurus
        public bool? IsWeightLoss1mo { get; set; }              // Penurunan BB dalam 1 bulan terakhir
        public bool? IsWeightStable3mo { get; set; }            // BB tidak naik dalam 3 bulan terakhir
        public bool? IsDiareGt5 { get; set; }                   // Diare > 5x dalam 1 minggu terakhir
        public bool? IsVomitgt5 { get; set; }                   // (Catatan: deskripsi menyebut >3x/minggu)
        public bool? IsNafsuMakanMenurun { get; set; }          // Asupan ↓ karena nafsu makan ↓

        public string? GangguanMetabolisme { get; set; }       // Deskripsi gangguan metabolisme (opsional)

        public bool? IsWeightLossOrWeightGain { get; set; }     // BB bertambah/berkurang berlebihan selama kehamilan
        public bool? IsHBHCTBermasalah { get; set; }            // HB <10 g/dL atau HCT <30%
        public bool? IsPenyakitBerat { get; set; }
        public bool? IsMalnutrisi { get; set; }
        public string? Keterangan { get; set; } // Keterangan tambahan (opsional)

    }
}
