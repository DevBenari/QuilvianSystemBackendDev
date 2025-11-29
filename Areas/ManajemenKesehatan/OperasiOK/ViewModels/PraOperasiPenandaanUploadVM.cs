using Microsoft.AspNetCore.Mvc;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels
{
    public class PraOperasiPenandaanUploadVM
    {
        [FromForm]
        public IFormFile? FilePenandaanOperasiBag1 { get; set; }

        [FromForm]
        public IFormFile? FilePenandaanOperasiBag2 { get; set; }
    }
}
