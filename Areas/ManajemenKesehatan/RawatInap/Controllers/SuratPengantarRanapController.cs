using System.Data;
using System.Security.Claims;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Services;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class SuratPengantarRanapController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly ILogger<SuratPengantarRanapController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<SuratPengantarRanapHub> _hubContext;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly IDepositRanapNumberService _depositRanapNumberService;

        public SuratPengantarRanapController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SuratPengantarRanapController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<SuratPengantarRanapHub> hubContext,
            ITTDService ttdService,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IDepositRanapNumberService depositRanapNumberService
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _ttdService = ttdService;
            _generateInvoiceBillingService = generateInvoiceBillingService;
            _depositRanapNumberService = depositRanapNumberService;
        }

        private static string HitungUmurLengkap(DateTime? tanggalLahir)
        {
            if (!tanggalLahir.HasValue) return "-";

            var today = DateTime.Today;
            int tahun = today.Year - tanggalLahir.Value.Year;
            int bulan = today.Month - tanggalLahir.Value.Month;
            int hari = today.Day - tanggalLahir.Value.Day;

            if (hari < 0)
            {
                bulan--;
                var prevMonth = today.AddMonths(-1);
                hari += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            }

            if (bulan < 0)
            {
                tahun--;
                bulan += 12;
            }

            return $"{tahun} tahun {bulan} bulan {hari} hari";
        }

        private async Task<bool> CanCreateSuratForKunjunganAsync(Guid kunjunganId)
        {
            // 1) Ambil NoRM dari Kunjungan
            var pasien = await (from k in _applicationDbContext.Kunjungans.AsNoTracking()
                                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                                    on k.PasienId equals p.PendaftaranPasienBaruId
                                where k.KunjunganID == kunjunganId
                                select new { p.NoRekamMedis })
                               .FirstOrDefaultAsync();

            // Kalau kunjungan/pasien/NoRM tidak ada → tidak boleh buat
            if (pasien == null || string.IsNullOrWhiteSpace(pasien.NoRekamMedis))
            {
                return false;
            }

            // 2) Cek apakah NoRM ini sudah punya Surat Pengantar Ranap aktif
            var sudahAda = await (from s in _applicationDbContext.SuratPengantarRawatInaps.AsNoTracking()
                                  join k in _applicationDbContext.Kunjungans.AsNoTracking()
                                      on s.KunjunganId equals k.KunjunganID
                                  join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                                      on k.PasienId equals p.PendaftaranPasienBaruId
                                  join bb in _applicationDbContext.BookingBedRanaps.AsNoTracking()
                                      on k.KunjunganID equals bb.KunjunganId 
                                  where p.NoRekamMedis == pasien.NoRekamMedis && bb.TglKeluar == null
                                  select 1)
                                 .AnyAsync();


            // Return true jika belum ada (negasi dari sudahAda), false jika sudah ada
            return !sudahAda;
        }

        private async Task<string> GenerateNomorSuratPengantarRanapAsync(int tahun)
        {
            // Ambil nomor surat terakhir tahun berjalan
            // Format: 001/SP-RI/MMC/2026
            var lastNomorSurat = await _applicationDbContext.SuratPengantarRawatInaps
                .Where(x => x.NomorSuratPengantar != null &&
                            x.NomorSuratPengantar.EndsWith($"/SP-RI/MMC/{tahun}"))
                .OrderByDescending(x => x.NomorSuratPengantar)
                .Select(x => x.NomorSuratPengantar)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNomorSurat))
            {
                var firstPart = lastNomorSurat.Split('/').FirstOrDefault();
                if (int.TryParse(firstPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{nextNumber:D3}/SP-RI/MMC/{tahun}";
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Base query (IQueryable) + AsNoTracking
            var baseQuery =
                from a in _applicationDbContext.SuratPengantarRawatInaps.AsNoTracking()
                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into userJoin
                from u in userJoin.DefaultIfEmpty()

                join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k.KunjunganID

                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on k.PasienId equals p.PendaftaranPasienBaruId

                join d in _applicationDbContext.Dokters.AsNoTracking()
                    on k.DokterId equals d.DokterId

                join poli in _applicationDbContext.Polikliniks.AsNoTracking()
                    on k.PoliklinikId equals poli.PoliklinikId

                // LEFT JOIN AsuransiPasien
                join ap in _applicationDbContext.AsuransiPasiens.AsNoTracking()
                    on k.AsuransiPasienId equals (Guid?)ap.AsuransiPasienId into asuransiPasienGroup
                from ap in asuransiPasienGroup.DefaultIfEmpty()

                    // LEFT JOIN Asuransi
                join ar in _applicationDbContext.Asuransis.AsNoTracking()
                    on ap.AsuransiId equals ar.AsuransiId into asuransiGroup
                from ar in asuransiGroup.DefaultIfEmpty()

                where a.IsDelete != true
                      && k.IsDelete != true

                select new
                {
                    // Surat Pengantar
                    a.SuratPengantarRawatInapId,
                    a.KunjunganId,
                    a.NomorSuratPengantar,
                    a.Diagnosa,
                    a.ICDId,
                    a.AlasanRanap,
                    a.RencanaTindakLanjut,
                    a.AsalUnit,
                    a.IndikasiTindakan,
                    a.JenisOperasi,
                    a.TawaranLayanan,
                    a.HarapanHasil,
                    a.IsAdaHambatan,
                    a.PathTTDDokterDPJP,
                    a.Status,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    // Kunjungan
                    k.NoRekamMedis,
                    k.TipePasien,
                    k.TipePembayaran,
                    k.JenisKunjungan,
                    k.IsFinished,

                    // Dokter
                    DokterId = d.DokterId,
                    DokterName = d.NmDokter,

                    // Poli
                    PoliklinikId = poli.PoliklinikId,
                    PoliklinikName = poli.NamaPoliklinik,

                    // Pasien
                    PasienId = p.PendaftaranPasienBaruId,
                    PasienName = p.NamaLengkap,
                    JenisKelamin = p.JenisKelamin,
                    p.NoPasien,
                    TanggalLahir = p.TanggalLahir, // umur dihitung setelah paging

                    // Asuransi
                    NamaAsuransi = ar != null ? ar.NamaAsuransi : null,
                    NoPolis = ap != null ? ap.NoPolis : null
                };

            // Sorting (di DB)
            var query = baseQuery.OrderByDescending(x => x.CreateDateTime);

            // Count + paging (di DB)
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            // Jika ingin tetap 404 ketika kosong, aktifkan ini:
            // if (rows.Count == 0)
            //     return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            // Hitung umur setelah paging (hanya untuk data yang tampil)
            var result = rows.Select(r => new
            {
                r.SuratPengantarRawatInapId,
                r.KunjunganId,
                r.NomorSuratPengantar,
                r.Diagnosa,
                r.ICDId,
                r.AlasanRanap,
                r.RencanaTindakLanjut,
                r.AsalUnit,
                r.IndikasiTindakan,
                r.JenisOperasi,
                r.TawaranLayanan,
                r.HarapanHasil,
                r.IsAdaHambatan,
                r.PathTTDDokterDPJP,
                r.Status,
                r.CreateDateTime,
                r.CreateBy,
                r.CreateByName,

                r.NoRekamMedis,
                r.TipePasien,
                r.TipePembayaran,
                r.JenisKunjungan,
                r.IsFinished,

                r.DokterId,
                r.DokterName,

                r.PoliklinikId,
                r.PoliklinikName,

                r.PasienId,
                r.PasienName,
                r.JenisKelamin,
                Umur = HitungUmurLengkap(r.TanggalLahir),
                r.NoPasien,

                r.NamaAsuransi,
                r.NoPolis
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


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // =======================================
            // 1) BASE QUERY: filter dulu pada tabel utama
            // =======================================
            var baseQuery = _applicationDbContext.SuratPengantarRawatInaps
                .AsNoTracking()
                .Where(a =>
                    a.SuratPengantarRawatInapId == id &&
                    (a.IsDelete == false || a.IsDelete == null)
                );

            // =======================================
            // 2) JOIN QUERY: baru join setelah baseQuery kecil ✅
            // =======================================
            var result = await (
                from a in baseQuery

                join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    .Where(k => (k.IsDelete == false || k.IsDelete == null))
                    on a.KunjunganId equals k.KunjunganID

                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on k.PasienId equals p.PendaftaranPasienBaruId

                join d in _applicationDbContext.Dokters.AsNoTracking()
                    on k.DokterId equals d.DokterId

                join poli in _applicationDbContext.Polikliniks.AsNoTracking()
                    on k.PoliklinikId equals poli.PoliklinikId

                // LEFT JOIN UserActive
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId into userJoin
                from u in userJoin.DefaultIfEmpty()

                    // LEFT JOIN AsuransiPasien
                join ap in _applicationDbContext.AsuransiPasiens.AsNoTracking()
                    on k.AsuransiPasienId equals (Guid?)ap.AsuransiPasienId into asuransiPasienGroup
                from ap in asuransiPasienGroup.DefaultIfEmpty()

                    // LEFT JOIN Asuransi
                join ar in _applicationDbContext.Asuransis.AsNoTracking()
                    on ap.AsuransiId equals ar.AsuransiId into asuransiGroup
                from ar in asuransiGroup.DefaultIfEmpty()

                select new
                {
                    // Data Surat Pengantar
                    a.SuratPengantarRawatInapId,
                    a.KunjunganId,
                    a.NomorSuratPengantar,
                    a.Diagnosa,
                    a.ICDId,
                    a.AlasanRanap,
                    a.RencanaTindakLanjut,
                    a.AsalUnit,
                    a.Status,
                    a.IndikasiTindakan,
                    a.JenisOperasi,
                    a.HarapanHasil,
                    a.IsAdaHambatan,
                    a.PathTTDDokterDPJP,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    // Data Kunjungan
                    k.NoRekamMedis,
                    k.TipePasien,
                    k.TipePembayaran,
                    k.JenisKunjungan,
                    k.IsFinished,

                    // Data Dokter
                    DokterId = d.DokterId,
                    DokterName = d.NmDokter,

                    // Data Poli
                    PoliklinikId = poli.PoliklinikId,
                    PoliklinikName = poli.NamaPoliklinik,

                    // Data Pasien
                    PasienId = p.PendaftaranPasienBaruId,
                    PasienName = p.NamaLengkap,
                    JenisKelamin = p.JenisKelamin,
                    Umur = HitungUmurLengkap(p.TanggalLahir),
                    p.NoPasien,

                    // Data Asuransi
                    NamaAsuransi = ar != null ? ar.NamaAsuransi : null,
                    NoPolis = ap != null ? ap.NoPolis : null
                }
            ).FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(new { message = "Surat pengantar tidak ditemukan." });
            }

            return Ok(result);
        }


        //[HttpGet("DataPasien/{kunjunganId}")]
        //public async Task<IActionResult> GetDataPasienByKunjunganId(Guid kunjunganId)
        //{
        //    // Cek apakah KunjunganId valid  
        //    if (kunjunganId == Guid.Empty)
        //    {
        //        return BadRequest(new { message = "KunjunganId tidak valid." });
        //    }

        //    // Ambil data Kunjungan berdasarkan KunjunganId  
        //    var query =
        //        from k in _applicationDbContext.Kunjungans
        //        join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
        //        join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
        //        join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId
        //        where k.KunjunganID == kunjunganId && (k.IsDelete == false || k.IsDelete == null)
        //        select new
        //        {
        //            k.KunjunganID,
        //            k.NoRekamMedis,
        //            k.TipePasien,
        //            k.TipePembayaran,
        //            k.JenisKunjungan,
        //            k.IsFinished,
        //            k.TglMasukRanap,
        //            k.TglKeluarRanap,
        //            DokterId = d.DokterId,
        //            DokterName = d.NmDokter,
        //            PoliklinikId = poli.PoliklinikId,
        //            PoliklinikName = poli.NamaPoliklinik,
        //            PasienId = p.PendaftaranPasienBaruId,
        //            PasienName = p.NamaLengkap,
        //            Umur = HitungUmurLengkap(p.TanggalLahir),
        //            p.JenisKelamin,
        //        };

        //    var result = await query.FirstOrDefaultAsync();

        //    if (result == null)
        //    {
        //        return NotFound(new { message = "Data pasien tidak ditemukan." });
        //    }

        //    return Ok(new
        //    {
        //        message = "Data pasien ditemukan || 200 OK",
        //        data = result
        //    });
        //}

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SuratPengantarRawatInapViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Model tidak valid.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new
                        {
                            field = x.Key,
                            errors = x.Value!.Errors.Select(e => e.ErrorMessage)
                        })
                });
            }

            if (!vm.KunjunganId.HasValue || vm.KunjunganId == Guid.Empty)
            {
                return BadRequest(new { message = "KunjunganId wajib diisi." });
            }

            if (!vm.DokterDPJPId.HasValue || vm.DokterDPJPId == Guid.Empty)
            {
                return BadRequest(new { message = "DokterDPJPId wajib diisi." });
            }

            if (!vm.DepositRanap.HasValue || vm.DepositRanap <= 0)
            {
                return BadRequest(new
                {
                    message = "Kunjungan IP (rawat inap) wajib mengisi nominal deposit."
                });
            }

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi." });
                }

                var userActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (userActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan." });
                }

                var now = DateTime.UtcNow;
                var nowOffset = DateTimeOffset.UtcNow;
                var kunjunganId = vm.KunjunganId.Value;
                var dokterDpjpId = vm.DokterDPJPId.Value;
                var userActiveId = userActive.UserActiveId;

                var ttd = await _ttdService.CheckTTDAsync(dokterDpjpId);
                if (ttd == null || string.IsNullOrWhiteSpace(ttd.Path))
                {
                    return BadRequest(new { message = "TTD dokter DPJP tidak ditemukan." });
                }

                await using var transaction = await _applicationDbContext.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable);

                // Cek ulang di dalam transaction supaya lebih aman dari request paralel
                var canCreate = await CanCreateSuratForKunjunganAsync(kunjunganId);
                if (!canCreate)
                {
                    return StatusCode(StatusCodes.Status409Conflict, new
                    {
                        message = "Kunjungan ini sudah dalam proses rawat inap aktif."
                    });
                }

                // Generate nomor surat
                var nomorSurat = await GenerateNomorSuratPengantarRanapAsync(now.Year);

                // Buat surat pengantar rawat inap
                var surat = new SuratPengantarRawatInap
                {
                    SuratPengantarRawatInapId = Guid.NewGuid(),
                    KunjunganId = kunjunganId,
                    Diagnosa = vm.Diagnosa,
                    ICDId = vm.ICDId,
                    AlasanRanap = vm.AlasanRanap,
                    RencanaTindakLanjut = vm.RencanaTindakLanjut,
                    AsalUnit = vm.AsalUnit,
                    NomorSuratPengantar = nomorSurat,
                    Status = FilterStatusSuratPengantarRanap.Menunggu.ToString(),
                    IndikasiTindakan = vm.IndikasiTindakan,
                    JenisOperasi = vm.JenisOperasi,
                    TawaranLayanan = vm.TawaranLayanan,
                    HarapanHasil = vm.HarapanHasil,
                    IsAdaHambatan = vm.IsAdaHambatan,
                    PathTTDDokterDPJP = ttd.Path,
                    CreateBy = userActiveId,
                    CreateDateTime = nowOffset
                };

                _applicationDbContext.SuratPengantarRawatInaps.Add(surat);

                // Cari biaya admin rawat inap
                var biayaAdmin = await _applicationDbContext.BiayaAdministrasis
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BiayaAdministrasiKode == "IP");

                // Invoice billing
                var invoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                    kunjunganId,
                    now
                );

                // Tambahkan billing biaya admin bila ada
                if (biayaAdmin != null)
                {
                    // Ambil urutan billing terakhir utk kunjungan ini
                    var existingBillingCount = await _applicationDbContext.Billings
                        .CountAsync(b => b.KunjunganId == kunjunganId);

                    var billingKode = (existingBillingCount + 1).ToString("D3");

                    var bill = new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = kunjunganId,
                        ItemId = biayaAdmin.BiayaAdministrasiId,
                        NamaItem = biayaAdmin.NamaBiayaAdministrasi,
                        HargaItem = biayaAdmin.NominalBiayaAdministrasi,
                        QtyItem = 1,
                        SubTotalItem = biayaAdmin.NominalBiayaAdministrasi,
                        InvoiceBilling = invoice,
                        IsListWhiteOff = false,
                        BillingKode = billingKode,
                        JenisBilling = "Biaya Admin",
                        StatusBilling = false,
                        BillingDate = now,
                        TanggalInvoice = now,
                        TanggalJatuhTempo = now.Date.AddDays(90),
                        CreateDateTime = nowOffset,
                        CreateBy = userActiveId
                    };

                    _applicationDbContext.Billings.Add(bill);
                }

                // Buat deposit ranap
                var noKwitansi = await _depositRanapNumberService.GenerateNoKwitansiAsync();

                var deposit = new DepositRanap
                {
                    DepositRanapId = Guid.NewGuid(),
                    KunjunganId = kunjunganId,
                    TglTransaksi = now,
                    NominalMasuk = vm.DepositRanap.Value,
                    SaldoDeposit = vm.DepositRanap.Value,
                    NoKwitansi = noKwitansi,
                    StatusDeposit = "Pemasukkan",
                    CreateDateTime = nowOffset,
                    CreateBy = userActiveId
                };

                _applicationDbContext.DepositRanaps.Add(deposit);

                var result = await _applicationDbContext.SaveChangesAsync();

                if (result <= 0)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        message = "Data tidak berhasil disimpan ke database."
                    });
                }

                await transaction.CommitAsync();

                // Kirim notifikasi setelah commit berhasil
                await _hubContext.Clients.All.SendAsync("Surat pengantar rawat inap ditambah", new
                {
                    action = "create",
                    suratid = surat.SuratPengantarRawatInapId,
                    kunjunganId = surat.KunjunganId
                });

                return Created("", new
                {
                    message = "Tambah Data Berhasil",
                    suratPengantarRawatInapId = surat.SuratPengantarRawatInapId,
                    nomorSurat = surat.NomorSuratPengantar
                });
            }
            catch (DbUpdateException)
            {
                // Detail error sebaiknya di-log, jangan dikirim mentah ke client
                return StatusCode(500, new
                {
                    message = "Gagal menyimpan data ke database."
                });
            }
            catch (Exception)
            {
                // Detail error sebaiknya di-log, jangan dikirim mentah ke client
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan internal pada server."
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SuratPengantarRawatInapViewModel vm)
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
                var data = await _applicationDbContext.SuratPengantarRawatInaps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // get path ttd dokterDPJP
                var ttd = await _ttdService.CheckTTDAsync((Guid)vm.DokterDPJPId);

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.Diagnosa = vm.Diagnosa;
                data.ICDId = vm.ICDId;
                data.AlasanRanap = vm.AlasanRanap;
                data.RencanaTindakLanjut = vm.RencanaTindakLanjut;
                data.AsalUnit = vm.AsalUnit;
                data.IndikasiTindakan = vm.IndikasiTindakan;
                data.JenisOperasi = vm.JenisOperasi;
                data.TawaranLayanan = vm.TawaranLayanan;
                data.HarapanHasil = vm.HarapanHasil;
                data.IsAdaHambatan = vm.IsAdaHambatan;
                data.PathTTDDokterDPJP = ttd.Path;


                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.SuratPengantarRawatInaps.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                // Notifikasi signalR
                await _hubContext.Clients.All.SendAsync("Surat pengantar rawat inap diupdate", new
                {
                    action = "update",
                    suratid = data.SuratPengantarRawatInapId,
                    kunjunganId = data.KunjunganId,
                });

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

        [HttpPut("{id}/Status-SuratPengantarRanap")]
        public async Task<IActionResult> UpdateIsFinished(Guid id, [FromBody] StatusSuratPengantarRanapVM request)
        {
            var data = await _applicationDbContext.SuratPengantarRawatInaps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.Status = request.status.ToString();
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
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
                var data = await _applicationDbContext.SuratPengantarRawatInaps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.SuratPengantarRawatInaps.Update(data);
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
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            Guid? suratpengantarId = null,
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

                // ======================================================
                // 1) BASE QUERY (FILTER DULU ✅) -> ini yang bikin cepat
                // ======================================================
                var baseQuery = _applicationDbContext.SuratPengantarRawatInaps
                    .AsNoTracking()
                    .Where(a => (a.IsDelete == false || a.IsDelete == null));

                if (suratpengantarId.HasValue)
                {
                    baseQuery = baseQuery.Where(u=>u.SuratPengantarRawatInapId==suratpengantarId.Value);
                }

                // Filter search (NomorSuratPengantar) di tabel utama -> paling murah
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.ToLower()}%";
                    baseQuery = baseQuery.Where(a => EF.Functions.ILike(a.NomorSuratPengantar, pattern));
                }

                // Filter tanggal (CreateDateTime) di tabel utama -> paling murah
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                    DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                    baseQuery = baseQuery.Where(a =>
                        a.CreateDateTime >= startUtc &&
                        a.CreateDateTime <= endUtc);
                }

                // Filter periode (CreateDateTime) di tabel utama -> paling murah
                if (periode.HasValue)
                {
                    DateTime today = DateTime.UtcNow.Date;

                    switch (periode)
                    {
                        case PeriodeFilter.Today:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime.Date == today);
                            break;

                        case PeriodeFilter.ThisWeek:
                            baseQuery = baseQuery.Where(a =>
                                a.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                                a.CreateDateTime.Date <= today);
                            break;

                        case PeriodeFilter.LastWeek:
                            baseQuery = baseQuery.Where(a =>
                                a.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                a.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                            break;

                        case PeriodeFilter.ThisMonth:
                            baseQuery = baseQuery.Where(a =>
                                a.CreateDateTime.Month == today.Month &&
                                a.CreateDateTime.Year == today.Year);
                            break;

                        case PeriodeFilter.LastMonth:
                            var lastMonth = today.AddMonths(-1);
                            baseQuery = baseQuery.Where(a =>
                                a.CreateDateTime.Month == lastMonth.Month &&
                                a.CreateDateTime.Year == lastMonth.Year);
                            break;

                        case PeriodeFilter.ThisYear:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime.Year == today.Year);
                            break;

                        case PeriodeFilter.LastYear:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime.Year == today.Year - 1);
                            break;

                        case PeriodeFilter.Last3Months:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime >= today.AddMonths(-3));
                            break;

                        case PeriodeFilter.Last6Months:
                            baseQuery = baseQuery.Where(a => a.CreateDateTime >= today.AddMonths(-6));
                            break;
                    }
                }

                // ======================================================
                // 2) JOIN BARU SETELAH FILTER ✅
                // ======================================================
                var query =
                    from a in baseQuery

                        // join Kunjungan + filter delete di kunjungan (penting!)
                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                            .Where(k => (k.IsDelete == false || k.IsDelete == null))
                        on a.KunjunganId equals k.KunjunganID

                    join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                        on k.PasienId equals p.PendaftaranPasienBaruId

                    join d in _applicationDbContext.Dokters.AsNoTracking()
                        on k.DokterId equals d.DokterId

                    join poli in _applicationDbContext.Polikliniks.AsNoTracking()
                        on k.PoliklinikId equals poli.PoliklinikId

                    // LEFT JOIN UserActive
                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u.UserActiveId into userJoin
                    from u in userJoin.DefaultIfEmpty()

                         // LEFT JOIN AsuransiPasien
                    join ap in _applicationDbContext.AsuransiPasiens.AsNoTracking()
                        on k.AsuransiPasienId equals (Guid?)ap.AsuransiPasienId into asuransiPasienGroup
                    from ap in asuransiPasienGroup.DefaultIfEmpty()

                        // LEFT JOIN Asuransi
                    join ar in _applicationDbContext.Asuransis.AsNoTracking()
                        on ap.AsuransiId equals ar.AsuransiId into asuransiGroup
                    from ar in asuransiGroup.DefaultIfEmpty()

                    select new
                    {
                        // Data Surat Pengantar
                        a.SuratPengantarRawatInapId,
                        a.KunjunganId,
                        a.NomorSuratPengantar,
                        a.Diagnosa,
                        a.ICDId,
                        a.AlasanRanap,
                        a.RencanaTindakLanjut,
                        a.AsalUnit,
                        a.Status,
                        a.IndikasiTindakan,
                        a.JenisOperasi,
                        a.TawaranLayanan,
                        a.HarapanHasil,
                        a.IsAdaHambatan,
                        a.PathTTDDokterDPJP,
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,

                        // Data Kunjungan
                        k.NoRekamMedis,
                        k.TipePasien,
                        k.TipePembayaran,
                        k.JenisKunjungan,
                        k.IsFinished,

                        // Data Dokter
                        DokterId = d.DokterId,
                        DokterName = d.NmDokter,

                        // Data Poli
                        PoliklinikId = poli.PoliklinikId,
                        PoliklinikName = poli.NamaPoliklinik,

                        // Data Pasien
                        PasienId = p.PendaftaranPasienBaruId,
                        PasienName = p.NamaLengkap,
                        JenisKelamin = p.JenisKelamin,
                        Umur = HitungUmurLengkap(p.TanggalLahir),
                        p.NoPasien,

                        // data Asuransi
                        NamaAsuransi = ar != null ? ar.NamaAsuransi : null,
                        NoPolis = ap != null ? ap.NoPolis : null
                    };

                // ======================================================
                // 3) SORTING (benar ASC/DESC)
                // ======================================================
                bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

                query = desc
                    ? orderBy switch
                    {
                        "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                        "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                        "NomorSuratPengantar" => query.OrderByDescending(u => u.NomorSuratPengantar),
                        _ => query.OrderByDescending(u => u.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                        "CreateByName" => query.OrderBy(u => u.CreateByName),
                        "NomorSuratPengantar" => query.OrderBy(u => u.NomorSuratPengantar),
                        _ => query.OrderBy(u => u.CreateDateTime)
                    };

                // ======================================================
                // 4) PAGINATION (ASYNC ✅)
                // ======================================================
                var totalRows = await query.CountAsync();
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
                    return NotFound(new { message = "Page not found." });

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                // ======================================================
                // 5) RETURN
                // ======================================================
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
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

    }
}
