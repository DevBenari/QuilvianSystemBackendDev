namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class TransferPasienViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? BedId { get; set; }
        public string? DiagnosaUtama { get; set; }
        public string? DiagnosaSekunder { get; set; }
        public Guid? DokterId1 { get; set; } // Dokter utama
        public Guid? DokterId2 { get; set; } // Dokter pendamping
        public Guid? DokterId3 { get; set; } // Dokter tambahan (jika ada)
        public string? IndikasiRanap { get; set; } // Alasan pasien dirawat
        public bool? IsAlergic { get; set; }
        public string? AlergicOf { get; set; } // Sebutkan alerginya
        public string? AlasanPindahPasien { get; set; }
        public DateTime? TglPindah { get; set; }
        //public Guid? PengawasanHarianId { get; set; }
        //public Guid? ObservasiCairanId { get; set; }
        //public Guid? IndikatorPengkajianId { get; set; }
        //public Guid? PemberianObatId { get; set; }
        public decimal? TotalScoreAldrete { get; set; }
        public decimal? TotalScoreSteward { get; set; }
        public bool? IsICU { get; set; }
        public string? BarangDiserahkan { get; set; } // Barang yang diserahkan ketika pindah pasien
        public string? IntervensiPerawat { get; set; }
        public string? PlanningTindakan { get; set; }
        public DateTime? TglMasuk { get; set; }

        // 🔹 File Upload (TTD)
        public string? TTDMenyerahkanPath { get; set; } // URL/File Path hasil upload
        public Guid? PetugasMenyerahkanId { get; set; }

        public string? TTDMengetahuiPath { get; set; }
        public Guid? PetugasMengetahuiId { get; set; }

        public string? TTDPenerimaPath { get; set; }
        public Guid? PetugasPenerimaId { get; set; }
        public string? Keterangan { get; set; }
    }
}
