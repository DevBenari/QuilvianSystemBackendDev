using Microsoft.AspNetCore.Mvc;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Tindakan.Controllers
{
    public class TindakanPasienGiziController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
