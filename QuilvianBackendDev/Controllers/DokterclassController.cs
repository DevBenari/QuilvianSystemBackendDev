//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QuilvianBackendDev.Repositories;

//namespace QuilvianBackendDev.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class DokterclassController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public DokterclassController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            var records = await _context.DokterClasss.ToListAsync();
//            if (records == null || !records.Any())
//            {
//                return NotFound(new { message = "Tidak ada data ditemukan." });
//            }
//            return Ok(new { message = "Data ditemukan.", data = records });
//        }
//    }
//}
