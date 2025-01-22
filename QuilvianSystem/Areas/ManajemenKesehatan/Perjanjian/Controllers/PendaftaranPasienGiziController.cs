using Microsoft.AspNetCore.Mvc;

namespace QuilvianSystem.Areas.ManajemenKesehatan.Perjanjian.Controllers
{
    public class PendaftaranPasienGiziController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
