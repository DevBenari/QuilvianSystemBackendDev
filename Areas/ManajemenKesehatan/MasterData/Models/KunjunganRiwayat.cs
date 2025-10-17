namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    public class KunjunganRiwayat
    {
        public string? Jenis { get; set; } = ""; // "IP" (Rawat Inap) atau "OP" (Rawat Jalan)
        public int Jumlah { get; set; }
    }
}
