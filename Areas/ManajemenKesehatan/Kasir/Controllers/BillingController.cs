using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using static BillingKunjunganReadService;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IBillingKunjunganReadService _billingKunjunganReadService;
        private readonly ILogger<BillingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPerkiraanBillingRanapService _perkiraanRanap;

        public BillingController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BillingController> logger,
            IWebHostEnvironment webHostEnvironment,
            IBillingKunjunganReadService billingKunjunganReadService,
            IPerkiraanBillingRanapService perkiraanRanap)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _billingKunjunganReadService = billingKunjunganReadService;
            _perkiraanRanap = perkiraanRanap;
        }

        public static string HitungUmurLengkap(DateTime? tanggalLahir)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBillingById(Guid id)
        {
            var billing = await _applicationDbContext.Billings
                .FirstOrDefaultAsync(b => b.BillingId == id && !b.IsDelete);
            if (billing == null)
                return NotFound(new { message = "Data billing tidak ditemukan!" });
            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = billing
            });
        }

        [HttpPut("{id}/Status-PengambilanObat")]
        public async Task<IActionResult> UpdateStatusObatBilling(Guid id, [FromBody] StatusItemBillingViewModel request)
        {
            var data = await _applicationDbContext.Billings.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusPengambilan = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            _applicationDbContext.Billings.Update(data);
            
            // update status pengambilan obat pada DetailResep
            var resep = await _applicationDbContext.Reseps
            .FirstOrDefaultAsync(r => r.KunjunganId == data.KunjunganId && !r.IsDelete);

            if (resep == null)
            {
                return NotFound(new { message = "Resep tidak ditemukan untuk kunjungan ini." });
            }

            // Langkah 2: Ambil detail resep yang aktif (tidak dihapus) berdasarkan ResepId
            var detailResepList = await _applicationDbContext.DetailReseps
                .Where(dr => dr.ResepId == resep.ResepId && !dr.IsDelete)
                .ToListAsync();

            // Langkah 3: Cek apakah ada detail resep yang cocok dengan ObatId dari Billing.ItemId
            var detailResep = detailResepList
                .FirstOrDefault(dr => dr.ObatId == data.ItemId);

            if (detailResep == null)
            {
                return NotFound(new { message = "Obat dengan ItemId tidak ditemukan di resep untuk kunjungan ini." });
            }
            // Langkah 4: Update status pengambilan obat pada DetailResep
            detailResep.StatusPengambilanObat = request.Status;
            detailResep.UpdateDateTime = DateTimeOffset.UtcNow;
            detailResep.UpdateBy = userId;

            _applicationDbContext.DetailReseps.Update(detailResep);
            _applicationDbContext.SaveChanges();

            return Ok(new { message = "Status pengambilan obat berhasil diperbarui." });
        }

        [HttpGet("GetBillingByKunjunganId/{kunjunganId}")]
        public async Task<IActionResult> GetBillingByKunjunganId(Guid kunjunganId, CancellationToken ct)
        {
            var data = await _billingKunjunganReadService.GetBillingKeseluruhanAsync(kunjunganId, DateTime.Now, ct);
            if (data == null)
                return NotFound(new { message = "Data billing tidak ditemukan." });

            return Ok(new { status = "success", data });
        }

        [HttpGet("perkiraan-billing-ip/{kunjunganId}")]
        public async Task<IActionResult> GetPerkiraanBillingRawatInap(Guid kunjunganId, CancellationToken ct)
        {
            try
            {
                var data = await _perkiraanRanap.GetPerkiraanBillingIpAsync(kunjunganId, ct);
                if (data == null)
                    return NotFound(new { status = "failed", message = "Data kunjungan tidak ditemukan." });

                return Ok(new { status = "success", data });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = "failed", message = ex.Message });
            }
        }

        [HttpGet("ObatFarmasiByKunjunganId/{kunjunganId}")]
        public async Task<IActionResult> GetObatFarmasiByKunjunganId(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var resep = await _applicationDbContext.Reseps
                    .Where(r => r.KunjunganId == kunjunganId)
                    .OrderByDescending(r => r.CreateDateTime)
                    .FirstOrDefaultAsync();

                if (resep == null)
                    return NotFound(new { message = "Resep tidak ditemukan untuk kunjungan ini." });

                // Load DetailResep, Obat, Racikan, Billing
                var detailResepData = await (
                    from dr in _applicationDbContext.DetailReseps
                    where dr.ResepId == resep.ResepId

                    join o in _applicationDbContext.Obats on dr.ObatId equals o.ObatId into obatJoin
                    from obat in obatJoin.DefaultIfEmpty()

                    join rcn in _applicationDbContext.Racikans on dr.RacikanId equals rcn.RacikanId into racikanJoin
                    from racikan in racikanJoin.DefaultIfEmpty()

                    join b in _applicationDbContext.Billings
                        on new { resep.KunjunganId, ItemId = (Guid?)(dr.IsRacikan == true ? dr.RacikanId : dr.ObatId) }
                        equals new { b.KunjunganId, b.ItemId } into billingJoin
                    from billing in billingJoin.DefaultIfEmpty()

                    join satuan in _applicationDbContext.Satuans on obat.SatuanId equals satuan.SatuanId into satuanJoin
                    from satuan in satuanJoin.DefaultIfEmpty()

                    join bentuk in _applicationDbContext.BentukObats on obat.BentukObatId equals bentuk.BentukSatuanId into bentukJoin
                    from bentuk in bentukJoin.DefaultIfEmpty()

                    select new
                    {
                        DetailResep = dr,
                        Obat = obat,
                        Racikan = racikan,
                        Billing = billing
                    }
                ).ToListAsync();

                // Ambil semua komposisi racikan sekaligus
                var allRacikanIdsInDetail = detailResepData
                    .Where(x => x.DetailResep.IsRacikan == true && x.DetailResep.RacikanId.HasValue)
                    .Select(x => x.DetailResep.RacikanId.Value)
                    .Distinct()
                    .ToList();

                var racikanDetailsWithObat = await (
                    from rd in _applicationDbContext.RacikanDetails
                    where allRacikanIdsInDetail.Contains((Guid)rd.RacikanId)
                    join o in _applicationDbContext.Obats on rd.ObatId equals o.ObatId
                    select new
                    {
                        rd.RacikanId,
                        rd.ObatId,
                        ObatName = o.ObatName,
                        rd.QtyUsed,
                        rd.KomposisiDosis,
                        o.HTEPrice
                    }
                ).ToListAsync();

                var racikanDetailsGrouped = racikanDetailsWithObat
                    .GroupBy(x => x.RacikanId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var daftarObat = new List<object>();
                var daftarRacikan = new List<object>();

                foreach (var item in detailResepData)
                {
                    var dr = item.DetailResep;
                    var obat = item.Obat;
                    var racikan = item.Racikan;
                    var billing = item.Billing;
                    var isRacikan = dr.IsRacikan.GetValueOrDefault(false);

                    if (isRacikan && dr.RacikanId != null)
                    {
                        List<object> komposisiList = new List<object>();
                        if (racikanDetailsGrouped.TryGetValue(dr.RacikanId.Value, out var rdList))
                        {
                            komposisiList = rdList.Select(rd => new
                            {
                                rd.ObatId,
                                rd.ObatName,
                                rd.QtyUsed,
                                rd.KomposisiDosis,
                                rd.HTEPrice
                            }).ToList<object>();
                        }

                        daftarRacikan.Add(new
                        {
                            billing?.BillingId,
                            dr.RacikanId,
                            NamaRacikan = racikan?.NamaRacikan,
                            racikan?.KodeRacikan,
                            dr.Signa,
                            dr.SignaTambahan,
                            HargaSatuanObat = billing?.HargaItem,
                            SubTotalObat = billing?.SubTotalItem,
                            BilledQty = billing?.QtyItem,
                            billing?.BillingKode,
                            billing?.JenisBilling,
                            dr.StatusPengambilanObat,
                            Komposisi = komposisiList,
                            IsIteratur = dr.IsIteratur.GetValueOrDefault(false),
                            dr.JumlahIteratur,
                            TglMulaiIteratur = dr.TglMulaiIteratur?.ToString("yyyy-MM-dd"),
                            dr.JarakPenebusan,
                            MasaAktifIteratur = dr.MasaAktifIteratur?.ToString("yyyy-MM-dd")
                        });
                    }
                    else
                    {
                        daftarObat.Add(new
                        {
                            billing?.BillingId,
                            dr.ObatId,
                            NamaObat = obat?.ObatName,
                            dr.TakaranDosis,
                            dr.Signa,
                            dr.SignaTambahan,
                            HargaSatuanObat = billing?.HargaItem,
                            SubTotalObat = (billing?.QtyItem ?? 0) * (billing?.HargaItem ?? 0),
                            IsCoveredByAsuransi = false, // default false, karena tidak dicek
                            BilledQty = billing?.QtyItem,
                            billing?.BillingKode,
                            billing?.JenisBilling,
                            dr.StatusPengambilanObat,
                            dr.StatusCoverObat,
                            IsIteratur = dr.IsIteratur.GetValueOrDefault(false),
                            dr.JumlahIteratur,
                            TglMulaiIteratur = dr.TglMulaiIteratur?.ToString("yyyy-MM-dd"),
                            dr.JarakPenebusan,
                            MasaAktifIteratur = dr.MasaAktifIteratur?.ToString("yyyy-MM-dd")
                        });
                    }
                }

                return Ok(new
                {
                    resep.ResepId,
                    resep.KunjunganId,
                    resep.PasienId,
                    resep.NamaPasien,
                    resep.DokterId,
                    resep.NamaDokter,
                    resep.PoliklinikId,
                    resep.NamaPoliklinik,
                    resep.StatusPembuatanResep,
                    resep.StatusPengambilanResep,
                    resep.IsLunas,
                    resep.IsCancelled,
                    DaftarObat = daftarObat,
                    DaftarRacikan = daftarRacikan
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetObatFarmasiByKunjunganId: {ex.Message}");
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        //{
        //    try
        //    {
        //        if (!_applicationDbContext.Database.CanConnect())
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //        var resep = await _applicationDbContext.Reseps
        //            .Where(r => r.KunjunganId == kunjunganId)
        //            .OrderByDescending(r => r.CreateDateTime)
        //            .FirstOrDefaultAsync();

        //        if (resep == null)
        //            return NotFound(new { message = "Resep tidak ditemukan untuk kunjungan ini." });

        //        var detailList = await _applicationDbContext.DetailReseps
        //            .Where(dr => dr.ResepId == resep.ResepId)
        //            .ToListAsync();

        //        var daftarObat = new List<object>();
        //        var daftarRacikan = new List<object>();

        //        foreach (var item in detailList)
        //        {
        //            var billing = await _applicationDbContext.Billings
        //                .FirstOrDefaultAsync(b => b.KunjunganId == resep.KunjunganId &&
        //                                          (item.IsRacikan == true ? b.ItemId == item.RacikanId : b.ItemId == item.ObatId));

        //            bool isCovered = await _applicationDbContext.ObatAsuransis
        //                .AnyAsync(oa => oa.AsuransiId == resep.AsuransiId &&
        //                                oa.ObatId == item.ObatId &&
        //                                !oa.IsDelete);

        //            if (item.IsRacikan == true && item.RacikanId != null)
        //            {
        //                var racikan = await _applicationDbContext.Racikans
        //                    .FirstOrDefaultAsync(r => r.RacikanId == item.RacikanId);

        //                var racikanDetails = await (
        //                    from rd in _applicationDbContext.RacikanDetails
        //                    join o in _applicationDbContext.Obats on rd.ObatId equals o.ObatId
        //                    where rd.RacikanId == item.RacikanId
        //                    select new
        //                    {
        //                        rd.ObatId,
        //                        o.ObatName,
        //                        rd.QtyUsed,
        //                        rd.KomposisiDosis,
        //                        o.HargaJual,
        //                        Subtotal = rd.QtyUsed * o.HargaJual
        //                    }
        //                ).ToListAsync();

        //                daftarRacikan.Add(new
        //                {
        //                    billing?.BillingId,
        //                    item.RacikanId,
        //                    NamaRacikan = racikan?.NamaRacikan,
        //                    item.KeteranganRacikan,
        //                    item.DosisRacikan,
        //                    item.Signa,
        //                    item.SignaTambahan,
        //                    racikan?.KodeRacikan,
        //                    HargaSatuanObat = billing?.HargaItem,
        //                    SubTotalObat = billing?.SubTotalItem,
        //                    BilledQty = billing?.QtyItem,
        //                    billing?.BillingKode,
        //                    billing?.JenisBilling,

        //                    item.StatusPengambilanObat,
        //                    Komposisi = racikanDetails
        //                });
        //            }
        //            else
        //            {
        //                var obat = await _applicationDbContext.Obats
        //                    .FirstOrDefaultAsync(o => o.ObatId == item.ObatId);

        //                daftarObat.Add(new
        //                {
        //                    billing?.BillingId,
        //                    item.ObatId,
        //                    NamaObat = obat?.ObatName,
        //                    item.TakaranDosis,
        //                    item.Signa,
        //                    item.SignaTambahan,
        //                    HargaSatuanObat = billing?.HargaItem,
        //                    SubTotalObat = billing?.QtyItem * billing?.HargaItem,
        //                    IsCoveredByAsuransi = isCovered,
        //                    BilledQty = billing?.QtyItem,
        //                    billing?.BillingKode,
        //                    billing?.JenisBilling,
        //                    item.StatusPengambilanObat
        //                });
        //            }
        //        }

        //        return Ok(new
        //        {
        //            resep.ResepId,
        //            resep.KunjunganId,
        //            resep.PasienId,
        //            resep.NamaPasien,
        //            resep.DokterId,
        //            resep.NamaDokter,
        //            resep.PoliklinikId,
        //            resep.NamaPoliklinik,
        //            resep.StatusPembuatanResep,
        //            resep.StatusPengambilanResep,
        //            resep.IsLunas,
        //            resep.IsCancelled,
        //            DaftarObat = daftarObat,
        //            DaftarRacikan = daftarRacikan
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
        //    }
        //}

        //[HttpGet("BillingTindakan/{kunjunganId}")]
        //public async Task<IActionResult> GetBillingTindakanByKunjunganId(Guid kunjunganId)
        //{
        //    try
        //    {
        //        if (!_applicationDbContext.Database.CanConnect())
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //        var tindakanQuery = await (
        //            from tk in _applicationDbContext.TindakanKunjungans
        //            join k in _applicationDbContext.Kunjungans
        //                on tk.KunjunganId equals k.KunjunganID
        //            where k.AsuransiId != null // agar aman saat .Value  

        //            join mt in _applicationDbContext.Tindakans
        //                on tk.TindakanId equals mt.TindakanId

        //            join tda in _applicationDbContext.TindakanAsuransis
        //                on new { TindakanId = tk.TindakanId, AsuransiId = k.AsuransiId.Value }
        //                equals new { TindakanId = tda.TindakanId, AsuransiId = tda.AsuransiId } into tdaGroup
        //            from mta in tdaGroup.DefaultIfEmpty()

        //            join b in _applicationDbContext.Billings
        //                on new { KunjunganId = tk.KunjunganId, ItemId = tk.TindakanId }
        //                equals new { KunjunganId = b.KunjunganId.Value, ItemId = b.ItemId.Value } into billingGroup
        //            from billing in billingGroup.DefaultIfEmpty()

        //            where tk.KunjunganId == kunjunganId && (mta == null || !mta.IsDelete)

        //            select new
        //            {
        //                tk.KunjunganId,
        //                tk.TindakanId,
        //                NamaTindakan = mt.NamaTindakan,
        //                IsCoveredByAsuransi = mta != null,

        //                // Info Billing  
        //                BillingId = billing != null ? billing.BillingId : (Guid?)null,
        //                BillingKode = billing.BillingKode,
        //                HargaItem = billing.HargaItem,
        //                QtyItem = billing.QtyItem,
        //                SubTotalItem = billing.SubTotalItem,
        //                BillingDate = billing.BillingDate
        //            }
        //        ).ToListAsync();

        //        return Ok(tindakanQuery);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
        //    }
        //}

        //[HttpGet("BillingAdmin/{kunjunganId}")]
        //public async Task<IActionResult> GetBiayaAdministrasiByKunjunganId(Guid kunjunganId)
        //{
        //    try
        //    {
        //        if (!_applicationDbContext.Database.CanConnect())
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //        var billing = await _applicationDbContext.Billings
        //            .Where(b => b.KunjunganId == kunjunganId && b.BillingKode == "Biaya Admin" && !b.IsDelete)
        //            .Select(b => new
        //            {
        //                b.BillingId,
        //                b.KunjunganId,
        //                b.ItemId,
        //                b.NamaItem,
        //                b.HargaItem,
        //                b.QtyItem,
        //                b.SubTotalItem,
        //                b.BillingKode,
        //                b.BillingDate
        //            })
        //            .FirstOrDefaultAsync();

        //        if (billing == null)
        //        {
        //            return NotFound(new { message = "Data billing administrasi tidak ditemukan untuk kunjungan ini." });
        //        }

        //        return Ok(new
        //        {
        //            message = "Data billing administrasi ditemukan.",
        //            data = billing
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
        //    }
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBilling(Guid id, [FromBody] BillingViewModel vm)
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

                // cari data
                var billing = await _applicationDbContext.Billings
                    .FirstOrDefaultAsync(b => b.BillingId == id);

                if (billing == null)
                    return NotFound(new { message = "Data billing tidak ditemukan." });

                var kodePrefix = billing.JenisBilling.Trim().ToLower();

                decimal harga = 0;

                switch (kodePrefix)
                {
                    case "obat":
                        var obat = await _applicationDbContext.Obats
                            .FirstOrDefaultAsync(o => o.ObatId == billing.ItemId && !o.IsDelete);
                        if (obat == null)
                            return NotFound(new { message = "Data obat tidak ditemukan." });

                        harga = obat.HTEPrice;
                        break;

                    case "tindakan":
                        //// Ambil Tindakan
                        //var tindakan = await _applicationDbContext.Tindakans
                        //    .FirstOrDefaultAsync(t => t.TindakanId == billing.ItemId && !t.IsDelete);
                        //if (tindakan == null)
                        //    return NotFound(new { message = "Data tindakan tidak ditemukan." });

                        //// Ambil kunjungan
                        //var kunjungan = await _applicationDbContext.Kunjungans
                        //    .FirstOrDefaultAsync(k => k.KunjunganID == billing.KunjunganId);
                        //if (kunjungan == null)
                        //    return NotFound(new { message = "Data kunjungan tidak ditemukan." });

                        //// Ambil kelas berdasarkan jenis kunjungan
                        //var kelas = await _applicationDbContext.Kelass
                        //    .FirstOrDefaultAsync(k => k.KodeKelas == kunjungan.JenisKunjungan);
                        //if (kelas == null)
                        //    return NotFound(new { message = "Kelas untuk jenis kunjungan ini tidak ditemukan." });

                        //// Ambil tarif kelas untuk tindakan dan kelas
                        //var tarifKelas = await _applicationDbContext.TarifKelass
                        //    .FirstOrDefaultAsync(t => t.TindakanId == tindakan.TindakanId && t.KelasId == kelas.KelasId);
                        //if (tarifKelas == null)
                        //    return NotFound(new { message = "Tarif untuk tindakan dan kelas ini tidak ditemukan." });

                        //harga = tarifKelas.TarifTotal ?? 0;
                        return Forbid("Tidak bisa mengedit Tindakan.");

                    default:
                        return BadRequest(new { message = "BillingKode tidak dikenali (harus OB atau TD)." });
                }

                // Update billing
                billing.QtyItem = vm.QtyItem;
                billing.HargaItem = harga;
                billing.SubTotalItem = harga * (vm.QtyItem ?? 1); // default 1 jika null
                billing.DiskonId = vm.DiskonId;
                billing.Keterangan = vm.Keterangan;
                billing.UpdateDateTime = DateTimeOffset.UtcNow;
                billing.UpdateBy = userActiveId;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Billing berhasil diperbarui." });
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


        [HttpGet("GetBillingPaged")]
        public async Task<IActionResult> GetBillingPaged(
            [FromQuery] Guid? kunjunganId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null,
            [FromQuery] DateTime? asOf = null,
            CancellationToken ct = default)
        {
            // guard sederhana
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // (opsional) jika startDate/endDate diisi tapi salah urutan
            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
                return BadRequest(new { status = "failed", message = "startDate tidak boleh lebih besar dari endDate." });

            var result = await _billingKunjunganReadService.GetBillingPagedAsync(new BillingPagedQuery
            {
                KunjunganId = kunjunganId,
                Page = page,
                PageSize = pageSize,
                StartDate = startDate,
                EndDate = endDate,
                Periode = periode,
                AsOf = asOf
            }, ct);

            return Ok(result);
        }



    }
}
