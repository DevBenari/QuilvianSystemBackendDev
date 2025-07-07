using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;

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

        private readonly ILogger<BillingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BillingController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BillingController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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

        [HttpGet("GetBillingByKunjunganId/{kunjunganId}")]
        public async Task<IActionResult> GetBillingByKunjunganId(Guid kunjunganId)
        {
            // Ambil semua billing berdasarkan KunjunganId
            var billings = await _applicationDbContext.Billings
                .Where(b => b.KunjunganId == kunjunganId)
                .ToListAsync();

            var query =
                from k in _applicationDbContext.Kunjungans
                join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                join a in _applicationDbContext.Asuransis on k.AsuransiId equals a.AsuransiId into asuransiTempGroup
                from a in asuransiTempGroup.DefaultIfEmpty()
                join ap in _applicationDbContext.AsuransiPasiens on p.PendaftaranPasienBaruId.ToString() equals ap.PasienId into asuransiPasienGroup
                from ap in asuransiPasienGroup.DefaultIfEmpty()
                join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId
                join r in _applicationDbContext.Reseps.Where(resep => !resep.IsDelete) on k.KunjunganID equals r.KunjunganId into resepGroup
                from r in resepGroup.DefaultIfEmpty()
                join dr in _applicationDbContext.DetailReseps.Where(detail => !detail.IsDelete) on r.ResepId equals dr.ResepId into detailResepGroup
                from dr in detailResepGroup.DefaultIfEmpty()
                join o in _applicationDbContext.Obats on dr.ObatId equals o.ObatId into obatGroup
                from o in obatGroup.DefaultIfEmpty()
                join to in _applicationDbContext.TindakanKunjungans on k.KunjunganID equals to.KunjunganId into tindakanGroup
                from to in tindakanGroup.DefaultIfEmpty()
                join t in _applicationDbContext.Tindakans on to.TindakanId equals t.TindakanId into tindakanMasterGroup
                from t in tindakanMasterGroup.DefaultIfEmpty()
                join adm in _applicationDbContext.BiayaAdministrasis on k.JenisKunjungan equals adm.BiayaAdministrasiKode into admGroup
                from adm in admGroup.DefaultIfEmpty()
                join kasir in _applicationDbContext.MainKasirs on k.KunjunganID equals kasir.KunjunganId into kasirGroup
                from kasir in kasirGroup.DefaultIfEmpty()
                join dk in _applicationDbContext.MainKasirDetails on kasir.KasirId equals dk.MainKasirId into MainKasirDetailsGroup
                from dk in MainKasirDetailsGroup.DefaultIfEmpty()
                join mp in _applicationDbContext.MetodePembayarans on dk.MetodePembayaranId equals mp.MetodePembayaranId into metodeGroup
                from mp in metodeGroup.DefaultIfEmpty()
                join rc in _applicationDbContext.Racikans on dr.RacikanId equals rc.RacikanId into racikanGroup
                from rc in racikanGroup.DefaultIfEmpty()
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
                    dk,
                    mp,
                    rc
                };

            var result = await query.ToListAsync();

            var kasirData = result.GroupBy(x => x.k.KunjunganID)
                .Select(group =>
                {
                    var firstItem = group.First();

                    return new
                    {
                        KasirId = firstItem.kasir?.KasirId ?? Guid.Empty,
                        firstItem.k.KunjunganID,
                        firstItem.k.JenisKunjungan,
                        NoRegistrasi = firstItem.k.Antrian,
                        firstItem.k.TipePembayaran,
                        TglRegistrasi = firstItem.k.CreateDateTime.ToString("dddd, dd MMMM yyyy", new CultureInfo("id-ID")),
                        firstItem.k.PasienId,
                        NoRM = firstItem.p?.NoRekamMedis ?? "-",
                        NamaPasien = firstItem.p?.NamaLengkap ?? "-",
                        UmurPasien = HitungUmurLengkap(firstItem.p?.TanggalLahir),
                        NoTelepon1 = firstItem.p?.NoTelepon1 ?? "-",
                        firstItem.p?.JenisKelamin,
                        firstItem.k.AsuransiId,
                        NamaPerusahaan = firstItem.a?.NamaAsuransi ?? null,
                        NoPolis = firstItem.ap?.NoPolis ?? "-",
                        firstItem.k.DokterId,
                        NamaDokter = firstItem.d?.NmDokter ?? "-",
                        firstItem.k.PoliklinikId,
                        NamaPoliklinik = firstItem.poli?.NamaPoliklinik ?? "-",
                        firstItem.adm?.BiayaAdministrasiId,
                        firstItem.adm?.NominalBiayaAdministrasi,
                        PaymentMethodId = firstItem.mp?.MetodePembayaranId,
                        PaymentMethodName = firstItem.mp?.NamaMetode ?? "-",
                        firstItem.k?.IsFinishedKasir,
                        firstItem.kasir?.CreateBy,
                        firstItem.kasir?.CreateDateTime,

                        DaftarResepObat = group
                            .Where(x => x.dr != null && x.o != null)
                            .GroupBy(x => x.dr.DetailResepId)
                            .Select(g =>
                            {
                                var item = g.First();
                                var billing = billings.FirstOrDefault(b =>
                                    b.JenisBilling == "Obat" &&
                                    (b.ItemId == item.dr.ObatId || b.ItemId == item.dr.RacikanId));

                                return new
                                {
                                    item.r.ResepId,
                                    item.dr.DetailResepId,
                                    BillingId = billing?.BillingId,
                                    billing?.JenisBilling,
                                    billing?.BillingKode,
                                    item.dr.ObatId,
                                    item.dr?.JumlahIteratur,
                                    NamaObat = item.o.ObatName,
                                    item.dr?.Qty,
                                    HargaObat = item.o.HargaJual,
                                    item.dr?.RacikanId,
                                    item.dr?.KeteranganRacikan,
                                    item.rc?.NamaRacikan,
                                    item.dr?.IsIteratur,
                                    item.dr?.JarakPenebusan,
                                    TglMulaiIteratur = item.dr.TglMulaiIteratur?.ToString("yyyy-MM-dd"),
                                    MasaAktifIteratur = item.dr.MasaAktifIteratur?.ToString("yyyy-MM-dd"),
                                    StatusCoverObat = item.dr?.StatusCoverObat ?? false,
                                    TotalBiayaObat = item.dr?.TotalHargaObat ?? item.dr?.Qty * item.o.HargaJual
                                };
                            }).ToList(),

                        DaftarTindakan = group
                            .Where(x => x.to != null && x.t != null)
                            .GroupBy(x => x.to.TindakanKunjunganId)
                            .Select(g =>
                            {
                                var item = g.First();
                                var billing = billings.FirstOrDefault(b =>
                                    b.JenisBilling == "Tindakan" && b.ItemId == item.to.TindakanId);

                                return new
                                {
                                    item.to.TindakanId,
                                    BillingId = billing?.BillingId,
                                    billing?.JenisBilling,
                                    billing?.BillingKode,
                                    item.t.NamaTindakan,
                                    QtyTindakan = item.to.Quantity,
                                    HargaTindakan = item.to.Total,
                                    StatusCoverTindakan = firstItem.a != null &&
                                        _applicationDbContext.TindakanAsuransis.Any(y =>
                                            y.TindakanId == item.to.TindakanId && y.AsuransiId == firstItem.a.AsuransiId)
                                };
                            }).ToList(),

                        DaftarBiayaAdmin = billings
                            .Where(b => b.JenisBilling == "Biaya Admin")
                            .Select(b => new
                            {
                                b.BillingId,
                                b.JenisBilling,
                                b.BillingKode,
                                b.NamaItem,
                                b.HargaItem,
                                b.QtyItem,
                                b.SubTotalItem,
                                b.Keterangan
                            }).ToList(),

                        TotalObat = group
                            .Where(x => x.dr != null && x.o != null)
                            .DistinctBy(x => x.dr.DetailResepId)
                            .Sum(x => x.dr.Qty * x.o.HargaJual),

                        TotalTindakan = group
                            .Where(x => x.to != null && x.t != null)
                            .DistinctBy(x => x.to.TindakanKunjunganId)
                            .Sum(x => x.to.Quantity * (x.to.Total ?? 0)),

                        TotalTagihan =
                            group.Where(x => x.dr != null && x.o != null)
                                .DistinctBy(x => x.dr.DetailResepId)
                                .Sum(x => x.dr.Qty * x.o.HargaJual)
                            + group.Where(x => x.to != null && x.t != null)
                                .DistinctBy(x => x.to.TindakanKunjunganId)
                                .Sum(x => x.to.Quantity * (x.to.Total ?? 0))
                            + billings.Where(b => b.JenisBilling == "Biaya Admin")
                                .Sum(b => b.SubTotalItem ?? 0)
                    };
                }).ToList();

            if (!kasirData.Any())
            {
                return NotFound(new { message = "Data billing untuk kunjungan ini tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new { status = "success", data = kasirData.FirstOrDefault() });
        }
        //public async Task<IActionResult> GetBillingByKunjunganId(Guid kunjunganId)
        //{
        //    var kunjungan = await _applicationDbContext.Billings.Where(b => b.KunjunganId == kunjunganId && !b.IsDelete).ToListAsync();
        //    if (kunjungan == null)
        //        return NotFound(new { message = "Data kunjungan tidak ditemukan!" });

        //    return Ok(new
        //    {
        //        message = "Ditemukan || 200 OK",
        //        data = kunjungan
        //    });
        //}

        [HttpGet("BillingObat/{kunjunganId}")]
        public async Task<IActionResult> GetResepDetailsByKunjunganIdEntity(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // LEFT JOIN Reseps x DetailReseps
                var resepQuery = await (
                    from r in _applicationDbContext.Reseps
                    where r.KunjunganId == kunjunganId
                    join dr in _applicationDbContext.DetailReseps
                        on r.ResepId equals dr.ResepId into drGroup
                    from dr in drGroup.DefaultIfEmpty() // LEFT JOIN
                    select new
                    {
                        r.KunjunganId,
                        r.AsuransiId,
                        ObatId = dr != null ? dr.ObatId : (Guid?)null,
                        IsRacikan = dr != null ? dr.IsRacikan : (bool?)null,
                        RacikanId = dr != null ? dr.RacikanId : (Guid?)null,
                        Signa = dr != null ? dr.Signa : null,
                        SignaTambahan = dr != null ? dr.SignaTambahan : null
                    }).ToListAsync();

                var result = new List<object>();

                foreach (var item in resepQuery)
                {
                    // Nama Obat
                    var obat = await _applicationDbContext.Obats
                        .Where(o => o.ObatId == item.ObatId)
                        .FirstOrDefaultAsync();

                    // Status cover asuransi (false jika tidak ditemukan)
                    bool isCovered = await _applicationDbContext.ObatAsuransis
                        .AnyAsync(oa => oa.AsuransiId == item.AsuransiId && oa.ObatId == item.ObatId && !oa.IsDelete);

                    // Tentukan ItemId (obat atau racikan)
                    var itemId = (item.IsRacikan == true && item.RacikanId.HasValue)
                        ? item.RacikanId.Value
                        : item.ObatId;

                    var billing = await _applicationDbContext.Billings
                        .Where(b => b.KunjunganId == item.KunjunganId && b.ItemId == itemId)
                        .FirstOrDefaultAsync();

                    var racikan = item.RacikanId.HasValue
                        ? await _applicationDbContext.Racikans
                            .Where(mr => mr.RacikanId == item.RacikanId)
                            .Select(mr => mr.NamaRacikan)
                            .FirstOrDefaultAsync()
                        : null;

                    result.Add(new
                    {
                        billing?.BillingId,
                        item.KunjunganId,
                        item.ObatId,
                        NamaObat = item.IsRacikan == true ? racikan : obat?.ObatName,
                        HargaSatuanObat = billing?.HargaItem,
                        SubTotalObat = item.IsRacikan == true ? billing?.HargaItem : billing?.HargaItem * billing?.QtyItem,
                        item.IsRacikan,
                        item.RacikanId,
                        item.Signa,
                        item.SignaTambahan,
                        IsCoveredByAsuransi = isCovered,
                        BilledQty = billing?.QtyItem,
                        billing?.BillingKode,
                        billing?.JenisBilling,
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("ObatFarmasiByKunjunganId/{kunjunganId}")]
        public async Task<IActionResult> GetObatFarmasiByKunjunganId(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // Ambil data resep utama
                var resep = await _applicationDbContext.Reseps
                    .Where(r => r.KunjunganId == kunjunganId)
                    .OrderByDescending(r => r.CreateDateTime)
                    .FirstOrDefaultAsync();

                if (resep == null)
                    return NotFound(new { message = "Resep tidak ditemukan untuk kunjungan ini." });

                // Ambil detail resep terkait
                var detailList = await (
                    from dr in _applicationDbContext.DetailReseps
                    where dr.ResepId == resep.ResepId
                    select new
                    {
                        dr.ObatId,
                        dr.IsRacikan,
                        dr.RacikanId,
                        dr.KeteranganRacikan,
                        dr.DosisRacikan,
                        dr.TakaranDosis,
                        dr.Signa,
                        dr.SignaTambahan,
                        dr.StatusPengambilanObat
                    }
                ).ToListAsync();

                var daftarObat = new List<object>();

                foreach (var item in detailList)
                {
                    // Ambil data obat
                    var obat = await _applicationDbContext.Obats
                        .FirstOrDefaultAsync(o => o.ObatId == item.ObatId);

                    // Cek status asuransi
                    bool isCovered = await _applicationDbContext.ObatAsuransis
                        .AnyAsync(oa => oa.AsuransiId == resep.AsuransiId && oa.ObatId == item.ObatId && !oa.IsDelete);

                    var itemId = item.IsRacikan == true ? item.RacikanId : item.ObatId;

                    var billing = await _applicationDbContext.Billings
                        .FirstOrDefaultAsync(b => b.KunjunganId == resep.KunjunganId && b.ItemId == itemId);

                    var Racikan = item.IsRacikan == true
                        ? await _applicationDbContext.Racikans
                            .Where(r => r.RacikanId == item.RacikanId)
                            .FirstOrDefaultAsync()
                        : null;

                    daftarObat.Add(new
                    {
                        billing?.BillingId,
                        itemId,
                        NamaObat = obat?.ObatName,
                        HargaSatuanObat = billing?.HargaItem,
                        SubTotalObat = item.IsRacikan == true ? billing?.HargaItem : billing?.HargaItem * billing?.QtyItem,
                        item.IsRacikan,
                        item.RacikanId,
                        NamaRacikan = item.IsRacikan == true ? Racikan?.NamaRacikan : null,
                        item.KeteranganRacikan,
                        item.DosisRacikan,
                        item.TakaranDosis,
                        item.Signa,
                        item.SignaTambahan,
                        IsCoveredByAsuransi = isCovered,
                        BilledQty = billing?.QtyItem,
                        billing?.BillingKode,
                        billing?.JenisBilling,
                        item.StatusPengambilanObat
                    });
                }

                // Return resep + daftar obat
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
                    DaftarObat = daftarObat
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("BillingTindakan/{kunjunganId}")]
        public async Task<IActionResult> GetBillingTindakanByKunjunganId(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var tindakanQuery = await (
                    from tk in _applicationDbContext.TindakanKunjungans
                    join k in _applicationDbContext.Kunjungans
                        on tk.KunjunganId equals k.KunjunganID
                    where k.AsuransiId != null // agar aman saat .Value  

                    join mt in _applicationDbContext.Tindakans
                        on tk.TindakanId equals mt.TindakanId

                    join tda in _applicationDbContext.TindakanAsuransis
                        on new { TindakanId = tk.TindakanId, AsuransiId = k.AsuransiId.Value }
                        equals new { TindakanId = tda.TindakanId, AsuransiId = tda.AsuransiId } into tdaGroup
                    from mta in tdaGroup.DefaultIfEmpty()

                    join b in _applicationDbContext.Billings
                        on new { KunjunganId = tk.KunjunganId, ItemId = tk.TindakanId }
                        equals new { KunjunganId = b.KunjunganId.Value, ItemId = b.ItemId.Value } into billingGroup
                    from billing in billingGroup.DefaultIfEmpty()

                    where tk.KunjunganId == kunjunganId && (mta == null || !mta.IsDelete)

                    select new
                    {
                        tk.KunjunganId,
                        tk.TindakanId,
                        NamaTindakan = mt.NamaTindakan,
                        IsCoveredByAsuransi = mta != null,

                        // Info Billing  
                        BillingId = billing != null ? billing.BillingId : (Guid?)null,
                        BillingKode = billing.BillingKode,
                        HargaItem = billing.HargaItem,
                        QtyItem = billing.QtyItem,
                        SubTotalItem = billing.SubTotalItem,
                        BillingDate = billing.BillingDate
                    }
                ).ToListAsync();

                return Ok(tindakanQuery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("BillingAdmin/{kunjunganId}")]
        public async Task<IActionResult> GetBiayaAdministrasiByKunjunganId(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var billing = await _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == kunjunganId && b.BillingKode == "Biaya Admin" && !b.IsDelete)
                    .Select(b => new
                    {
                        b.BillingId,
                        b.KunjunganId,
                        b.ItemId,
                        b.NamaItem,
                        b.HargaItem,
                        b.QtyItem,
                        b.SubTotalItem,
                        b.BillingKode,
                        b.BillingDate
                    })
                    .FirstOrDefaultAsync();

                if (billing == null)
                {
                    return NotFound(new { message = "Data billing administrasi tidak ditemukan untuk kunjungan ini." });
                }

                return Ok(new
                {
                    message = "Data billing administrasi ditemukan.",
                    data = billing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

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

                        harga = obat.HargaJual;
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
    }
}
