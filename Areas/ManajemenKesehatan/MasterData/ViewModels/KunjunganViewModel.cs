namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class KunjunganViewModel
    {
        public Guid? AsuransiId { get; set; }
        public Guid? PoliklinikId { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? TindakanId { get; set; }
        public Guid? PasienId { get; set; }

        public string NoRekamMedis { get; set; }
        public string TipePasien { get; set; }
        public string TipePembayaran { get; set; }
        public string JenisKunjungan { get; set; }
    }
}
