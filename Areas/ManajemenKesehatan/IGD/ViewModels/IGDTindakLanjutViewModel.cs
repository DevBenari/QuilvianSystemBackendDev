namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class IGDTindakLanjutViewModel
    {        // Relasi utama
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? BedId { get; set; }

        // Informasi waktu
        public DateTime? WaktuPindah { get; set; }

        // Tindakan & Status
        public string? TindakanLanjutan { get; set; }
        public string? StatusPasien { get; set; }
        public DateTime? WaktuStatus { get; set; }

        // Kontrol
        public string? KontrolKe { get; set; }
        public DateTime? WaktuKontrol { get; set; }

        // Transportasi & Rujukan
        public string? Transportasi { get; set; }
        public string? AlasanMenolakDirawat { get; set; }
        public string? RsRujukan { get; set; }
        public string? AlasanDirujuk { get; set; }

        // Pemeriksaan Fisik
        public string? TingkatKesadaran { get; set; }
        public string? Eyes { get; set; }
        public string? Motorik { get; set; }
        public string? Verbal { get; set; }
        public string? Pupil { get; set; }
        public string? Reaksi { get; set; }

        // Vital Sign
        public decimal? Suhu { get; set; }
        public decimal? TekananDarahSystolic { get; set; }
        public decimal? TekananDarahDiastolic { get; set; }
        public decimal? Nadi { get; set; }
        public decimal? RR { get; set; }
        public decimal? SPO2 { get; set; }

        // Hasil Pemeriksaan
        public Guid? HasilLabId { get; set; }
        public Guid? HasilCTScanId { get; set; }
        public Guid? HasilEKGId { get; set; }
        public Guid? HasilRontgenId { get; set; }
        public Guid? HasilUSGId { get; set; }

        // Lembar Pemeriksaan (lembar upload)
        public decimal? LembarLab { get; set; }
        public decimal? LembarCTScan { get; set; }
        public decimal? LembarEKG { get; set; }
        public decimal? LembarRontgen { get; set; }
        public decimal? LembarUSG { get; set; }

        // Petugas
        public Guid? PerawatIgdId { get; set; }
        public Guid? PerawatKamarId { get; set; }

        // Keterangan tambahan
        public string? Keterangan { get; set; }

        public string? TindakLanjut { get; set; }
        public string? KeadaanPasienPulang { get; set; }
        public string? KesimpulanAkhir { get; set; }

        public DateTime? WaktuDipulangkan { get; set; }

        public string? UPF { get; set; }
        public string? Bangsal { get; set; }
        public Guid? KelasId { get; set; }
        public string? IndikasiRanap { get; set; }
        public DateTime? WaktuDirujuk { get; set; }
        public string? Observasi { get; set; }
        public string? TempatMeninggal { get; set; }
        public DateTime? TanggalMeninggal { get; set; }
        public string? PenyebabMeninggal { get; set; }
        public string? MobilisasiSaatPulang { get; set; }
        public bool? IsVisum { get; set; }
        public decimal? JumlahHariIzin { get; set; }
        public DateTime? TanggalAwalIzin { get; set; }
        public DateTime? TanggalAkhirIzin { get; set; }
        public Guid? TTDPerawatId { get; set; }
        public Guid? TTDDokterId { get; set; }
    }
}
