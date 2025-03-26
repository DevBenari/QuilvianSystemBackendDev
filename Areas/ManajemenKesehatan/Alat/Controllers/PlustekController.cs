using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Tesseract;
using OpenCvSharp;  // Tambahkan namespace OpenCVSharp

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlustekController : Controller
    {
        private readonly string _webRootPath;

        public PlustekController(IWebHostEnvironment env)
        {
            _webRootPath = env.WebRootPath; // Dapatkan path ke wwwroot
        }

        [HttpPost]
        public async Task<IActionResult> ProcessKTPFile()
        {
            // Path ke folder wwwroot/scanktp
            var folderPath = Path.Combine(_webRootPath, "scanktp");

            if (!Directory.Exists(folderPath))
            {
                return Json(new { success = false, message = "Folder scanktp tidak ditemukan." });
            }

            var files = Directory.GetFiles(folderPath, "*.JPG");
            if (files.Length == 0)
            {
                return Json(new { success = false, message = "Tidak ada file JPG dalam folder." });
            }

            string extractedText = string.Empty;
            foreach (var file in files)
            {
                // Terapkan pemrosesan gambar menggunakan OpenCV sebelum OCR
                string processedImagePath = ProcessImageWithOpenCV(file);

                // Ekstrak teks menggunakan Tesseract setelah gambar diproses
                extractedText = ExtractTextFromImage(processedImagePath);

                // Hanya mengambil satu file pertama
                break;
            }

            // Menghapus semua file dalam folder setelah pemrosesan
            DeleteAllFilesInFolder(folderPath);

            return Json(new { success = true, message = "File diproses dengan sukses.", extractedText });
        }

        private string ProcessImageWithOpenCV(string imagePath)
        {
            string processedImagePath = Path.Combine(Path.GetDirectoryName(imagePath), "processed_" + Path.GetFileName(imagePath));

            // Membaca gambar menggunakan OpenCV
            Mat img = Cv2.ImRead(imagePath, ImreadModes.Color);

            // Mengubah gambar ke grayscale
            Mat grayImage = new Mat();
            Cv2.CvtColor(img, grayImage, ColorConversionCodes.BGR2GRAY);

            // Meningkatkan kontras dengan thresholding (dapat disesuaikan)
            Mat threshImage = new Mat();
            Cv2.Threshold(grayImage, threshImage, 150, 255, ThresholdTypes.Binary);

            // Mengurangi noise dengan filter Gaussian (dapat disesuaikan)
            Mat denoisedImage = new Mat();
            Cv2.GaussianBlur(threshImage, denoisedImage, new OpenCvSharp.Size(5, 5), 0);

            // Simpan gambar yang sudah diproses
            Cv2.ImWrite(processedImagePath, denoisedImage);

            return processedImagePath;
        }

        private string ExtractTextFromImage(string imagePath)
        {
            string extractedText = string.Empty;
            string tessdataPath = Path.Combine(_webRootPath, "scanktp", "tessdata");

            if (!Directory.Exists(tessdataPath))
            {
                throw new DirectoryNotFoundException($"Tesseract data directory tidak ditemukan: {tessdataPath}");
            }

            try
            {
                // Menggunakan Tesseract OCR dengan model bahasa Indonesia (ind)
                using (var engine = new TesseractEngine(tessdataPath, "eng+ind", EngineMode.Default))
                {
                    var img = Pix.LoadFromFile(imagePath);
                    var page = engine.Process(img, PageSegMode.Auto);  // Gunakan Auto untuk mode segmen
                    extractedText = page.GetText();  // Mendapatkan teks hasil OCR
                }

            }
            catch (Exception ex)
            {
                extractedText = $"Error saat memproses gambar: {ex.Message}";
            }

            return extractedText;
        }

        // Fungsi untuk menghapus semua file dalam folder
        private void DeleteAllFilesInFolder(string folderPath)
        {
            try
            {
                var files = Directory.GetFiles(folderPath);
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);  // Menghapus file
                }
            }
            catch (Exception ex)
            {
                // Tangani error jika terjadi kesalahan saat menghapus file
                // Misalnya, log error atau kirim pesan kesalahan.
                Console.WriteLine($"Error saat menghapus file: {ex.Message}");
            }
        }
    }
}
