using System.Globalization;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class BookingBedRanapViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? KamarId { get; set; }
        public Guid? BedId { get; set; }
        public string? TglMasuk { get; set; }
        //public string? TglKeluar { get; set; }
        public string? NoKamar { get; set; }
        public bool? StatusBed { get; set; }
        public string? Keterangan { get; set; }
    }
}
