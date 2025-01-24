using Microsoft.AspNetCore.Mvc;

namespace QuilvianBackendDev.Areas.ManajemenKesehatan.Tindakan.Controllers
{
    public class TindakanPasienGiziController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
