using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Drawing;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class PendaftaranPasienLuarController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendaftaranPasienLuarController
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


        // GET: api/PendaftaranPasien/5
        [HttpGet("Registrasi/{id}")]
        public async Task<ActionResult<PendaftaranPasien>> GetPendaftaranPasienLuar(Guid id)
        {
            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasiens.FindAsync(id);

            if (pendaftaranPasien == null)
            {
                return NotFound();
            }

            return pendaftaranPasien;
        }

        [HttpPost("Registrasi")]
        public async Task<ActionResult<PendaftaranPasien>> PostPendaftaranPasienLuar(PendaftaranPasien pendaftaranPasien)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var lastCode = _applicationDbContext.PendaftaranPasiens
                                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                                .OrderByDescending(k => k.NoRekamMedis)
                                .FirstOrDefault();

            if (lastCode == null)
            {
                pendaftaranPasien.NoRekamMedis = "REG" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.NoRekamMedis.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    pendaftaranPasien.NoRekamMedis = "REG" + setDateNow + "0001";
                }
                else
                {
                    pendaftaranPasien.NoRekamMedis = "REG" + setDateNow + (Convert.ToInt32(lastCode.NoRekamMedis.Substring(9, lastCode.NoRekamMedis.Length - 9)) + 1).ToString("D4");
                }
            }
            // Generate GUID untuk pasien baru
            pendaftaranPasien.PendaftaranPasienId = Guid.NewGuid();

            // Buat QR Code berdasarkan NamaLengkap dan NoRekamMedis
            string qrContent = $"Nama: {pendaftaranPasien.NamaLengkap}\nNo RM: {pendaftaranPasien.NoRekamMedis}";

            // Simpan data ke database
            _applicationDbContext.PendaftaranPasiens.Add(pendaftaranPasien);
            await _applicationDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPendaftaranPasienLuar), new { id = pendaftaranPasien.PendaftaranPasienId }, pendaftaranPasien);
        }


        // GET: api/PendaftaranPasien/5
        [HttpGet("Baru/{id}")]
        public async Task<ActionResult<PendaftaranPasienBaru>> GetPendaftaranPasienBaruLuar(Guid id)
        {
            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasienBarus.FindAsync(id);

            if (pendaftaranPasien == null)
            {
                return NotFound();
            }

            return pendaftaranPasien;
        }

        [HttpPost("Baru")]
        public async Task<ActionResult<PendaftaranPasienBaru>> PostPendaftaranPasienBaruLuar(PendaftaranPasienBaru pendaftaranPasienBaru)
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
                pendaftaranPasienBaru.NoRekamMedis = "LPA" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.NoRekamMedis.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    pendaftaranPasienBaru.NoRekamMedis = "LPA" + setDateNow + "0001";
                }
                else
                {
                    pendaftaranPasienBaru.NoRekamMedis = "LPA" + setDateNow + (Convert.ToInt32(lastCode.NoRekamMedis.Substring(9, lastCode.NoRekamMedis.Length - 9)) + 1).ToString("D4");
                }
            }
            // Generate GUID untuk pasien baru
            pendaftaranPasienBaru.PendaftaranPasienBaruId = Guid.NewGuid();

            // Buat QR Code berdasarkan NamaLengkap dan NoRekamMedis
            string qrContent = $"Nama: {pendaftaranPasienBaru.NamaLengkap}\nNo RM: {pendaftaranPasienBaru.NoRekamMedis}";
            pendaftaranPasienBaru.QrCode = GenerateQrCodeWithDefaultText(qrContent); // Simpan QR Code Base64 ke properti QrCode

            // Simpan data ke database
            _applicationDbContext.PendaftaranPasienBarus.Add(pendaftaranPasienBaru);
            await _applicationDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPendaftaranPasienBaruLuar), new { id = pendaftaranPasienBaru.PendaftaranPasienBaruId }, pendaftaranPasienBaru);
        }

        // Fungsi

        private string GenerateQrCodeWithDefaultText(string content)
        {
            string centerText = "MMC"; // Teks default

            using var qrGenerator = new QRCodeGenerator();

            // Menggunakan level koreksi kesalahan lebih tinggi (Q)
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
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
