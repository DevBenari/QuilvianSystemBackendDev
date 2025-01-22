using Microsoft.AspNetCore.Mvc;

namespace QuilvianSystem.Areas.ManajemenKesehatan.Tindakan.Controllers
{
    public class TindakanPasienGiziController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
