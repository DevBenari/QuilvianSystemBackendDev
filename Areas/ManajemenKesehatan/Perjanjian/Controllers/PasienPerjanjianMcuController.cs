//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Cors;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.VisualStudio.Web.CodeGeneration;
//using QRCoder;
//using QuilvianSystemBackendDev.Areas.AccountingAndFinancial.Models;
//using QuilvianSystemBackendDev.Models;
//using QuilvianSystemBackendDev.Repositories;
//using System.Drawing;
//using ZXing.QrCode.Internal;

//namespace QuilvianSystemBackendDev.Areas.AccountingAndFinancial.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    //[Authorize] 
//    //[EnableCors("AllowSpecific")]
//    public class PasienPerjanjianMcuController : Controller
//    {
//        private readonly ApplicationDbContext _applicationDbContext;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public PasienPerjanjianMcuController
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
//        public async Task<ActionResult<IEnumerable<PendaftaranPasienBaru>>> GetPendaftaranPasien()
//        {
//            return await _applicationDbContext.PendaftaranPasienBarus.ToListAsync();
//        }

//        // GET: api/PendaftaranPasien/5
//        [HttpGet("{id}")]
//        public async Task<ActionResult<PendaftaranPasienBaru>> GetPendaftaranPasien(Guid id)
//        {
//            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasienBarus.FindAsync(id);

//            if (pendaftaranPasien == null)
//            {
//                return NotFound();
//            }

//            return pendaftaranPasien;
//        }

//        [HttpPost]
//        public async Task<ActionResult<PendaftaranPasienBaru>> PostPendaftaranPasien(PendaftaranPasienBaru pendaftaranPasien)
//        {
//            // Generate GUID untuk pasien baru
//            pendaftaranPasien.PendaftaranPasienBaruId = Guid.NewGuid();

//            // Buat QR Code berdasarkan NamaLengkap dan NoRekamMedis
//            string qrContent = $"Nama: {pendaftaranPasien.NamaLengkap}\nNo RM: {pendaftaranPasien.NoRekamMedis}";
//            pendaftaranPasien.QrCode = GenerateQrCodeWithDefaultText(qrContent); // Simpan QR Code Base64 ke properti QrCode

//            // Simpan data ke database
//            _applicationDbContext.PendaftaranPasienBarus.Add(pendaftaranPasien);
//            await _applicationDbContext.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetPendaftaranPasien), new { id = pendaftaranPasien.PendaftaranPasienBaruId }, pendaftaranPasien);
//        }
//        private string GenerateQrCodeWithDefaultText(string content)
//        {
//            string centerText = "MMC"; // Teks default

//            using var qrGenerator = new QRCodeGenerator();

//            // Menggunakan level koreksi kesalahan lebih tinggi (Q)
//            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
//            using var qrCode = new QRCoder.QRCode(qrCodeData);
//            using var qrBitmap = qrCode.GetGraphic(10);  // Ukuran elemen lebih kecil

//            // Tambahkan teks di tengah dengan ukuran font yang lebih kecil
//            using var graphics = Graphics.FromImage(qrBitmap);
//            var font = new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold);  // Ukuran font lebih kecil
//            var textSize = graphics.MeasureString(centerText, font);
//            var textX = (qrBitmap.Width - textSize.Width) / 2;
//            var textY = (qrBitmap.Height - textSize.Height) / 2;
//            graphics.DrawString(centerText, font, Brushes.Black, new PointF(textX, textY));

//            // Konversi QR Code ke Base64
//            using var ms = new MemoryStream();
//            qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//            return Convert.ToBase64String(ms.ToArray());
//        }

//        // PUT: api/PendaftaranPasien/5
//        [HttpPut("{id}")]
//        public async Task<IActionResult> PutPendaftaranPasien(Guid id, PendaftaranPasienBaru pendaftaranPasien)
//        {
//            if (id != pendaftaranPasien.PendaftaranPasienBaruId)
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
//            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasienBarus.FindAsync(id);
//            if (pendaftaranPasien == null)
//            {
//                return NotFound();
//            }

//            _applicationDbContext.PendaftaranPasienBarus.Remove(pendaftaranPasien);
//            await _applicationDbContext.SaveChangesAsync();

//            return NoContent();
//        }

//        private bool PendaftaranPasienExists(Guid id)
//        {
//            return _applicationDbContext.PendaftaranPasienBarus.Any(e => e.PendaftaranPasienBaruId == id);
//        }
//    }
//}
