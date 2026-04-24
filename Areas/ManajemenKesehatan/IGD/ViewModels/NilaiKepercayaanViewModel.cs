namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels
{
    public class NilaiKepercayaanViewModel
    {
        public Guid? PasienId { get; set; }
        public DateTime? TanggalTTD { get; set; }
        public string? NamaPenandaTangan { get; set; }
        public DateTime? TanggalLahirPenandaTangan { get; set; }
        public string? UmurPenandaTangan { get; set; }
        public string? GenderPenandaTangan { get; set; }
        public string? AlamatPenandaTangan { get; set; }
        public string? HubDenganPasien { get; set; }
        public string? AgamaPasien { get; set; }
        public string? GenderPasien { get; set; }
        public IFormFile? LabelPasien { get; set; }
        public string? NilaiBertentangan { get; set; }
        public IFormFile? TTDPenandaTangan { get; set; }
    }
}
