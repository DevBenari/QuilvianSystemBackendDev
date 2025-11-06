namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels
{
    public class RuangBedahBookingDetailVM
    {
        public Guid? DetailBookingBedahId { get; set; }
        public Guid? BookingRuanganBedahId { get; set; }
        public Guid? JenisOperasiId { get; set; }
        public Guid? TindakanId { get; set; }
        public List<Guid>? UserActiveId { get; set; } = new();
        public decimal? PersentaseTindakan { get; set; }
        public decimal? DiskonDokter { get; set; }
        public string? Keterangan { get; set; }
    }
}
