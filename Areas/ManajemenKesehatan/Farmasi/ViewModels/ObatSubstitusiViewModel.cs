namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels
{
    public class ObatSubstitusiViewModel
    {
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }
        public Guid ResepId { get; set; }
        public Guid? PengambilObatId { get; set; }   // ID apoteker yang mengambil obat
        public Guid? PengemasObatId { get; set; }    // ID apoteker yang mengemas obat
        public DateTime? WaktuAccDokter { get; set; } // Dokter approval time
        public Guid? DokterAccId { get; set; }        // Dokter yang ACC
        public string? Keterangan { get; set; }
    }
}
