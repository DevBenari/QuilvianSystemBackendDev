//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Cors;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.VisualStudio.Web.CodeGeneration;
//using QRCoder;
//using QuilvianSystem.Areas.AccountingAndFinancial.Models;
//using QuilvianSystem.Areas.MasterData.Models;
//using QuilvianSystem.Models;
//using QuilvianSystem.Repositories;
//using System.Drawing;
//using ZXing.QrCode.Internal;

//namespace QuilvianSystem.Areas.AccountingAndFinancial.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    //[Authorize] 
//    //[EnableCors("AllowSpecific")]
//    public class DaftarPasienPoliController : Controller
//    {
//        private readonly ApplicationDbContext _applicationDbContext;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public DaftarPasienPoliController
//        (
//                ApplicationDbContext applicationDbContext,
//                UserManager<ApplicationUser> userManager,
//                IWebHostEnvironment webHostEnvironment,
//                SignInManager<ApplicationUser> signInManager
//        )
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _applicationDbContext = applicationDbContext;
//            _webHostEnvironment = webHostEnvironment;
//        }
//        // GET: api/PendaftaranPasien
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<PendaftaranPasien>>> GetPendaftaranPasien()
//        {
//            return await _applicationDbContext.PendaftaranPasiens.ToListAsync();
//        }

//        // GET: api/PendaftaranPasien/5
//        [HttpGet("{id}")]
//        public async Task<ActionResult<PendaftaranPasien>> GetPendaftaranPasien(Guid id)
//        {
//            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasiens.FindAsync(id);

//            if (pendaftaranPasien == null)
//            {
//                return NotFound();
//            }

//            return pendaftaranPasien;
//        }

//        [HttpPost]
//        public async Task<ActionResult<PendaftaranPasien>> PostPendaftaranPasien(PendaftaranPasien pendaftaranPasien)
//        {
//            var dateNow = DateTimeOffset.Now;
//            var day = dateNow.Day;
//            var month = dateNow.Month;
//            var year = dateNow.Year;
//            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

//            var lastCode = _applicationDbContext.PendaftaranPasiens
//                                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
//                                .OrderByDescending(k => k.NoRekamMedis)
//                                .FirstOrDefault();

//            if (lastCode == null)
//            {
//                pendaftaranPasien.NoRekamMedis = "REG" + setDateNow + "0001";
//            }
//            else
//            {
//                var lastCodeTrim = lastCode.NoRekamMedis.Substring(3, 6);

//                if (lastCodeTrim != setDateNow)
//                {
//                    pendaftaranPasien.NoRekamMedis = "REG" + setDateNow + "0001";
//                }
//                else
//                {
//                    pendaftaranPasien.NoRekamMedis = "REG" + setDateNow + (Convert.ToInt32(lastCode.NoRekamMedis.Substring(9, lastCode.NoRekamMedis.Length - 9)) + 1).ToString("D4");
//                }
//            }
//            // Generate GUID untuk pasien baru
//            pendaftaranPasien.PendaftaranPasienId = Guid.NewGuid();

//            // Buat QR Code berdasarkan NamaLengkap dan NoRekamMedis
//            string qrContent = $"Nama: {pendaftaranPasien.NamaLengkap}\nNo RM: {pendaftaranPasien.NoRekamMedis}";

//            // Simpan data ke database
//            _applicationDbContext.PendaftaranPasiens.Add(pendaftaranPasien);
//            await _applicationDbContext.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetPendaftaranPasien), new { id = pendaftaranPasien.PendaftaranPasienId }, pendaftaranPasien);
//        }

//        // PUT: api/PendaftaranPasien/5
//        [HttpPut("{id}")]
//        public async Task<IActionResult> PutPendaftaranPasien(Guid id, PendaftaranPasien pendaftaranPasien)
//        {
//            if (id != pendaftaranPasien.PendaftaranPasienId)
//            {
//                return BadRequest();
//            }

//            _applicationDbContext.Entry(pendaftaranPasien).State = EntityState.Modified;

//            try
//            {
//                await _applicationDbContext.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!PendaftaranPasienExists(id))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }

//            return NoContent();
//        }

//        // DELETE: api/PendaftaranPasien/5
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeletePendaftaranPasien(Guid id)
//        {
//            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasiens.FindAsync(id);
//            if (pendaftaranPasien == null)
//            {
//                return NotFound();
//            }

//            _applicationDbContext.PendaftaranPasiens.Remove(pendaftaranPasien);
//            await _applicationDbContext.SaveChangesAsync();

//            return NoContent();
//        }

//        private bool PendaftaranPasienExists(Guid id)
//        {
//            return _applicationDbContext.PendaftaranPasiens.Any(e => e.PendaftaranPasienId == id);
//        }

//        // Fungsi
//        // End Fungsi
//    }
//}
