namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class IGDObservasiViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? Airway { get; set; }            // Jalan Napas
        public string? Breathing { get; set; }         // Pernapasan
        public string? Circulation { get; set; }       // Sirkulasi
        public string? Disability { get; set; }        // Status Neurologis
        public string? Eye { get; set; }               // Respon membuka mata
        public string? Motor { get; set; }             // Respon gerak motorik
        public string? Verbal { get; set; }            // Respon Bicara
        public string? AlatBantuNapas { get; set; }
        public string? AlatBantuOksigenasi { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? PerawatId { get; set; }
        public DateTime? TglObservasi { get; set; }
        public string? Keterangan { get; set; }
        public Decimal? ATS { get; set; }

        public List<IGDObservasiDetailViewModel>? Details { get; set; }
    }
}
