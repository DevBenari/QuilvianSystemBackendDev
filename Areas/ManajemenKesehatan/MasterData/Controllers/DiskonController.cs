using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DiskonController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DiskonController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DiskonController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DiskonController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                var baseQuery =
                    from a in _applicationDbContext.Diskons.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    where a.IsDelete == false || a.IsDelete == null
                    select new
                    {
                        a.DiskonId,
                        a.NamaDiskon,
                        a.TglBerlaku,
                        a.TglBerakhir,
                        a.IsAsuransi,
                        a.AsuransiId,
                        a.MetodePembayaranId,
                        a.PersenDiskon,
                        a.NominalDiskon,
                        a.KodeVoucher,
                        a.IsDireksiApproved,
                        a.IsDiskonCombined,
                        a.KategoriDiskon,
                        a.Qty,
                        a.TipeDiskonDokter,
                        a.ValueDiskonDokter,
                        a.Keterangan,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.CreateDateTime,
                        a.UpdateBy,
                        a.UpdateDateTime
                    };

                var totalRows = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var headers = await baseQuery
                    .OrderByDescending(a => a.CreateDateTime)
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (!headers.Any())
                {
                    return NotFound(new
                    {
                        message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found"
                    });
                }

                var diskonIds = headers.Select(x => x.DiskonId).ToList();

                var details = await _applicationDbContext.DiskonDetails
                    .AsNoTracking()
                    .Where(d => diskonIds.Contains((Guid)d.DiskonId) && (d.IsDelete == false || d.IsDelete == null))
                    .Select(d => new
                    {
                        d.DetailDiskonId,
                        d.DiskonId,
                        d.LayananId,
                        d.ItemId,
                        d.KodeLayanan,
                        d.KategoriLayanan,
                        d.MaxQty,
                        d.MaxHarga,
                        d.Keterangan,
                        d.CreateBy,
                        d.CreateDateTime,
                        d.UpdateBy,
                        d.UpdateDateTime
                    })
                    .OrderByDescending(d => d.CreateDateTime)
                    .ToListAsync();

                var result = headers.Select(h => new
                {
                    h.DiskonId,
                    h.NamaDiskon,
                    h.KodeVoucher,
                    h.TglBerlaku,
                    h.TglBerakhir,
                    h.IsAsuransi,
                    h.AsuransiId,
                    h.MetodePembayaranId,
                    h.PersenDiskon,
                    h.NominalDiskon,
                    h.Keterangan,
                    h.CreateBy,
                    h.CreateByName,
                    h.CreateDateTime,
                    h.UpdateBy,
                    h.UpdateDateTime,
                    Details = details
                        .Where(d => d.DiskonId == h.DiskonId)
                        .ToList()
                }).ToList();

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = result,
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var header = await (
                    from a in _applicationDbContext.Diskons.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    where a.DiskonId == id && (a.IsDelete == false || a.IsDelete == null)
                    select new
                    {
                        a.DiskonId,
                        a.NamaDiskon,
                        a.TglBerlaku,
                        a.TglBerakhir,
                        a.IsAsuransi,
                        a.AsuransiId,
                        a.MetodePembayaranId,
                        a.PersenDiskon,
                        a.NominalDiskon,
                        a.KodeVoucher,
                        a.IsDireksiApproved,
                        a.IsDiskonCombined,
                        a.KategoriDiskon,
                        a.Qty,
                        a.TipeDiskonDokter,
                        a.ValueDiskonDokter,
                        a.Keterangan,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.CreateDateTime,
                        a.UpdateBy,
                        a.UpdateDateTime
                    }
                ).FirstOrDefaultAsync();

                if (header == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                var details = await (
                    from d in _applicationDbContext.DiskonDetails.AsNoTracking()
                    where d.DiskonId == id && (d.IsDelete == false || d.IsDelete == null)
                    select new
                    {
                        d.DetailDiskonId,
                        d.DiskonId,
                        d.LayananId,
                        d.ItemId,
                        d.KodeLayanan,
                        d.KategoriLayanan,
                        d.MaxQty,
                        d.MaxHarga,
                        d.Keterangan,
                        d.CreateBy,
                        d.CreateDateTime
                    }
                ).ToListAsync();

                return Ok(new
                {
                    message = "Ditemukan || 200 OK",
                    data = new
                    {
                        header,
                        details
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DiskonViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                if (string.IsNullOrWhiteSpace(vm.NamaDiskon))
                {
                    return BadRequest(new { message = "Nama diskon wajib diisi." });
                }

                if (vm.TglBerlaku.HasValue && vm.TglBerakhir.HasValue && vm.TglBerakhir < vm.TglBerlaku)
                {
                    return BadRequest(new { message = "Tanggal berakhir tidak boleh lebih kecil dari tanggal berlaku." });
                }

                bool isDuplicate = await _applicationDbContext.Diskons
                    .AnyAsync(c => c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim()
                                && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama diskon ini telah tersedia." });
                }

                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                var diskonId = Guid.NewGuid();

                var data = new Diskon
                {
                    DiskonId = diskonId,
                    NamaDiskon = vm.NamaDiskon,
                    KodeVoucher = vm.KodeVoucher,
                    TglBerlaku = vm.TglBerlaku,
                    TglBerakhir = vm.TglBerakhir,
                    IsAsuransi = vm.IsAsuransi,
                    MetodePembayaranId = vm.MetodePembayaranId,
                    AsuransiId = vm.AsuransiId,
                    PersenDiskon = vm.PersenDiskon,
                    NominalDiskon = vm.NominalDiskon,
                    IsDireksiApproved = false,
                    IsDiskonCombined = false,
                    KategoriDiskon = vm.KategoriDiskon,
                    Qty = vm.Qty,
                    TipeDiskonDokter = vm.TipeDiskonDokter,
                    ValueDiskonDokter = vm.ValueDiskonDokter,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.Diskons.Add(data);

                if (vm.Details != null && vm.Details.Any())
                {
                    var detailEntities = vm.Details.Select(d => new DiskonDetail
                    {
                        DetailDiskonId = Guid.NewGuid(),
                        DiskonId = diskonId,
                        LayananId = d.LayananId,
                        ItemId = d.ItemId,
                        KodeLayanan = d.KodeLayanan,
                        KategoriLayanan = d.KategoriLayanan,
                        MaxQty = d.MaxQty,
                        MaxHarga = d.MaxHarga,
                        Keterangan = d.Keterangan,
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        IsDelete = false
                    }).ToList();

                    _applicationDbContext.DiskonDetails.AddRange(detailEntities);
                }

                var result = await _applicationDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        diskonId
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DiskonViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ambil email user login dari claim
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // ambil user active
                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                // validasi nama diskon
                if (string.IsNullOrWhiteSpace(vm.NamaDiskon))
                {
                    return BadRequest(new { message = "Nama diskon wajib diisi." });
                }

                // validasi tanggal
                if (vm.TglBerlaku.HasValue && vm.TglBerakhir.HasValue && vm.TglBerakhir < vm.TglBerlaku)
                {
                    return BadRequest(new { message = "Tanggal berakhir tidak boleh lebih kecil dari tanggal berlaku." });
                }

                // cek data diskon
                var existingDiskon = await _applicationDbContext.Diskons
                    .FirstOrDefaultAsync(x => x.DiskonId == id && x.IsDelete == false);

                if (existingDiskon == null)
                {
                    return NotFound(new { message = "Data diskon tidak ditemukan." });
                }

                // cek duplikasi nama diskon selain id yang sedang diedit
                bool isDuplicate = await _applicationDbContext.Diskons
                    .AnyAsync(c =>
                        c.DiskonId != id &&
                        c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim() &&
                        c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama diskon ini telah tersedia." });
                }

                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                // update header diskon
                existingDiskon.NamaDiskon = vm.NamaDiskon;
                existingDiskon.KodeVoucher = vm.KodeVoucher;
                existingDiskon.TglBerlaku = vm.TglBerlaku;
                existingDiskon.TglBerakhir = vm.TglBerakhir;
                existingDiskon.IsAsuransi = vm.IsAsuransi;
                existingDiskon.MetodePembayaranId = vm.MetodePembayaranId;
                existingDiskon.AsuransiId = vm.AsuransiId;
                existingDiskon.PersenDiskon = vm.PersenDiskon;
                existingDiskon.NominalDiskon = vm.NominalDiskon;
                existingDiskon.KategoriDiskon = vm.KategoriDiskon;
                existingDiskon.Qty = vm.Qty;
                existingDiskon.TipeDiskonDokter = vm.TipeDiskonDokter;
                existingDiskon.ValueDiskonDokter = vm.ValueDiskonDokter;
                existingDiskon.Keterangan = vm.Keterangan;
                existingDiskon.UpdateBy = userActiveId;
                existingDiskon.UpdateDateTime = DateTimeOffset.UtcNow;

                // tambah detail baru tanpa menghapus detail lama
                if (vm.Details != null && vm.Details.Any())
                {
                    var existingDetails = await _applicationDbContext.DiskonDetails
                        .Where(x => x.DiskonId == id && x.IsDelete == false)
                        .ToListAsync();

                    var newDetails = new List<DiskonDetail>();

                    foreach (var d in vm.Details)
                    {
                        bool detailSudahAda = existingDetails.Any(x =>
                            x.LayananId == d.LayananId &&
                            x.KodeLayanan == d.KodeLayanan &&
                            x.KategoriLayanan == d.KategoriLayanan &&
                            x.MaxQty == d.MaxQty &&
                            x.MaxHarga == d.MaxHarga &&
                            x.IsDelete == false);

                        if (!detailSudahAda)
                        {
                            newDetails.Add(new DiskonDetail
                            {
                                DetailDiskonId = Guid.NewGuid(),
                                DiskonId = id,
                                LayananId = d.LayananId,
                                ItemId = d.ItemId,
                                KodeLayanan = d.KodeLayanan,
                                KategoriLayanan = d.KategoriLayanan,
                                MaxQty = d.MaxQty,
                                MaxHarga = d.MaxHarga,
                                Keterangan = d.Keterangan,
                                CreateBy = userActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow,
                                IsDelete = false
                            });
                        }
                    }

                    if (newDetails.Any())
                    {
                        await _applicationDbContext.DiskonDetails.AddRangeAsync(newDetails);
                    }
                }

                var result = await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Ubah Data Berhasil || 200 OK",
                        diskonId = id
                    });
                }

                return Ok(new
                {
                    message = "Tidak ada perubahan data.",
                    diskonId = id
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }
        
        [HttpPut("Update-AprovalDireksi/{id}")]
        public async Task<IActionResult> AprovalDireksi(Guid id, [FromBody] UpdateIsDireksiApprovedVM request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;
                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }
                // Periksa apakah pasien dan asuransi ada di database
                //var pasienExists = _applicationDbContext.PendaftaranPasienBarus
                //                      .Any(p => p.PendaftaranPasienBaruId == request.PasienId);
                //var asuransiExists = _applicationDbContext.Asuransis
                //                      .Any(a => a.AsuransiId == request.AsuransiId);
                //if (!pasienExists || !asuransiExists)
                //{
                //    return NotFound(new { message = "Pasien atau Asuransi tidak ditemukan!" });
                //}
                //validate model state
                if (ModelState.IsValid)
                {
                    var data = _applicationDbContext.Diskons.Find(id);
                    if (data == null)
                    {
                        return NotFound(new { message = "Data tidak ditemukan." });
                    }
                    data.IsDireksiApproved = request.Status;

                    data.UpdateDateTime = DateTimeOffset.UtcNow;
                    data.UpdateBy = UserActiveId;

                    _applicationDbContext.Diskons.Update(data);
                    await _applicationDbContext.SaveChangesAsync();
                    return Ok(new { message = "Data berhasil diubah!", data });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("Update-CombinedDiskon/{id}")]
        public async Task<IActionResult> CombinedDiskon(Guid id, [FromBody] UpdateIsCombinedVM request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;
                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }
                // Periksa apakah pasien dan asuransi ada di database
                //var pasienExists = _applicationDbContext.PendaftaranPasienBarus
                //                      .Any(p => p.PendaftaranPasienBaruId == request.PasienId);
                //var asuransiExists = _applicationDbContext.Asuransis
                //                      .Any(a => a.AsuransiId == request.AsuransiId);
                //if (!pasienExists || !asuransiExists)
                //{
                //    return NotFound(new { message = "Pasien atau Asuransi tidak ditemukan!" });
                //}
                //validate model state
                if (ModelState.IsValid)
                {
                    var data = _applicationDbContext.Diskons.Find(id);
                    if (data == null)
                    {
                        return NotFound(new { message = "Data tidak ditemukan." });
                    }
                    data.IsDiskonCombined = request.Status;

                    data.UpdateDateTime = DateTimeOffset.UtcNow;
                    data.UpdateBy = UserActiveId;

                    _applicationDbContext.Diskons.Update(data);
                    await _applicationDbContext.SaveChangesAsync();
                    return Ok(new { message = "Data berhasil diubah!", data });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                var data = await _applicationDbContext.Diskons
                    .FirstOrDefaultAsync(x => x.DiskonId == id && x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new { message = "Data diskon tidak ditemukan." });
                }

                var details = await _applicationDbContext.DiskonDetails
                    .Where(x => x.DiskonId == id && x.IsDelete == false)
                    .ToListAsync();

                data.IsDelete = true;
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                if (details.Any())
                {
                    foreach (var item in details)
                    {
                        item.IsDelete = true;
                        item.DeleteBy = userActiveId;
                        item.DeleteDateTime = DateTimeOffset.UtcNow;
                    }

                    _applicationDbContext.DiskonDetails.UpdateRange(details);
                }

                _applicationDbContext.Diskons.Update(data);

                var result = await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data header dan detail berhasil dihapus (soft delete) || 200 OK",
                        diskonId = id
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menghapus data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? namaLayanan = null,
            string? kodedisk = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                var now = DateTime.UtcNow;
                var todayStart = now.Date;
                var tomorrowStart = todayStart.AddDays(1);

                var baseQuery =
                    from a in _applicationDbContext.Diskons.AsNoTracking()
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    where a.IsDelete == false || a.IsDelete == null
                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        a.DiskonId,
                        a.NamaDiskon,
                        a.KodeVoucher,
                        a.TglBerlaku,
                        a.TglBerakhir,
                        a.IsAsuransi,
                        a.AsuransiId,
                        a.MetodePembayaranId,
                        a.PersenDiskon,
                        a.NominalDiskon,
                        a.IsDireksiApproved,
                        a.IsDiskonCombined,
                        a.KategoriDiskon,
                        a.Qty,
                        a.TipeDiskonDokter,
                        a.ValueDiskonDokter,
                        a.Keterangan,
                        a.UpdateBy,
                        a.UpdateDateTime
                    };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchPattern = $"%{search.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.NamaDiskon!, searchPattern));
                }

                if (!string.IsNullOrWhiteSpace(kodedisk))
                {
                    var searchPattern = $"%{kodedisk.Trim()}%";

                    baseQuery = baseQuery.Where(x =>
                        EF.Functions.ILike(x.KodeVoucher!, searchPattern));
                }

                if (!string.IsNullOrWhiteSpace(namaLayanan))
                {
                    var layananPattern = $"%{namaLayanan.Trim()}%";

                    var diskonIdByLayananQuery =
                        from d in _applicationDbContext.DiskonDetails.AsNoTracking()
                        join l in _applicationDbContext.Layanans.AsNoTracking()
                            on d.LayananId equals l.LayananId
                        where (d.IsDelete == false || d.IsDelete == null)
                              && d.DiskonId != null
                              && EF.Functions.ILike(l.NamaLayanan!, layananPattern)
                        select d.DiskonId.Value;

                    baseQuery = baseQuery.Where(x => diskonIdByLayananQuery.Contains(x.DiskonId));
                }

                if (startDate.HasValue)
                {
                    var startUtc = startDate.Value.Date.ToUniversalTime();
                    baseQuery = baseQuery.Where(x => x.CreateDateTime >= startUtc);
                }

                if (endDate.HasValue)
                {
                    var endUtcExclusive = endDate.Value.Date.AddDays(1).ToUniversalTime();
                    baseQuery = baseQuery.Where(x => x.CreateDateTime < endUtcExclusive);
                }

                if (periode.HasValue)
                {
                    switch (periode.Value)
                    {
                        case PeriodeFilter.Today:
                            {
                                baseQuery = baseQuery.Where(x =>
                                    x.CreateDateTime >= todayStart &&
                                    x.CreateDateTime < tomorrowStart);
                                break;
                            }

                        case PeriodeFilter.ThisWeek:
                            {
                                var diff = ((int)todayStart.DayOfWeek + 6) % 7; // Monday = 0
                                var startOfWeek = todayStart.AddDays(-diff);
                                var endOfWeekExclusive = startOfWeek.AddDays(7);

                                baseQuery = baseQuery.Where(x =>
                                    x.CreateDateTime >= startOfWeek &&
                                    x.CreateDateTime < endOfWeekExclusive);
                                break;
                            }

                        case PeriodeFilter.LastWeek:
                            {
                                var diff = ((int)todayStart.DayOfWeek + 6) % 7; // Monday = 0
                                var startOfThisWeek = todayStart.AddDays(-diff);
                                var startOfLastWeek = startOfThisWeek.AddDays(-7);

                                baseQuery = baseQuery.Where(x =>
                                    x.CreateDateTime >= startOfLastWeek &&
                                    x.CreateDateTime < startOfThisWeek);
                                break;
                            }

                        case PeriodeFilter.ThisMonth:
                            {
                                var startOfMonth = new DateTime(todayStart.Year, todayStart.Month, 1);
                                var startOfNextMonth = startOfMonth.AddMonths(1);

                                baseQuery = baseQuery.Where(x =>
                                    x.CreateDateTime >= startOfMonth &&
                                    x.CreateDateTime < startOfNextMonth);
                                break;
                            }

                        case PeriodeFilter.LastMonth:
                            {
                                var startOfThisMonth = new DateTime(todayStart.Year, todayStart.Month, 1);
                                var startOfLastMonth = startOfThisMonth.AddMonths(-1);

                                baseQuery = baseQuery.Where(x =>
                                    x.CreateDateTime >= startOfLastMonth &&
                                    x.CreateDateTime < startOfThisMonth);
                                break;
                            }

                        case PeriodeFilter.ThisYear:
                            {
                                var startOfYear = new DateTime(todayStart.Year, 1, 1);
                                var startOfNextYear = startOfYear.AddYears(1);

                                baseQuery = baseQuery.Where(x =>
                                    x.CreateDateTime >= startOfYear &&
                                    x.CreateDateTime < startOfNextYear);
                                break;
                            }

                        case PeriodeFilter.LastYear:
                            {
                                var startOfThisYear = new DateTime(todayStart.Year, 1, 1);
                                var startOfLastYear = startOfThisYear.AddYears(-1);

                                baseQuery = baseQuery.Where(x =>
                                    x.CreateDateTime >= startOfLastYear &&
                                    x.CreateDateTime < startOfThisYear);
                                break;
                            }

                        case PeriodeFilter.Last3Months:
                            {
                                var start3Months = todayStart.AddMonths(-3);
                                baseQuery = baseQuery.Where(x => x.CreateDateTime >= start3Months);
                                break;
                            }

                        case PeriodeFilter.Last6Months:
                            {
                                var start6Months = todayStart.AddMonths(-6);
                                baseQuery = baseQuery.Where(x => x.CreateDateTime >= start6Months);
                                break;
                            }
                    }
                }

                baseQuery = (orderBy?.ToLower(), sortDirection?.ToLower()) switch
                {
                    ("createdatetime", "asc") => baseQuery.OrderBy(x => x.CreateDateTime),
                    
                    _ => baseQuery.OrderByDescending(x => x.CreateDateTime)
                };

                var totalRows = await baseQuery.CountAsync();
                var totalPages = totalRows == 0 ? 0 : (int)Math.Ceiling(totalRows / (double)perPage);

                var headers = await baseQuery
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (headers.Count == 0 && page > 1 && totalRows > 0)
                {
                    return NotFound(new
                    {
                        status = "error",
                        message = "Page not found."
                    });
                }

                var diskonIds = headers.Select(x => x.DiskonId).ToList();

                var details = await (
                    from d in _applicationDbContext.DiskonDetails.AsNoTracking()
                    join l in _applicationDbContext.Layanans.AsNoTracking()
                        on d.LayananId equals l.LayananId into layananGroup
                    from l in layananGroup.DefaultIfEmpty()
                    where d.DiskonId != null
                          && diskonIds.Contains(d.DiskonId.Value)
                          && (d.IsDelete == false || d.IsDelete == null)
                    orderby d.CreateDateTime descending
                    select new
                    {
                        d.DetailDiskonId,
                        d.DiskonId,
                        d.LayananId,
                        d.ItemId,
                        NamaLayanan = l != null ? l.NamaLayanan : null,
                        d.KodeLayanan,
                        d.KategoriLayanan,
                        d.MaxQty,
                        d.MaxHarga,
                        d.Keterangan,
                        d.CreateBy,
                        d.CreateDateTime,
                        d.UpdateBy,
                        d.UpdateDateTime
                    }
                ).ToListAsync();

                var detailLookup = details
                    .GroupBy(x => x.DiskonId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var rows = headers.Select(h => new
                {
                    h.CreateDateTime,
                    h.CreateBy,
                    h.CreateByName,
                    h.DiskonId,
                    h.NamaDiskon,
                    h.KodeVoucher,
                    h.TglBerlaku,
                    h.TglBerakhir,
                    h.IsAsuransi,
                    h.AsuransiId,
                    h.MetodePembayaranId,
                    h.PersenDiskon,
                    h.NominalDiskon,
                    h.Keterangan,
                    h.UpdateBy,
                    h.UpdateDateTime,
                    Details = detailLookup.TryGetValue(h.DiskonId, out var itemDetails)
                    ? itemDetails.Cast<object>().ToList()
                    : new List<object>()
                }).ToList();

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = rows,
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }
    }
}