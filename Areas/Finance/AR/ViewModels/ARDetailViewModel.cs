namespace QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels
{
    public class ARDetailViewModel
    {
        public Guid AsuransiId { get; set; }
        public Guid ARHeaderId { get; set; }
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }

        public string NoRM { get; set; }
        public string NamaPasien { get; set; }
        public string NoBilling { get; set; }
        public string NoRegistrasi { get; set; }

        public DateTime TglKunjungan { get; set; }
        public DateTime? TglKeluar { get; set; }

        public decimal TotalPiutang { get; set; }
        public decimal TotalPembayaran { get; set; }
        public decimal DiskonTagihan { get; set; }
        public decimal SelisihTagihan { get; set; }
        public decimal TotalSetelahDiskon { get; set; }

        public string Keterangan { get; set; }
    }
}
