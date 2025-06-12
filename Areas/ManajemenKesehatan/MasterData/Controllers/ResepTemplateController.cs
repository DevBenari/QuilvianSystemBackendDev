using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;
using SkiaSharp;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ResepTemplateController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ResepTemplateController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }


        // **View ResepTemplate**
        [HttpGet]
        public async Task<IActionResult> GetResepTemplates(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query untuk mengambil data resep template
            var query = _applicationDbContext.ResepTemplates
                .Where(r => !r.IsDelete)  // Jika ada properti "IsDelete" untuk soft delete
                .Select(r => new
                {
                    r.ResepTemplateId,
                    r.ObatId,
                    ObatName = r.Obat.ObatName,
                    r.KodeResepTemplate,
                    r.Judul,
                    r.DokterId,
                    r.Qty,
                    r.Signa,
                    r.SignaTambahan,
                    r.InteraturObat,
                    CreateDateTime = r.CreateDateTime,
                    CreateBy = r.CreateBy,
                    UpdateDateTime = r.UpdateDateTime,
                    UpdateBy = r.UpdateBy
                });

            // Menghitung jumlah total data
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data berdasarkan halaman yang diminta
            var listdata = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Data berhasil ditemukan.",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.ResepTemplates.Find(id);
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


        // **Create ResepTemplate**
        [HttpPost]
        public async Task<IActionResult> CreateResepTemplate([FromBody] ResepTemplateViewModel resepTemplateViewModel)
        {
            if (resepTemplateViewModel == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Ambil User ID dari JWT Claims
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // Mendapatkan tanggal sekarang
                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Menentukan KodeResepTemplate berdasarkan tanggal dan urutan
                var lastCode = await _applicationDbContext.ResepTemplates
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(r => r.KodeResepTemplate)
                    .FirstOrDefaultAsync();

                string KodeResepTemplate;
                if (lastCode == null || lastCode.KodeResepTemplate.Substring(2, 6) != setDateNow)
                {
                    KodeResepTemplate = $"CR{setDateNow}00001"; // Format kode resep template baru dimulai dari 1
                }
                else
                {
                    int lastNumber = Convert.ToInt32(lastCode.KodeResepTemplate.Substring(8)); // Ambil angka dari kode yang terakhir
                    KodeResepTemplate = $"CR{setDateNow}{(lastNumber + 1).ToString("D5")}"; // Format 5 digit
                }

                // Cek jika sudah ada data yang sama berdasarkan KodeResepTemplate
                var isDuplicate = await _applicationDbContext.ResepTemplates
                    .AnyAsync(r => r.KodeResepTemplate == KodeResepTemplate);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data dengan kode resep template yang sama sudah ada || 409 Conflict Data" });
                }

                // Convert ViewModel ke Entity ResepTemplate
                var resepTemplate = new ResepTemplate
                {
                    ResepTemplateId = Guid.NewGuid(),
                    KodeResepTemplate = KodeResepTemplate,  // Gunakan kode yang sudah dihasilkan
                    ObatId = resepTemplateViewModel.ObatId,
                    Judul = resepTemplateViewModel.Judul,
                    DokterId = resepTemplateViewModel.DokterId,
                    Qty = resepTemplateViewModel.Qty,
                    Signa = resepTemplateViewModel.Signa,
                    SignaTambahan = resepTemplateViewModel.SignaTambahan,
                    InteraturObat = resepTemplateViewModel.InteraturObat,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // Insert data baru ke database
                _applicationDbContext.ResepTemplates.Add(resepTemplate);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // **Update ResepTemplate**
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResepTemplate(Guid id, [FromBody] ResepTemplateViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid!" });

            try
            {
                var resepTemplate = await _applicationDbContext.ResepTemplates
                    .FirstOrDefaultAsync(rt => rt.ResepTemplateId == id);

                if (resepTemplate == null)
                    return NotFound(new { message = "Resep Template tidak ditemukan!" });

                // Update data
                resepTemplate.Judul = vm.Judul;
                resepTemplate.DokterId = vm.DokterId;
                resepTemplate.Qty = vm.Qty;
                resepTemplate.Signa = vm.Signa;
                resepTemplate.SignaTambahan = vm.SignaTambahan;
                resepTemplate.InteraturObat = vm.InteraturObat;

                _applicationDbContext.ResepTemplates.Update(resepTemplate);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Resep Template berhasil diperbarui || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("ResepByDokter/{idDokter}")]
        public async Task<IActionResult> GetResepTemplateByDokter(Guid idDokter)
        {
            try
            {
                // Query untuk mengambil data yang unik berdasarkan Judul
                var query = from rt in _applicationDbContext.ResepTemplates
                            where rt.DokterId == idDokter
                            group rt by rt.Judul into grouped
                            select new
                            {
                                Judul = grouped.Key,
                                ResepTemplates = grouped.Select(rt => new
                                {
                                    rt.KodeResepTemplate,
                                    rt.ResepTemplateId,
                                    rt.ObatId,
                                    rt.Qty,
                                    rt.Signa,
                                    rt.SignaTambahan,
                                    rt.InteraturObat,
                                    rt.CreateBy,
                                    rt.CreateDateTime,
                                }).ToList()
                            };

                var result = await query.ToListAsync();

                if (!result.Any())
                {
                    return NotFound(new { message = "Resep Template tidak ditemukan!" });
                }

                return Ok(new { message = "Berhasil || 200 OK", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }
        // deelete resep template
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResepTemplate(Guid id)
        {
            try
            {
                var resepTemplate = await _applicationDbContext.ResepTemplates
                    .FirstOrDefaultAsync(r => r.ResepTemplateId == id);

                if (resepTemplate == null)
                {
                    return NotFound(new { message = "ResepTemplate dengan ID tersebut tidak ditemukan." });
                }

                // Hard delete: menghapus data dari database
                _applicationDbContext.ResepTemplates.Remove(resepTemplate);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus (hard delete)." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        // **Get All ResepTemplate (Paged)**
        [HttpGet("paged")]
        public async Task<IActionResult> PagedResepTemplate(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query utama untuk ResepTemplate
            var query = from rt in _applicationDbContext.ResepTemplates
                        select new
                        {
                            rt.ObatId,
                            rt.Judul,
                            rt.DokterId,
                            rt.Qty,
                            rt.Signa,
                            rt.SignaTambahan,
                            rt.InteraturObat,
                            rt.KodeResepTemplate, // Menambahkan KodeResepTemplate dalam query,
                            rt.CreateBy,
                            rt.CreateDateTime,
                        };

            // Hitung total data sebelum paginasi
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }
    }
}
