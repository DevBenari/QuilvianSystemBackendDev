using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ResepTebusController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ResepTebusController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ResepTebusController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResepTebusController> logger,
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
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query utama
            var query = (from r in _applicationDbContext.ResepTebuss
                         join u in _applicationDbContext.UserActives
                             on r.CreateBy equals u.UserActiveId
                         where r.IsDelete == false // jika ada field IsDelete
                         select new
                         {
                             r.ResepTebusId,
                             r.CreateDateTime,
                             r.CreateBy,
                             CreateByName = u.FullName,
                             r.NamaPenebus,
                             r.AntrianResep,
                             r.StatusPembuatanResep,
                             r.StatusPengambilan,
                             r.IsCancelled,
                             r.IsLunas,
                             TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ? r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null,
                             DaftarObat = (from d in _applicationDbContext.ResepTebusDetails
                                           join o in _applicationDbContext.Obats // Asumsi nama tabel obat adalah MasterObat
                                               on d.ObatId equals o.ObatId // Asumsi primary key tabel obat adalah ObatId
                                           where d.ResepTebusId == r.ResepTebusId
                                           select new
                                           {
                                               d.ResepTebusDetailId,
                                               d.ResepTebusId,
                                               d.ObatId,
                                               o.ObatName, // Menambahkan NamaObat dari tabel MasterObat
                                               d.Qty,
                                               d.Signa,
                                               d.SignaTambahan,
                                               d.HargaObat,
                                               d.IsRacikan,
                                               d.CreateBy,
                                               d.CreateDateTime,
                                           }).ToList(),

                             DaftarRacikan = (from d in _applicationDbContext.ResepTebusDetails
                                              join ra in _applicationDbContext.Racikans
                                              on d.RacikanId equals ra.RacikanId
                                              where d.ResepTebusId == r.ResepTebusId
                                              select new
                                              {
                                                  ra.RacikanId,
                                                  ra.NamaRacikan,
                                                  ra.Keterangan,
                                              }).ToList(),
                         }).OrderByDescending(a => a.CreateDateTime);

            // Sorting
            query = query.OrderByDescending(a => a.CreateDateTime); // Fix: Ensure OrderByDescending is applied to IQueryable

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResepById(Guid id)
        {
            var resep = await _applicationDbContext.ResepTebuss.FirstOrDefaultAsync(r => r.ResepTebusId == id);
            if (resep == null)
                return NotFound(new { message = "Resep tidak ditemukan!" });

            var obatDetails = (from d in _applicationDbContext.ResepTebusDetails
                               join o in _applicationDbContext.Obats // Asumsi nama tabel obat adalah MasterObat
                                   on d.ObatId equals o.ObatId // Asumsi primary key tabel obat adalah ObatId
                               where d.ResepTebusId == id
                               select new
                               {
                                   d.ResepTebusDetailId,
                                   d.ResepTebusId,
                                   d.ObatId,
                                   d.RacikanId,
                                   o.ObatName, // Menambahkan NamaObat dari tabel MasterObat
                                   d.Qty,
                                   d.Signa,
                                   d.SignaTambahan,
                                   d.HargaObat,
                                   d.IsRacikan,
                                   d.CreateBy,
                                   d.CreateDateTime,
                               }).ToListAsync();
            var racikanDetails = (from d in _applicationDbContext.ResepTebusDetails
                                  join ra in _applicationDbContext.Racikans
                                  on d.RacikanId equals ra.RacikanId
                                  where d.ResepTebusId == id
                                  select new
                                  {
                                      ra.RacikanId,
                                      ra.NamaRacikan,
                                      ra.Keterangan,
                                  }).ToListAsync();

            var result = new
            {
                resep.ResepTebusId,
                resep.NamaPenebus,
                resep.AntrianResep,
                resep.StatusPembuatanResep,
                resep.StatusPengambilan,
                resep.IsCancelled,
                resep.IsLunas,
                TanggalPembuatanResepFormatted = resep.TanggalPembuatanResep.HasValue ? resep.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null,
                DetailObatResep = obatDetails,
                DetailRacikan = racikanDetails,
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResepTebusViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // buat antrean untuk resep tebusan
                var today = DateTimeOffset.UtcNow.Date;

                var lastResep = await _applicationDbContext.Reseps
                    .Where(r => r.CreateDateTime.Date == today)
                    .OrderByDescending(r => r.AntrianResep)
                    .FirstOrDefaultAsync();
                int nextAntrian = (lastResep?.AntrianResep ?? 0) + 1;

                // **Buat Data Baru**
                var data = new ResepTebus
                {
                    ResepTebusId = Guid.NewGuid(),
                    NamaPenebus = vm.NamaPenebus,
                    AntrianResep = nextAntrian,
                    StatusPembuatanResep = vm.StatusPembuatanResep,
                    StatusPengambilan = false,
                    IsCancelled = false,
                    IsLunas = false,
                    TanggalPembuatanResep = DateTime.UtcNow,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.ResepTebuss.Add(data);
                if (vm.DaftarObat != null && vm.DaftarObat.Any())
                {
                    var daftarobat = vm.DaftarObat.Select(obat => new ResepTebusDetail
                    {
                        ResepTebusDetailId = Guid.NewGuid(),
                        ResepTebusId = data.ResepTebusId,
                        ObatId = obat.ObatId,
                        Qty = obat.Qty,
                        Signa = obat.Signa,
                        SignaTambahan = obat.SignaTambahan,
                        HargaObat = obat.HargaObat,
                        RacikanId = obat.RacikanId,
                        //TotalHargaObat = obat.HargaObat * (obat.Qty ?? 0), // Menghitung total harga obat
                        //StatusCoverObat = obat.StatusCoverObat,
                        //JenisObat = obat.JenisObat,
                        //RacikanId = obat.RacikanId,
                        IsRacikan = obat.IsRacikan,
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                    }).ToList();

                    _applicationDbContext.ResepTebusDetails.AddRange(daftarobat);
                    // **Pengurangan Stok untuk Obat**
                    foreach (var obat in vm.DaftarObat)
                    {
                        var obatDb = await _applicationDbContext.Obats.FindAsync(obat.ObatId);

                        if (obatDb == null)
                        {
                            return NotFound(new { message = "Obat tidak ditemukan." });
                        }

                        int qty = obat.Qty ?? 0; // Jika Qty adalah null, gunakan 0 sebagai default
                        if (obatDb.Stock <= qty)
                        {
                            return BadRequest(new { message = $"Stok obat {obatDb.ObatName} tidak cukup." });
                        }

                        obatDb.Stock -= qty;

                        // Update stok obat di database
                        _applicationDbContext.Obats.Update(obatDb);
                    }
                }
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}/ResepTebus-is-cancelled")]
        public async Task<IActionResult> UpdateIsFinished(Guid id, [FromBody] IsCancelledResepViewModel request)
        {
            var data = await _applicationDbContext.ResepTebuss.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.IsCancelled = request.IsCancelled;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/ResepTebus-is-taken")]
        public async Task<IActionResult> UpdateStatusAmbilResep(Guid id, [FromBody] StatusPengambilanViewModel request)
        {
            var data = await _applicationDbContext.ResepTebuss.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusPengambilan = request.StatusPengambilan;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/StatusResepTebus")]
        public async Task<IActionResult> UpdateStatusResep(Guid id, [FromBody] StatusResepViewModel request)
        {
            var data = await _applicationDbContext.ResepTebuss.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusPembuatanResep = request.Status.ToString();
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/ResepTebus-is-Lunas")]
        public async Task<IActionResult> UpdateIsLunas(Guid id, [FromBody] IsLunasResepViewModel request)
        {
            var data = await _applicationDbContext.ResepTebuss.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.IsLunas = request.IsLunas;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ResepTebusViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
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

                // **Cari Data**
                var data = await _applicationDbContext.ResepTebuss.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.NamaPenebus = vm.NamaPenebus;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.ResepTebuss.Update(data);

                var dfObatLama = _applicationDbContext.ResepTebusDetails.Where(d => d.ResepTebusId == id).ToList();

                // **Mengembalikan stok obat yang sebelumnya terpakai**
                foreach (var detail in dfObatLama)
                {
                    var obatDb = await _applicationDbContext.Obats.FindAsync(detail.ObatId);
                    if (obatDb != null)
                    {
                        // Mengembalikan stok obat yang sudah terpakai
                        obatDb.Stock += detail.Qty.GetValueOrDefault();

                        _applicationDbContext.Obats.Update(obatDb);
                    }
                }

                if (vm.DaftarObat == null || !vm.DaftarObat.Any())
                {
                    _applicationDbContext.ResepTebusDetails.RemoveRange(dfObatLama);
                }
                else
                {
                    foreach (var obat in vm.DaftarObat)
                    {
                        var existingDetail = dfObatLama.FirstOrDefault(x => x.ObatId == obat.ObatId);

                        if (existingDetail != null)
                        {
                            // **Update existing**
                            existingDetail.Qty = obat.Qty;
                            existingDetail.Signa = obat.Signa;
                            existingDetail.SignaTambahan = obat.SignaTambahan;
                            existingDetail.UpdateBy = userActiveId;
                            existingDetail.UpdateDateTime = DateTimeOffset.UtcNow;

                            _applicationDbContext.ResepTebusDetails.Update(existingDetail);
                        }
                        else
                        {
                            // **Insert new**
                            var newDetail = new ResepTebusDetail
                            {
                                ResepTebusDetailId = Guid.NewGuid(),
                                ResepTebusId = data.ResepTebusId,
                                ObatId = obat.ObatId,
                                Qty = obat.Qty,
                                Signa = obat.Signa,
                                HargaObat = obat.HargaObat,
                                RacikanId = obat.RacikanId,
                                //TotalHargaObat = obat.HargaObat * (obat.Qty ?? 0), // Menghitung total harga obat
                                //StatusCoverObat = obat.StatusCoverObat,
                                SignaTambahan = obat.SignaTambahan,
                                //JenisObat = obat.JenisObat,
                                //RacikanId = obat.RacikanId,
                                IsRacikan = obat.IsRacikan,


                                CreateBy = userActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow,
                            };

                            _applicationDbContext.ResepTebusDetails.Add(newDetail);
                        }

                        // **Kurangi stok obat**
                        var obatDbUpdate = await _applicationDbContext.Obats.FindAsync(obat.ObatId);

                        if (obatDbUpdate == null)
                        {
                            return NotFound(new { message = $"Obat dengan ID {obat.ObatId} tidak ditemukan." });
                        }

                        // Cek jika stok obat cukup
                        if (obatDbUpdate.Stock < obat.Qty)
                        {
                            return BadRequest(new { message = $"Stok obat {obatDbUpdate.ObatName} tidak cukup." });
                        }

                        // **Kurangi stok obat** sesuai dengan jumlah (Qty) yang diresepkan
                        obatDbUpdate.Stock -= obat.Qty.GetValueOrDefault();

                        // Update stok di database
                        _applicationDbContext.Obats.Update(obatDbUpdate);
                    }
                }

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResep(Guid id)
        {
            try
            {
                // ambill data user
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // cari data resep
                var resep = await _applicationDbContext.ResepTebuss.FindAsync(id);
                if (resep == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Hapus DetailResepTebus terkait
                var detailReseps = _applicationDbContext.ResepTebusDetails.Where(dr => dr.ResepTebusId == id).ToList();
                if (detailReseps.Any())
                {
                    _applicationDbContext.ResepTebusDetails.RemoveRange(detailReseps);
                }

                // Hapus Resep
                _applicationDbContext.ResepTebuss.Remove(resep);
                await _applicationDbContext.SaveChangesAsync();
                return Ok(new { message = "Hapus Data Berhasil || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedResepTebus(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query utama
            var query = from r in _applicationDbContext.ResepTebuss
                        join u in _applicationDbContext.UserActives
                            on r.CreateBy equals u.UserActiveId
                        where r.IsDelete == false // jika ada field IsDelete
                        select new
                        {
                            r.ResepTebusId,
                            r.CreateDateTime,
                            r.CreateBy,
                            CreateByName = u.FullName,
                            r.NamaPenebus,
                            r.AntrianResep,
                            r.StatusPembuatanResep,
                            r.StatusPengambilan,
                            r.IsCancelled,
                            r.IsLunas,
                            TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ? r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null,
                            DaftarObat = (from d in _applicationDbContext.ResepTebusDetails
                                          join o in _applicationDbContext.Obats // Asumsi nama tabel obat adalah MasterObat
                                              on d.ObatId equals o.ObatId // Asumsi primary key tabel obat adalah ObatId
                                          where d.ResepTebusId == r.ResepTebusId
                                          select new
                                          {
                                              d.ResepTebusDetailId,
                                              d.ResepTebusId,
                                              d.ObatId,
                                              o.ObatName, // Menambahkan NamaObat dari tabel MasterObat
                                              d.Qty,
                                              d.Signa,
                                              d.SignaTambahan,
                                              d.HargaObat,
                                              d.IsRacikan,
                                              d.CreateBy,
                                              d.CreateDateTime,
                                          }).ToList(),

                            DaftarRacikan = (from d in _applicationDbContext.ResepTebusDetails
                                             join ra in _applicationDbContext.Racikans
                                             on d.RacikanId equals ra.RacikanId
                                             where d.ResepTebusId == r.ResepTebusId
                                             select new
                                             {
                                                 ra.RacikanId,
                                                 ra.NamaRacikan,
                                                 ra.Keterangan,
                                             }).ToList(),
                        };

            // Search
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    string searchLower = search.ToLower();
            //    query = query.Where(d =>
            //        EF.Functions.ILike(d.KdDokter, $"%{searchLower}%") ||
            //        EF.Functions.ILike(d.NmDokter, $"%{searchLower}%"));
            //}

            // Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(d => d.CreateDateTime >= startUtc && d.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode waktu
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(d => d.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        var weekStart = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(d => d.CreateDateTime.Date >= weekStart && d.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        query = query.Where(d => d.CreateDateTime.Date >= lastWeekStart && d.CreateDateTime.Date <= lastWeekEnd);
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(d => d.CreateDateTime.Month == today.Month && d.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(d => d.CreateDateTime.Month == lastMonth.Month && d.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(d => d.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(d => d.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(d => d.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(d => d.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting
            query = sortDirection?.ToLower() == "desc"
                ? orderBy?.ToLower() switch
                {
                    "createdatetime" => query.OrderByDescending(d => d.CreateDateTime),
                    "createbyname" => query.OrderByDescending(d => d.CreateByName),
                    _ => query.OrderByDescending(d => d.CreateDateTime)
                }
                : orderBy?.ToLower() switch
                {
                    "createdatetime" => query.OrderBy(d => d.CreateDateTime),
                    "createbyname" => query.OrderBy(d => d.CreateByName),
                    _ => query.OrderBy(d => d.CreateDateTime)
                };

            // pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }
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
    }
}
