using Microsoft.AspNetCore.Mvc;

namespace QuilvianBackendDev.Areas.ManajemenKesehatan.Perjanjian.Controllers
{
    public class PendaftaranPasienBayiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
