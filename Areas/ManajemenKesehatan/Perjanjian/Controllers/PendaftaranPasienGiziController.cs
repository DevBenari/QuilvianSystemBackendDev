using Microsoft.AspNetCore.Mvc;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Perjanjian.Controllers
{
    public class PendaftaranPasienGiziController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
