using System.Data;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;
using static BillingKunjunganReadService;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class MainKasirController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHubContext<MainKasirHub> _hubContext;
        private readonly ILogger<MainKasirController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private IBillingService _billingService;
        private readonly ITTDService _ttdService;
        private readonly INoKwitansiService _noKwitansiService;
        private readonly IGenerateUrutanAngsuran _generateUrutanAngsuran;
        private readonly ICountAngsuran _countAngsuran;
        private readonly IBillingKunjunganReadService _billingKunjunganReadService;
        private readonly IResepStockService _resepTebusStockService;


        public MainKasirController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<MainKasirController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<MainKasirHub> hubContext,
            IBillingService billingService,
            ITTDService ttdService,
            INoKwitansiService noKwitansiService,
            IGenerateUrutanAngsuran generateUrutanAngsuran,
            ICountAngsuran countAngsuran,
            IBillingKunjunganReadService billingKunjunganReadService,
            IResepStockService resepTebusStockService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _billingService = billingService;
            _ttdService = ttdService;
            _noKwitansiService = noKwitansiService;
            _generateUrutanAngsuran = generateUrutanAngsuran;
            _countAngsuran = countAngsuran;
            _billingKunjunganReadService = billingKunjunganReadService;
            _resepTebusStockService = resepTebusStockService;
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
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.KasirId,
                             a.KunjunganId,
                             a.DiskonId,
                             a.GrandTotalPembayaran,
                             a.Keterangan,
                             a.TglPembayaran,
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
            // ================================
            // 1. AMBIL KUNJUNGAN BERDASARKAN KASIR
            // ================================
            var kunjunganId = await _applicationDbContext.MainKasirs
                .Where(mk => mk.KasirId == id)
                .Select(mk => mk.KunjunganId)
                .FirstOrDefaultAsync();

            if (kunjunganId == Guid.Empty)
                return NotFound(new { message = "Kunjungan tidak ditemukan untuk KasirId ini." });


            // ================================
            // 2. AMBIL BILLINGS
            // ================================
            var billings = await _applicationDbContext.Billings
                .Where(b => b.KunjunganId == kunjunganId && (b.IsDelete == false || b.IsDelete == null))
                .ToListAsync();


            // ================================
            // 3. QUERY JOIN UTAMA (SATU KALI)
            // ================================
            var query =
                from k in _applicationDbContext.Kunjungans
                join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                join a in _applicationDbContext.Asuransis on k.AsuransiId equals a.AsuransiId into asuransiTempGroup
                from a in asuransiTempGroup.DefaultIfEmpty()
                join ap in _applicationDbContext.AsuransiPasiens on p.PendaftaranPasienBaruId equals ap.PasienId into asuransiPasienGroup
                from ap in asuransiPasienGroup.DefaultIfEmpty()
                join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId

                // LAB
                join lbd in _applicationDbContext.LabBookingDetails on k.PasienId equals lbd.PasienId into labGroup
                from lbd in labGroup.DefaultIfEmpty()
                join lp in _applicationDbContext.LabPemeriksaans on lbd.PemeriksaanLabId equals lp.PemeriksaanLabId into pemeriksaanGroup
                from lp in pemeriksaanGroup.DefaultIfEmpty()

                    // RESEP
                join r in _applicationDbContext.Reseps.Where(x => !x.IsDelete) on k.KunjunganID equals r.KunjunganId into resepGroup
                from r in resepGroup.DefaultIfEmpty()
                join dr in _applicationDbContext.DetailReseps.Where(x => !x.IsDelete) on r.ResepId equals dr.ResepId into detailResepGroup
                from dr in detailResepGroup.DefaultIfEmpty()
                join o in _applicationDbContext.Obats on dr.ObatId equals o.ObatId into obatGroup
                from o in obatGroup.DefaultIfEmpty()
                join ra in _applicationDbContext.Racikans on dr.RacikanId equals ra.RacikanId into racikanGroup
                from ra in racikanGroup.DefaultIfEmpty()

                    // TINDAKAN
                join to in _applicationDbContext.TindakanKunjungans on k.KunjunganID equals to.KunjunganId into tindakanGroup
                from to in tindakanGroup.DefaultIfEmpty()
                join t in _applicationDbContext.Tindakans on to.TindakanId equals t.TindakanId into tindakanMasterGroup
                from t in tindakanMasterGroup.DefaultIfEmpty()

                    // ADMIN & KASIR
                join adm in _applicationDbContext.BiayaAdministrasis on k.JenisKunjungan equals adm.BiayaAdministrasiKode into admGroup
                from adm in admGroup.DefaultIfEmpty()
                join kasir in _applicationDbContext.MainKasirs on k.KunjunganID equals kasir.KunjunganId into kasirGroup
                from kasir in kasirGroup.DefaultIfEmpty()
                join dk in _applicationDbContext.MainKasirDetails on kasir.KasirId equals dk.MainKasirId into MainKasirDetailsGroup
                from dk in MainKasirDetailsGroup.DefaultIfEmpty()
                join mp in _applicationDbContext.MetodePembayarans on dk.MetodePembayaranId equals mp.MetodePembayaranId into metodeGroup
                from mp in metodeGroup.DefaultIfEmpty()

                where kasir.KasirId == id && !k.IsDelete
                select new { k, p, a, ap, d, poli, r, dr, o, ra, to, t, adm, kasir, dk, mp, lbd, lp };

            var result = await query.ToListAsync();
            if (!result.Any())
                return NotFound(new { message = "Data tidak ditemukan." });


            // ================================
            // 4. AMBIL RACIKAN ID
            // ================================
            var racikanIds = result
                .Where(x => x.dr != null && x.dr.IsRacikan == true && x.dr.RacikanId.HasValue)
                .Select(x => x.dr.RacikanId.Value)
                .Distinct()
                .ToList();


            // ================================
            // 5. AMBIL KOMPOSISI RACIKAN SEKALIGUS
            // ================================
            List<dynamic> racikanDetails = racikanIds.Any()
                ? await (
                    from rd in _applicationDbContext.RacikanDetails
                    join ob in _applicationDbContext.Obats on rd.ObatId equals ob.ObatId
                    where rd.RacikanId.HasValue
                          && racikanIds.Contains(rd.RacikanId.Value)
                          && !rd.IsDelete
                    select new
                    {
                        RacikanId = rd.RacikanId.Value,
                        rd.DetailRacikanId,
                        rd.ObatId,
                        ob.ObatName,
                        ob.ObatCode,
                        rd.QtyUsed,
                        rd.KomposisiDosis,
                        rd.CreateBy,
                        rd.CreateDateTime,
                        ob.HTEPrice
                    }
                ).ToListAsync<dynamic>()
                : new List<dynamic>();

            // BUAT MAP RACIKAN → LIST KOMPOSISI
            var racikanMap = racikanDetails
                .GroupBy(x => (Guid)x.RacikanId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(item => new {
                        item.DetailRacikanId,
                        item.ObatId,
                        item.ObatName,
                        item.ObatCode,
                        item.QtyUsed,
                        item.KomposisiDosis,
                        item.CreateBy,
                        item.CreateDateTime,
                        item.HTEPrice
                    }).ToList()
                );

            // ================================
            // 6. GROUP BY KUNJUNGAN → BENTUK RESPONSE
            // ================================
            var data = result
                .GroupBy(x => x.k.KunjunganID)
                .Select(group =>
                {
                    var first = group.First();

                    // PEMERIKSAAN LAB
                    var daftarLab = group
                        .Where(x => x.lbd != null && (x.lbd.IsDelete == false || x.lbd.IsDelete == null))
                        .GroupBy(x => x.lbd.DetailBookingLabId)
                        .Select(g =>
                        {
                            var item = g.First();
                            var billing = billings.FirstOrDefault(b =>
                                b.ItemId == item.lbd.DetailBookingLabId &&
                                b.JenisBilling == "Pemeriksaan Lab");

                            return new
                            {
                                item.lbd.DetailBookingLabId,
                                item.lp?.NamaPemeriksaan,
                                item.lp?.HargaPemeriksaan,
                                Qty = billing?.QtyItem ?? 1,
                                Subtotal = billing?.SubTotalItem ?? item.lp?.HargaPemeriksaan ?? 0,
                                BillingId = billing?.BillingId,
                                BillingKode = billing?.BillingKode,
                                StatusPemeriksaan = item.lbd?.StatusPemeriksaan ?? "-"
                            };
                        }).ToList();

                    var totalLab = daftarLab.Sum(x => x.Subtotal);


                    // OBAT NON RACIKAN
                    var daftarObat = group
                        .Where(x => x.dr != null && x.o != null && (x.dr.IsRacikan == false || x.dr.IsRacikan == null))
                        .GroupBy(x => x.dr.DetailResepId)
                        .Select(g =>
                        {
                            var item = g.First();
                            var billing = billings.FirstOrDefault(b =>
                                b.ItemId == item.dr.ObatId &&
                                b.JenisBilling == "Obat");

                            return new
                            {
                                item.dr.ObatId,
                                item.o.ObatName,
                                Qty = billing?.QtyItem ?? item.dr.Qty,
                                Harga = billing?.HargaItem ?? item.o.HTEPrice,
                                Subtotal = billing?.SubTotalItem ?? (item.dr.Qty * item.o.HTEPrice),
                                BillingId = billing?.BillingId,
                                BillingKode = billing?.BillingKode,
                                item.dr.Signa,
                                item.dr.SignaTambahan,
                                item.dr.StatusPengambilanObat
                            };
                        }).ToList();

                    var totalObat = daftarObat.Sum(x => x.Subtotal);


                    // RACIKAN + KOMPOSISI
                    var daftarRacikan = group
                        .Where(x => x.dr != null && x.dr.IsRacikan == true && x.ra != null)
                        .GroupBy(x => x.dr.RacikanId)
                        .Select(g =>
                        {
                            var x = g.First();
                            var bill = billings.FirstOrDefault(b =>
                                b.ItemId == x.dr.RacikanId && b.JenisBilling == "Obat");

                            racikanMap.TryGetValue(x.dr.RacikanId.Value, out var komps);

                            return new
                            {
                                x.r?.ResepId,
                                x.dr.RacikanId,
                                x.ra.NamaRacikan,
                                x.ra.KodeRacikan,
                                Qty = bill?.QtyItem,
                                Harga = bill?.HargaItem,
                                Subtotal = bill?.SubTotalItem,
                                BillingId = bill?.BillingId,
                                BillingKode = bill?.BillingKode,
                                x.dr.Signa,
                                x.dr.SignaTambahan,
                                x.dr.StatusPengambilanObat,
                                Komposisi = komps?.Select(k => new {
                                    k.ObatId,
                                    k.ObatName,
                                    k.QtyUsed,
                                    k.KomposisiDosis,
                                    k.HTEPrice
                                })
                            };
                        }).ToList();

                    var totalRacikan = daftarRacikan.Sum(x => x.Subtotal);


                    // TINDAKAN
                    var daftarTindakan = group
                        .Where(x => x.to != null && x.t != null)
                        .GroupBy(x => x.to.TindakanKunjunganId)
                        .Select(g =>
                        {
                            var item = g.First();
                            var billing = billings.FirstOrDefault(b =>
                                b.ItemId == item.to.TindakanId &&
                                b.JenisBilling == "Tindakan");

                            return new
                            {
                                item.t.TindakanId,
                                item.t.NamaTindakan,
                                Qty = billing?.QtyItem ?? item.to.Quantity ?? 1,
                                Harga = billing?.HargaItem ?? item.to.Total ?? 0,
                                Subtotal = billing?.SubTotalItem ?? (item.to.Quantity ?? 1) * (item.to.Total ?? 0),
                                BillingId = billing?.BillingId,
                                BillingKode = billing?.BillingKode
                            };
                        }).ToList();

                    var totalTindakan = daftarTindakan.Sum(x => x.Subtotal);


                    // ADMINISTRASI
                    var daftarAdmin = billings
                        .Where(b => b.JenisBilling == "Biaya Admin")
                        .Select(b => new
                        {
                            b.BillingId,
                            b.NamaItem,
                            b.HargaItem,
                            b.QtyItem,
                            b.SubTotalItem,
                            b.BillingKode
                        }).ToList();

                    var totalAdmin = daftarAdmin.Sum(x => x.SubTotalItem ?? 0);


                    return new
                    {
                        first.k.KunjunganID,
                        first.k.JenisKunjungan,
                        first.kasir?.KasirId,
                        first.k.PasienId,
                        first.p?.NamaLengkap,
                        first.p?.NoRekamMedis,
                        first.d?.NmDokter,
                        first.poli?.NamaPoliklinik,
                        first.k.CreateDateTime,
                        first.k.TipePembayaran,
                        first.a?.NamaAsuransi,

                        DaftarPemeriksaanLab = daftarLab,
                        DaftarObat = daftarObat,
                        DaftarRacikan = daftarRacikan,
                        DaftarTindakan = daftarTindakan,
                        DaftarBiayaAdmin = daftarAdmin,

                        TotalPemeriksaanLab = totalLab,
                        TotalObat = totalObat,
                        TotalRacikan = totalRacikan,
                        TotalTindakan = totalTindakan,
                        TotalBiayaAdmin = totalAdmin,
                        TotalKeseluruhan = totalLab + totalObat + totalRacikan + totalTindakan + totalAdmin
                    };
                })
                .FirstOrDefault();


            return Ok(new { status = "success", data });
        }

        [HttpGet("KasirId/{id}")]
        public async Task<IActionResult> GetRiwayatBayarByKasirId(Guid id)
        {
            // =========================
            // 1) Header (MainKasir)
            // =========================
            var header = await (
                from x in _applicationDbContext.MainKasirs.AsNoTracking()
                join p0 in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on x.PasienId equals p0.PendaftaranPasienBaruId into pGroup
                from p in pGroup.DefaultIfEmpty()

                where x.KasirId == id && x.IsDelete != true
                select new
                {
                    x.KasirId,
                    x.KunjunganId,
                    x.PasienId,
                    x.NoBill,
                    x.StatusBilling,
                    // ✅ tambahan pasien
                    NamaLengkap = p != null ? p.NamaLengkap : null,
                    NoRekamMedis = p != null ? p.NoRekamMedis : null,

                    x.InvoiceBilling,
                    x.JumlahAngsuran,
                    x.StatusPembayaran,
                    x.IsVerified,
                    x.TTDUserVerfiedId,
                    x.PathUserVerified,
                    x.Deposito,
                    x.SubTotalAsuransi,
                    x.SubTotalMandiri,
                    x.SubTotalAsuransiExcess,
                    x.TotalPembayaran,
                    x.GrandTotalPembayaran,
                    x.TotalBiayaObat,
                    x.TotalBiayaTindakan,
                    x.Keterangan,
                    x.TglPembayaran,
                    x.DiskonId,

                    x.CreateDateTime,
                    CreateBy = (Guid?)x.CreateBy,
                    x.UpdateDateTime,
                    UpdateBy = (Guid?)x.UpdateBy
                }
            ).FirstOrDefaultAsync();


            if (header == null)
                return NotFound(new { message = "MainKasir tidak ditemukan." });

            // =========================
            // 2) Details (MainKasirDetail)
            // =========================
            var details = await _applicationDbContext.MainKasirDetails
                .AsNoTracking()
                .Where(d => d.MainKasirId == id && d.IsDelete != true)
                .OrderBy(d => d.TglPembayaran ?? DateTime.MaxValue)
                .ThenBy(d => d.CreateDateTime)
                .Select(d => new
                {
                    d.MainKasirDetailId,
                    d.MainKasirId,
                    d.MetodePembayaranId,
                    d.ReferenceId,
                    d.KunjunganId,
                    d.PasienId,
                    d.TotalPembayaran,
                    d.SisaPembayaran,
                    d.NoKwitansi,
                    d.AngsuranKe,
                    d.NamaMetode,
                    d.NominalPembayaran,
                    d.Keterangan,
                    d.TglPembayaran,

                    d.CreateDateTime,
                    CreateBy = (Guid?)d.CreateBy,     // aman Guid/Guid?
                    d.UpdateDateTime,
                    UpdateBy = (Guid?)d.UpdateBy      // aman Guid/Guid?
                })
                .ToListAsync();

            // =========================
            // 3) Load nama user sekali (hindari N+1)
            // =========================
            var userIds = new HashSet<Guid>();

            if (header.CreateBy.HasValue) userIds.Add(header.CreateBy.Value);
            if (header.UpdateBy.HasValue) userIds.Add(header.UpdateBy.Value);
            if (header.TTDUserVerfiedId.HasValue) userIds.Add(header.TTDUserVerfiedId.Value);

            foreach (var d in details)
            {
                if (d.CreateBy.HasValue) userIds.Add(d.CreateBy.Value);
                if (d.UpdateBy.HasValue) userIds.Add(d.UpdateBy.Value);
            }

            var userDict = await _applicationDbContext.UserActives
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserActiveId))
                .Select(u => new { u.UserActiveId, u.FullName })
                .ToDictionaryAsync(x => x.UserActiveId, x => x.FullName);

            string? GetUserName(Guid? userId)
                => userId.HasValue && userDict.TryGetValue(userId.Value, out var name) ? name : null;

            // =========================
            // 4) Return anonymous object
            // =========================
            return Ok(new
            {
                message = "Berhasil mengambil MainKasir + Details || 200 OK",
                data = new
                {
                    Header = new
                    {
                        header.KasirId,
                        header.KunjunganId,
                        header.PasienId,
                        header.NamaLengkap,
                        header.NoRekamMedis,
                        header.NoBill,
                        header.StatusBilling,
                        header.InvoiceBilling,
                        header.JumlahAngsuran,
                        header.StatusPembayaran,
                        header.IsVerified,
                        header.TTDUserVerfiedId,
                        VerifiedByName = GetUserName(header.TTDUserVerfiedId),
                        header.PathUserVerified,
                        header.GrandTotalPembayaran,
                        header.SubTotalAsuransi,
                        header.SubTotalAsuransiExcess,
                        header.SubTotalMandiri,
                        header.TotalBiayaObat,
                        header.TotalBiayaTindakan,
                        header.Keterangan,
                        header.TglPembayaran,
                        header.DiskonId,

                        header.CreateDateTime,
                        header.CreateBy,
                        CreateByName = GetUserName(header.CreateBy),

                        header.UpdateDateTime,
                        header.UpdateBy,
                        UpdateByName = GetUserName(header.UpdateBy),
                    },
                    Details = details.Select(d => new
                    {
                        d.MainKasirDetailId,
                        d.MainKasirId,
                        d.MetodePembayaranId,
                        d.ReferenceId,
                        d.KunjunganId,
                        d.PasienId,
                        d.TotalPembayaran,
                        d.SisaPembayaran,
                        d.NoKwitansi,
                        d.AngsuranKe,
                        d.NamaMetode,
                        d.NominalPembayaran,
                        d.Keterangan,
                        d.TglPembayaran,

                        d.CreateDateTime,
                        d.CreateBy,
                        CreateByName = GetUserName(d.CreateBy),

                        d.UpdateDateTime,
                        d.UpdateBy,
                        UpdateByName = GetUserName(d.UpdateBy),
                    }).ToList()
                }
            });
        }

        [HttpGet("Billing-Kasir/{kunjunganId:guid}")]
        public async Task<IActionResult> GetBillingKasirByKunjunganId(
            Guid kunjunganId,
            [FromQuery] DateTime? asOf = null,
            [FromQuery] EnumJenisKunjungan? jenisKunjungan = null,
            [FromQuery] string? asalKunjungan = null,
            CancellationToken ct = default)
        {
            var billingDto = await _billingKunjunganReadService
                .GetBillingByKunjunganId(
                    kunjunganId,
                    asOf,
                    jenisKunjungan,
                    asalKunjungan,
                    ct
                );

            if (billingDto == null)
            {
                return NotFound(new
                {
                    status = "not_found",
                    message = "Data billing tidak ditemukan atau tidak sesuai filter kunjungan.",
                    filter = new
                    {
                        kunjunganId,
                        jenisKunjungan,
                        asalKunjungan
                    }
                });
            }

            var kasirs = await _billingKunjunganReadService
                .GetMainKasirDanDetailPembayaranAsync(kunjunganId, ct);

            return Ok(new
            {
                status = "success",
                data = new
                {
                    KunjunganId = kunjunganId,
                    Billing = billingDto,
                    Pembayaran = new
                    {
                        Kasirs = kasirs
                    }
                }
            });
        }

        [HttpGet("Billing-Kasir/{NoRM}")]
        public async Task<IActionResult> GetRiwayatBillingPasienByNoRm(
        string NoRM,
        [FromQuery] DateTime? asOf = null,
        CancellationToken ct = default)
        {
            // =========================
            // 1️⃣ Ambil riwayat billing dari service
            // =========================
            var riwayat = await _billingKunjunganReadService
                .GetRiwayatBillingPasienByNoRmFastAsync(NoRM, asOf, ct);

            // =========================
            // 2️⃣ Safety null handling
            // =========================
            riwayat ??= new List<object>();

            return Ok(new
            {
                status = "success",
                data = new
                {
                    NoRekamMedis = NoRM,
                    AsOf = asOf ?? DateTime.Now,
                    TotalKunjungan = riwayat.Count,
                    Riwayat = riwayat
                }
            });
        }

        [HttpGet("RekapHarian")]
        public async Task<IActionResult> GetPendapatanHarian(
           [FromQuery] Guid kasirUserId,
           [FromQuery] DateTime? tanggal,
           CancellationToken ct)
        {
            if (kasirUserId == Guid.Empty)
                return BadRequest(new { message = "kasirUserId wajib diisi." });

            var result = await _billingKunjunganReadService.GetPendapatanKasirHarianAsync(kasirUserId, tanggal, ct);

            return Ok(new
            {
                status = "success",
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MainKasirViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!vm.KunjunganId.HasValue || vm.KunjunganId.Value == Guid.Empty)
                return BadRequest(new { message = "KunjunganId wajib diisi." });

            if (vm.Details == null || !vm.Details.Any())
                return BadRequest(new { message = "Detail pembayaran wajib diisi minimal 1 item." });

            if (!await _applicationDbContext.Database.CanConnectAsync())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var userActiveId = await _applicationDbContext.UserActives
                .Where(u => u.Email == emailLogin)
                .Select(u => (Guid?)u.UserActiveId)
                .FirstOrDefaultAsync();

            if (!userActiveId.HasValue)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                var kunjunganId = vm.KunjunganId.Value;

                // 1) Validasi kunjungan dan cari deposit
                var kunjunganOk = await _applicationDbContext.Kunjungans
                        .AsNoTracking()
                        .Where(k => k.KunjunganID == kunjunganId && !k.IsDelete)
                        .Select(k => new { k.KunjunganID})
                        .FirstOrDefaultAsync();

                if (kunjunganOk==null)
                    return NotFound(new { message = "Kunjungan tidak ditemukan atau sudah dihapus." });

                // 2) Ambil header kalau sudah ada (header dibuat sekali)
                var existingHeader = await _applicationDbContext.MainKasirs
                    .FirstOrDefaultAsync(k => k.KunjunganId == kunjunganId && !k.IsDelete);

                var isNewHeader = existingHeader == null;
                var kasirId = isNewHeader ? Guid.NewGuid() : existingHeader!.KasirId;

                // 3) Tentukan Tgl Pembayaran
                var tglPembayaran = DateTimeOffset.UtcNow;

                // 4) Total tagihan (prioritas: header -> vm -> detail)
                decimal totalTagihan =
                    (existingHeader?.GrandTotalPembayaran)
                    ?? (vm.GrandTotalPembayaran)
                    ?? (vm.Details.Max(d => d.TotalPembayaran ?? 0m));

                // 5) Total bayar yang sudah pernah masuk (kalau header sudah ada)
                decimal totalPaidBefore = 0m;
                if (!isNewHeader)
                {
                    totalPaidBefore = await _applicationDbContext.MainKasirDetails
                        .AsNoTracking()
                        .Where(d => d.MainKasirId == kasirId)
                        .SumAsync(d => (decimal?)(d.NominalPembayaran ?? 0m)) ?? 0m;
                }

                // Jika sudah lunas sebelumnya, tolak pembayaran tambahan
                var sisaBefore = totalTagihan - totalPaidBefore;
                if (!isNewHeader && sisaBefore <= 0)
                    return Conflict(new { message = "Tagihan untuk kunjungan ini sudah lunas. Tidak dapat menambah pembayaran lagi." });

                // 6) Hitung pembayaran masuk SEKARANG (boleh multi metode)
                var totalNominalBayarNow = vm.Details.Sum(d => d.NominalPembayaran ?? 0m);

                // Sisa setelah pembayaran sekarang
                var totalPaidAfter = totalPaidBefore + totalNominalBayarNow;
                var rawSisaAfter = totalTagihan - totalPaidAfter;

                // Kalau ada overpay, sisa dibuat 0 dan hitung kembalian
                var kembalian = rawSisaAfter < 0 ? Math.Abs(rawSisaAfter) : 0m;
                var sisaAfter = rawSisaAfter < 0 ? 0m : rawSisaAfter;

                // 7) Generate AngsuranKe otomatis (lunas sekali bayar => 0, cicil => max+1)
                var angsuranKe = await _generateUrutanAngsuran.GenerateAsync(
                    kunjunganId,
                    sisaAfter,
                    HttpContext.RequestAborted
                );

                // 8) Status final
                var statusFromVm = (vm.StatusPembayaran ?? "").Trim();
                var finalStatus = (sisaAfter <= 0) ? "Lunas" : (string.IsNullOrWhiteSpace(statusFromVm) ? "Cicil" : statusFromVm);

                // 9) Generate NoKwitansi (per request)
                string? noKwitansi = null;
                if (sisaAfter <= 0) // hanya saat lunas
                {
                    noKwitansi = await _noKwitansiService.GenerateNoKwitansiAsync(tglPembayaran, HttpContext.RequestAborted);
                }

                // 10) TTD dan invoice billing
                var ttd = (vm.TTDUserVerfiedId.HasValue)
                    ? await _ttdService.CheckTTDAsync(vm.TTDUserVerfiedId.Value)
                    : null;

                var ivc = await _applicationDbContext.Billings.AsNoTracking()
                    .Where(b => b.KunjunganId == vm.KunjunganId)
                    .Select(b => b.InvoiceBilling)
                    .FirstOrDefaultAsync();

                // 11) Create / Update header
                MainKasir headerEntity;

                if (isNewHeader)
                {
                    headerEntity = new MainKasir
                    {
                        KasirId = kasirId,
                        KunjunganId = kunjunganId,
                        PasienId = vm.PasienId,
                        JumlahAngsuran = await _countAngsuran.CountAsync((Guid)vm.KunjunganId),
                        StatusPembayaran = finalStatus,
                        IsVerified = vm.IsVerified,
                        InvoiceBilling = ivc,
                        DiskonId = vm.DiskonId,
                        SubTotalAsuransi = vm.SubTotalAsuransi,
                        SubTotalMandiri = vm.SubTotalMandiri,
                        TotalPembayaran = vm.TotalPembayaran,
                        JumlahPajak = vm.JumlahPajak,
                        Deposito = vm.DepositRanap,
                        SisaDeposito = vm.SisaDeposit,
                        GrandTotalPembayaran = vm.GrandTotalPembayaran ?? totalTagihan,
                        TotalBiayaObat = vm.TotalBiayaObat,
                        TotalBiayaTindakan = vm.TotalBiayaTindakan,
                        HargaDiskon = vm.HargaDiskon,
                        Keterangan = vm.Keterangan,
                        TglPembayaran = tglPembayaran,
                        IsDelete = false,
                        TTDUserVerfiedId = vm.TTDUserVerfiedId,
                        PathUserVerified = ttd?.Path,

                        CreateBy = userActiveId.Value,
                        CreateDateTime = DateTimeOffset.UtcNow,
                    };

                    _applicationDbContext.MainKasirs.Add(headerEntity);
                }
                else
                {
                    headerEntity = existingHeader!;

                    // Update status & tanggal pembayaran terakhir
                    headerEntity.StatusPembayaran = finalStatus;
                    headerEntity.TglPembayaran = tglPembayaran;
                    headerEntity.InvoiceBilling = ivc;
                    // kalau mau update field ini tiap cicilan, silakan; kalau tidak, boleh dihapus
                    headerEntity.IsVerified = vm.IsVerified;
                    headerEntity.TTDUserVerfiedId = vm.TTDUserVerfiedId ?? headerEntity.TTDUserVerfiedId;
                    headerEntity.PathUserVerified = (ttd?.Path) ?? headerEntity.PathUserVerified;

                    headerEntity.UpdateBy = userActiveId.Value;
                    headerEntity.UpdateDateTime = DateTimeOffset.UtcNow;

                    _applicationDbContext.MainKasirs.Update(headerEntity);
                }

                // 12) INSERT DETAIL (INI YANG DIUBAH):
                // ✅ running sisa per baris + kwitansi unik per baris + 1 angsuran untuk semua detail dalam request ini
                decimal cumulativePaidNow = 0m;
                var detailEntities = new List<MainKasirDetail>();

                foreach (var detailVm in vm.Details)
                {
                    var bayarNow = detailVm.NominalPembayaran ?? 0m;
                    if (bayarNow <= 0m) continue;

                    cumulativePaidNow += bayarNow;

                    // running sisa berdasarkan sisaBefore (sebelum transaksi ini)
                    var sisaPerDetail = sisaBefore - cumulativePaidNow;
                    if (sisaPerDetail < 0m) sisaPerDetail = 0m; // safety (kalau allow overpay)

                    // ✅ kwitansi unik per baris (konsisten dengan CreateSplit)
                    var noKwitansiPerDetail = await _noKwitansiService.GenerateNoKwitansiAsync(tglPembayaran, ct);

                    var detail = new MainKasirDetail
                    {
                        MainKasirDetailId = Guid.NewGuid(),
                        MainKasirId = kasirId,
                        KunjunganId = kunjunganId,
                        PasienId = detailVm.PasienId ?? vm.PasienId,
                        // Diskon
                        TipeDiskonDokter = detailVm.TipeDiskonDokter,
                        ValueDiskonDokter = detailVm.ValueDiskonDokter,
                        
                        TotalPembayaran = totalTagihan,
                        NominalPembayaran = bayarNow,

                        // ✅ sisa per baris (running)
                        SisaPembayaran = sisaPerDetail,

                        MetodePembayaranId = detailVm.MetodePembayaranId,
                        NoKwitansi = noKwitansiPerDetail,

                        // ✅ sama untuk semua detail dalam request ini
                        AngsuranKe = angsuranKe,

                        ReferenceId = detailVm.ReferenceId,
                        NamaMetode = detailVm.NamaMetode,
                        Keterangan = detailVm.Keterangan,

                        TglPembayaran = tglPembayaran.UtcDateTime,

                        CreateBy = (Guid)userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        IsDelete = false
                    };

                    detailEntities.Add(detail);
                }

                if (detailEntities.Count == 0)
                    return BadRequest(new { message = "Tidak ada detail pembayaran yang valid untuk disimpan." });

                _applicationDbContext.MainKasirDetails.AddRange(detailEntities);

                // 13) Save
                var saved = await _applicationDbContext.SaveChangesAsync(ct);
                if (saved <= 0)
                {
                    await trx.RollbackAsync(ct);
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }

                // 14) Update billing hanya saat berubah jadi lunas
                int affectedBilling = 0;
                var becameLunas = (sisaBefore > 0m && sisaAfter <= 0m);
                if (becameLunas)
                {
                    affectedBilling = await _billingService.MarkBillingKunjunganAsPaidAsync(kunjunganId, (Guid)userActiveId, ct);
                }

                await trx.CommitAsync(ct);

                // 15) SignalR
                await _hubContext.Clients.All.SendAsync("Data pembayaran Created", new
                {
                    Action = isNewHeader ? "create_header" : "append_detail",
                    kasirId = kasirId,
                    kunjunganId = kunjunganId,
                    angsuranKe = angsuranKe,
                    status = finalStatus,
                    billingUpdated = affectedBilling
                }, ct);

                // ✅ karena kwitansi sekarang per baris, return list supaya jelas
                var noKwitansiDetails = detailEntities.Select(d => new
                {
                    d.MetodePembayaranId,
                    d.NominalPembayaran,
                    d.NoKwitansi,
                    d.SisaPembayaran
                }).ToList();

                return Ok(new
                {
                    message = isNewHeader ? "Header kasir dibuat & pembayaran tersimpan." : "Pembayaran cicilan tersimpan.",
                    action = isNewHeader ? "create_header" : "append_detail",
                    kasirId = kasirId,
                    kunjunganId = kunjunganId,

                    // kompatibilitas jika frontend butuh 1 field
                    noKwitansi = noKwitansiDetails.FirstOrDefault()?.NoKwitansi,
                    noKwitansiDetails = noKwitansiDetails,

                    angsuranKe = angsuranKe,
                    statusPembayaran = finalStatus,
                    totalTagihan = totalTagihan,
                    totalPaidBefore = totalPaidBefore,
                    totalPaidNow = totalNominalBayarNow,
                    sisaPembayaran = sisaAfter,
                    kembalian = kembalian,
                    totalDetailInserted = detailEntities.Count,
                    billingUpdated = affectedBilling
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync(HttpContext.RequestAborted);
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(HttpContext.RequestAborted);
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{kasirId}")]
        public async Task<IActionResult> Update(Guid kasirId, [FromBody] MainKasirViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!vm.KunjunganId.HasValue || vm.KunjunganId.Value == Guid.Empty)
                return BadRequest(new { message = "KunjunganId wajib diisi." });

            if (!await _applicationDbContext.Database.CanConnectAsync(ct))
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var userActiveId = await _applicationDbContext.UserActives
                .Where(u => u.Email == emailLogin)
                .Select(u => (Guid?)u.UserActiveId)
                .FirstOrDefaultAsync(ct);

            if (!userActiveId.HasValue)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var kunjunganId = vm.KunjunganId.Value;

                // 1) Ambil header kasir
                var headerEntity = await _applicationDbContext.MainKasirs
                    .FirstOrDefaultAsync(k => k.KasirId == kasirId && !k.IsDelete, ct);

                if (headerEntity == null)
                    return NotFound(new { message = "Header kasir tidak ditemukan." });

                if (headerEntity.KunjunganId != kunjunganId)
                    return BadRequest(new { message = "KunjunganId tidak sesuai dengan header kasir." });

                // 2) Validasi kunjungan
                var kunjunganOk = await _applicationDbContext.Kunjungans
                        .AsNoTracking()
                        .Where(k => k.KunjunganID == kunjunganId && !k.IsDelete)
                        .Select(k => new { k.KunjunganID })
                        .FirstOrDefaultAsync(); // <== sengaja tidak pakai ct (sesuai instruksi kamu)

                if (kunjunganOk == null)
                    return NotFound(new { message = "Kunjungan tidak ditemukan atau sudah dihapus." });

                // 3) Cek apakah detail sudah ada (pakai AnyAsync)
                var hasDetail = await _applicationDbContext.MainKasirDetails
                    .AsNoTracking()
                    .AnyAsync(d => d.MainKasirId == kasirId && !d.IsDelete, ct);

                var tglPembayaran = DateTimeOffset.UtcNow;

                // 4) Invoice billing
                var ivc = await _applicationDbContext.Billings
                    .AsNoTracking()
                    .Where(b => b.KunjunganId == kunjunganId)
                    .Select(b => b.InvoiceBilling)
                    .FirstOrDefaultAsync(ct);

                // 5) Total tagihan (prioritas: header -> vm -> detailVm.TotalPembayaran)
                decimal totalTagihan =
                    (headerEntity.GrandTotalPembayaran)
                    ?? (vm.GrandTotalPembayaran)
                    ?? (vm.Details?.Max(d => d.TotalPembayaran ?? 0m) ?? 0m);

                decimal totalTagihanNet = totalTagihan;

                // batas maksimum = total awal yang tersimpan (anggap “resep/ketetapan awal”)
                decimal maxAllowedTotal = headerEntity.GrandTotalPembayaran ?? totalTagihanNet;

                // status lama untuk billing update
                var wasLunas = string.Equals(headerEntity.StatusPembayaran, "Lunas", StringComparison.OrdinalIgnoreCase);

                // ==========================
                // A) JIKA DETAIL SUDAH ADA -> ABAIKAN vm.Details, BOLEH TURUN TOTAL (DENGAN BATAS)
                // ==========================
                decimal totalPaid = 0m;
                decimal sisaAfter = 0m;
                int? angsuranKe = null;
                string? warning = null;
                decimal kembalian = 0m;

                if (hasDetail)
                {
                    warning = (vm.Details != null && vm.Details.Any())
                        ? "Detail pembayaran sudah ada, data Details pada request diabaikan (tidak menambah/mengubah detail)."
                        : null;

                    totalPaid = await _applicationDbContext.MainKasirDetails
                        .AsNoTracking()
                        .Where(d => d.MainKasirId == kasirId && !d.IsDelete)
                        .SumAsync(d => (decimal?)(d.NominalPembayaran ?? 0m), ct) ?? 0m;

                    // Jika FE kirim total baru (misal qty obat ditebus berkurang), boleh turun ASAL:
                    // - tidak boleh naik melebihi maxAllowedTotal
                    // - tidak boleh turun di bawah totalPaid (karena endpoint ini tidak handle refund)
                    if (vm.GrandTotalPembayaran.HasValue)
                    {
                        var requestedTotal = vm.GrandTotalPembayaran.Value;

                        if (requestedTotal > maxAllowedTotal)
                            return BadRequest(new
                            {
                                message = "Total tagihan tidak boleh melebihi batas maksimum (resep/ketetapan awal).",
                                maxAllowedTotal,
                                requestedTotal
                            });

                        if (requestedTotal < totalPaid)
                            return BadRequest(new
                            {
                                message = "Total tagihan tidak boleh lebih kecil dari total yang sudah dibayar (tidak ada mekanisme refund pada endpoint ini).",
                                totalPaid,
                                requestedTotal
                            });

                        totalTagihanNet = requestedTotal; // ✅ terima penurunan / tetap
                    }
                    else
                    {
                        // tidak ada update total dari FE => pakai total tersimpan
                        totalTagihanNet = headerEntity.GrandTotalPembayaran ?? totalTagihanNet;
                    }

                    var rawSisa = totalTagihanNet - totalPaid;
                    sisaAfter = rawSisa < 0 ? 0m : rawSisa;
                }
                else
                {
                    // ==========================
                    // B) JIKA DETAIL BELUM ADA -> TAMBAH BARU DARI vm.Details
                    // ==========================
                    if (vm.Details == null || !vm.Details.Any())
                        return BadRequest(new { message = "Detail pembayaran wajib diisi minimal 1 item." });

                    // total bayar sekarang
                    var totalNominalBayarNow = vm.Details.Sum(d => d.NominalPembayaran ?? 0m);

                    // ✅ konsisten dengan POST: boleh overpay => kembalian
                    var rawSisa = totalTagihanNet - totalNominalBayarNow;
                    kembalian = rawSisa < 0 ? Math.Abs(rawSisa) : 0m;
                    sisaAfter = rawSisa < 0 ? 0m : rawSisa;

                    // generate angsuran utk batch ini
                    angsuranKe = await _generateUrutanAngsuran.GenerateAsync(
                        kunjunganId,
                        sisaAfter,
                        ct
                    );

                    // insert detail + running sisa per baris + kwitansi per baris
                    decimal cumulativePaid = 0m;
                    var detailEntities = new List<MainKasirDetail>();

                    foreach (var dvm in vm.Details)
                    {
                        var bayarNow = dvm.NominalPembayaran ?? 0m;
                        if (bayarNow <= 0m) continue;

                        cumulativePaid += bayarNow;

                        // running sisa (kalau overpay, sisaPerDetail dibuat 0)
                        var sisaPerDetail = totalTagihanNet - cumulativePaid;
                        if (sisaPerDetail < 0m) sisaPerDetail = 0m;

                        var noKwitansiPerDetail = await _noKwitansiService.GenerateNoKwitansiAsync(tglPembayaran, ct);

                        detailEntities.Add(new MainKasirDetail
                        {
                            MainKasirDetailId = Guid.NewGuid(),
                            MainKasirId = kasirId,
                            KunjunganId = kunjunganId,
                            PasienId = dvm.PasienId ?? vm.PasienId,
                            TipeDiskonDokter = dvm.TipeDiskonDokter,
                            ValueDiskonDokter = dvm.ValueDiskonDokter,

                            TotalPembayaran = totalTagihanNet,
                            NominalPembayaran = bayarNow,
                            SisaPembayaran = sisaPerDetail,

                            MetodePembayaranId = dvm.MetodePembayaranId,
                            NoKwitansi = noKwitansiPerDetail,
                            AngsuranKe = angsuranKe,

                            ReferenceId = dvm.ReferenceId,
                            NamaMetode = dvm.NamaMetode,
                            Keterangan = dvm.Keterangan,

                            TglPembayaran = tglPembayaran.UtcDateTime,

                            CreateBy = userActiveId.Value,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            IsDelete = false
                        });
                    }

                    if (detailEntities.Count == 0)
                        return BadRequest(new { message = "Tidak ada detail pembayaran yang valid untuk disimpan." });

                    _applicationDbContext.MainKasirDetails.AddRange(detailEntities);

                    totalPaid = totalNominalBayarNow;
                }

                // 6) Final status (otomatis berdasarkan sisaAfter)
                var statusFromVm = (vm.StatusPembayaran ?? "").Trim();
                var finalStatus = (sisaAfter <= 0m)
                    ? "Lunas"
                    : (string.IsNullOrWhiteSpace(statusFromVm) ? "Cicil" : statusFromVm);

                // 7) TTD (opsional)
                var ttd = (vm.TTDUserVerfiedId.HasValue)
                    ? await _ttdService.CheckTTDAsync(vm.TTDUserVerfiedId.Value)
                    : null;

                // 8) Update header (selalu)
                headerEntity.PasienId = vm.PasienId ?? headerEntity.PasienId;
                headerEntity.JumlahAngsuran = await _countAngsuran.CountAsync(kunjunganId);
                headerEntity.IsSudahDibuatAR = false;

                headerEntity.StatusPembayaran = finalStatus;
                headerEntity.IsVerified = vm.IsVerified;

                headerEntity.InvoiceBilling = ivc;
                headerEntity.DiskonId = vm.DiskonId;
                headerEntity.SubTotalAsuransi = vm.SubTotalAsuransi;
                headerEntity.SubTotalMandiri = vm.SubTotalMandiri;

                // TotalPembayaran: hanya update kalau belum ada detail (biar tidak “geser” setelah ada pembayaran)
                if (!hasDetail)
                    headerEntity.TotalPembayaran = vm.TotalPembayaran;

                headerEntity.Deposito = vm.DepositRanap;
                headerEntity.SisaDeposito = vm.SisaDeposit;

                // ✅ GrandTotalPembayaran selalu pakai totalTagihanNet (sudah tervalidasi turun/tetap)
                headerEntity.GrandTotalPembayaran = totalTagihanNet;
                headerEntity.JumlahPajak = vm.JumlahPajak;
                headerEntity.TotalBiayaObat = vm.TotalBiayaObat;
                headerEntity.TotalBiayaTindakan = vm.TotalBiayaTindakan;
                headerEntity.Keterangan = vm.Keterangan ?? headerEntity.Keterangan;
                headerEntity.HargaDiskon = vm.HargaDiskon ?? headerEntity.HargaDiskon;
                headerEntity.TglPembayaran = tglPembayaran;

                headerEntity.TTDUserVerfiedId = vm.TTDUserVerfiedId ?? headerEntity.TTDUserVerfiedId;
                headerEntity.PathUserVerified = (ttd?.Path) ?? headerEntity.PathUserVerified;

                headerEntity.CreateBy = userActiveId.Value;
                headerEntity.UpdateBy = userActiveId.Value;
                headerEntity.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.MainKasirs.Update(headerEntity);

                // 9) Save
                var saved = await _applicationDbContext.SaveChangesAsync(ct);
                if (saved <= 0)
                {
                    await trx.RollbackAsync(ct);
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }

                // 10) Update billing hanya saat berubah jadi lunas
                int affectedBilling = 0;
                var isLunasNow = string.Equals(finalStatus, "Lunas", StringComparison.OrdinalIgnoreCase);
                if (!wasLunas && isLunasNow)
                {
                    affectedBilling = await _billingService.MarkBillingKunjunganAsPaidAsync(kunjunganId, (Guid)userActiveId, ct);
                }

                await trx.CommitAsync(ct);

                // 11) SignalR
                await _hubContext.Clients.All.SendAsync("Data pembayaran Updated", new
                {
                    Action = hasDetail ? "update_header_only" : "update_header_and_insert_detail",
                    kasirId,
                    kunjunganId,
                    angsuranKe,
                    status = finalStatus,
                    billingUpdated = affectedBilling
                }, ct);

                return Ok(new
                {
                    message = hasDetail
                        ? "Header diperbarui. Detail sudah ada, tidak ditambah."
                        : "Header diperbarui & detail baru berhasil ditambahkan.",
                    warning,
                    action = hasDetail ? "update_header_only" : "update_header_and_insert_detail",
                    kasirId,
                    kunjunganId,
                    statusPembayaran = finalStatus,
                    totalTagihan = totalTagihanNet,
                    totalPaid,
                    sisaPembayaran = sisaAfter,
                    kembalian,
                    maxAllowedTotal, // opsional, biar FE tahu batas maksimum
                    billingUpdated = affectedBilling
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}/Update-StatusBilling")]
        public async Task<IActionResult> UpdateStatusBilling(Guid id, [FromBody] UpdateStatusBillingViewModel request)
        {
            var data = await _applicationDbContext.MainKasirs.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data kasir tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.AsNoTracking().FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusBilling = request.StatusBilling;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi SignalR
            await _hubContext.Clients.All.SendAsync("Status-Billing Changed", new
            {
                action = "updateStatusBilling",
                kasirId = data.KasirId,
            });

            return Ok(new { message = "Status Billing berhasil diperbarui." });
        }


        [HttpPost("Resep-Tebus")]
        public async Task<IActionResult> PayResepTebus(
        [FromBody] MainKasirTebusResepViewModel vm,
        CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!vm.ResepTebusId.HasValue || vm.ResepTebusId.Value == Guid.Empty)
                return BadRequest(new { message = "ResepTebusId wajib diisi." });

            if (vm.Details == null || !vm.Details.Any())
                return BadRequest(new { message = "Detail pembayaran wajib diisi." });

            if (vm.Details.Count != 1)
                return BadRequest(new { message = "Resep tebus tidak boleh split payment. Hanya boleh 1 metode pembayaran." });

            if (!await _applicationDbContext.Database.CanConnectAsync(ct))
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi." });

            var userActiveId = await _applicationDbContext.UserActives
                .Where(x => x.Email == emailLogin)
                .Select(x => (Guid?)x.UserActiveId)
                .FirstOrDefaultAsync(ct);

            if (!userActiveId.HasValue)
                return Unauthorized(new { message = "User aktif tidak ditemukan." });

            await using var trx = await _applicationDbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, ct);

            try
            {
                var resepTebusId = vm.ResepTebusId.Value;

                var resepTebus = await _applicationDbContext.ResepTebuss
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ResepTebusId == resepTebusId &&
                        x.IsCancelled != true &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (resepTebus == null)
                    return NotFound(new { message = "Resep tebus tidak ditemukan atau sudah dibatalkan." });

                if (resepTebus.IsLunas == true)
                    return Conflict(new { message = "Resep tebus ini sudah lunas." });

                var header = await _applicationDbContext.MainKasirs
                    .FirstOrDefaultAsync(x =>
                        x.ResepTebusId == resepTebusId &&
                        x.KunjunganId == null &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (header == null)
                    return NotFound(new { message = "Header kasir resep tebus belum dibuat." });

                if (string.Equals(header.StatusPembayaran, "Lunas", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { message = "Tagihan resep tebus ini sudah lunas." });

                var sudahAdaPembayaran = await _applicationDbContext.MainKasirDetails
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MainKasirId == header.KasirId &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (sudahAdaPembayaran)
                    return Conflict(new { message = "Pembayaran resep tebus sudah pernah dibuat. Resep tebus tidak boleh angsuran." });

                decimal totalTagihan =
                    header.GrandTotalPembayaran
                    ?? resepTebus.TotalHargaResep
                    ?? 0m;

                if (totalTagihan <= 0)
                    return BadRequest(new { message = "Total tagihan resep tebus belum valid." });

                var detailVm = vm.Details.Single();

                var nominalBayar = detailVm.NominalPembayaran ?? 0m;

                if (nominalBayar <= 0)
                    return BadRequest(new { message = "Nominal pembayaran wajib lebih dari 0." });

                if (nominalBayar < totalTagihan)
                {
                    var kurangBayar = totalTagihan - nominalBayar;

                    return BadRequest(new
                    {
                        message = "Pembayaran kurang dari total tagihan. Resep tebus wajib langsung lunas.",
                        totalTagihan,
                        nominalBayar,
                        kurangBayar
                    });
                }

                var kembalian = nominalBayar > totalTagihan
                    ? nominalBayar - totalTagihan
                    : 0m;

                var tglPembayaran = DateTimeOffset.UtcNow;

                var noKwitansi = await _noKwitansiService.GenerateNoKwitansiAsync(
                    tglPembayaran,
                    ct);

                var detail = new MainKasirDetail
                {
                    MainKasirDetailId = Guid.NewGuid(),
                    MainKasirId = header.KasirId,

                    KunjunganId = null,
                    PasienId = null,
                    //DiskonId = detailVm.DiskonId,

                    TipeDiskonDokter = detailVm.TipeDiskonDokter,
                    ValueDiskonDokter = detailVm.ValueDiskonDokter,

                    TotalPembayaran = totalTagihan,
                    NominalPembayaran = nominalBayar,
                    SisaPembayaran = 0,

                    MetodePembayaranId = detailVm.MetodePembayaranId,
                    ReferenceId = resepTebusId,
                    NamaMetode = detailVm.NamaMetode,
                    Keterangan = detailVm.Keterangan,

                    NoKwitansi = noKwitansi,
                    AngsuranKe = 0,

                    TglPembayaran = tglPembayaran.UtcDateTime,

                    CreateBy = userActiveId.Value,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.MainKasirDetails.Add(detail);

                header.StatusPembayaran = "Lunas";
                header.StatusBilling = "Lunas";
                header.GrandTotalPembayaran = totalTagihan;
                header.TotalPembayaran = totalTagihan;
                header.TotalBiayaObat = totalTagihan;
                header.TotalBiayaTindakan = 0;
                header.TglPembayaran = tglPembayaran;
                header.IsVerified = vm.IsVerified ?? header.IsVerified;
                header.TTDUserVerfiedId = vm.TTDUserVerfiedId ?? header.TTDUserVerfiedId;
                header.Keterangan = vm.Keterangan ?? header.Keterangan;
                header.JumlahAngsuran = 0;
                header.UpdateBy = userActiveId.Value;
                header.UpdateDateTime = DateTimeOffset.UtcNow;

                var affectedBilling = await _billingService.MarkBillingResepTebusAsPaidAsync(
                    resepTebusId,
                    userActiveId.Value,
                    ct);

                await _resepTebusStockService.FinalizeResepTebusAsync(
                    resepTebusId,
                    userActiveId.Value,
                    ct);

                var saved = await _applicationDbContext.SaveChangesAsync(ct);

                if (saved <= 0)
                {
                    await trx.RollbackAsync(ct);
                    return StatusCode(500, new { message = "Data pembayaran tidak berhasil disimpan." });
                }

                await trx.CommitAsync(ct);

                return Ok(new
                {
                    message = "Pembayaran resep tebus berhasil lunas dan stok berhasil dipotong.",
                    kasirId = header.KasirId,
                    resepTebusId,
                    statusPembayaran = "Lunas",
                    statusBilling = "Lunas",
                    totalTagihan,
                    nominalBayar,
                    kembalian,
                    sisaPembayaran = 0,
                    billingUpdated = affectedBilling,
                    noKwitansi = detail.NoKwitansi,
                    metodePembayaranId = detail.MetodePembayaranId,
                    namaMetode = detail.NamaMetode
                });
            }
            catch (InvalidOperationException ex)
            {
                await trx.RollbackAsync(ct);
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
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
                var resep = await _applicationDbContext.MainKasirs.FindAsync(id);
                if (resep == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Hapus DetailResep terkait
                var detailReseps = _applicationDbContext.MainKasirDetails.Where(dk => dk.MainKasirId == id).ToList();
                if (detailReseps.Any())
                {
                    _applicationDbContext.MainKasirDetails.RemoveRange(detailReseps);
                }

                // Hapus Resep
                _applicationDbContext.MainKasirs.Remove(resep);
                await _applicationDbContext.SaveChangesAsync();
                return Ok(new { message = "Hapus Data Berhasil || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedKasir(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? pasienId = null,
            //Guid? petugasId =null,
            string? statusBilling = null,
            string? asalKunjungan = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] EnumJenisKunjungan? jenisKunjungan = null,
            [FromQuery] StatusBayarEnum? status = null,
            [FromQuery] bool? iclosed = null,
            [FromQuery] string? search = null,
            [FromQuery] string? namaAsuransi = null,
            [FromQuery] bool? ispks = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null,
            [FromQuery] DateTime? asOf = null,
            CancellationToken ct = default
        )
        {
            // 1) Billing paged (kamu sudah punya)
            var paged = await _billingKunjunganReadService.GetBillingPagedAsync(new BillingPagedQuery
            {
                Page = page,
                PageSize = perPage,
                KunjunganId = kunjunganId,
                //PetugasId = petugasId,
                StatusBilling = statusBilling,
                sb = status,
                PasienId = pasienId,
                asal = asalKunjungan,
                jk = jenisKunjungan,
                //NamaAsuransi = namaAsuransi,
                isClosed = iclosed,
                isPks = ispks,
                Search = search,
                StartDate = startDate,
                EndDate = endDate,
                Periode = periode,
                AsOf = asOf
            }, ct);

            if (paged.TotalKunjungan == 0 || paged.Data == null || !paged.Data.Any())
            {
                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        Page = page,
                        PerPage = perPage,
                        TotalData = 0,
                        TotalPage = 0,
                        Items = new List<object>()
                    }
                });
            }

            // 2) Items: isi sama seperti Billing-Kasir/{kunjunganId}
            var items = new List<object>();

            foreach (var billingObj in paged.Data)
            {
                // di output GetBillingPagedAsync kamu pakai KunjunganID
                var kid = (Guid)((dynamic)billingObj).KunjunganID;

                // penting: ini SEQUENTIAL supaya DbContext aman
                var kasirs = await _billingKunjunganReadService
                    .GetMainKasirDanDetailPembayaranAsync(kid, ct);

                items.Add(new
                {
                    KunjunganId = kid,
                    Billing = billingObj, // <- billing detail (mirip GetBillingKeseluruhanAsync versi paged kamu)
                    Pembayaran = new
                    {
                        TotalKasir = kasirs?.Count ?? 0,
                        Kasirs = kasirs ?? Array.Empty<object>()
                    }
                });
            }

            return Ok(new
            {
                status = "success",
                data = new
                {
                    Page = paged.Page,
                    PerPage = paged.PageSize,
                    TotalData = paged.TotalKunjungan,
                    TotalPage = paged.TotalPages,
                    Items = items
                }
            });
        }

        [HttpGet("pendapatan-harian/paged")]
        public async Task<IActionResult> GetPendapatanHarianPaged(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var q = new PendapatanHarianPagedQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _billingKunjunganReadService.GetPendapatanHarianPagedAsync(q, ct);

            return Ok(new
            {
                status = "success",
                data = result
            });
        }

        //[HttpGet("paged")]
        //public async Task<IActionResult> PagedKasir(
        //    int page = 1,
        //    int perPage = 10,
        //    Guid? kunjunganId = null,
        //    Guid? pasienId = null,
        //    string? search = null,              // ✅ search baru
        //    string? asuransiNama = null,         // ✅ filter nama asuransi
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
        //    [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
        //    [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null
        //)
        //{
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;
        //    if (perPage > 100) perPage = 100;

        //    // =========================
        //    // 1) Query Header (MainKasir) + Join user/pasien/kunjungan/asuransi
        //    // =========================
        //    var query =
        //        from a in _applicationDbContext.MainKasirs.AsNoTracking()

        //        join u0 in _applicationDbContext.UserActives.AsNoTracking()
        //            on a.CreateBy equals u0.UserActiveId into uu
        //        from u in uu.DefaultIfEmpty()

        //        join p0 in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
        //            on a.PasienId equals p0.PendaftaranPasienBaruId into pp
        //        from p in pp.DefaultIfEmpty()

        //        join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
        //            on a.KunjunganId equals k0.KunjunganID into kk
        //        from k in kk.DefaultIfEmpty()

        //        join as0 in _applicationDbContext.Asuransis.AsNoTracking()
        //            on k.AsuransiId equals as0.AsuransiId into aa
        //        from asu in aa.DefaultIfEmpty()

        //        where a.IsDelete != true
        //        select new
        //        {
        //            a.KasirId,
        //            a.KunjunganId,
        //            a.PasienId,

        //            p.NamaLengkap,
        //            p.NoRekamMedis,

        //            AsuransiId = (Guid?)k.AsuransiId,
        //            NamaAsuransi = asu != null ? asu.NamaAsuransi : null,

        //            a.InvoiceBilling,
        //            a.JumlahAngsuran,
        //            a.StatusPembayaran,
        //            a.IsVerified,
        //            a.TTDUserVerfiedId,
        //            a.PathUserVerified,
        //            a.GrandTotalPembayaran,
        //            a.TotalBiayaObat,
        //            a.TotalBiayaTindakan,
        //            a.Keterangan,
        //            a.TglPembayaran,
        //            a.DiskonId,

        //            a.CreateDateTime,
        //            CreateBy = (Guid?)a.CreateBy,
        //            CreateByName = u != null ? u.FullName : null,
        //            a.UpdateDateTime,
        //            UpdateBy = (Guid?)a.UpdateBy
        //        };

        //    // =========================
        //    // 2) Filter dasar
        //    // =========================
        //    if (kunjunganId.HasValue && kunjunganId.Value != Guid.Empty)
        //        query = query.Where(x => x.KunjunganId == kunjunganId.Value);

        //    if (pasienId.HasValue && pasienId.Value != Guid.Empty)
        //        query = query.Where(x => x.PasienId == pasienId.Value);

        //    // ✅ Filter Nama Asuransi
        //    if (!string.IsNullOrWhiteSpace(asuransiNama))
        //    {
        //        var likeAsu = $"%{asuransiNama.Trim()}%";
        //        query = query.Where(x =>
        //            x.NamaAsuransi != null && EF.Functions.ILike(x.NamaAsuransi, likeAsu));
        //    }

        //    // =========================
        //    // 3) Search BARU (invoice, nama pasien, no RM, no kwitansi)
        //    //     - NoKwitansi ada di MainKasirDetails -> pakai EXISTS/Any
        //    // =========================
        //    if (!string.IsNullOrWhiteSpace(search))
        //    {
        //        var like = $"%{search.Trim()}%";

        //        query = query.Where(x =>
        //            (x.InvoiceBilling != null && EF.Functions.ILike(x.InvoiceBilling, like)) ||
        //            (x.NamaLengkap != null && EF.Functions.ILike(x.NamaLengkap, like)) ||
        //            (x.NoRekamMedis != null && EF.Functions.ILike(x.NoRekamMedis, like)) ||
        //            _applicationDbContext.MainKasirDetails.AsNoTracking().Any(d =>
        //                d.IsDelete != true &&
        //                d.MainKasirId.HasValue &&
        //                d.MainKasirId.Value == x.KasirId &&
        //                d.NoKwitansi != null &&
        //                EF.Functions.ILike(d.NoKwitansi, like)
        //            )
        //        );
        //    }

        //    // =========================
        //    // 4) Filter tanggal
        //    // =========================
        //    if (startDate.HasValue && endDate.HasValue)
        //    {
        //        DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
        //        DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

        //        query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc);
        //    }

        //    // =========================
        //    // 5) Filter periode (tetap seperti punyamu)
        //    // =========================
        //    if (periode.HasValue)
        //    {
        //        DateTime today = DateTime.UtcNow.Date;

        //        switch (periode)
        //        {
        //            case PeriodeFilter.Today:
        //                query = query.Where(x => x.CreateDateTime.Date == today);
        //                break;

        //            case PeriodeFilter.ThisWeek:
        //                query = query.Where(x =>
        //                    x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
        //                    x.CreateDateTime.Date <= today);
        //                break;

        //            case PeriodeFilter.LastWeek:
        //                query = query.Where(x =>
        //                    x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
        //                    x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
        //                break;

        //            case PeriodeFilter.ThisMonth:
        //                query = query.Where(x =>
        //                    x.CreateDateTime.Month == today.Month &&
        //                    x.CreateDateTime.Year == today.Year);
        //                break;

        //            case PeriodeFilter.LastMonth:
        //                query = query.Where(x =>
        //                    x.CreateDateTime.Month == today.Month - 1 &&
        //                    x.CreateDateTime.Year == today.Year);
        //                break;

        //            case PeriodeFilter.ThisYear:
        //                query = query.Where(x => x.CreateDateTime.Year == today.Year);
        //                break;

        //            case PeriodeFilter.LastYear:
        //                query = query.Where(x => x.CreateDateTime.Year == today.Year - 1);
        //                break;

        //            case PeriodeFilter.Last3Months:
        //                query = query.Where(x => x.CreateDateTime >= today.AddMonths(-3));
        //                break;

        //            case PeriodeFilter.Last6Months:
        //                query = query.Where(x => x.CreateDateTime >= today.AddMonths(-6));
        //                break;
        //        }
        //    }

        //    // =========================
        //    // 6) Sorting
        //    // =========================
        //    bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        //    query = desc
        //        ? orderBy switch
        //        {
        //            "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
        //            "CreateByName" => query.OrderByDescending(x => x.CreateByName),
        //            "KasirId" => query.OrderByDescending(x => x.KasirId),
        //            _ => query.OrderByDescending(x => x.CreateDateTime)
        //        }
        //        : orderBy switch
        //        {
        //            "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
        //            "CreateByName" => query.OrderBy(x => x.CreateByName),
        //            "KasirId" => query.OrderBy(x => x.KasirId),
        //            _ => query.OrderBy(x => x.CreateDateTime)
        //        };

        //    // =========================
        //    // 7) Paging Count
        //    // =========================
        //    var totalRows = await query.CountAsync();
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

        //    if (totalRows == 0)
        //        return NotFound(new { message = "Data tidak ditemukan." });

        //    if (page > totalPages)
        //        return NotFound(new { message = "Page not found." });

        //    // =========================
        //    // 8) Fetch page headers
        //    // =========================
        //    var header = await query
        //        .Skip((page - 1) * perPage)
        //        .Take(perPage)
        //        .ToListAsync();

        //    var kasirIds = header.Select(h => h.KasirId).ToList();

        //    // =========================
        //    // 9) Fetch details (ONE query)
        //    // =========================
        //    var details = await _applicationDbContext.MainKasirDetails
        //        .AsNoTracking()
        //        .Where(d => d.IsDelete != true && d.MainKasirId.HasValue && kasirIds.Contains(d.MainKasirId.Value))
        //        .OrderBy(d => d.TglPembayaran ?? DateTime.MaxValue)
        //        .ThenBy(d => d.CreateDateTime)
        //        .Select(d => new
        //        {
        //            d.MainKasirDetailId,
        //            MainKasirId = d.MainKasirId.Value,
        //            d.MetodePembayaranId,
        //            d.ReferenceId,
        //            d.KunjunganId,
        //            d.PasienId,
        //            d.TotalPembayaran,
        //            d.SisaPembayaran,
        //            d.NoKwitansi,
        //            d.AngsuranKe,
        //            d.NamaMetode,
        //            d.NominalPembayaran,
        //            d.Keterangan,
        //            d.TglPembayaran,

        //            d.CreateDateTime,
        //            CreateBy = (Guid?)d.CreateBy,
        //            d.UpdateDateTime,
        //            UpdateBy = (Guid?)d.UpdateBy
        //        })
        //        .ToListAsync();

        //    var detailLookup = details.ToLookup(x => x.MainKasirId);

        //    // =========================
        //    // 10) Load nama user sekali (hindari N+1)
        //    // =========================
        //    var userIds = new HashSet<Guid>();

        //    foreach (var h in header)
        //    {
        //        if (h.CreateBy.HasValue) userIds.Add(h.CreateBy.Value);
        //        if (h.UpdateBy.HasValue) userIds.Add(h.UpdateBy.Value);
        //        if (h.TTDUserVerfiedId.HasValue) userIds.Add(h.TTDUserVerfiedId.Value);
        //    }

        //    foreach (var d in details)
        //    {
        //        if (d.CreateBy.HasValue) userIds.Add(d.CreateBy.Value);
        //        if (d.UpdateBy.HasValue) userIds.Add(d.UpdateBy.Value);
        //    }

        //    var userDict = userIds.Count == 0
        //        ? new Dictionary<Guid, string>()
        //        : await _applicationDbContext.UserActives
        //            .AsNoTracking()
        //            .Where(u => userIds.Contains(u.UserActiveId))
        //            .Select(u => new { u.UserActiveId, u.FullName })
        //            .ToDictionaryAsync(x => x.UserActiveId, x => x.FullName);

        //    string? GetUserName(Guid? userId)
        //        => userId.HasValue && userDict.TryGetValue(userId.Value, out var name) ? name : null;

        //    // =========================
        //    // 11) Compose rows
        //    // =========================
        //    var rows = header.Select(h => new
        //    {
        //        Header = new
        //        {
        //            h.KasirId,
        //            h.KunjunganId,
        //            h.PasienId,
        //            h.NamaLengkap,
        //            h.NoRekamMedis,
        //            h.AsuransiId,
        //            h.NamaAsuransi,
        //            h.InvoiceBilling,
        //            h.JumlahAngsuran,
        //            h.StatusPembayaran,
        //            h.IsVerified,
        //            h.TTDUserVerfiedId,
        //            VerifiedByName = GetUserName(h.TTDUserVerfiedId),
        //            h.PathUserVerified,
        //            h.GrandTotalPembayaran,
        //            h.TotalBiayaObat,
        //            h.TotalBiayaTindakan,
        //            h.Keterangan,
        //            h.TglPembayaran,
        //            h.DiskonId,

        //            h.CreateDateTime,
        //            h.CreateBy,
        //            CreateByName = h.CreateByName ?? GetUserName(h.CreateBy),

        //            h.UpdateDateTime,
        //            h.UpdateBy,
        //            UpdateByName = GetUserName(h.UpdateBy),
        //        },
        //        Details = detailLookup[h.KasirId].Select(d => new
        //        {
        //            d.MainKasirDetailId,
        //            d.MainKasirId,
        //            d.MetodePembayaranId,
        //            d.ReferenceId,
        //            d.KunjunganId,
        //            d.PasienId,
        //            d.TotalPembayaran,
        //            d.SisaPembayaran,
        //            d.NoKwitansi,
        //            d.AngsuranKe,
        //            d.NamaMetode,
        //            d.NominalPembayaran,
        //            d.Keterangan,
        //            d.TglPembayaran,

        //            d.CreateDateTime,
        //            d.CreateBy,
        //            CreateByName = GetUserName(d.CreateBy),

        //            d.UpdateDateTime,
        //            d.UpdateBy,
        //            UpdateByName = GetUserName(d.UpdateBy),
        //        }).ToList()
        //    }).ToList();

        //    return Ok(new
        //    {
        //        status = "success",
        //        message = "Data retrieved successfully",
        //        data = new
        //        {
        //            Rows = rows,
        //            TotalRows = totalRows,
        //            CurrentPage = page,
        //            PerPage = perPage,
        //            TotalPages = totalPages
        //        }
        //    });
        //}


    }
}
