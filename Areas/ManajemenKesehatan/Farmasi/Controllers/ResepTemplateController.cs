using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;
using SkiaSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using Microsoft.AspNetCore.SignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ResepTemplateController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ResepController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ResepTemplateController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResepController> logger,
            IWebHostEnvironment webHostEnvironment
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }


        // **View ResepTemplate**
        [HttpGet]
        public async Task<IActionResult> GetResepTemplates(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query untuk mengambil data resep template
            var query = _applicationDbContext.ResepTemplates
                .Where(r => !r.IsDelete)  
                .Select(r => new
                {
                    r.ResepTemplateId,
                    r.KodeResepTemplate,
                    r.Judul,
                    r.Diagnosa,
                    r.Deskripsi,
                    r.DokterId,

                    DaftarObat = ( from d in _applicationDbContext.ResepTemplateDetails
                                   join o in _applicationDbContext.Obats 
                                   on d.ObatId equals o.ObatId
                                   where d.ResepTemplateId == r.ResepTemplateId && (d.IsRacikan == false || d.IsRacikan == null)
                                   select new
                                   {
                                       d.ResepTemplateDetailId,
                                       d.ObatId,
                                       o.ObatName,
                                       d.Qty,
                                       d.TakaranDosis,
                                       d.Signa,
                                       d.SignaTambahan,
                                       d.JenisObat,
                                       d.HargaObat,
                                       d.StatusCoverObat,
                                       d.IsRacikan,
                                       d.RacikanId
                                   }).ToList(),

                    DaftarRacikan = (from d in _applicationDbContext.ResepTemplateDetails
                                     join rck in _applicationDbContext.Racikans
                                     on d.RacikanId equals rck.RacikanId
                                     where d.ResepTemplateId == r.ResepTemplateId && d.IsRacikan == true
                                     select new
                                     {
                                         d.ResepTemplateDetailId,
                                         d.Qty,
                                         d.Signa,
                                         d.SignaTambahan,
                                         d.JenisObat,
                                         d.IsRacikan,
                                         d.RacikanId,
                                         rck.NamaRacikan,
                                         rck.Keterangan,
                                         DaftarRacikan = (from rd in _applicationDbContext.RacikanDetails
                                                          join o in _applicationDbContext.Obats
                                                          on rd.ObatId equals o.ObatId
                                                          where rd.RacikanId == rck.RacikanId
                                                          select new
                                                          {
                                                              rd.DetailRacikanId,
                                                              rd.ObatId,
                                                              o.ObatName,
                                                              rd.KomposisiDosis,
                                                              rd.QtyUsed,
                                                              rd.HargaKomposisi
                                                          }).ToList()
                                     }).ToList(),

                    r.CreateDateTime,
                    r.CreateBy,
                }).OrderByDescending(a => a.CreateDateTime);

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
            var resepTemplate = await _applicationDbContext.ResepTemplates
                .FirstOrDefaultAsync(r => r.ResepTemplateId == id);
            if (resepTemplate == null)
            {
                return NotFound(new { message = "Resep Template tidak ditemukan." });
            }

            var daftarObat = await _applicationDbContext.ResepTemplateDetails
                 .Where(d => d.ResepTemplateId == resepTemplate.ResepTemplateId 
                 && (d.IsRacikan == false || d.IsRacikan == null))
                 .Join(_applicationDbContext.Obats,
                       d => d.ObatId,
                       o => o.ObatId,
                       (d, o) => new
                       {
                           d.ResepTemplateDetailId,
                           d.ObatId,
                           o.ObatName,
                           d.Qty,
                           d.HargaObat,
                           d.Signa,
                           d.SignaTambahan,
                           d.TakaranDosis,
                           d.StatusCoverObat,
                           d.CreateBy,
                           d.CreateDateTime
                       })
                 .ToListAsync();

            var daftarRacikanRaw = await _applicationDbContext.ResepTemplateDetails
                .Where(d => d.ResepTemplateId == resepTemplate.ResepTemplateId 
                        && d.RacikanId != null)
                .Join(_applicationDbContext.Racikans,
                    d => d.RacikanId,
                    ra => ra.RacikanId,
                    (d, ra) => new
                    {
                        ra.RacikanId,
                        ra.NamaRacikan,
                        d.Qty,
                        d.Signa,
                        d.SignaTambahan,
                        ra.CreateBy,
                        ra.CreateDateTime,
                        DaftarRacikanDetail = _applicationDbContext.RacikanDetails
                            .Where(rd => rd.RacikanId == ra.RacikanId)
                            .Join(_applicationDbContext.Obats,
                                  rd => rd.ObatId,
                                  ob => ob.ObatId,
                                  (rd, ob) => new
                                  {
                                      rd.DetailRacikanId,
                                      rd.ObatId,
                                      ob.ObatName,
                                      rd.QtyUsed,
                                      rd.KomposisiDosis,
                                      rd.CreateBy,
                                      rd.CreateDateTime
                                  })
                            .ToList()
                    })
                .ToListAsync(); // materialize dulu

            // Ambil unik racikan per RacikanId
            var daftarRacikan = daftarRacikanRaw
                .GroupBy(r => r.RacikanId)
                .Select(g => g.First())
                .ToList();

            var result = new
            {
                resepTemplate.ResepTemplateId,
                resepTemplate.DokterId,
                resepTemplate.Judul,
                resepTemplate.Diagnosa,
                resepTemplate.Deskripsi,
                resepTemplate.CreateBy,
                DaftarObat = daftarObat,
                DaftarRacikan = daftarRacikan,
            };
            return Ok(result);

        }


        // **Create ResepTemplate**
        [HttpPost]
        public async Task<IActionResult> CreateResepTemplate([FromBody] ResepTemplateViewModel vm)
        {
            if (vm == null)
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
                var dateNow = DateTime.UtcNow.Date;
                var setDateNow = dateNow.ToString("yyMMdd");
                var todayString = dateNow.ToString("yyyyMMdd");

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
                    Judul = vm.Judul,
                    Diagnosa = vm.Diagnosa,
                    Deskripsi = vm.Deskripsi,
                    DokterId = vm.DokterId,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // Insert data baru ke database
                _applicationDbContext.ResepTemplates.Add(resepTemplate);

                // insert data obat dan obat racikan
                if (vm.DaftarObat?.Any() == true)
                {
                    var obatIds = vm.DaftarObat.Where(o => o.ObatId != null).Select(o => o.ObatId.Value).Distinct().ToList();
                    var obatDbList = await _applicationDbContext.Obats
                        .Where(o => obatIds.Contains(o.ObatId))
                        .ToDictionaryAsync(o => o.ObatId);

                    var detailResepDict = new Dictionary<Guid, ResepTemplateDetail>();

                    foreach (var obat in vm.DaftarObat.Where(o => o.IsRacikan != true))
                    {
                        if (obat.ObatId == null) continue;
                        var obatId = obat.ObatId.Value;

                        if (!obatDbList.ContainsKey(obatId))
                            return BadRequest(new { message = $"Obat tidak ditemukan: {obatId}" });

                        var obatDb = obatDbList[obatId];
                        var qtyInput = obat.Qty ?? 0;

                        if (!detailResepDict.TryGetValue(obatId, out var resepDetail))
                        {
                            resepDetail = new ResepTemplateDetail
                            {
                                ResepTemplateDetailId = Guid.NewGuid(),
                                ResepTemplateId = resepTemplate.ResepTemplateId,
                                ObatId = obatId,
                                Qty = qtyInput,
                                Signa = obat.Signa,
                                SignaTambahan = obat.SignaTambahan,
                                HargaObat = obatDb.HTEPrice,
                                StatusCoverObat = obat.StatusCoverObat,
                                JenisObat = obat.JenisObat,
                                IsRacikan = false,
                                RacikanId = null,
                                TakaranDosis = obatDb.TakaranDosis,
                                CreateBy = getUserActive.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            detailResepDict[obatId] = resepDetail;
                            _applicationDbContext.ResepTemplateDetails.Add(resepDetail);
                        }
                        else
                        {
                            resepDetail.Qty += qtyInput;
                        }

                    }

                    // Racikan tetap ditangani seperti biasa
                    foreach (var obat in vm.DaftarObat.Where(o => o.IsRacikan == true))
                    {
                        if (obat.Racikan == null || !obat.Racikan.Any())
                            continue;

                        foreach (var racikan in obat.Racikan)
                        {
                            var racikanId = Guid.NewGuid();
                            int racikanCountToday = await _applicationDbContext.Racikans.CountAsync(r => r.CreateDateTime.Date == dateNow);
                            string kodeRacikan = $"RCK-{(racikanCountToday + 1):D3}{todayString}";

                            // Buat entitas racikan
                            var racikanEntity = new Racikan
                            {
                                RacikanId = racikanId,
                                NamaRacikan = racikan.NamaRacikan,
                                Keterangan = racikan.Keterangan,
                                Signa = racikan.Signa,
                                SignaTambahan = racikan.SignaTambahan,
                                QtyRacikan = obat.Qty ?? 1, // default jika null
                                KodeRacikan = kodeRacikan,
                                CreateBy = getUserActive.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            _applicationDbContext.Racikans.Add(racikanEntity);


                            // Detail racikan (komposisi)
                            foreach (var detailRacikan in racikan.DaftarRacikan)
                            {
                                var obatDbRacikan = await _applicationDbContext.Obats.FindAsync(detailRacikan.ObatId);
                                if (obatDbRacikan == null)
                                    return BadRequest(new { message = $"Obat tidak ditemukan: {detailRacikan.ObatId}" });

                                // Perhitungan jumlah pakai
                                var qtyPakai = Math.Ceiling((decimal)((detailRacikan.KomposisiDosis * racikanEntity.QtyRacikan) / obatDbRacikan.TakaranDosis));
                                var hargaOb = qtyPakai * obatDbRacikan.HTEPrice;


                                // Tambahkan detail racikan
                                var racikanDetail = new RacikanDetail
                                {
                                    DetailRacikanId = Guid.NewGuid(),
                                    RacikanId = racikanId,
                                    ObatId = detailRacikan.ObatId,
                                    QtyUsed = (int)qtyPakai,
                                    KomposisiDosis = detailRacikan.KomposisiDosis,
                                    HargaKomposisi = hargaOb,
                                    CreateBy = getUserActive.UserActiveId,
                                    CreateDateTime = DateTimeOffset.UtcNow
                                };
                                _applicationDbContext.RacikanDetails.Add(racikanDetail);
                            }

                            // Tambahkan ke tabel DetailResep
                            var resepDetail = new ResepTemplateDetail
                            {
                                ResepTemplateDetailId = Guid.NewGuid(),
                                ResepTemplateId = resepTemplate.ResepTemplateId,
                                ObatId = null,
                                Qty = racikanEntity.QtyRacikan,
                                Signa = racikanEntity.Signa,
                                SignaTambahan = racikanEntity.SignaTambahan,
                                StatusCoverObat = false,
                                JenisObat = obat.JenisObat,
                                IsRacikan = true,
                                RacikanId = racikanId,
                                TakaranDosis = null,
                                CreateBy = getUserActive.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            _applicationDbContext.ResepTemplateDetails.Add(resepDetail);
                        }
                    }
                }
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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
            if (vm == null)
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

                // Cari data existing ResepTemplate
                var resepTemplate = await _applicationDbContext.ResepTemplates
                    .FirstOrDefaultAsync(r => r.ResepTemplateId == id);

                if (resepTemplate == null)
                {
                    return NotFound(new { message = "Resep Template tidak ditemukan." });
                }

                // Update field yang diperbolehkan
                resepTemplate.Judul = vm.Judul;
                resepTemplate.Diagnosa = vm.Diagnosa;
                resepTemplate.Deskripsi = vm.Deskripsi;
                resepTemplate.DokterId = vm.DokterId;

                resepTemplate.UpdateBy = userActiveId;
                resepTemplate.UpdateDateTime = DateTimeOffset.UtcNow;

                // Hapus detail lama (baik obat biasa maupun racikan)
                var oldDetails = _applicationDbContext.ResepTemplateDetails
                    .Where(d => d.ResepTemplateId == resepTemplate.ResepTemplateId);
                _applicationDbContext.ResepTemplateDetails.RemoveRange(oldDetails);

                var oldRacikanIds = oldDetails.Where(d => d.RacikanId != null)
                    .Select(d => d.RacikanId.Value)
                    .ToList();

                if (oldRacikanIds.Any())
                {
                    var oldRacikans = _applicationDbContext.Racikans
                        .Where(r => oldRacikanIds.Contains(r.RacikanId));
                    _applicationDbContext.Racikans.RemoveRange(oldRacikans);

                    var oldRacikanDetails = _applicationDbContext.RacikanDetails
                        .Where(rd => oldRacikanIds.Contains((Guid)rd.RacikanId));
                    _applicationDbContext.RacikanDetails.RemoveRange(oldRacikanDetails);
                }

                // Insert ulang detail obat & racikan dari ViewModel
                if (vm.DaftarObat?.Any() == true)
                {
                    var obatIds = vm.DaftarObat.Where(o => o.ObatId != null).Select(o => o.ObatId.Value).Distinct().ToList();
                    var obatDbList = await _applicationDbContext.Obats
                        .Where(o => obatIds.Contains(o.ObatId))
                        .ToDictionaryAsync(o => o.ObatId);

                    foreach (var obat in vm.DaftarObat.Where(o => o.IsRacikan != true))
                    {
                        if (obat.ObatId == null) continue;
                        var obatId = obat.ObatId.Value;

                        if (!obatDbList.ContainsKey(obatId))
                            return BadRequest(new { message = $"Obat tidak ditemukan: {obatId}" });

                        var obatDb = obatDbList[obatId];

                        var resepDetail = new ResepTemplateDetail
                        {
                            ResepTemplateDetailId = Guid.NewGuid(),
                            ResepTemplateId = resepTemplate.ResepTemplateId,
                            ObatId = obatId,
                            Qty = obat.Qty ?? 0,
                            Signa = obat.Signa,
                            SignaTambahan = obat.SignaTambahan,
                            HargaObat = obatDb.HTEPrice,
                            StatusCoverObat = obat.StatusCoverObat,
                            JenisObat = obat.JenisObat,
                            IsRacikan = false,
                            RacikanId = null,
                            TakaranDosis = obatDb.TakaranDosis,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };
                        _applicationDbContext.ResepTemplateDetails.Add(resepDetail);
                    }

                    // Racikan
                    foreach (var obat in vm.DaftarObat.Where(o => o.IsRacikan == true))
                    {
                        if (obat.Racikan == null || !obat.Racikan.Any())
                            continue;

                        foreach (var racikan in obat.Racikan)
                        {
                            var racikanId = Guid.NewGuid();
                            string kodeRacikan = $"RCK-{DateTime.UtcNow:yyyyMMddHHmmss}";

                            var racikanEntity = new Racikan
                            {
                                RacikanId = racikanId,
                                NamaRacikan = racikan.NamaRacikan,
                                Keterangan = racikan.Keterangan,
                                Signa = racikan.Signa,
                                SignaTambahan = racikan.SignaTambahan,
                                QtyRacikan = obat.Qty ?? 1,
                                KodeRacikan = kodeRacikan,
                                CreateBy = userActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            _applicationDbContext.Racikans.Add(racikanEntity);

                            foreach (var detailRacikan in racikan.DaftarRacikan)
                            {
                                var obatDbRacikan = await _applicationDbContext.Obats.FindAsync(detailRacikan.ObatId);
                                if (obatDbRacikan == null)
                                    return BadRequest(new { message = $"Obat tidak ditemukan: {detailRacikan.ObatId}" });

                                var qtyPakai = Math.Ceiling((decimal)((detailRacikan.KomposisiDosis * racikanEntity.QtyRacikan) / obatDbRacikan.TakaranDosis));
                                var hargaOb = qtyPakai * obatDbRacikan.HTEPrice;

                                var racikanDetail = new RacikanDetail
                                {
                                    DetailRacikanId = Guid.NewGuid(),
                                    RacikanId = racikanId,
                                    ObatId = detailRacikan.ObatId,
                                    QtyUsed = (int)qtyPakai,
                                    KomposisiDosis = detailRacikan.KomposisiDosis,
                                    HargaKomposisi = hargaOb,
                                    CreateBy = userActiveId,
                                    CreateDateTime = DateTimeOffset.UtcNow
                                };
                                _applicationDbContext.RacikanDetails.Add(racikanDetail);
                            }

                            var resepDetail = new ResepTemplateDetail
                            {
                                ResepTemplateDetailId = Guid.NewGuid(),
                                ResepTemplateId = resepTemplate.ResepTemplateId,
                                ObatId = null,
                                Qty = racikanEntity.QtyRacikan,
                                Signa = racikanEntity.Signa,
                                SignaTambahan = racikanEntity.SignaTambahan,
                                StatusCoverObat = false,
                                JenisObat = obat.JenisObat,
                                IsRacikan = true,
                                RacikanId = racikanId,
                                TakaranDosis = null,
                                CreateBy = userActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };
                            _applicationDbContext.ResepTemplateDetails.Add(resepDetail);
                        }
                    }
                }

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });

                return StatusCode(500, new { message = "Data tidak berhasil diupdate ke database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("ResepByDokter/{idDokter}")]
        public async Task<IActionResult> GetResepTemplateByDokter(Guid idDokter)
        {
            var resepTemplates = await _applicationDbContext.ResepTemplates
                .Where(r => r.DokterId == idDokter)
                .ToListAsync();

            if (resepTemplates == null || !resepTemplates.Any())
            {
                return NotFound(new { message = "Resep Template untuk dokter ini tidak ditemukan." });
            }

            var result = new List<object>();

            foreach (var resepTemplate in resepTemplates)
            {
                var daftarObat = await _applicationDbContext.ResepTemplateDetails
                    .Where(d => d.ResepTemplateId == resepTemplate.ResepTemplateId
                             && (d.IsRacikan == false || d.IsRacikan == null))
                    .Join(_applicationDbContext.Obats,
                          d => d.ObatId,
                          o => o.ObatId,
                          (d, o) => new
                          {
                              d.ResepTemplateDetailId,
                              d.ObatId,
                              o.ObatName,
                              d.Qty,
                              d.HargaObat,
                              d.Signa,
                              d.SignaTambahan,
                              d.TakaranDosis,
                              d.StatusCoverObat,
                              d.CreateBy,
                              d.CreateDateTime
                          })
                    .ToListAsync();

                var daftarRacikanRaw = await _applicationDbContext.ResepTemplateDetails
                    .Where(d => d.ResepTemplateId == resepTemplate.ResepTemplateId
                             && d.RacikanId != null)
                    .Join(_applicationDbContext.Racikans,
                        d => d.RacikanId,
                        ra => ra.RacikanId,
                        (d, ra) => new
                        {
                            ra.RacikanId,
                            ra.NamaRacikan,
                            d.Qty,
                            d.Signa,
                            d.SignaTambahan,
                            ra.CreateBy,
                            ra.CreateDateTime,
                            DaftarRacikanDetail = _applicationDbContext.RacikanDetails
                                .Where(rd => rd.RacikanId == ra.RacikanId)
                                .Join(_applicationDbContext.Obats,
                                      rd => rd.ObatId,
                                      ob => ob.ObatId,
                                      (rd, ob) => new
                                      {
                                          rd.DetailRacikanId,
                                          rd.ObatId,
                                          ob.ObatName,
                                          rd.QtyUsed,
                                          rd.KomposisiDosis,
                                          rd.CreateBy,
                                          rd.CreateDateTime
                                      })
                                .ToList()
                        })
                    .ToListAsync();

                var daftarRacikan = daftarRacikanRaw
                    .GroupBy(r => r.RacikanId)
                    .Select(g => g.First())
                    .ToList();

                result.Add(new
                {
                    resepTemplate.ResepTemplateId,
                    resepTemplate.DokterId,
                    resepTemplate.Judul,
                    resepTemplate.Diagnosa,
                    resepTemplate.Deskripsi,
                    resepTemplate.CreateBy,
                    DaftarObat = daftarObat,
                    DaftarRacikan = daftarRacikan,
                });
            }

            return Ok(result);
        }

        // deelete resep template
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResepTemplate(Guid id)
        {
            // Mulai transaksi agar konsisten
            using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // Ambil user (untuk audit kolom DeleteBy)
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;
                var deleteTime = DateTimeOffset.UtcNow;

                // Ambil header template
                var resepTemplate = await _applicationDbContext.ResepTemplates
                    .FirstOrDefaultAsync(r => r.ResepTemplateId == id && (r.IsDelete == false || r.IsDelete == null));

                if (resepTemplate == null)
                    return NotFound(new { message = "ResepTemplate tidak ditemukan atau sudah dihapus." });

                // Ambil semua detail template (baik non-racikan maupun racikan)
                var templateDetails = await _applicationDbContext.ResepTemplateDetails
                    .Where(d => d.ResepTemplateId == id && (d.IsDelete == false || d.IsDelete == null))
                    .ToListAsync();

                // Kumpulkan RacikanId dari detail yang racikan
                var racikanIds = templateDetails
                    .Where(d => d.IsRacikan == true && d.RacikanId != null)
                    .Select(d => d.RacikanId!.Value)
                    .Distinct()
                    .ToList();

                // Soft delete RacikanDetails yang terkait
                if (racikanIds.Any())
                {
                    var racikanDetails = await _applicationDbContext.RacikanDetails
                        .Where(rd => racikanIds.Contains((Guid)rd.RacikanId) && (rd.IsDelete == false || rd.IsDelete == null))
                        .ToListAsync();

                    foreach (var rd in racikanDetails)
                    {
                        rd.IsDelete = true;
                        rd.DeleteBy = userActiveId;
                        rd.DeleteDateTime = deleteTime;
                    }

                    // Soft delete Racikan (header)
                    var racikans = await _applicationDbContext.Racikans
                        .Where(r => racikanIds.Contains(r.RacikanId) && (r.IsDelete == false || r.IsDelete == null))
                        .ToListAsync();

                    foreach (var r in racikans)
                    {
                        r.IsDelete = true;
                        r.DeleteBy = userActiveId;
                        r.DeleteDateTime = deleteTime;
                    }
                }

                // Soft delete detail template
                foreach (var d in templateDetails)
                {
                    d.IsDelete = true;
                    d.DeleteBy = userActiveId;
                    d.DeleteDateTime = deleteTime;
                }

                // Soft delete header template
                resepTemplate.IsDelete = true;
                resepTemplate.DeleteBy = userActiveId;
                resepTemplate.DeleteDateTime = deleteTime;

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new { message = "ResepTemplate dihapus (soft delete) beserta detail, racikan, dan komposisinya." });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        // **Get All ResepTemplate (Paged)**
        [HttpGet("paged")]
        public async Task<IActionResult> GetResepTemplatesPaged(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Base query (left join ke UserActives untuk CreateByName)
            var query =
                from r in _applicationDbContext.ResepTemplates
                join ua in _applicationDbContext.UserActives
                    on r.CreateBy equals ua.UserActiveId into uaJoin
                from u in uaJoin.DefaultIfEmpty()
                where r.IsDelete == false || r.IsDelete == null
                select new
                {
                    r.ResepTemplateId,
                    r.KodeResepTemplate,
                    r.Judul,
                    r.Diagnosa,
                    r.Deskripsi,
                    r.DokterId,

                    // Non-racikan
                    DaftarObat = (
                        from d in _applicationDbContext.ResepTemplateDetails
                        join o in _applicationDbContext.Obats on d.ObatId equals o.ObatId
                        where d.ResepTemplateId == r.ResepTemplateId
                              && (d.IsRacikan == false || d.IsRacikan == null)
                              && (d.IsDelete == false || d.IsDelete == null)
                        select new
                        {
                            d.ResepTemplateDetailId,
                            d.ObatId,
                            o.ObatName,
                            d.Qty,
                            d.TakaranDosis,
                            d.Signa,
                            d.SignaTambahan,
                            d.JenisObat,
                            d.HargaObat,
                            d.StatusCoverObat,
                            d.IsRacikan,
                            d.RacikanId
                        }
                    ).ToList(),

                    // Racikan
                    DaftarRacikan = (
                        from d in _applicationDbContext.ResepTemplateDetails
                        join rck in _applicationDbContext.Racikans on d.RacikanId equals rck.RacikanId
                        where d.ResepTemplateId == r.ResepTemplateId
                              && d.IsRacikan == true
                              && (d.IsDelete == false || d.IsDelete == null)
                              && (rck.IsDelete == false || rck.IsDelete == null)
                        select new
                        {
                            d.ResepTemplateDetailId,
                            d.Qty,
                            d.Signa,
                            d.SignaTambahan,
                            d.JenisObat,
                            d.IsRacikan,
                            d.RacikanId,
                            rck.NamaRacikan,
                            rck.Keterangan,
                            DaftarRacikan = (
                                from rd in _applicationDbContext.RacikanDetails
                                join o in _applicationDbContext.Obats on rd.ObatId equals o.ObatId
                                where rd.RacikanId == rck.RacikanId
                                      && (rd.IsDelete == false || rd.IsDelete == null)
                                select new
                                {
                                    rd.DetailRacikanId,
                                    rd.ObatId,
                                    o.ObatName,
                                    rd.KomposisiDosis,
                                    rd.QtyUsed,
                                    rd.HargaKomposisi
                                }
                            ).ToList()
                        }
                    ).ToList(),

                    r.CreateDateTime,
                    r.CreateBy,
                    CreateByName = u.FullName
                };

            // SEARCH (Judul/KodeResepTemplate/Creator name) — mendukung 1 huruf
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = $"%{search.ToLower()}%";
                query = query.Where(x =>
                    EF.Functions.ILike(x.Judul ?? string.Empty, s) ||
                    EF.Functions.ILike(x.KodeResepTemplate ?? string.Empty, s) ||
                    EF.Functions.ILike(x.CreateByName ?? string.Empty, s)
                );
            }

            // FILTER TANGGAL (CreateDateTime) jika start & end disediakan
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc);
            }

            // FILTER PERIODE (opsional)
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            x.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(x =>
                            x.CreateDateTime.Month == lastMonth.Month &&
                            x.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // SORTING aman (fallback ke CreateDateTime)
            query = (sortDirection?.ToLower() == "asc")
                ? (orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    "Judul" => query.OrderBy(x => x.Judul),
                    "KodeResepTemplate" => query.OrderBy(x => x.KodeResepTemplate),
                    _ => query.OrderBy(x => x.CreateDateTime)
                })
                : (orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                    "Judul" => query.OrderByDescending(x => x.Judul),
                    "KodeResepTemplate" => query.OrderByDescending(x => x.KodeResepTemplate),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                });

            // PAGINATION
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
