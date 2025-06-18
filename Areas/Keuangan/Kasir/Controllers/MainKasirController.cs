using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.Keuangan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.Keuangan.Models.Kasir;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.Keuangan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class MainKasirController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<MainKasirController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MainKasirController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<MainKasirController> logger,
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

            // Query data
            var query = (from a in _applicationDbContext.MainKasirs
                         join u in _applicationDbContext.UserActives
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null 
                         select new
                         {
                             CreateDateTime = a.CreateDateTime,
                             CreateBy = a.CreateBy,
                             CreateByName = u.FullName,
                             KasirId = a.KasirId,
                             KunjunganId = a.KunjunganId,
                             BiayaAdministrasiKode = a.BiayaAdministrasiKode,
                             MetodePembayaranId = a.MetodePembayaranId,
                             NamaMetode = a.NamaMetode,
                             DiskonId = a.DiskonId,
                             NominalPembayaran = a.NominalPembayaran,
                             StatusPembayaran = a.StatusPembayaran,
                             Keterangan = a.Keterangan,
                             TglPembayaran = a.TglPembayaran,
                             ReferenceId = a.ReferenceId
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

        [HttpGet("BillingByKunjungan/{kunjunganId}")]
        public async Task<IActionResult> GetKasirData(Guid kunjunganId)
        {
            var query =
                // INNER JOIN Kunjungan dengan PendaftaranPasienBaru
                (from k in _applicationDbContext.Kunjungans
                 join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId

                 // LEFT JOIN Asuransi
                 join a in _applicationDbContext.Asuransis on k.AsuransiId equals a.AsuransiId into asuransiTempGroup
                 from a in asuransiTempGroup.DefaultIfEmpty()

                     // LEFT JOIN AsuransiPasien (pastikan k.PasienId dapat dikonversi ke string jika ap.PasienId string)
                 join ap in _applicationDbContext.AsuransiPasiens on p.PendaftaranPasienBaruId.ToString() equals ap.PasienId into asuransiPasienGroup
                 from ap in asuransiPasienGroup.DefaultIfEmpty()

                     // INNER JOIN Kunjungan ke tabel Dokter dan Poliklinik
                 join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                 join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId

                 // LEFT JOIN Reseps (filter IsDelete di sini)
                 join r in _applicationDbContext.Reseps.Where(resep => !resep.IsDelete) on k.KunjunganID equals r.KunjunganId into resepGroup
                 from r in resepGroup.DefaultIfEmpty() // Penting: DefaultIfEmpty untuk LEFT JOIN

                     // LEFT JOIN DetailResep (filter IsDelete di sini)
                 join dr in _applicationDbContext.DetailReseps.Where(detail => !detail.IsDelete) on r.ResepId equals dr.ResepId into detailResepGroup
                 from dr in detailResepGroup.DefaultIfEmpty() 

                     // LEFT JOIN Obat
                     // Perhatikan: o.ObatId harus non-null untuk join. Jika dr null, o juga akan null.
                 join o in _applicationDbContext.Obats on dr.ObatId equals o.ObatId into obatGroup
                 from o in obatGroup.DefaultIfEmpty() 

                     // LEFT JOIN TindakanKunjungan
                 join to in _applicationDbContext.TindakanKunjungans on k.KunjunganID equals to.KunjunganId into tindakanGroup
                 from to in tindakanGroup.DefaultIfEmpty()

                     // LEFT JOIN Tindakan
                     // Perhatikan: t.TindakanId harus non-null untuk join. Jika to null, t juga akan null.
                 join t in _applicationDbContext.Tindakans on to.TindakanId equals t.TindakanId into tindakanMasterGroup
                 from t in tindakanMasterGroup.DefaultIfEmpty() 

                     // LEFT JOIN BiayaAdministrasi
                     // Perhatikan: adm.BiayaAdministrasiKode harus non-null untuk join. Jika k.JenisKunjungan null, adm juga akan null.
                 join adm in _applicationDbContext.BiayaAdministrasis on k.JenisKunjungan equals adm.BiayaAdministrasiKode into admGroup
                 from adm in admGroup.DefaultIfEmpty() 

                     // LEFT JOIN ke tabel Kasir (MainKasir)
                 join kasir in _applicationDbContext.MainKasirs on k.KunjunganID equals kasir.KunjunganId into kasirGroup
                 from kasir in kasirGroup.DefaultIfEmpty() 

                     // LEFT JOIN ke tabel Diskon
                 join dsk in _applicationDbContext.Diskons on kasir.DiskonId equals dsk.DiskonId into diskonGroup
                 from dsk in diskonGroup.DefaultIfEmpty() 

                     // LEFT JOIN ke tabel Metode Pembayaran
                 join mp in _applicationDbContext.MetodePembayarans on kasir.MetodePembayaranId equals mp.MetodePembayaranId into metodeGroup
                 from mp in metodeGroup.DefaultIfEmpty() 

                 where k.KunjunganID == kunjunganId && !k.IsDelete
                 select new
                 {
                     k,
                     p,
                     a,
                     ap,
                     d,
                     poli,
                     r,
                     dr,
                     o,
                     to,
                     t,
                     adm,
                     kasir,
                     dsk,
                     mp
                 });

            var result = await query.ToListAsync();

            var kasirData = result.GroupBy(x => x.k.KunjunganID) // Grouping by KunjunganID
                .Select(group => {
                    var firstItem = group.First(); // Ambil satu item dari grup untuk data Kunjungan, Pasien, dll.

                    return new
                    {
                        KasirId = Guid.NewGuid(), // Ini akan membuat GUID baru setiap kali, mungkin bukan yang Anda inginkan
                        KunjunganID = firstItem.k.KunjunganID,
                        JenisKunjungan = firstItem.k.JenisKunjungan,
                        NoRegistrasi = firstItem.k.Antrian,
                        firstItem.k.TipePembayaran,
                        TglRegistrasi = firstItem.k.CreateDateTime.ToString("dddd, dd MMMM yyyy", new CultureInfo("id-ID")), // Tambahkan tahun
                        firstItem.k.PasienId,
                        NoRM = firstItem.p?.NoRekamMedis ?? "-",
                        NamaPasien = firstItem.p?.NamaLengkap ?? "-",
                        UmurPasien = firstItem.p?.TanggalLahir.HasValue == true
                            ? (int?)((DateTime.Today.Year - firstItem.p.TanggalLahir.Value.Year) -
                                    (DateTime.Today < firstItem.p.TanggalLahir.Value.AddYears(DateTime.Today.Year - firstItem.p.TanggalLahir.Value.Year) ? 1 : 0))
                            : null,
                        JenisKelamin = firstItem.p?.JenisKelamin,
                        AsuransiId = firstItem.k.AsuransiId,
                        NamaPerusahaan = firstItem.a?.NamaAsuransi ?? "Tunai", // NamaAsuransi akan null jika tidak ada asuransi
                        NoPolis = firstItem.ap?.NoPolis ?? "-",
                        DokterId = firstItem.k.DokterId,
                        NamaDokter = firstItem.d?.NmDokter ?? "-",
                        PoliklinikId = firstItem.k.PoliklinikId,
                        NamaPoliklinik = firstItem.poli?.NamaPoliklinik ?? "-",
                        BiayaAdministrasiId = firstItem.adm?.BiayaAdministrasiId,
                        BiayaAdministrasi = firstItem.adm?.NominalBiayaAdministrasi,
                        PaymentMethodId = firstItem.mp?.MetodePembayaranId,
                        PaymentMethodName = firstItem.mp?.NamaMetode ?? "-",
                        DiskonId = firstItem.dsk?.DiskonId,
                        NamaDiskon = firstItem.dsk?.NamaDiskon ?? "-",
                        CreateBy = firstItem.kasir?.CreateBy,
                        CreateDateTime = firstItem.kasir?.CreateDateTime,

                        // Koleksi untuk item yang bisa banyak (Resep, Obat, Tindakan)
                        DaftarResepObat = group
                            .Where(x => x.dr != null && x.o != null) // Filter hanya yang punya DetailResep dan Obat
                            .Select(x => new
                            {
                                x.r.ResepId, // ResepId dari resep utama
                                x.dr.DetailResepId,
                                x.dr.ObatId,
                                NamaObat = x.o.ObatName,
                                x.dr.Qty,
                                HargaObat = x.o.HargaJual,
                                StatusCoverObat = (x.dr != null && x.o != null && firstItem.a != null)
                                    ? _applicationDbContext.ObatAsuransis.Any(y => y.ObatId == x.o.ObatId && y.AsuransiId == firstItem.a.AsuransiId)
                                    : false,
                                TotalBiayaObat = x.dr.Qty * x.o.HargaJual
                            }).Distinct().ToList(), // Gunakan Distinct untuk menghindari duplikasi dalam daftar obat

                        DaftarTindakan = group
                            .Where(x => x.to != null && x.t != null) // Filter hanya yang punya TindakanKunjungan dan Tindakan
                            .Select(x => new
                            {
                                x.to.TindakanId, // TindakanId dari TindakanKunjungan
                                NamaTindakan = x.t.NamaTindakan,
                                QtyTindakan = x.to.Quantity,
                                HargaTindakan = x.to.Total,
                                StatusCoverTindakan = (x.to != null && x.t != null && firstItem.a != null)
                                    ? _applicationDbContext.TindakanAsuransis.Any(y => y.TindakanId == x.to.TindakanId && y.AsuransiId == firstItem.a.AsuransiId)
                                    : false
                            }).Distinct().ToList() // Gunakan Distinct untuk menghindari duplikasi dalam daftar tindakan
                    };
                }).ToList();


            if (!kasirData.Any())
            {
                return NotFound(new { message = "Data billing kasir untuk kunjungan ini tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new { status = "success", data = kasirData.FirstOrDefault() }); // Mengembalikan hanya satu item karena ini adalah view untuk satu kunjunganId
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.MainKasirs.Find(id);
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MainKasirViewModel vm)
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

                
                
                
                // Inside the Create method, replace the problematic line:
                var data = new MainKasir
                {
                    KasirId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    BiayaAdministrasiKode = vm.BiayaAdministrasiKode,
                    MetodePembayaranId = vm.MetodePembayaranId,
                    NamaMetode = vm.NamaMetode,
                    DiskonId = vm.DiskonId,
                    NominalPembayaran = vm.NominalPembayaran,
                    StatusPembayaran = vm.StatusPembayaran,
                    Keterangan = vm.Keterangan,
                    TglPembayaran = DateTimeOffset.UtcNow, // Atau sesuai kebutuhan
                    ReferenceId = vm.ReferenceId, // Atau sesuai kebutuhan
                    IsDelete = false,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.MainKasirs.Add(data);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MainKasirViewModel vm)
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
                var data = await _applicationDbContext.MainKasirs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.BiayaAdministrasiKode = vm.BiayaAdministrasiKode;
                data.MetodePembayaranId = vm.MetodePembayaranId;
                data.NamaMetode = vm.NamaMetode;
                data.DiskonId = vm.DiskonId;
                data.NominalPembayaran = vm.NominalPembayaran;
                data.StatusPembayaran = vm.StatusPembayaran;
                data.Keterangan = vm.Keterangan;
                data.ReferenceId = vm.ReferenceId; // Atau sesuai kebutuhan

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.MainKasirs.Update(data);
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
        public async Task<IActionResult> Delete(Guid id)
        {
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
                var data = await _applicationDbContext.MainKasirs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.MainKasirs.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedBenefits(
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

            // Query data
            var query = (from a in _applicationDbContext.MainKasirs
                         join u in _applicationDbContext.UserActives
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false
                         select new
                         {
                             CreateDateTime = a.CreateDateTime,
                             CreateBy = a.CreateBy,
                             CreateByName = u.FullName,
                             KasirId = a.KasirId,
                             KunjunganId = a.KunjunganId,
                             BiayaAdministrasiKode = a.BiayaAdministrasiKode,
                             MetodePembayaranId = a.MetodePembayaranId,
                             NamaMetode = a.NamaMetode,
                             DiskonId = a.DiskonId,
                             NominalPembayaran = a.NominalPembayaran,
                             StatusPembayaran = a.StatusPembayaran,
                             Keterangan = a.Keterangan,
                             TglPembayaran = a.TglPembayaran,
                             ReferenceId = a.ReferenceId
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KasirId.ToString(), search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KasirId" => query.OrderByDescending(u => u.KasirId),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "KasirId" => query.OrderBy(u => u.KasirId),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
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
