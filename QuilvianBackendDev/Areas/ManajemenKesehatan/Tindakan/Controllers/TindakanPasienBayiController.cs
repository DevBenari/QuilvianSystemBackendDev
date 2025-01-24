using Microsoft.AspNetCore.Mvc;

namespace QuilvianBackendDev.Areas.ManajemenKesehatan.Tindakan.Controllers
{
    public class TindakanPasienBayiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
