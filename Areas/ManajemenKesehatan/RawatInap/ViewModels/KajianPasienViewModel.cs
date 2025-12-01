namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class KajianPasienViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? UserActiveId { get; set; }
        public Guid? VitalSignId { get; set; }
        public string? KeadaanUmum { get; set; }
        public string? KeadaanKulit { get; set; }
        public string? KeadaanKepalaLeher { get; set; }
        public string? KeadaanDada { get; set; }
        public string? KeadaanJantung { get; set; }
        public string? KeadaanParuParu { get; set; }
        public string? KeadaanAbdomen { get; set; }
        public string? KeadaanGenitalia { get; set; }
        public string? KeadaanAnggotaGerak { get; set; }
        public string? KeadaanLainnya { get; set; }
        public string? StatusLokalis { get; set; }
        public string? PemeriksaanPenunjang { get; set; }
        public string? DiagnosaSaatIni { get; set; }
        public string? DiagnosaBanding { get; set; }
        public string? DaftarMasalah { get; set; }
        public string? Program { get; set; }
        public string? Terapi { get; set; }
        public string? EdukasiKepada { get; set; }
        public string? Keterangan { get; set; }
        public DateTime? TglKajian { get; set; }
        public string? KajianUtamaPengkajian { get; set; }
        public Guid? CurrentMedicationId { get; set; }
        public DateTime? TglTindakLanjut { get; set; }
        public string? IndikasiTindakLanjut { get; set; }
        public Guid? KamarId { get; set; }
        public string? NamaTempat { get; set; }
        public string? PenyampaianEdukasi { get; set; }
        public string? BahasaDigunakan { get; set; }
        public string? JenisHambatan { get; set; }

        // Pemeriksaan DBN (Tidak Ada Kelainan)
        public bool? IsDBNKepala { get; set; }
        public bool? IsDBNMata { get; set; }
        public bool? IsDBNMulut { get; set; }
        public bool? IsDBNTHT { get; set; }
        public bool? IsDBNLeher { get; set; }
        public bool? IsDBNThorak { get; set; }
        public bool? IsDBNJantung { get; set; }
        public bool? IsDBNParu { get; set; }
        public bool? IsDBNPunggung { get; set; }
        public bool? IsDBNAbdomen { get; set; }
        public bool? IsDBNGenital { get; set; }
        public bool? IsDBNEkstremitas { get; set; }

        // Parameter kepala & leher dipisah
        public string? KeadaanKepala { get; set; }
        public string? KeadaanLeher { get; set; }

        public string? KeadaanMata { get; set; }
        public string? KeadaanMulut { get; set; }
        public string? KeadaanTHT { get; set; }
        public string? KeadaanThorak { get; set; }
        public string? KeadaanPunggung { get; set; }
        public string? KeadaanEkstremitas { get; set; }
    }
}
