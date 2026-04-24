using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterTTDController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadUrl;
        public MasterTTDController(
                ApplicationDbContext context,
                IConfiguration configuration
            ) 
            {
            _context = context;
                _uploadUrl = configuration["FileStorage:UploadUrl"];
            }

        [HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query =
                from m in _context.Set<MasterTTD>()
                orderby m.CreateDateTime descending
                select new
                {
                    m.TTDId,
                    m.UserActiveId,
                    m.TTDPath,
                    m.Keterangan,
                    m.CreateDateTime, m.CreateBy, m.UpdateDateTime, m.UpdateBy, m.DeleteDateTime, m.DeleteBy, m.IsDelete
                };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var listData = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();
            if (!listData.Any()) return NotFound(new { message = "Belum ada data || 404 Not Found" });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listData,
                pagination = new { CurrentPage = page, PerPage = perPage, TotalRows = totalRows, TotalPages = totalPages }
            });
        }

        [HttpGet("{UserActiveId}")]
        public async Task<IActionResult> GetTtdById(Guid UserActiveId)
        {
            var listdata = await _context.MasterTTDs
                .FirstOrDefaultAsync(x => x.UserActiveId == UserActiveId);

            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }


        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)] // 10 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
        public async Task<IActionResult> UploadTTD([FromForm] MasterTTDVM vm)
        {
            try
            {
                if (vm.TTDPath == null || vm.TTDPath.Length == 0)
                    return BadRequest(new { message = "File TTD tidak boleh kosong." });

                // Validasi ekstensi
                var allowedExt = new[] { ".jpg", ".jpeg" };
                var ext = Path.GetExtension(vm.TTDPath.FileName).ToLower();

                if (!allowedExt.Contains(ext))
                    return BadRequest(new { message = "Format file harus JPG atau JPEG." });

                var fileName = $"{vm.UserActiveId}{ext}";
                var folderTarget = "TTDUser"; // folder di Flask
                var filePath = $"/{folderTarget}/{fileName}";

                // ======================================================
                // ?? FIX: Siapkan MemoryStream untuk dikirim ke Flask
                // ======================================================
                using var ms = new MemoryStream();
                await vm.TTDPath.CopyToAsync(ms);
                ms.Position = 0;

                // Gunakan default image/jpeg jika ContentType kosong
                var contentType = string.IsNullOrWhiteSpace(vm.TTDPath.ContentType)
                    ? "image/jpeg"
                    : vm.TTDPath.ContentType;

                var fileContent = new StreamContent(ms);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                fileContent.Headers.ContentLength = ms.Length;

                // Multipart form untuk Flask
                using var content = new MultipartFormDataContent();
                content.Add(fileContent, "file", fileName);
                content.Add(new StringContent(folderTarget), "folderTarget");

                // ======================================================
                // ?? Kirim file ke server Flask
                // ======================================================
                using var client = new HttpClient();
                var response = await client.PostAsync(_uploadUrl, content);

                if (!response.IsSuccessStatusCode)
                    return StatusCode(500, new { message = $"Gagal upload ke Flask. Status: {response.StatusCode}" });

                // ======================================================
                // ?? Simpan metadata ke database
                // ======================================================

                // cek duplicate untuk UserActiveId
                var existingTTD = await _context.MasterTTDs
                    .FirstOrDefaultAsync(t => t.UserActiveId == vm.UserActiveId && !t.IsDelete);
                if (existingTTD != null)
                {
                    return BadRequest(new { message = "UserActiveId sudah memiliki TTD. Gunakan update jika ingin mengganti TTD." });
                }

                    var entity = new MasterTTD
                {
                    TTDId = Guid.NewGuid(),
                    UserActiveId = vm.UserActiveId,
                    TTDPath = filePath,  // path di Flask
                    Keterangan = vm.Keterangan,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _context.MasterTTDs.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Upload TTD berhasil via Flask",
                    ttdId = entity.TTDId,
                    ttdPath = entity.TTDPath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(Guid id, [FromForm] MasterTTDVM vm)
        //{
        //    var entity = await _context.Set<MasterTTD>().FindAsync(id);
        //    if (entity == null)
        //        return NotFound(new { message = "Data tidak ditemukan." });

        //    // Update data biasa
        //    entity.UserActiveId = vm.UserActiveId;
        //    entity.Keterangan = vm.Keterangan;
        //    entity.UpdateDateTime = DateTimeOffset.UtcNow;

        //    // ======================================================
        //    // ?? Jika ada file baru, upload ke Flask
        //    // ======================================================
        //    if (vm.TTDPath != null && vm.TTDPath.Length > 0)
        //    {
        //        var allowedExt = new[] { ".jpg", ".jpeg" };
        //        var ext = Path.GetExtension(vm.TTDPath.FileName).ToLower();

        //        if (!allowedExt.Contains(ext))
        //            return BadRequest(new { message = "Format file harus JPG atau JPEG." });

        //        var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        //        var fileName = $"{vm.UserActiveId}_{safeTime}{ext}";
        //        var folderTarget = "TTDUser";
        //        var newFilePath = $"/{folderTarget}/{fileName}";

        //        // =====================================
        //        // ?? Upload File ke Flask
        //        // =====================================
        //        using var ms = new MemoryStream();
        //        await vm.TTDPath.CopyToAsync(ms);
        //        ms.Position = 0;

        //        var contentType = string.IsNullOrWhiteSpace(vm.TTDPath.ContentType)
        //            ? "image/jpeg"
        //            : vm.TTDPath.ContentType;

        //        var fileContent = new StreamContent(ms);
        //        fileContent.Headers.ContentType =
        //            new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        //        fileContent.Headers.ContentLength = ms.Length;

        //        using var content = new MultipartFormDataContent();
        //        content.Add(fileContent, "file", fileName);
        //        content.Add(new StringContent(folderTarget), "folderTarget");

        //        using var client = new HttpClient();

        //        var uploadUrl = $"{_uploadUrl}/upload";
        //        var uploadResponse = await client.PostAsync(uploadUrl, content);

        //        if (!uploadResponse.IsSuccessStatusCode)
        //        {
        //            return StatusCode(500, new
        //            {
        //                message = "Gagal upload file baru ke server Flask.",
        //                status = uploadResponse.StatusCode
        //            });
        //        }

        //        // =====================================
        //        // ??? Hapus file lama dari Flask
        //        // =====================================
        //        if (!string.IsNullOrWhiteSpace(entity.TTDPath))
        //        {
        //            try
        //            {
        //                // contoh path: "/TTDUser/xxx.jpg"
        //                var oldPath = entity.TTDPath.TrimStart('/');
        //                var parts = oldPath.Split('/');

        //                if (parts.Length >= 2)
        //                {
        //                    var oldFolder = parts[0];
        //                    var oldFile = parts[1];

        //                    var deleteUrl = $"{_uploadUrl}/delete?folder={oldFolder}&filename={oldFile}";
        //                    var delResponse = await client.DeleteAsync(deleteUrl);

        //                    if (!delResponse.IsSuccessStatusCode)
        //                    {
        //                        Console.WriteLine($"[WARNING] Gagal hapus file lama di Flask: {delResponse.StatusCode}");
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("[ERROR] Gagal hapus file lama di Flask: " + ex.Message);
        //            }
        //        }

        //        // Simpan path baru
        //        entity.TTDPath = newFilePath;
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Update berhasil." });
        //}


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _context.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = await _context.Set<MasterTTD>().FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            // ================================================
            // ?? Hapus File di Flask
            // ================================================
            try
            {
                if (!string.IsNullOrWhiteSpace(data.TTDPath))
                {
                    // Contoh: "/TTDUser/abc123.jpg"
                    var path = data.TTDPath.Replace("\\", "/").TrimStart('/');

                    var split = path.Split('/');
                    if (split.Length >= 2)
                    {
                        var folder = split[0];
                        var fileName = split[1];

                        using var client = new HttpClient();
                        var deleteUrl = $"{_uploadUrl}/delete?folder={folder}&filename={fileName}";
                        var delResponse = await client.DeleteAsync(deleteUrl);

                        // Tidak wajib sukses, tapi kita log saja
                        if (!delResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"[WARNING] Gagal hapus file di Flask: {delResponse.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Delete Flask Error: " + ex.Message);
                // Tidak gagal total — tetap lanjut hapus DB
            }

            // ================================================
            // ??? Hapus Data di Database
            // ================================================
            _context.Remove(data);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data & file berhasil dihapus || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }

    }
}
