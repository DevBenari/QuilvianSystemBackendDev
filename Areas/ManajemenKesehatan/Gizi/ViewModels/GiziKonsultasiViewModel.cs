namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.ViewModels
{
    public class GiziKonsultasiViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }

        public DateTime? TglKonsultasi { get; set; }

        public string? Diagnosa { get; set; }

        public Guid? DokterPerujukId { get; set; }
        public Guid? DokterKonsulenId { get; set; }

        public string? DiagnosaHasil { get; set; }

        // Karena TindakanId berupa array → pakai List
        public List<Guid>? TindakanId { get; set; }

        public string? Keterangan { get; set; }
    }
}
