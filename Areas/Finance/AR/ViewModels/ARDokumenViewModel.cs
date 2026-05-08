namespace QuilvianSystemBackendDev.Areas.Finance.AR.ViewModels
{
    public class ARDokumenViewModel
    {
        public Guid ARDokumenId { get; set; }
        public Guid ARHeaderId { get; set; }
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }

        public string NoRM { get; set; }
        public string NamaPasien { get; set; }

        // file biasanya disimpan sebagai path atau URL, bukan binary langsung di DB
        public string DokTagihanPerawatan { get; set; }
        public string DokDetailBiaya { get; set; }

        public DateTime? TglTerimaDok { get; set; }

        public string Keterangan { get; set; }
    }
}
