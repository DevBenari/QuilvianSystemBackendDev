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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
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
                             r.ResepId,
                             r.KunjunganId,
                             r.CreateDateTime,
                             r.CreateBy,
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
                             r.StatusPengambilanResep,
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
                                               d.StatusPengambilanObat,
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
                                   d.TakaranDosis,
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
                                      d.DosisRacikan,
                                      d.KeteranganRacikan,
                                      d.CreateBy,
                                      d.CreateDateTime
                                  }).ToListAsync();

            var result = new
            {
                resep.ResepId,
                resep.KunjunganId,
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
                resep.StatusPengambilanResep,
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
                    StatusPengambilanResep = false,
                    IsCancelled = false, // Jika IsCanceled adalah null, gunakan false sebagai default
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
                            DosisRacikan = obat.DosisRacikan,
                            IsIteratur = obat.IsIteratur,
                            JumlahIteratur = obat.JumlahIteratur,
                            TglMulaiIteratur = tglIteratur,
                            JarakPenebusan = obat.JarakPenebusan,
                            MasaAktifIteratur = masaAktifIteratur,
                            KeteranganRacikan = obat.KeteranganRacikan,
                            StatusPengambilanObat = false, // Default value
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                        };
                    }).ToList();


                    _applicationDbContext.DetailReseps.AddRange(daftarobat);

                    // Hitung jumlah billing OBAT sebelumnya
                    int billingObatCount = await _applicationDbContext.Billings
                        .Where(b => b.KunjunganId == vm.KunjunganId && b.BillingKode.ToLower()=="obat")
                        .CountAsync();
                    int billingIndex = billingObatCount;

                    // **Pengurangan Stok untuk Obat**
                    foreach (var obat in vm.DaftarObat)
                    {
                        // Validasi ID wajib
                        if (obat.IsRacikan ==  true && obat.RacikanId == null)
                            return BadRequest(new { message = "RacikanId tidak boleh kosong untuk racikan." });

                        if (obat.IsRacikan == false && obat.ObatId == null)
                            return BadRequest(new { message = "ObatId tidak boleh kosong untuk non-racikan." });

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

                        decimal? hargaItem;
                        string? namaItem;
                        // cek apakah obat ini racikan atau tidak dan hitung harganya
                        if (obat.IsRacikan == true )
                        {
                            // Ambil nama racikan dari database jika belum tersedia
                            var namaRacikan = await _applicationDbContext.Racikans
                                .Where(r => r.RacikanId == obat.RacikanId)
                                .Select(r => r.NamaRacikan)
                                .FirstOrDefaultAsync();

                            var totalDosisRacikan = (vm.Dosis * (obat.Qty ?? 0)) / obatDb.TakaranDosis; 
                            var hargaRacikan = (obat.HargaObat * totalDosisRacikan);

                            hargaItem = hargaRacikan;
                            namaItem = namaRacikan ?? "Racikan";

                        }
                        else
                        {
                            namaItem = obatDb.ObatName;
                            hargaItem = obat.HargaObat;
                        }

                        // hitung subtotal
                        var subTotal = obat.IsRacikan == true ? hargaItem : hargaItem * obat.Qty;

                        // increment billoing kode untuk setiap obat dan racikan
                        billingIndex++;
                        string billingKode = $"{billingIndex.ToString("D3")}";

                        // Tambahkan satu Billing per ObatId atau RacikanId
                        var billing = new Billing
                        {
                            KunjunganId = vm.KunjunganId,
                            DiskonId = vm.DiskonId,
                            BillingDate = DateTime.UtcNow,
                            BillingKode = billingKode,
                            ItemId = obat.IsRacikan == true ? obat.RacikanId : obat.ObatId,
                            NamaItem = namaItem,
                            HargaItem = hargaItem,
                            QtyItem = qty,
                            SubTotalItem = subTotal,
                            Keterangan = obat.SignaTambahan, // atau sesuaikan tipe dan nilainya
                            JenisBilling = "Obat",
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
        public async Task<IActionResult> UpdateStatusAmbilResep(Guid id, [FromBody] StatusPengambilanResepViewModel request)
        {
            var data = await _applicationDbContext.Reseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusPengambilanResep = request.StatusPengambilan;
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
                return BadRequest(new { message = "Data tidak valid!" });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                var kunjungan = await _applicationDbContext.Kunjungans
                    .Where(k => k.KunjunganID == vm.KunjunganId)
                    .FirstOrDefaultAsync();
                if (kunjungan == null)
                    return NotFound(new { message = "Data antrian kunjungan tidak ditemukan." });

                string antrian = kunjungan.Antrian;
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
                    StatusPengambilanResep = false,
                    IsCancelled = false,
                    IsLunas = false,
                    TanggalPembuatanResep = DateTime.UtcNow,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                _applicationDbContext.Reseps.Add(resep);

                if (vm.DaftarObat != null && vm.DaftarObat.Any())
                {
                    int billingObatCount = await _applicationDbContext.Billings
                        .Where(b => b.KunjunganId == vm.KunjunganId && b.JenisBilling.ToLower() == "obat")
                        .CountAsync();
                    int billingIndex = billingObatCount;

                    foreach (var obat in vm.DaftarObat)
                    {
                        if (obat.IsRacikan == true && obat.RacikanId == null)
                            return BadRequest(new { message = "RacikanId tidak boleh kosong untuk racikan." });
                        if (obat.IsRacikan == false && obat.ObatId == null)
                            return BadRequest(new { message = "ObatId tidak boleh kosong untuk non-racikan." });

                        var obatDb = await _applicationDbContext.Obats.FindAsync(obat.ObatId);
                        if (obatDb == null)
                            return NotFound(new { message = "Obat tidak ditemukan." });

                        int qty = obat.Qty ?? 0;
                        if (obatDb.Stock <= qty)
                            return BadRequest(new { message = $"Stok obat {obatDb.ObatName} tidak cukup." });

                        obatDb.Stock -= qty;
                        _applicationDbContext.Obats.Update(obatDb);

                        DateTime? tglIteratur = null, masaAktifIteratur = null;
                        if (!string.IsNullOrWhiteSpace(obat.TglMulaiIteratur) &&
                            DateTime.TryParseExact(obat.TglMulaiIteratur, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal, out var parsed1))
                            tglIteratur = DateTime.SpecifyKind(parsed1, DateTimeKind.Utc);

                        if (!string.IsNullOrWhiteSpace(obat.MasaAktifIteratur) &&
                            DateTime.TryParseExact(obat.MasaAktifIteratur, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal, out var parsed2))
                            masaAktifIteratur = DateTime.SpecifyKind(parsed2, DateTimeKind.Utc);

                        var resepDetail = new ResepDetail
                        {
                            DetailResepId = Guid.NewGuid(),
                            ResepId = resep.ResepId,
                            ObatId = obat.ObatId,
                            Qty = qty,
                            Signa = obat.Signa,
                            SignaTambahan = obat.SignaTambahan,
                            HargaObat = obat.HargaObat,
                            TotalHargaObat = obat.HargaObat * qty,
                            StatusCoverObat = obat.StatusCoverObat,
                            JenisObat = obat.JenisObat,
                            RacikanId = obat.RacikanId,
                            IsRacikan = obat.IsRacikan,
                            DosisRacikan = obat.DosisRacikan,
                            TakaranDosis = obatDb.TakaranDosis, // <-- DITAMBAHKAN DISINI
                            IsIteratur = obat.IsIteratur,
                            JumlahIteratur = obat.JumlahIteratur,
                            TglMulaiIteratur = tglIteratur,
                            MasaAktifIteratur = masaAktifIteratur,
                            KeteranganRacikan = obat.KeteranganRacikan,
                            StatusPengambilanObat = false,
                            JarakPenebusan = obat.JarakPenebusan,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };
                        _applicationDbContext.DetailReseps.Add(resepDetail);

                        billingIndex++;
                        string billingKode = billingIndex.ToString("D3");

                        decimal? hargaItem;
                        string namaItem;

                        if (obat.IsRacikan == true)
                        {
                            var namaRacikan = await _applicationDbContext.Racikans
                                .Where(r => r.RacikanId == obat.RacikanId)
                                .Select(r => r.NamaRacikan)
                                .FirstOrDefaultAsync();

                            var totalDosisRacikan = (vm.Dosis * qty) / obatDb.TakaranDosis;
                            var hargaRacikan = obat.HargaObat * totalDosisRacikan;

                            hargaItem = hargaRacikan;
                            namaItem = namaRacikan ?? "Racikan";
                        }
                        else
                        {
                            hargaItem = obat.HargaObat;
                            namaItem = obatDb.ObatName;
                        }

                        var subTotal = obat.IsRacikan == true ? hargaItem : hargaItem * qty;

                        var billing = new Billing
                        {
                            KunjunganId = vm.KunjunganId,
                            DiskonId = vm.DiskonId,
                            BillingDate = DateTime.UtcNow,
                            BillingKode = billingKode,
                            ItemId = obat.IsRacikan == true ? obat.RacikanId : obat.ObatId,
                            NamaItem = namaItem,
                            HargaItem = hargaItem,
                            QtyItem = qty,
                            SubTotalItem = subTotal,
                            Keterangan = obat.SignaTambahan,
                            JenisBilling = "Obat",
                        };
                        _applicationDbContext.Billings.Add(billing);
                    }
                }

                int result = await _applicationDbContext.SaveChangesAsync();
                if (result > 0)
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                else
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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
        [FromQuery] bool? StatusPengambilanResep = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query dasar
            var query = _applicationDbContext.Reseps
                .Where(r => !r.IsDelete)
                .Join(_applicationDbContext.UserActives,
                      r => r.CreateBy,
                      u => u.UserActiveId,
                      (r, u) => new { Resep = r, User = u });

            // Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(q => q.Resep.CreateDateTime >= startUtc && q.Resep.CreateDateTime <= endUtc);
            }

            // Filter tambahan
            if (IsLunas.HasValue)
                query = query.Where(q => q.Resep.IsLunas == IsLunas.Value);

            // Filter StatusPengambilan (optional)
            if (StatusPengambilanResep.HasValue)
                query = query.Where
                    (q => q.Resep.StatusPengambilanResep != null &&
                    q.Resep.StatusPengambilanResep.ToString() == StatusPengambilanResep.Value.ToString());

            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(q => q.Resep.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        var startWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(q => q.Resep.CreateDateTime.Date >= startWeek && q.Resep.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        query = query.Where(q => q.Resep.CreateDateTime.Date >= lastWeekStart && q.Resep.CreateDateTime.Date <= lastWeekEnd);
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(q => q.Resep.CreateDateTime.Month == today.Month && q.Resep.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(q => q.Resep.CreateDateTime.Month == lastMonth.Month && q.Resep.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(q => q.Resep.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(q => q.Resep.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(q => q.Resep.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(q => q.Resep.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting
            query = sortDirection?.ToLower() == "desc"
                ? orderBy?.ToLower() switch
                {
                    "createbyname" => query.OrderByDescending(q => q.User.FullName),
                    "createdatetime" => query.OrderByDescending(q => q.Resep.CreateDateTime),
                    _ => query.OrderByDescending(q => q.Resep.CreateDateTime)
                }
                : orderBy?.ToLower() switch
                {
                    "createbyname" => query.OrderBy(q => q.User.FullName),
                    "createdatetime" => query.OrderBy(q => q.Resep.CreateDateTime),
                    _ => query.OrderBy(q => q.Resep.CreateDateTime)
                };

            // Hitung total & pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "No data found",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            if (page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            // Ambil data page tertentu dan lakukan projection
            var rows = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList()
                .Select(q => new
                {
                    q.Resep.ResepId,
                    q.Resep.KunjunganId,
                    q.Resep.CreateDateTime,
                    q.Resep.CreateBy,
                    q.Resep.AntrianRegistrasi,
                    q.Resep.AntrianResep,
                    q.Resep.AsuransiId,
                    q.Resep.NamaAsuransi,
                    q.Resep.PasienId,
                    q.Resep.NamaPasien,
                    q.Resep.PoliklinikId,
                    q.Resep.NamaPoliklinik,
                    q.Resep.DokterId,
                    q.Resep.NamaDokter,
                    q.Resep.StatusPembuatanResep,
                    q.Resep.StatusPengambilanResep,
                    q.Resep.IsCancelled,
                    q.Resep.IsLunas,
                    TanggalPembuatanResep = q.Resep.TanggalPembuatanResep?.ToString("yyyy-MM-dd"),
                    CreateByName = q.User.FullName,

                    DaftarObat = _applicationDbContext.DetailReseps
                        .Where(d => d.ResepId == q.Resep.ResepId)
                        .Join(_applicationDbContext.Obats,
                              d => d.ObatId,
                              o => o.ObatId,
                              (d, o) => new
                              {
                                  d.DetailResepId,
                                  d.ResepId,
                                  d.ObatId,
                                  d.IsRacikan,
                                  d.JenisObat,
                                  o.ObatName,
                                  d.Qty,
                                  d.HargaObat,
                                  d.Signa,
                                  d.SignaTambahan,
                                  d.IsIteratur,
                                  d.JumlahIteratur,
                                  d.JarakPenebusan,
                                  TglMulaiIteratur = d.TglMulaiIteratur,
                                  MasaAktifIteratur = d.MasaAktifIteratur,
                                  d.TakaranDosis,
                                  d.StatusCoverObat,
                                  d.StatusPengambilanObat,
                                  d.CreateBy,
                                  d.CreateDateTime
                              })
                        .ToList(),

                    DaftarRacikan = _applicationDbContext.DetailReseps
                        .Where(d => d.ResepId == q.Resep.ResepId && d.RacikanId != null)
                        .Join(_applicationDbContext.Racikans,
                              d => d.RacikanId,
                              ra => ra.RacikanId,
                              (d, ra) => new
                              {
                                  ra.RacikanId,
                                  q.Resep.ResepId,
                                  ra.NamaRacikan,
                                  d.DosisRacikan,
                                  q.Resep.CreateBy,
                                  q.Resep.CreateDateTime
                              })
                        .Distinct()
                        .ToList()
                })
                .ToList();

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
        public IActionResult PagedResepBelumLunas(
        int page = 1,
        int perPage = 10,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] PeriodeFilter? periode = null,
        [FromQuery] bool? StatusPengambilanResep = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Base query: hanya yang IsLunas == false
            var query = _applicationDbContext.Reseps
                .Where(r => !r.IsDelete && r.IsLunas == false)
                .Join(_applicationDbContext.UserActives,
                      r => r.CreateBy,
                      u => u.UserActiveId,
                      (r, u) => new { Resep = r, User = u });

            // Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(q => q.Resep.CreateDateTime >= startUtc && q.Resep.CreateDateTime <= endUtc);
            }

            // Filter StatusPengambilan (optional)
            if (StatusPengambilanResep.HasValue)
                query = query.Where
                    (q => q.Resep.StatusPengambilanResep != null && 
                    q.Resep.StatusPengambilanResep.ToString() == StatusPengambilanResep.Value.ToString());

            // Filter periode
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(q => q.Resep.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        var startWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(q => q.Resep.CreateDateTime.Date >= startWeek && q.Resep.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        query = query.Where(q => q.Resep.CreateDateTime.Date >= lastWeekStart && q.Resep.CreateDateTime.Date <= lastWeekEnd);
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(q => q.Resep.CreateDateTime.Month == today.Month && q.Resep.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(q => q.Resep.CreateDateTime.Month == lastMonth.Month && q.Resep.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(q => q.Resep.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(q => q.Resep.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(q => q.Resep.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(q => q.Resep.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting
            query = sortDirection?.ToLower() == "desc"
                ? orderBy?.ToLower() switch
                {
                    "createbyname" => query.OrderByDescending(q => q.User.FullName),
                    "createdatetime" => query.OrderByDescending(q => q.Resep.CreateDateTime),
                    _ => query.OrderByDescending(q => q.Resep.CreateDateTime)
                }
                : orderBy?.ToLower() switch
                {
                    "createbyname" => query.OrderBy(q => q.User.FullName),
                    "createdatetime" => query.OrderBy(q => q.Resep.CreateDateTime),
                    _ => query.OrderBy(q => q.Resep.CreateDateTime)
                };

            // Paging
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "No data found",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            if (page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            // Projection & hasil
            var rows = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList()
                .Select(q => new
                {
                    q.Resep.ResepId,
                    q.Resep.KunjunganId,
                    q.Resep.CreateDateTime,
                    q.Resep.CreateBy,
                    q.Resep.AntrianRegistrasi,
                    q.Resep.AntrianResep,
                    q.Resep.AsuransiId,
                    q.Resep.NamaAsuransi,
                    q.Resep.PasienId,
                    q.Resep.NamaPasien,
                    q.Resep.PoliklinikId,
                    q.Resep.NamaPoliklinik,
                    q.Resep.DokterId,
                    q.Resep.NamaDokter,
                    q.Resep.StatusPembuatanResep,
                    q.Resep.StatusPengambilanResep,
                    q.Resep.IsCancelled,
                    q.Resep.IsLunas,
                    TanggalPembuatanResepFormatted = q.Resep.TanggalPembuatanResep?.ToString("yyyy-MM-dd"),
                    CreateByName = q.User.FullName,

                    DaftarObat = _applicationDbContext.DetailReseps
                        .Where(d => d.ResepId == q.Resep.ResepId)
                        .Join(_applicationDbContext.Obats,
                              d => d.ObatId,
                              o => o.ObatId,
                              (d, o) => new
                              {
                                  d.DetailResepId,
                                  d.ResepId,
                                  d.ObatId,
                                  d.IsRacikan,
                                  d.JenisObat,
                                  o.ObatName,
                                  d.Qty,
                                  d.HargaObat,
                                  d.Signa,
                                  d.SignaTambahan,
                                  d.IsIteratur,
                                  d.JumlahIteratur,
                                  d.JarakPenebusan,
                                  TglMulaiIteratur = d.TglMulaiIteratur,
                                  MasaAktifIteratur = d.MasaAktifIteratur,
                                  d.StatusCoverObat,
                                  d.StatusPengambilanObat,
                                  d.CreateBy,
                                  d.CreateDateTime
                              })
                        .ToList(),

                    DaftarRacikan = _applicationDbContext.DetailReseps
                        .Where(d => d.ResepId == q.Resep.ResepId && d.RacikanId != null)
                        .Join(_applicationDbContext.Racikans,
                              d => d.RacikanId,
                              ra => ra.RacikanId,
                              (d, ra) => new
                              {
                                  ra.RacikanId,
                                  q.Resep.ResepId,
                                  ra.NamaRacikan,
                                  q.Resep.CreateBy,
                                  q.Resep.CreateDateTime
                              })
                        .Distinct()
                        .ToList()
                })
                .ToList();

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

