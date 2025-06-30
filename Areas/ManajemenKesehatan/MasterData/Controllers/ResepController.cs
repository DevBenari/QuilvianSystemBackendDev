using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ResepController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ResepController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ResepController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResepController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResep(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query utama
            var query = (from r in _applicationDbContext.Reseps
                         join u in _applicationDbContext.UserActives
                             on r.CreateBy equals u.UserActiveId
                         where r.IsDelete == false // jika ada field IsDelete
                         select new
                         {
                             ResepId = r.ResepId,
                             KunjunganId = r.KunjunganId,
                             CreateDateTime = r.CreateDateTime,
                             CreateBy = r.CreateBy,
                             r.AntrianRegistrasi,
                             r.AntrianResep,
                             r.AsuransiId,
                             r.NamaAsuransi,
                             r.PasienId,
                             r.NamaPasien,
                             r.PoliklinikId,
                             r.NamaPoliklinik,
                             r.DokterId,
                             r.NamaDokter,
                             r.StatusPembuatanResep,
                             r.StatusPengambilan,
                             r.IsCancelled,
                             r.IsLunas,
                             TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ? r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null,
                             CreateByName = u.FullName,
                             DaftarObat = (from d in _applicationDbContext.DetailReseps
                                           join o in _applicationDbContext.Obats // Asumsi nama tabel obat adalah MasterObat
                                               on d.ObatId equals o.ObatId // Asumsi primary key tabel obat adalah ObatId
                                           where d.ResepId == r.ResepId
                                           select new
                                           {
                                               d.DetailResepId,
                                               d.ResepId,
                                               d.ObatId,
                                               d.IsRacikan,
                                               d.JenisObat,
                                               o.ObatName, // Menambahkan NamaObat dari tabel MasterObat
                                               d.Qty,
                                               d.HargaObat,
                                               d.Signa,
                                               d.SignaTambahan,
                                               d.IsIteratur,
                                               d.JumlahIteratur,
                                               TglMulaiIteratur = d.TglMulaiIteratur.HasValue ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                                               MasaAktifIteratur = d.MasaAktifIteratur.HasValue ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                                               d.JarakPenebusan,
                                               d.StatusCoverObat,
                                               d.CreateBy,
                                               d.CreateDateTime,
                                           }).ToList(),

                             DaftarRacikan = (from d in _applicationDbContext.DetailReseps
                                              join ra in _applicationDbContext.Racikans // Asumsi nama tabel obat adalah MasterObat
                                                  on d.RacikanId equals ra.RacikanId // Asumsi primary key tabel obat adalah ObatId
                                              where d.ResepId == r.ResepId
                                              select new
                                              {
                                                  ra.RacikanId,
                                                  r.ResepId,
                                                  ra.NamaRacikan,
                                                  r.CreateBy,
                                                  r.CreateDateTime
                                              }).ToList()

                         }).OrderByDescending(a => a.CreateDateTime);

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
            var resep = await _applicationDbContext.Reseps.FirstOrDefaultAsync(r => r.ResepId == id);
            if (resep == null)
                return NotFound(new { message = "Resep tidak ditemukan!" });

            var obatDetails = (from d in _applicationDbContext.DetailReseps
                               join o in _applicationDbContext.Obats // Asumsi nama tabel obat adalah MasterObat
                                   on d.ObatId equals o.ObatId // Asumsi primary key tabel obat adalah ObatId
                               where d.ResepId == id
                               select new
                               {
                                   d.DetailResepId,
                                   d.ResepId,
                                   d.ObatId,
                                   d.IsRacikan,
                                   d.JenisObat,
                                   o.ObatName, // Menambahkan NamaObat dari tabel MasterObat
                                   d.Qty,
                                   d.HargaObat,
                                   d.Signa,
                                   d.SignaTambahan,
                                   d.IsIteratur,
                                   d.JumlahIteratur,
                                   TglMulaiIteratur = d.TglMulaiIteratur.HasValue ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                                   MasaAktifIteratur = d.MasaAktifIteratur.HasValue ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                                   d.JarakPenebusan,
                                   d.StatusCoverObat,
                                   d.CreateBy,
                                   d.CreateDateTime,
                               }).ToListAsync();

            var racikanDetails = (from d in _applicationDbContext.DetailReseps
                                  join ra in _applicationDbContext.Racikans // Asumsi nama tabel obat adalah MasterObat
                                      on d.RacikanId equals ra.RacikanId // Asumsi primary key tabel obat adalah ObatId
                                  where d.ResepId == id
                                  select new
                                  {
                                      ra.RacikanId,
                                      d.ResepId,
                                      ra.NamaRacikan,
                                      d.CreateBy,
                                      d.CreateDateTime
                                  }).ToListAsync();

            var result = new
            {
                ResepId = resep.ResepId,
                KunjunganId = resep.KunjunganId,
                resep.AsuransiId,
                resep.NamaAsuransi,
                resep.PasienId,
                resep.NamaPasien,
                resep.PoliklinikId,
                resep.NamaPoliklinik,
                resep.DokterId,
                resep.NamaDokter,
                resep.AntrianResep,
                resep.AntrianRegistrasi,
                resep.StatusPembuatanResep,
                resep.StatusPengambilan,
                resep.IsCancelled,
                resep.IsLunas,
                TanggalPembuatanResepFormatted = resep.TanggalPembuatanResep.HasValue ? resep.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null,
                DetailObatResep = obatDetails,
                DetailRacikanResep = racikanDetails,
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateResep([FromBody] ResepViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid!" });

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

                // get nomor antrian kunjungan
                var kunjungan = await _applicationDbContext.Kunjungans
                            .Where(k => k.KunjunganID == vm.KunjunganId)
                            .FirstOrDefaultAsync();
                if (kunjungan == null)
                {
                    return NotFound(new { message = "Data antrian kunjungan tidak ditemukan." });
                }
                string antrian = kunjungan.Antrian;

                // Buat nomor antrean resep
                var today = DateTime.UtcNow.Date;

                var lastResep = await _applicationDbContext.Reseps
                    .Where(r => r.CreateDateTime.Date == today)
                    .OrderByDescending(r => r.AntrianResep)
                    .FirstOrDefaultAsync();

                int nextAntrian = (lastResep?.AntrianResep ?? 0) + 1;

                var resep = new Resep
                {
                    ResepId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    AsuransiId = vm.AsuransiId,
                    NamaAsuransi = vm.NamaAsuransi,
                    PasienId = vm.PasienId,
                    NamaPasien = vm.NamaPasien,
                    PoliklinikId = vm.PoliklinikId,
                    NamaPoliklinik = vm.NamaPoliklinik,
                    DokterId = vm.DokterId,
                    NamaDokter = vm.NamaDokter,
                    AntrianResep = nextAntrian,
                    AntrianRegistrasi = antrian,
                    StatusPembuatanResep = vm.StatusPembuatanResep,
                    StatusPengambilan = false, // Jika StatusPengambilan adalah null, gunakan false sebagai default
                    IsCancelled =  false, // Jika IsCanceled adalah null, gunakan false sebagai default
                    IsLunas = false,
                    TanggalPembuatanResep = DateTime.UtcNow, // Jika TanggalPembuatanResep adalah null, gunakan tanggal saat ini
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                _applicationDbContext.Reseps.Add(resep);

                if (vm.DaftarObat != null && vm.DaftarObat.Any())
                {
                    var daftarobat = vm.DaftarObat.Select(obat =>
                    {
                        DateTime? tglIteratur = null;
                        if (!string.IsNullOrWhiteSpace(obat.TglMulaiIteratur))
                        {
                            if (!DateTime.TryParseExact(obat.TglMulaiIteratur, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal, out var parsedDate))
                            {
                                throw new Exception($"Format TglMulaiIteratur tidak valid untuk. Gunakan format yyyy-MM-dd.");
                            }
                            tglIteratur = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                        }

                        DateTime? masaAktifIteratur = null;
                        if (!string.IsNullOrWhiteSpace(obat.MasaAktifIteratur))
                        {
                            if (!DateTime.TryParseExact(obat.MasaAktifIteratur, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal, out var parsedDate))
                            {
                                throw new Exception($"Format MasaAktifIteratur tidak valid. Gunakan format yyyy-MM-dd.");
                            }
                            masaAktifIteratur = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                        }

                        return new ResepDetail
                        {
                            DetailResepId = Guid.NewGuid(),
                            ResepId = resep.ResepId,
                            ObatId = obat.ObatId,
                            Qty = obat.Qty,
                            Signa = obat.Signa,
                            SignaTambahan = obat.SignaTambahan,
                            HargaObat = obat.HargaObat,
                            TotalHargaObat = obat.HargaObat * (obat.Qty ?? 0),
                            StatusCoverObat = obat.StatusCoverObat,
                            JenisObat = obat.JenisObat,
                            RacikanId = obat.RacikanId,
                            IsRacikan = obat.IsRacikan,
                            IsIteratur = obat.IsIteratur,
                            JumlahIteratur = obat.JumlahIteratur,
                            TglMulaiIteratur = tglIteratur,
                            JarakPenebusan = obat.JarakPenebusan,
                            MasaAktifIteratur = masaAktifIteratur,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                        };
                    }).ToList();


                    _applicationDbContext.DetailReseps.AddRange(daftarobat);

                    // Hitung jumlah billing sebelumnya untuk kunjungan ini
                    int billingStart = await _applicationDbContext.Billings
                        .Where(b => b.KunjunganId == vm.KunjunganId )
                        .CountAsync();
                    int billingIndex = billingStart + 1;

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

                        // buat BillingKode untuk setiap obat
                        string billingKode = $"OB{billingIndex.ToString("D3")}";
                        billingIndex++;

                        // Tambahkan satu Billing per ObatId
                        var billing = new Billing
                        {
                            KunjunganId = vm.KunjunganId,
                            DiskonId = vm.DiskonId,
                            BillingDate = DateTime.UtcNow,
                            BillingKode = billingKode,
                            ItemId = obat.ObatId,
                            NamaItem = obatDb.ObatName,
                            HargaItem = obat.HargaObat,
                            QtyItem = qty,
                            SubTotalItem = obat.HargaObat * qty,
                            Keterangan = obat.SignaTambahan, // atau sesuaikan tipe dan nilainya
                        };
                        _applicationDbContext.Billings.Add(billing);
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

        [HttpPut("{id}/is-cancelled")]
        public async Task<IActionResult> UpdateIsFinished(Guid id, [FromBody] IsCancelledResepViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
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

        [HttpPut("{id}/is-taken")]
        public async Task<IActionResult> UpdateStatusAmbilResep(Guid id, [FromBody] StatusPengambilanViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
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

        [HttpPut("{id}/StatusResep")]
        public async Task<IActionResult> UpdateStatusResep(Guid id, [FromBody] StatusResepViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
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

        [HttpPut("{id}/Resep-is-Lunas")]
        public async Task<IActionResult> UpdateIsLunas(Guid id, [FromBody] IsLunasResepViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
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
        public async Task<IActionResult> UpdateResep(Guid id, [FromBody] ResepViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid!" });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);

                if (getUserActive == null || string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                var data = await _applicationDbContext.Reseps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                data.KunjunganId = vm.KunjunganId;
                data.AsuransiId = vm.AsuransiId;
                data.NamaAsuransi = vm.NamaAsuransi;
                data.PasienId = vm.PasienId;
                data.NamaPasien = vm.NamaPasien;
                data.PoliklinikId = vm.PoliklinikId;
                data.NamaPoliklinik = vm.NamaPoliklinik;
                data.DokterId = vm.DokterId;
                data.NamaDokter = vm.NamaDokter;
                data.StatusPembuatanResep = vm.StatusPembuatanResep;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Reseps.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                var dfObatLama = _applicationDbContext.DetailReseps.Where(d => d.ResepId == id).ToList();

                foreach (var detail in dfObatLama)
                {
                    var obatDb = await _applicationDbContext.Obats.FindAsync(detail.ObatId);
                    if (obatDb != null)
                    {
                        obatDb.Stock += detail.Qty.GetValueOrDefault();
                        _applicationDbContext.Obats.Update(obatDb);
                    }
                }

                if (vm.DaftarObat == null || !vm.DaftarObat.Any())
                {
                    _applicationDbContext.DetailReseps.RemoveRange(dfObatLama);
                }
                else
                {
                    int billingStart = await _applicationDbContext.Billings
                        .Where(b => b.KunjunganId == vm.KunjunganId )
                        .CountAsync();
                    int billingIndex = billingStart + 1; // Start from the next available billing index

                    foreach (var obat in vm.DaftarObat)
                    {
                        var existingDetail = dfObatLama.FirstOrDefault(x => x.ObatId == obat.ObatId);

                        DateTime? tglIteratur = null;
                        if (!string.IsNullOrWhiteSpace(obat.TglMulaiIteratur))
                        {
                            if (!DateTime.TryParseExact(obat.TglMulaiIteratur, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal, out var parsedDate))
                            {
                                return BadRequest(new { message = $"Format TglMulaiIteratur tidak valid untuk obat {obat.ObatId}. Gunakan format yyyy-MM-dd." });
                            }
                            tglIteratur = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                        }

                        DateTime? masaAktifIteratur = null;
                        if (!string.IsNullOrWhiteSpace(obat.MasaAktifIteratur))
                        {
                            if (!DateTime.TryParseExact(obat.MasaAktifIteratur, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal, out var parsedDate))
                            {
                                return BadRequest(new { message = $"Format MasaAktifIteratur tidak valid untuk obat {obat.ObatId}. Gunakan format yyyy-MM-dd." });
                            }
                            masaAktifIteratur = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                        }

                        if (existingDetail != null)
                        {
                            existingDetail.Qty = obat.Qty;
                            existingDetail.Signa = obat.Signa;
                            existingDetail.SignaTambahan = obat.SignaTambahan;
                            existingDetail.UpdateBy = userActiveId;
                            existingDetail.UpdateDateTime = DateTimeOffset.UtcNow;
                            existingDetail.TglMulaiIteratur = tglIteratur;
                            existingDetail.MasaAktifIteratur = masaAktifIteratur;

                            _applicationDbContext.DetailReseps.Update(existingDetail);
                        }
                        else
                        {
                            var newDetail = new ResepDetail
                            {
                                DetailResepId = Guid.NewGuid(),
                                ResepId = data.ResepId,
                                ObatId = obat.ObatId,
                                Qty = obat.Qty,
                                Signa = obat.Signa,
                                HargaObat = obat.HargaObat,
                                TotalHargaObat = obat.HargaObat * (obat.Qty ?? 0),
                                StatusCoverObat = obat.StatusCoverObat,
                                SignaTambahan = obat.SignaTambahan,
                                JenisObat = obat.JenisObat,
                                RacikanId = obat.RacikanId,
                                IsRacikan = obat.IsRacikan,
                                IsIteratur = obat.IsIteratur,
                                JumlahIteratur = obat.JumlahIteratur,
                                TglMulaiIteratur = tglIteratur,
                                MasaAktifIteratur = masaAktifIteratur,
                                JarakPenebusan = obat.JarakPenebusan,
                                CreateBy = userActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow,
                            };

                            _applicationDbContext.DetailReseps.Add(newDetail);
                        }

                        var obatDbUpdate = await _applicationDbContext.Obats.FindAsync(obat.ObatId);
                        if (obatDbUpdate == null)
                        {
                            return NotFound(new { message = $"Obat dengan ID {obat.ObatId} tidak ditemukan." });
                        }

                        if (obatDbUpdate.Stock < obat.Qty)
                        {
                            return BadRequest(new { message = $"Stok obat {obatDbUpdate.ObatName} tidak cukup." });
                        }

                        obatDbUpdate.Stock -= obat.Qty.GetValueOrDefault();
                        _applicationDbContext.Obats.Update(obatDbUpdate);

                        // cari data billing
                        string billingKode = $"OB{billingIndex.ToString("D3")}";
                        billingIndex++;

                        var existingBilling = await _applicationDbContext.Billings
                            .FirstOrDefaultAsync(b => b.KunjunganId == vm.KunjunganId && b.ItemId == obat.ObatId);

                        if (existingBilling == null)
                        {
                            // Add new Billing if it doesn't exist for this item in this kunjungan
                            var billing = new Billing
                            {
                                KunjunganId = vm.KunjunganId,
                                DiskonId = vm.DiskonId,
                                BillingDate = DateTime.UtcNow,
                                BillingKode = billingKode,
                                ItemId = obat.ObatId,
                                NamaItem = obatDbUpdate.ObatName,
                                HargaItem = obat.HargaObat,
                                QtyItem = obat.Qty,
                                SubTotalItem = obat.HargaObat * obat.Qty,
                                Keterangan = obat.SignaTambahan, // belum fux
                                CreateBy = userActiveId, // Add these if your Billing has them
                                CreateDateTime = DateTimeOffset.UtcNow,
                            };
                            _applicationDbContext.Billings.Add(billing);
                        }
                        else
                        {
                            // Update existing Billing
                            existingBilling.HargaItem = obat.HargaObat;
                            existingBilling.QtyItem = obat.Qty;
                            existingBilling.SubTotalItem = obat.HargaObat * obat.Qty;
                            existingBilling.UpdateBy = userActiveId;
                            existingBilling.DiskonId = vm.DiskonId;
                            existingBilling.UpdateDateTime = DateTimeOffset.UtcNow; // Assuming UpdateDateTime exists in Billing
                            _applicationDbContext.Billings.Update(existingBilling);
                        }

                    }
                }

                int result = await _applicationDbContext.SaveChangesAsync();
                if (result > 0)
                {
                    return Ok(new { message = "Update Resep Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diupdate ke database." });
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
                // Autentikasi user
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // Cari data resep
                var resep = await _applicationDbContext.Reseps
                    .FirstOrDefaultAsync(r => r.ResepId == id && r.IsDelete == false);
                if (resep == null)
                    return NotFound(new { message = "Data resep tidak ditemukan atau sudah dihapus." });

                // Soft delete DetailResep
                var detailReseps = await _applicationDbContext.DetailReseps
                    .Where(dr => dr.ResepId == id && dr.IsDelete == false)
                    .ToListAsync();

                foreach (var detail in detailReseps)
                {
                    detail.IsDelete = true;
                    detail.DeleteBy = userActiveId;
                    detail.DeleteDateTime = DateTimeOffset.UtcNow;
                }

                // Soft delete Billing terkait kunjungan
                var billings = await _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == resep.KunjunganId && b.IsDelete == false)
                    .ToListAsync();

                foreach (var billing in billings)
                {
                    billing.IsDelete = true;
                    billing.DeleteBy = userActiveId;
                    billing.DeleteDateTime = DateTimeOffset.UtcNow;
                }

                // Soft delete Resep
                resep.IsDelete = true;
                resep.DeleteBy = userActiveId;
                resep.DeleteDateTime = DateTimeOffset.UtcNow;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus secara soft delete || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }


        [HttpGet("paged")]
        public IActionResult PagedResep(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null,
            [FromQuery] bool? IsLunas = null,
            [FromQuery] bool? StatusPengambilan = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Ambil data dari Dokters yang belum dihapus
            // Query utama
            var query = (from r in _applicationDbContext.Reseps
                         join u in _applicationDbContext.UserActives
                             on r.CreateBy equals u.UserActiveId
                         where r.IsDelete == false // jika ada field IsDelete
                         select new
                         {
                             ResepId = r.ResepId,
                             KunjunganId = r.KunjunganId,
                             CreateDateTime = r.CreateDateTime,
                             CreateBy = r.CreateBy,
                             r.AntrianRegistrasi,
                             r.AntrianResep,
                             r.AsuransiId,
                             r.NamaAsuransi,
                             r.PasienId,
                             r.NamaPasien,
                             r.PoliklinikId,
                             r.NamaPoliklinik,
                             r.DokterId,
                             r.NamaDokter,
                             r.StatusPembuatanResep,
                             r.StatusPengambilan,
                             r.IsCancelled,
                             r.IsLunas,
                             TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ? r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null,
                             CreateByName = u.FullName,
                             DaftarObat = (from d in _applicationDbContext.DetailReseps
                                           join o in _applicationDbContext.Obats // Asumsi nama tabel obat adalah MasterObat
                                               on d.ObatId equals o.ObatId // Asumsi primary key tabel obat adalah ObatId
                                           where d.ResepId == r.ResepId
                                           select new
                                           {
                                               d.DetailResepId,
                                               d.ResepId,
                                               d.ObatId,
                                               d.IsRacikan,
                                               d.JenisObat,
                                               o.ObatName, // Menambahkan NamaObat dari tabel MasterObat
                                               d.Qty,
                                               d.HargaObat,
                                               d.Signa,
                                               d.SignaTambahan,
                                               d.IsIteratur,
                                               d.JumlahIteratur,
                                               d.JarakPenebusan,
                                               TglMulaiIteratur = d.TglMulaiIteratur.HasValue ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                                               MasaAktifIteratur = d.MasaAktifIteratur.HasValue ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                                               d.StatusCoverObat,
                                               d.CreateBy,
                                               d.CreateDateTime,
                                           }).ToList(),

                            DaftarRacikan = (from d in _applicationDbContext.DetailReseps
                                             join ra in _applicationDbContext.Racikans 
                                                 on d.RacikanId equals ra.RacikanId 
                                             where d.ResepId == r.ResepId
                                             select new
                                             {
                                                 ra.RacikanId,
                                                 r.ResepId,
                                                 ra.NamaRacikan,
                                                 r.CreateBy,
                                                 r.CreateDateTime
                                             }).ToList()
                         });

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

            if (IsLunas.HasValue)
            {
                query = query.Where(u => u.IsLunas == IsLunas.Value);
            }

            if (StatusPengambilan.HasValue)
            {
                query = query.Where(u => u.StatusPengambilan == StatusPengambilan.Value);
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

        [HttpGet("pagedResepNotLunas")]
        public IActionResult PagedResepNotLunas(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null,
            [FromQuery] bool? StatusPengambilan = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Ambil data dari Dokters yang belum dihapus
            // Query utama
            var query = (from r in _applicationDbContext.Reseps
                         join u in _applicationDbContext.UserActives
                             on r.CreateBy equals u.UserActiveId
                         where r.IsDelete == false && r.IsLunas==false // jika ada field IsDelete
                         select new
                         {
                             ResepId = r.ResepId,
                             KunjunganId = r.KunjunganId,
                             CreateDateTime = r.CreateDateTime,
                             CreateBy = r.CreateBy,
                             r.AntrianRegistrasi,
                             r.AntrianResep,
                             r.AsuransiId,
                             r.NamaAsuransi,
                             r.PasienId,
                             r.NamaPasien,
                             r.PoliklinikId,
                             r.NamaPoliklinik,
                             r.DokterId,
                             r.NamaDokter,
                             r.StatusPembuatanResep,
                             r.StatusPengambilan,
                             r.IsCancelled,
                             r.IsLunas,
                             TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ? r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null,
                             CreateByName = u.FullName,
                             DaftarObat = (from d in _applicationDbContext.DetailReseps
                                           join o in _applicationDbContext.Obats // Asumsi nama tabel obat adalah MasterObat
                                               on d.ObatId equals o.ObatId // Asumsi primary key tabel obat adalah ObatId
                                           where d.ResepId == r.ResepId
                                           select new
                                           {
                                               d.DetailResepId,
                                               d.ResepId,
                                               d.ObatId,
                                               d.IsRacikan,
                                               d.JenisObat,
                                               o.ObatName, // Menambahkan NamaObat dari tabel MasterObat
                                               d.Qty,
                                               d.HargaObat,
                                               d.Signa,
                                               d.SignaTambahan,
                                               d.IsIteratur,
                                               d.JumlahIteratur,
                                               d.TglMulaiIteratur,
                                               d.JarakPenebusan,
                                               d.MasaAktifIteratur,
                                               d.StatusCoverObat,
                                               d.CreateBy,
                                               d.CreateDateTime,
                                           }).ToList(),

                             DaftarRacikan = (from d in _applicationDbContext.DetailReseps
                                              join ra in _applicationDbContext.Racikans
                                                  on d.RacikanId equals ra.RacikanId
                                              where d.ResepId == r.ResepId
                                              select new
                                              {
                                                  ra.RacikanId,
                                                  r.ResepId,
                                                  ra.NamaRacikan,
                                                  r.CreateBy,
                                                  r.CreateDateTime
                                              }).ToList()
                         });

            //Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(d =>
                    EF.Functions.ILike(d.NamaPasien, $"%{searchLower}%"));
            }

            // Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(d => d.CreateDateTime >= startUtc && d.CreateDateTime <= endUtc);
            }

            if (StatusPengambilan.HasValue)
            {
                query = query.Where(u => u.StatusPengambilan == StatusPengambilan.Value);
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
                    "NamaPasien" => query.OrderByDescending(d => d.NamaPasien),
                    _ => query.OrderByDescending(d => d.CreateDateTime)
                }
                : orderBy?.ToLower() switch
                {
                    "createdatetime" => query.OrderBy(d => d.CreateDateTime),
                    "createbyname" => query.OrderBy(d => d.CreateByName),
                    "NamaPasien" => query.OrderBy(d => d.NamaPasien),
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
