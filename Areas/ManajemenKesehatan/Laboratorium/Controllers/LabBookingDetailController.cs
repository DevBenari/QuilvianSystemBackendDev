using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class LabBookingDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _uploadUrl;

        private readonly ILogger<LabBookingDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabBookingDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabBookingDetailController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _uploadUrl = configuration["FileStorage:UploadUrl"];

        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from d in _applicationDbContext.LabBookingDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on d.CreateBy equals u.UserActiveId

                         // join lab
                         join l in _applicationDbContext.Labs
                         on d.LabId equals l.LabId into labGroup
                         from l in labGroup.DefaultIfEmpty()


                             // join ke lab booking
                         join b in _applicationDbContext.LabBookings
                         on d.BookingLabId equals b.BookingLabId into labBookings
                         from b in labBookings.DefaultIfEmpty()

                             // joimn ke lab pemeriksaan
                         join p in _applicationDbContext.LabPemeriksaans
                         on d.PemeriksaanLabId equals p.PemeriksaanLabId into labPemeriksaans
                         from p in labPemeriksaans.DefaultIfEmpty()

                         where d.IsDelete == false || d.IsDelete == null
                         select new
                         {
                             d.CreateDateTime,
                             d.CreateBy,
                             CreateByName = u.FullName,
                             d.DetailBookingLabId,
                             d.BookingLabId,
                             d.NoOrder,
                             d.PasienId,
                             b.KunjunganId,
                             d.PemeriksaanLabId,
                             NamaPemeriksaan = p.NamaPemeriksaan ?? "-",
                             d.LabId,
                             NamaLab = l.NamaLab ?? "-",
                             d.KategoriPatologiAnatomi,
                             d.JenisSpecimen,
                             d.LokasiSpecimen,
                             d.KeteranganKlinik,
                             d.PenyakitSebelumnya,
                             d.PenggunaanFiksasi,
                             d.JenisPemeriksaanGC,
                             d.JenisGC,
                             d.BahanNonGC,
                             d.BahanMicrobiologi,
                             d.MasaHaidTerakhir,
                             d.Diagnosa,
                             d.SpecimenJenisId,
                             d.SpecimenMethodId,
                             d.AsalSpecimenId,
                             d.AlasanPembatalan,
                             d.TTDPembatalanPath,
                             d.StatusPemeriksaan,
                             d.TanggalSelesai,
                             d.StatusVerifikasi,
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
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // ✅ Cek koneksi ke database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Query data lengkap dengan join
                var data = await (from d in _applicationDbContext.LabBookingDetails
                                  join u in _applicationDbContext.UserActives
                                    on d.CreateBy equals u.UserActiveId into userGroup
                                  from u in userGroup.DefaultIfEmpty()

                                      // join ke Lab
                                  join l in _applicationDbContext.Labs
                                    on d.LabId equals l.LabId into labGroup
                                  from l in labGroup.DefaultIfEmpty()

                                      // join ke LabBooking
                                  join b in _applicationDbContext.LabBookings
                                    on d.BookingLabId equals b.BookingLabId into bookingGroup
                                  from b in bookingGroup.DefaultIfEmpty()

                                      // join ke LabPemeriksaan
                                  join p in _applicationDbContext.LabPemeriksaans
                                    on d.PemeriksaanLabId equals p.PemeriksaanLabId into pemeriksaanGroup
                                  from p in pemeriksaanGroup.DefaultIfEmpty()

                                  where (d.IsDelete == false || d.IsDelete == null)
                                        && d.DetailBookingLabId == id
                                  select new
                                  {
                                      d.DetailBookingLabId,
                                      d.BookingLabId,
                                      d.PasienId,
                                      b.KunjunganId,
                                      d.PemeriksaanLabId,
                                      NamaPemeriksaan = p.NamaPemeriksaan ?? "-",
                                      d.LabId,
                                      NamaLab = l.NamaLab ?? "-",
                                      d.NoOrder,
                                      d.KategoriPatologiAnatomi,
                                      d.JenisSpecimen,
                                      d.LokasiSpecimen,
                                      d.KeteranganKlinik,
                                      d.PenyakitSebelumnya,
                                      d.PenggunaanFiksasi,
                                      d.JenisPemeriksaanGC,
                                      d.JenisGC,
                                      d.BahanNonGC,
                                      d.BahanMicrobiologi,
                                      d.MasaHaidTerakhir,
                                      d.Diagnosa,
                                      d.SpecimenJenisId,
                                      d.SpecimenMethodId,
                                      d.AsalSpecimenId,
                                      d.CreateBy,
                                      CreateByName = u.FullName ?? "(Tidak diketahui)",
                                      d.CreateDateTime,                             
                                      d.AlasanPembatalan,
                                      d.TTDPembatalanPath,
                                      d.StatusPemeriksaan,
                                      d.TanggalSelesai,
                                      d.StatusVerifikasi,
                                  })
                                  .FirstOrDefaultAsync();

                // ✅ Cek apakah data ditemukan
                if (data == null)
                    return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });

                // ✅ Return sukses
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LabBookingDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // 🔹 Ambil user login
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==========================================================
                // ✅ Ambil data lab untuk generate NoOrder
                // ==========================================================
                if (vm.LabId == null)
                    return BadRequest(new { message = "LabId wajib diisi untuk menentukan NoOrder." });

                var lab = await _applicationDbContext.Labs.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.LabId == vm.LabId);

                if (lab == null)
                    return NotFound(new { message = "Lab dengan ID tersebut tidak ditemukan." });

                var kodeKategori = lab.KodeKategori?.Trim().ToUpper() ?? "UNK";
                string labPrefix = kodeKategori.StartsWith("LAB") && kodeKategori.Length > 3
                    ? kodeKategori.Substring(3, Math.Min(3, kodeKategori.Length - 3))
                    : kodeKategori.Length > 3 ? kodeKategori.Substring(0, 3) : kodeKategori;

                var today = DateTime.UtcNow.Date;
                var lastOrderToday = await _applicationDbContext.LabBookingDetails
                    .Where(d => d.CreateDateTime.Date == today && d.NoOrder.StartsWith(labPrefix))
                    .OrderByDescending(d => d.NoOrder)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastOrderToday != null && lastOrderToday.NoOrder.Length >= labPrefix.Length + 4)
                {
                    string lastNumStr = lastOrderToday.NoOrder.Substring(labPrefix.Length);
                    if (int.TryParse(lastNumStr, out int lastNum))
                        nextNumber = lastNum + 1;
                }

                string newNoOrder = $"{labPrefix}{today:yyyyMMdd}{nextNumber:D4}";

                // ==========================================================
                // ✅ Buat data baru LabBookingDetail
                // ==========================================================
                var data = new LabBookingDetail
                {
                    DetailBookingLabId = Guid.NewGuid(),
                    BookingLabId = vm.BookingLabId,
                    PasienId = vm.PasienId,
                    PemeriksaanLabId = vm.PemeriksaanLabId,
                    LabId = vm.LabId,
                    KategoriPatologiAnatomi = vm.KategoriPatologiAnatomi,
                    JenisSpecimen = vm.JenisSpecimen,
                    LokasiSpecimen = vm.LokasiSpecimen,
                    KeteranganKlinik = vm.KeteranganKlinik,
                    PenyakitSebelumnya = vm.PenyakitSebelumnya,
                    PenggunaanFiksasi = vm.PenggunaanFiksasi,
                    JenisPemeriksaanGC = vm.JenisPemeriksaanGC,
                    JenisGC = vm.JenisGC,
                    BahanNonGC = vm.BahanNonGC,
                    BahanMicrobiologi = vm.BahanMicrobiologi,
                    MasaHaidTerakhir = vm.MasaHaidTerakhir,
                    Diagnosa = vm.Diagnosa,
                    AsalSpecimenId = vm.AsalSpecimenId,
                    SpecimenMethodId = vm.SpecimenMethodId,
                    SpecimenJenisId = vm.SpecimenJenisId,
                    NoOrder = newNoOrder,
                    StatusPemeriksaan = vm.StatusPemeriksaan,
                    StatusVerifikasi = vm.StatusVerifikasi,
                    TanggalSelesai = vm.TanggalSelesai,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                _applicationDbContext.LabBookingDetails.Add(data);
                await _applicationDbContext.SaveChangesAsync();

                // ==========================================================
                // ✅ Tambahkan otomatis ke Billing
                // ==========================================================
                if (vm.PemeriksaanLabId != null && vm.BookingLabId != null)
                {
                    var pemeriksaan = await _applicationDbContext.LabPemeriksaans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PemeriksaanLabId == vm.PemeriksaanLabId);

                    if (pemeriksaan != null)
                    {
                        var billing = new Billing
                        {
                            BillingId = Guid.NewGuid(),
                            KunjunganId = vm.KunjunganId ?? Guid.Empty, // jika dikirim dari ViewModel
                            ItemId = pemeriksaan.PemeriksaanLabId,
                            NamaItem = pemeriksaan.NamaPemeriksaan,
                            HargaItem = pemeriksaan.HargaPemeriksaan ?? 0,
                            QtyItem = 1,
                            SubTotalItem = pemeriksaan.HargaPemeriksaan ?? 0,
                            BillingKode = "LAB",
                            JenisBilling = "Pemeriksaan Lab",
                            BillingDate = DateTime.UtcNow,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            Keterangan = $"Booking Lab ({newNoOrder})"
                        };

                        _applicationDbContext.Billings.Add(billing);
                        await _applicationDbContext.SaveChangesAsync();
                    }
                }

                return Created("", new
                {
                    message = "Tambah Data Detail Booking Lab & Billing Berhasil || 201 Created",
                    data = new
                    {
                        data.DetailBookingLabId,
                        data.NoOrder
                    }
                });
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


        [HttpPut("Batal/{id}")]
        public async Task<IActionResult> BatalBooking(Guid id, [FromForm] LabBookingDetailBatalVM vm)
        {
            if (vm == null)
                return BadRequest(new { message = "Data pembatalan tidak valid." });

            var booking = await _applicationDbContext.LabBookingDetails
                .FirstOrDefaultAsync(b => b.DetailBookingLabId == id);

            if (booking == null)
                return NotFound(new { message = "Data booking tidak ditemukan." });

            // 🔐 Ambil user dari JWT Claims
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userActiveId = getUserActive.UserActiveId;

            // ==================================================
            // ✅ PROSES UPLOAD TTD PEMBATALAN
            // ==================================================
            string ttdPath = "";

            if (vm.TTDPembatalan != null && vm.TTDPembatalan.Length > 0)
            {
                var maxSize = 1 * 1024 * 1024; // 1 MB
                var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                var fileExtension = Path.GetExtension(vm.TTDPembatalan.FileName).ToLower();

                if (vm.TTDPembatalan.Length > maxSize)
                    return BadRequest(new { message = "Ukuran file tanda tangan terlalu besar! Maksimal 1MB." });

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Format tanda tangan tidak valid! Gunakan JPG atau JPEG." });

                var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                var ttdFileName = $"{getUserActive.FullName}_{safeTime}_TTDBatal{fileExtension}";

                using var client = new HttpClient();
                using var ms = new MemoryStream();
                await vm.TTDPembatalan.CopyToAsync(ms);
                ms.Position = 0;

                var content = new MultipartFormDataContent {
            {
                new StreamContent(ms) {
                    Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDPembatalan.ContentType) }
                },
                "file", ttdFileName
            },
            { new StringContent("TTDPembatalan"), "folderTarget" }
        };

                HttpResponseMessage flaskResponse;
                try
                {
                    flaskResponse = await client.PostAsync(_uploadUrl, content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gagal koneksi ke server Flask untuk upload TTD pembatalan.");
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke server Flask untuk upload tanda tangan." });
                }

                if (!flaskResponse.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Gagal upload tanda tangan ke server Flask." });

                var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                ttdPath = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? "";

                if (string.IsNullOrEmpty(ttdPath))
                    return StatusCode(500, new { message = "Gagal mendapatkan path TTD dari server Flask." });
            }
            else
            {
                return BadRequest(new { message = "Tanda tangan pembatalan harus diisi." });
            }

            // ==================================================
            // ✅ UPDATE DATA BOOKING MENJADI DIBATALKAN
            // ==================================================
            booking.AlasanPembatalan = vm.AlasanPembatalan;
            booking.TTDPembatalanPath = ttdPath;
            booking.UpdateBy = userActiveId;
            booking.UpdateDateTime = DateTimeOffset.UtcNow;

            _applicationDbContext.LabBookingDetails.Update(booking);

            // ==================================================
            // ✅ SOFT DELETE BILLING YANG TERKAIT PEMERIKSAAN LAB
            // ==================================================
            if (booking.PemeriksaanLabId != null)
            {
                var relatedBillings = await _applicationDbContext.Billings
                    .Where(b => b.ItemId == booking.PemeriksaanLabId
                             && b.JenisBilling == "Pemeriksaan Lab"
                             && (b.IsDelete == false || b.IsDelete == null))
                    .ToListAsync();

                if (relatedBillings.Any())
                {
                    foreach (var bill in relatedBillings)
                    {
                        bill.IsDelete = true;
                        bill.UpdateBy = userActiveId;
                        bill.UpdateDateTime = DateTimeOffset.UtcNow;
                    }

                    _applicationDbContext.Billings.UpdateRange(relatedBillings);
                }
            }

            await _applicationDbContext.SaveChangesAsync();

            // ==================================================
            // ✅ RESPONSE
            // ==================================================
            return Ok(new
            {
                message = "Booking lab berhasil dibatalkan dan billing terkait telah dihapus (soft delete).",
                bookingId = booking.DetailBookingLabId,
                alasan = booking.AlasanPembatalan,
                ttdUrl = ttdPath
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LabBookingDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ==========================================================
                // 🔐 Validasi koneksi database dan user login
                // ==========================================================
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==========================================================
                // 🔍 Cari data detail booking berdasarkan ID
                // ==========================================================
                var existingData = await _applicationDbContext.LabBookingDetails
                    .FirstOrDefaultAsync(d => d.DetailBookingLabId == id);

                if (existingData == null)
                    return NotFound(new { message = "Data detail booking lab tidak ditemukan." });

                // ==========================================================
                // ✅ Generate NoOrder jika LabId berubah
                // ==========================================================
                string newNoOrder = existingData.NoOrder; // default: tetap
                if (vm.LabId != existingData.LabId && vm.LabId != null)
                {
                    var lab = await _applicationDbContext.Labs.AsNoTracking()
                        .FirstOrDefaultAsync(l => l.LabId == vm.LabId);

                    if (lab == null)
                        return NotFound(new { message = "Lab dengan ID tersebut tidak ditemukan." });

                    var kodeKategori = lab.KodeKategori?.Trim().ToUpper() ?? "UNK";
                    string labPrefix = kodeKategori.StartsWith("LAB") && kodeKategori.Length > 3
                        ? kodeKategori.Substring(3, Math.Min(3, kodeKategori.Length - 3))
                        : kodeKategori.Length > 3 ? kodeKategori.Substring(0, 3) : kodeKategori;

                    var today = DateTime.UtcNow.Date;
                    var lastOrderToday = await _applicationDbContext.LabBookingDetails
                        .Where(d => d.CreateDateTime.Date == today && d.NoOrder.StartsWith(labPrefix))
                        .OrderByDescending(d => d.NoOrder)
                        .FirstOrDefaultAsync();

                    int nextNumber = 1;
                    if (lastOrderToday != null && lastOrderToday.NoOrder.Length >= labPrefix.Length + 4)
                    {
                        string lastNumStr = lastOrderToday.NoOrder.Substring(labPrefix.Length);
                        if (int.TryParse(lastNumStr, out int lastNum))
                            nextNumber = lastNum + 1;
                    }

                    newNoOrder = $"{labPrefix}{today:yyyyMMdd}{nextNumber:D4}";
                }

                // ==========================================================
                // ✅ Update field dari ViewModel
                // ==========================================================
                existingData.BookingLabId = vm.BookingLabId;
                existingData.PasienId = vm.PasienId;
                existingData.PemeriksaanLabId = vm.PemeriksaanLabId;
                existingData.LabId = vm.LabId;

                existingData.KategoriPatologiAnatomi = vm.KategoriPatologiAnatomi;
                existingData.JenisSpecimen = vm.JenisSpecimen;
                existingData.LokasiSpecimen = vm.LokasiSpecimen;
                existingData.KeteranganKlinik = vm.KeteranganKlinik;
                existingData.PenyakitSebelumnya = vm.PenyakitSebelumnya;
                existingData.PenggunaanFiksasi = vm.PenggunaanFiksasi;
                existingData.JenisPemeriksaanGC = vm.JenisPemeriksaanGC;
                existingData.JenisGC = vm.JenisGC;
                existingData.BahanNonGC = vm.BahanNonGC;
                existingData.BahanMicrobiologi = vm.BahanMicrobiologi;
                existingData.MasaHaidTerakhir = vm.MasaHaidTerakhir;
                existingData.Diagnosa = vm.Diagnosa;
                existingData.SpecimenJenisId = vm.SpecimenJenisId;
                existingData.SpecimenMethodId = vm.SpecimenMethodId;
                existingData.AsalSpecimenId = vm.AsalSpecimenId;
                existingData.StatusPemeriksaan = vm.StatusPemeriksaan;
                existingData.TanggalSelesai = vm.TanggalSelesai;
                existingData.NoOrder = newNoOrder;

                existingData.UpdateBy = userActiveId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabBookingDetails.Update(existingData);
                await _applicationDbContext.SaveChangesAsync();

                // ==========================================================
                // ✅ Sinkronisasi Billing Pemeriksaan Lab
                // ==========================================================
                if (vm.PemeriksaanLabId != null)
                {
                    var pemeriksaan = await _applicationDbContext.LabPemeriksaans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PemeriksaanLabId == vm.PemeriksaanLabId);

                    if (pemeriksaan != null)
                    {
                        // Cek apakah sudah ada billing untuk pemeriksaan ini
                        var existingBilling = await _applicationDbContext.Billings
                            .FirstOrDefaultAsync(b => b.ItemId == vm.PemeriksaanLabId
                                                   && b.JenisBilling == "Pemeriksaan Lab"
                                                   && (b.IsDelete == false || b.IsDelete == null));

                        if (existingBilling == null)
                        {
                            // Tambah billing baru
                            var billing = new Billing
                            {
                                BillingId = Guid.NewGuid(),
                                KunjunganId = vm.KunjunganId ?? Guid.Empty,
                                ItemId = pemeriksaan.PemeriksaanLabId,
                                NamaItem = pemeriksaan.NamaPemeriksaan,
                                HargaItem = pemeriksaan.HargaPemeriksaan ?? 0,
                                QtyItem = 1,
                                SubTotalItem = pemeriksaan.HargaPemeriksaan ?? 0,
                                BillingKode = "LAB",
                                JenisBilling = "Pemeriksaan Lab",
                                BillingDate = DateTime.UtcNow,
                                CreateBy = userActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow,
                                StatusPengambilan = false,
                                Keterangan = $"Otomatis dari Update Booking Lab ({newNoOrder})"
                            };

                            _applicationDbContext.Billings.Add(billing);
                        }
                        else
                        {
                            // Update billing lama (jika harga atau nama berubah)
                            existingBilling.NamaItem = pemeriksaan.NamaPemeriksaan;
                            existingBilling.HargaItem = pemeriksaan.HargaPemeriksaan ?? existingBilling.HargaItem;
                            existingBilling.SubTotalItem = existingBilling.HargaItem * (existingBilling.QtyItem ?? 1);
                            existingBilling.UpdateBy = userActiveId;
                            existingBilling.UpdateDateTime = DateTimeOffset.UtcNow;

                            _applicationDbContext.Billings.Update(existingBilling);
                        }

                        await _applicationDbContext.SaveChangesAsync();
                    }
                }

                // ==========================================================
                // ✅ RESPONSE
                // ==========================================================
                return Ok(new
                {
                    message = "Update Data Detail Booking Lab & Billing Berhasil || 200 OK",
                    data = new
                    {
                        existingData.DetailBookingLabId,
                        existingData.NoOrder
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal memperbarui data: {dbEx.InnerException?.Message}" });
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
                var data = await _applicationDbContext.LabBookingDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabBookingDetails.Update(data);
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
        public IActionResult Paged(
        int page = 1,
        int perPage = 10,
        string? NamaLaboratorium = null,
        Guid? kunjunganId = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from d in _applicationDbContext.LabBookingDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on d.CreateBy equals u.UserActiveId

                         // join lab
                         join l in _applicationDbContext.Labs 
                         on d.LabId equals l.LabId into labGroup
                         from l in labGroup.DefaultIfEmpty()


                         // join ke lab booking
                         join b in _applicationDbContext.LabBookings
                         on d.BookingLabId equals b.BookingLabId into labBookings
                         from b in labBookings.DefaultIfEmpty()

                         // joimn ke lab pemeriksaan
                         join p in _applicationDbContext.LabPemeriksaans
                         on d.PemeriksaanLabId equals p.PemeriksaanLabId into labPemeriksaans
                         from p in labPemeriksaans.DefaultIfEmpty()

                         where d.IsDelete == false || d.IsDelete == null
                         select new
                         {
                             d.CreateDateTime,
                             d.CreateBy,
                             CreateByName = u.FullName,
                             d.DetailBookingLabId,
                             d.BookingLabId,
                             d.PasienId,
                             d.NoOrder,
                             b.KunjunganId,
                             d.PemeriksaanLabId,
                             NamaPemeriksaan = p.NamaPemeriksaan ?? "-",
                             d.LabId,
                             NamaLab = l.NamaLab ?? "-",
                             d.KategoriPatologiAnatomi,
                             d.JenisSpecimen,
                             d.LokasiSpecimen,
                             d.KeteranganKlinik,
                             d.PenyakitSebelumnya,
                             d.PenggunaanFiksasi,
                             d.JenisPemeriksaanGC,
                             d.JenisGC,
                             d.BahanNonGC,
                             d.BahanMicrobiologi,
                             d.MasaHaidTerakhir,
                             d.Diagnosa,
                             d.SpecimenJenisId,
                             d.SpecimenMethodId,
                             d.AsalSpecimenId,
                             d.AlasanPembatalan,
                             d.TTDPembatalanPath,
                             d.StatusPemeriksaan,
                             d.TanggalSelesai,
                             d.StatusVerifikasi,
                         });

            // filter kunjungan id
            if (kunjunganId.HasValue )
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }

            //**Filter berdasarkan search(Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(NamaLaboratorium))
            {
                NamaLaboratorium = $"%{NamaLaboratorium.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaLab, NamaLaboratorium)
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
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
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
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
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
