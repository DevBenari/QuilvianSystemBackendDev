using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration;
using QRCoder;
using QuilvianSystem.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;
using System.Drawing;
using ZXing.QrCode.Internal;

namespace QuilvianSystem.Areas.ManajemenKesehatan.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] 
    //[EnableCors("AllowSpecific")]
    public class PendaftaranPasienBaruController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendaftaranPasienBaruController
        (
                ApplicationDbContext applicationDbContext,
                UserManager<ApplicationUser> userManager,
                IWebHostEnvironment webHostEnvironment,
                SignInManager<ApplicationUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationDbContext = applicationDbContext;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: api/PendaftaranPasien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PendaftaranPasienBaru>>> GetPendaftaranPasienBaru()
        {
            return await _applicationDbContext.PendaftaranPasienBarus.ToListAsync();
        }

        // GET: api/PendaftaranPasien/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PendaftaranPasienBaru>> GetPendaftaranPasienBaru(Guid id)
        {
            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasienBarus.FindAsync(id);

            if (pendaftaranPasien == null)
            {
                return NotFound();
            }

            return pendaftaranPasien;
        }

        [HttpPost]
        public async Task<ActionResult<PendaftaranPasienBaru>> PostPendaftaranPasienBaru(PendaftaranPasienBaru pendaftaranPasien)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var lastCode = _applicationDbContext.PendaftaranPasienBarus
                                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                                .OrderByDescending(k => k.NoRekamMedis)
                                .FirstOrDefault();

            if (lastCode == null)
            {
                pendaftaranPasien.NoRekamMedis = "PSN" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.NoRekamMedis.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    pendaftaranPasien.NoRekamMedis = "PSN" + setDateNow + "0001";
                }
                else
                {
                    pendaftaranPasien.NoRekamMedis = "PSN" + setDateNow + (Convert.ToInt32(lastCode.NoRekamMedis.Substring(9, lastCode.NoRekamMedis.Length - 9)) + 1).ToString("D4");
                }
            }
            // Generate GUID untuk pasien baru
            pendaftaranPasien.PendaftaranPasienBaruId = Guid.NewGuid();

            // Buat QR Code berdasarkan NamaLengkap dan NoRekamMedis
            string qrContent = $"Nama: {pendaftaranPasien.NamaLengkap}\nNo RM: {pendaftaranPasien.NoRekamMedis}";
            pendaftaranPasien.QrCode = GenerateQrCodeWithDefaultText(qrContent); // Simpan QR Code Base64 ke properti QrCode

            // Simpan data ke database
            _applicationDbContext.PendaftaranPasienBarus.Add(pendaftaranPasien);
            await _applicationDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPendaftaranPasienBaru), new { id = pendaftaranPasien.PendaftaranPasienBaruId }, pendaftaranPasien);
        }

        // PUT: api/PendaftaranPasien/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPendaftaranPasienBaru(Guid id, PendaftaranPasienBaru pendaftaranPasien)
        {
            if (id != pendaftaranPasien.PendaftaranPasienBaruId)
            {
                return BadRequest();
            }

            _applicationDbContext.Entry(pendaftaranPasien).State = EntityState.Modified;

            try
            {
                await _applicationDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PendaftaranPasienExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/PendaftaranPasien/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePendaftaranPasienBaru(Guid id)
        {
            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasienBarus.FindAsync(id);
            if (pendaftaranPasien == null)
            {
                return NotFound();
            }

            _applicationDbContext.PendaftaranPasienBarus.Remove(pendaftaranPasien);
            await _applicationDbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool PendaftaranPasienExists(Guid id)
        {
            return _applicationDbContext.PendaftaranPasienBarus.Any(e => e.PendaftaranPasienBaruId == id);
        }

        // Fungsi

        private string GenerateQrCodeWithDefaultText(string content)
        {
            string centerText = "MMC"; // Teks default

            using var qrGenerator = new QRCodeGenerator();

            // Menggunakan level koreksi kesalahan lebih tinggi (Q)
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.QRCode(qrCodeData);
            using var qrBitmap = qrCode.GetGraphic(10);  // Ukuran elemen lebih kecil

            // Tambahkan teks di tengah dengan ukuran font yang lebih kecil
            using var graphics = Graphics.FromImage(qrBitmap);
            var font = new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold);  // Ukuran font lebih kecil
            var textSize = graphics.MeasureString(centerText, font);
            var textX = (qrBitmap.Width - textSize.Width) / 2;
            var textY = (qrBitmap.Height - textSize.Height) / 2;
            graphics.DrawString(centerText, font, Brushes.Black, new PointF(textX, textY));

            // Konversi QR Code ke Base64
            using var ms = new MemoryStream();
            qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }

        // End Fungsi
    }
}
