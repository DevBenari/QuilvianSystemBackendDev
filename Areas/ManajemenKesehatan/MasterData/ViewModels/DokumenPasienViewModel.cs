namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DokumenPasienViewModel
    {
        public Guid? PasienId { get; set; }
        public string? JenisDokumen { get; set; }
        public IFormFile? Dokumen { get; set; }
        public string? Keterangan { get; set; }
    }
}
