using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
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
        private readonly IHubContext<MainKasirHub> _hubContext;
        private readonly ILogger<MainKasirController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private IBillingService _billingService;
        private readonly ITTDService _ttdService;
        private readonly INoKwitansiService _noKwitansiService;

        public MainKasirController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<MainKasirController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<MainKasirHub> hubContext,
            IBillingService billingService,
            ITTDService ttdService,
            INoKwitansiService noKwitansiService)
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

                    var totalLab = daftarLab.Sum(x => x.Subtotal );


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


        //[HttpGet("BillingByKunjungan/{kunjunganId}")]
        //public async Task<IActionResult> GetKasirData(Guid kunjunganId)
        //{
        //    var query =
        //        // INNER JOIN Kunjungan dengan PendaftaranPasienBaru
        //        from k in _applicationDbContext.Kunjungans
        //        join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId

        //        // LEFT JOIN Asuransi
        //        join a in _applicationDbContext.Asuransis on k.AsuransiId equals a.AsuransiId into asuransiTempGroup
        //        from a in asuransiTempGroup.DefaultIfEmpty()

        //            // LEFT JOIN AsuransiPasien (pastikan k.PasienId dapat dikonversi ke string jika ap.PasienId string)
        //        join ap in _applicationDbContext.AsuransiPasiens on p.PendaftaranPasienBaruId.ToString() equals ap.PasienId into asuransiPasienGroup
        //        from ap in asuransiPasienGroup.DefaultIfEmpty()

        //            // INNER JOIN Kunjungan ke tabel Dokter dan Poliklinik
        //        join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
        //        join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId

        //        // LEFT JOIN Reseps (filter IsDelete di sini)
        //        join r in _applicationDbContext.Reseps.Where(resep => !resep.IsDelete) on k.KunjunganID equals r.KunjunganId into resepGroup
        //        from r in resepGroup.DefaultIfEmpty() // Penting: DefaultIfEmpty untuk LEFT JOIN

        //            // LEFT JOIN DetailResep (filter IsDelete di sini)
        //        join dr in _applicationDbContext.DetailReseps.Where(detail => !detail.IsDelete) on r.ResepId equals dr.ResepId into detailResepGroup
        //        from dr in detailResepGroup.DefaultIfEmpty()

        //            // LEFT JOIN Obat
        //            // Perhatikan: o.ObatId harus non-null untuk join. Jika dr null, o juga akan null.
        //        join o in _applicationDbContext.Obats on dr.ObatId equals o.ObatId into obatGroup
        //        from o in obatGroup.DefaultIfEmpty()

        //            // LEFT JOIN TindakanKunjungan
        //        join to in _applicationDbContext.TindakanKunjungans on k.KunjunganID equals to.KunjunganId into tindakanGroup
        //        from to in tindakanGroup.DefaultIfEmpty()

        //            // LEFT JOIN Tindakan
        //            // Perhatikan: t.TindakanId harus non-null untuk join. Jika to null, t juga akan null.
        //        join t in _applicationDbContext.Tindakans on to.TindakanId equals t.TindakanId into tindakanMasterGroup
        //        from t in tindakanMasterGroup.DefaultIfEmpty()

        //            // LEFT JOIN BiayaAdministrasi
        //            // Perhatikan: adm.BiayaAdministrasiKode harus non-null untuk join. Jika k.JenisKunjungan null, adm juga akan null.
        //        join adm in _applicationDbContext.BiayaAdministrasis on k.JenisKunjungan equals adm.BiayaAdministrasiKode into admGroup
        //        from adm in admGroup.DefaultIfEmpty()

        //            // LEFT JOIN ke tabel Kasir (MainKasir)
        //        join kasir in _applicationDbContext.MainKasirs on k.KunjunganID equals kasir.KunjunganId into kasirGroup
        //        from kasir in kasirGroup.DefaultIfEmpty()

        //        join dk in _applicationDbContext.MainKasirDetails on kasir.KasirId equals dk.MainKasirId into MainKasirDetailsGroup
        //        from dk in MainKasirDetailsGroup.DefaultIfEmpty()
        //            //                                                                // LEFT JOIN ke tabel Diskon
        //            //join dsk in _applicationDbContext.Diskons on MainKasirDetails. equals dsk.DiskonId into diskonGroup
        //            //from dsk in diskonGroup.DefaultIfEmpty() 

        //            // LEFT JOIN ke tabel Metode Pembayaran
        //        join mp in _applicationDbContext.MetodePembayarans on dk.MetodePembayaranId equals mp.MetodePembayaranId into metodeGroup
        //        from mp in metodeGroup.DefaultIfEmpty()

        //        where k.KunjunganID == kunjunganId && !k.IsDelete
        //        select new
        //        {
        //            k,
        //            p,
        //            a,
        //            ap,
        //            d,
        //            poli,
        //            r,
        //            dr,
        //            o,
        //            to,
        //            t,
        //            adm,
        //            kasir,
        //            //dsk,
        //            dk,
        //            mp
        //        };

        //    var result = await query.ToListAsync();

        //    var kasirData = result.GroupBy(x => x.k.KunjunganID) // Grouping by KunjunganID
        //        .Select(group =>
        //        {
        //            var firstItem = group.First(); // Ambil satu item dari grup untuk data Kunjungan, Pasien, dll.

        //            return new
        //            {
        //                KasirId = firstItem.kasir?.KasirId ?? Guid.Empty, // Gunakan Guid.Empty jika kasir null
        //                firstItem.k.KunjunganID,
        //                firstItem.k.JenisKunjungan,
        //                NoRegistrasi = firstItem.k.Antrian,
        //                firstItem.k.TipePembayaran,
        //                TglRegistrasi = firstItem.k.CreateDateTime.ToString("dddd, dd MMMM yyyy", new CultureInfo("id-ID")), // Tambahkan tahun
        //                firstItem.k.PasienId,
        //                NoRM = firstItem.p?.NoRekamMedis ?? "-",
        //                NamaPasien = firstItem.p?.NamaLengkap ?? "-",
        //                UmurPasien = HitungUmurLengkap(firstItem.p?.TanggalLahir),
        //                NoPasien = firstItem.p?.NoPasien ?? "-",
        //                firstItem.p?.JenisKelamin,
        //                firstItem.k.AsuransiId,
        //                NamaPerusahaan = firstItem.a?.NamaAsuransi ?? null, // NamaAsuransi akan null jika tidak ada asuransi
        //                NoPolis = firstItem.ap?.NoPolis ?? "-",
        //                firstItem.k.DokterId,
        //                NamaDokter = firstItem.d?.NmDokter ?? "-",
        //                firstItem.k.PoliklinikId,
        //                NamaPoliklinik = firstItem.poli?.NamaPoliklinik ?? "-",
        //                firstItem.adm?.BiayaAdministrasiId,
        //                firstItem.adm?.NominalBiayaAdministrasi,
        //                PaymentMethodId = firstItem.mp?.MetodePembayaranId,
        //                PaymentMethodName = firstItem.mp?.NamaMetode ?? "-",
        //                //DiskonId = firstItem.dsk?.DiskonId,
        //                //NamaDiskon = firstItem.dsk?.NamaDiskon ?? "-",
        //                //NilaiDiskon = firstItem.dsk?.NominalDiskon ?? 0,
        //                //PersenanDiskon = firstItem.dsk?.PersenDiskon ?? 0,
        //                firstItem.k?.IsFinishedKasir,

        //                firstItem.kasir?.CreateBy,
        //                firstItem.kasir?.CreateDateTime,

        //                // Koleksi untuk item yang bisa banyak (Resep, Obat, Tindakan)
        //                DaftarResepObat = group
        //                    .Where(x => x.dr != null && x.o != null) // Filter hanya yang punya DetailResep dan Obat
        //                    .Select(x => new
        //                    {
        //                        x.r.ResepId, // ResepId dari resep utama
        //                        x.dr.DetailResepId,
        //                        x.dr.ObatId,
        //                        x.dr?.JumlahIteratur,
        //                        NamaObat = x.o.ObatName,
        //                        x.dr.Qty,
        //                        HargaObat = x.o.HargaJual,
        //                        x.dr.IsIteratur,
        //                        x.dr.JarakPenebusan,
        //                        TglMulaiIteratur = x.dr.TglMulaiIteratur.HasValue ? x.dr.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
        //                        MasaAktifIteratur = x.dr.MasaAktifIteratur.HasValue ? x.dr.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
        //                        StatusCoverObat = x.dr?.StatusCoverObat ?? false,
        //                        TotalBiayaObat = x.dr?.TotalHargaObat ?? x.dr?.Qty * x.o.HargaJual // Hitung total jika tidak ada TotalHargaObat
        //                    }).Distinct().ToList(), // Gunakan Distinct untuk menghindari duplikasi dalam daftar obat

        //                DaftarTindakan = group
        //                    .Where(x => x.to != null && x.t != null) // Filter hanya yang punya TindakanKunjungan dan Tindakan
        //                    .Select(x => new
        //                    {
        //                        x.to.TindakanId, // TindakanId dari TindakanKunjungan
        //                        x.t.NamaTindakan,
        //                        QtyTindakan = x.to.Quantity,
        //                        HargaTindakan = x.to.Total,
        //                        StatusCoverTindakan = x.to != null && x.t != null && firstItem.a != null
        //                            ? _applicationDbContext.TindakanAsuransis.Any(y => y.TindakanId == x.to.TindakanId && y.AsuransiId == firstItem.a.AsuransiId)
        //                            : false
        //                    }).Distinct().ToList(), // Gunakan Distinct untuk menghindari duplikasi dalam daftar tindakan


        //                //TOTAL TAGIHAN (Obat + Tindakan)
        //                TotalObat = group
        //                    .Where(x => x.dr != null && x.o != null)
        //                    .DistinctBy(x => x.dr.DetailResepId)
        //                    .Sum(x => x.dr.Qty * x.o.HargaJual),
        //                TotalTindakan = group
        //                    .Where(x => x.to != null && x.t != null)
        //                    .DistinctBy(x => x.to.TindakanKunjunganId)
        //                    .Sum(x => x.to.Quantity * (x.to.Total ?? 0)),
        //                TotalTagihan = group
        //                    // Total Obat
        //                    .Where(x => x.dr != null && x.o != null)
        //                    .DistinctBy(x => x.dr.DetailResepId)
        //                    .Sum(x => x.dr.Qty * x.o.HargaJual)
        //                    +

        //                    // Total Tindakan
        //                    group
        //                    .Where(x => x.to != null && x.t != null)
        //                    .DistinctBy(x => x.to.TindakanKunjunganId)
        //                    .Sum(x => x.to.Quantity * (x.to.Total ?? 0))

        //                    +
        //                    (firstItem.adm?.NominalBiayaAdministrasi ?? 0), // Tambahkan biaya administrasi jika ada
        //            };

        //        }).ToList();

        //    if (!kasirData.Any())
        //    {
        //        return NotFound(new { message = "Data billing kasir untuk kunjungan ini tidak ditemukan. || 404 Not Found" });
        //    }

        //    return Ok(new { status = "success", data = kasirData.FirstOrDefault() }); // Mengembalikan hanya satu item karena ini adalah view untuk satu kunjunganId
        //}


        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(Guid id)
        //{
        //    var kunjunganId = await _applicationDbContext.MainKasirs
        //        .Where(mk => mk.KasirId == id)
        //        .Select(mk => mk.KunjunganId)
        //        .FirstOrDefaultAsync();

        //    if (kunjunganId == Guid.Empty)
        //        return NotFound(new { message = "Kunjungan tidak ditemukan untuk KasirId ini." });

        //    var billings = await _applicationDbContext.Billings
        //        .Where(b => b.KunjunganId == kunjunganId)
        //        .ToListAsync();

        //    var query =
        //        from k in _applicationDbContext.Kunjungans
        //        join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
        //        join a in _applicationDbContext.Asuransis on k.AsuransiId equals a.AsuransiId into asuransiTempGroup
        //        from a in asuransiTempGroup.DefaultIfEmpty()
        //        join ap in _applicationDbContext.AsuransiPasiens on p.PendaftaranPasienBaruId.ToString() equals ap.PasienId into asuransiPasienGroup
        //        from ap in asuransiPasienGroup.DefaultIfEmpty()
        //        join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
        //        join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId
        //        join r in _applicationDbContext.Reseps.Where(resep => !resep.IsDelete) on k.KunjunganID equals r.KunjunganId into resepGroup
        //        from r in resepGroup.DefaultIfEmpty()
        //        join dr in _applicationDbContext.DetailReseps.Where(detail => !detail.IsDelete) on r.ResepId equals dr.ResepId into detailResepGroup
        //        from dr in detailResepGroup.DefaultIfEmpty()
        //        join o in _applicationDbContext.Obats on dr.ObatId equals o.ObatId into obatGroup
        //        from o in obatGroup.DefaultIfEmpty()
        //        join to in _applicationDbContext.TindakanKunjungans on k.KunjunganID equals to.KunjunganId into tindakanGroup
        //        from to in tindakanGroup.DefaultIfEmpty()
        //        join t in _applicationDbContext.Tindakans on to.TindakanId equals t.TindakanId into tindakanMasterGroup
        //        from t in tindakanMasterGroup.DefaultIfEmpty()
        //        join adm in _applicationDbContext.BiayaAdministrasis on k.JenisKunjungan equals adm.BiayaAdministrasiKode into admGroup
        //        from adm in admGroup.DefaultIfEmpty()
        //        join kasir in _applicationDbContext.MainKasirs on k.KunjunganID equals kasir.KunjunganId into kasirGroup
        //        from kasir in kasirGroup.DefaultIfEmpty()
        //        join dk in _applicationDbContext.MainKasirDetails on kasir.KasirId equals dk.MainKasirId into MainKasirDetailsGroup
        //        from dk in MainKasirDetailsGroup.DefaultIfEmpty()
        //        join mp in _applicationDbContext.MetodePembayarans on dk.MetodePembayaranId equals mp.MetodePembayaranId into metodeGroup
        //        from mp in metodeGroup.DefaultIfEmpty()
        //        join rc in _applicationDbContext.Racikans on dr.RacikanId equals rc.RacikanId into racikanGroup
        //        from rc in racikanGroup.DefaultIfEmpty()
        //        join rd in _applicationDbContext.RacikanDetails on dr.RacikanId equals rd.RacikanId into racikanDetailGroup
        //        from rd in racikanDetailGroup.DefaultIfEmpty()
        //        join oRacikan in _applicationDbContext.Obats on rd.ObatId equals oRacikan.ObatId into obatRacikanGroup
        //        from oRacikan in obatRacikanGroup.DefaultIfEmpty()
        //        join lbd in _applicationDbContext.LabBookingDetails on k.PasienId equals lbd.PasienId into labGroup
        //        from lbd in labGroup.DefaultIfEmpty()
        //        join lp in _applicationDbContext.LabPemeriksaans on lbd.PemeriksaanLabId equals lp.PemeriksaanLabId into pemeriksaanGroup
        //        from lp in pemeriksaanGroup.DefaultIfEmpty()

        //        where kasir.KasirId == id && !k.IsDelete
        //        select new { k, p, a, ap, d, poli, r, dr, o, to, t, adm, kasir, dk, mp, rc, rd, oRacikan, lbd, lp };

        //    var result = await query.ToListAsync();

        //    var kasirData = result.GroupBy(x => x.k.KunjunganID)
        //        .Select(group =>
        //        {
        //            var first = group.First();

        //            // ✅ Ambil semua billing lab terkait kunjungan ini
        //            var billingLabs = billings
        //                .Where(b => b.JenisBilling == "Pemeriksaan Lab" && b.KunjunganId == first.k.KunjunganID)
        //                .ToList();

        //            // ✅ Ambil daftar pemeriksaan lab dari tabel LabBookingDetails + LabPemeriksaans
        //            var daftarPemeriksaanLab = (
        //                from lbd in _applicationDbContext.LabBookingDetails
        //                join lp in _applicationDbContext.LabPemeriksaans on lbd.PemeriksaanLabId equals lp.PemeriksaanLabId
        //                where lbd.BookingLabId != null && lbd.PasienId == first.k.PasienId
        //                select new
        //                {
        //                    lbd.DetailBookingLabId,
        //                    lbd.BookingLabId,
        //                    lp.PemeriksaanLabId,
        //                    lp.NamaPemeriksaan,
        //                    lp.HargaPemeriksaan,
        //                    Qty = 1,
        //                    Subtotal = lp.HargaPemeriksaan,
        //                    StatusPemeriksaan = lbd.StatusPemeriksaan,
        //                    Billing = billingLabs.FirstOrDefault(b => b.ItemId == lp.PemeriksaanLabId)
        //                }
        //            ).ToList();

        //            return new
        //            {
        //                KasirId = first.kasir?.KasirId ?? Guid.Empty,
        //                first.k.KunjunganID,
        //                first.k.JenisKunjungan,
        //                NoRegistrasi = first.k.Antrian,
        //                first.k.TipePembayaran,
        //                TglRegistrasi = first.k.CreateDateTime.ToString("dddd, dd MMMM yyyy", new CultureInfo("id-ID")),
        //                first.k.PasienId,
        //                NoRM = first.p?.NoRekamMedis ?? "-",
        //                NamaPasien = first.p?.NamaLengkap ?? "-",
        //                UmurPasien = HitungUmurLengkap(first.p?.TanggalLahir),
        //                NoPasien = first.p?.NoPasien ?? "-",
        //                first.p?.JenisKelamin,
        //                first.k.AsuransiId,
        //                NamaPerusahaan = first.a?.NamaAsuransi ?? null,
        //                NoPolis = first.ap?.NoPolis ?? "-",
        //                first.k.DokterId,
        //                NamaDokter = first.d?.NmDokter ?? "-",
        //                first.k.PoliklinikId,
        //                NamaPoliklinik = first.poli?.NamaPoliklinik ?? "-",
        //                first.adm?.BiayaAdministrasiId,
        //                first.adm?.NominalBiayaAdministrasi,
        //                PaymentMethodId = first.mp?.MetodePembayaranId,
        //                PaymentMethodName = first.mp?.NamaMetode ?? "-",
        //                first.k?.IsFinishedKasir,
        //                first.kasir?.CreateBy,
        //                first.kasir?.CreateDateTime,

        //                // ✅ Tambahkan daftar pemeriksaan lab
        //                DaftarPemeriksaanLab = daftarPemeriksaanLab
        //                    .Select(x => new
        //                    {
        //                        x.PemeriksaanLabId,
        //                        x.NamaPemeriksaan,
        //                        Harga = x.HargaPemeriksaan,
        //                        x.Qty,
        //                        x.Subtotal,
        //                        BillingId = x.Billing?.BillingId,
        //                        BillingKode = x.Billing?.BillingKode,
        //                        x.StatusPemeriksaan
        //                    }).ToList(),

        //                // ✅ Total biaya pemeriksaan lab
        //                TotalPemeriksaanLab = (decimal)Math.Ceiling(
        //                    daftarPemeriksaanLab.Sum(x => x.Subtotal ?? 0)
        //                ),

        //                // ✅ Data obat & tindakan tetap seperti sebelumnya
        //                DaftarObat = group
        //                    .Where(x => x.dr != null && (x.dr.IsRacikan == false || x.dr.IsRacikan == null) && x.o != null)
        //                    .Select(x => new
        //                    {
        //                        x.r.ResepId,
        //                        x.dr.DetailResepId,
        //                        x.dr.ObatId,
        //                        NamaObat = x.o.ObatName,
        //                        Harga = x.o.HTEPrice,
        //                        x.dr.Qty,
        //                        x.dr.Signa,
        //                        x.dr.SignaTambahan,
        //                        x.dr.StatusCoverObat,
        //                        x.dr.StatusPengambilanObat
        //                    }).ToList(),

        //                DaftarTindakan = group
        //                    .Where(x => x.to != null && x.t != null)
        //                    .Select(x => new
        //                    {
        //                        x.t.TindakanId,
        //                        x.t.NamaTindakan,
        //                        x.to.Total
        //                    }).ToList(),

        //                TotalObat = (decimal)Math.Ceiling(
        //                    (decimal)group.Where(x => x.dr != null && x.o != null)
        //                        .DistinctBy(x => x.dr.DetailResepId)
        //                        .Sum(x => (x.dr.Qty * x.o.HTEPrice))
        //                ),

        //                TotalTindakan = (decimal)Math.Ceiling(
        //                    group.Where(x => x.to != null && x.t != null)
        //                        .DistinctBy(x => x.to.TindakanKunjunganId)
        //                        .Sum(x => (x.to.Quantity ?? 0) * (x.to.Total ?? 0))
        //                )
        //            };
        //        }).FirstOrDefault();

        //    if (kasirData == null)
        //        return NotFound(new { message = "Data billing kasir tidak ditemukan untuk MainKasirId ini." });

        //    return Ok(new { status = "success", data = kasirData });
        //}
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MainKasirViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

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
                // 1) Validasi kunjungan
                var kunjunganOk = await _applicationDbContext.Kunjungans
                    .AsNoTracking()
                    .AnyAsync(k => k.KunjunganID == vm.KunjunganId && !k.IsDelete);

                if (!kunjunganOk)
                    return NotFound(new { message = "Kunjungan tidak ditemukan atau sudah dihapus." });

                // 2) Cegah duplikasi
                var existingKasir = await _applicationDbContext.MainKasirs
                    .AsNoTracking()
                    .AnyAsync(k => k.KunjunganId == vm.KunjunganId && !k.IsDelete);

                if (existingKasir)
                    return Conflict(new { message = "Kasir untuk kunjungan ini sudah pernah dibuat." });

                // 3) Insert header
                var kasirId = Guid.NewGuid();
                var ttd = await _ttdService.CheckTTDAsync((Guid)vm.TTDUserVerfiedId);

                var data = new MainKasir
                {
                    KasirId = kasirId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    JumlahAngsuran = vm.JumlahAngsuran,
                    StatusPembayaran = vm.StatusPembayaran,
                    IsVerified = vm.IsVerified,
                    NoKwitansi = vm.NoKwitansi, 
                    DiskonId = vm.DiskonId,
                    GrandTotalPembayaran = vm.GrandTotalPembayaran,
                    TotalBiayaObat = vm.TotalBiayaObat,
                    Keterangan = vm.Keterangan,
                    TglPembayaran = DateTimeOffset.UtcNow,
                    IsDelete = false,
                    TTDUserVerfiedId = vm.TTDUserVerfiedId,
                    PathUserVerified = ttd?.Path,

                    CreateBy = userActiveId.Value,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                _applicationDbContext.MainKasirs.Add(data);

                // 4) Insert detail
                var detailEntities = vm.Details.Select(detail => new MainKasirDetail
                {
                    MainKasirDetailId = Guid.NewGuid(),
                    MainKasirId = kasirId,
                    KunjunganId = detail.KunjunganId,
                    PasienId = detail.PasienId,
                    TotalPembayaran = detail.TotalPembayaran,
                    NominalPembayaran = detail.NominalPembayaran,
                    SisaPembayaran = detail.TotalPembayaran - detail.NominalPembayaran,
                    MetodePembayaranId = detail.MetodePembayaranId,
                    InvoiceBilling = detail.InvoiceBilling,
                    AngsuranKe = 1, //hari kamis bikin ini
                    ReferenceId = detail.ReferenceId,
                    NamaMetode = detail.NamaMetode,
                    Keterangan = detail.Keterangan,
                    TglPembayaran = DateTime.UtcNow,

                    CreateBy = userActiveId.Value,
                    CreateDateTime = DateTimeOffset.UtcNow
                }).ToList();

                _applicationDbContext.MainKasirDetails.AddRange(detailEntities);

                // 5) Save sekali
                var saved = await _applicationDbContext.SaveChangesAsync();
                if (saved <= 0)
                {
                    await trx.RollbackAsync();
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }

                // 6) ✅ Update billing pakai SERVICE (bulk update)
                var affectedBilling = await _billingService.MarkBillingAsPaidAsync((Guid)vm.KunjunganId);

                await trx.CommitAsync();

                // 7) SignalR
                await _hubContext.Clients.All.SendAsync("Data pembayaran Created", new
                {
                    Action = "create",
                    data = data.KasirId,
                    billingUpdated = affectedBilling
                });

                return Ok(new
                {
                    message = "Data berhasil disimpan || 200 OK",
                    kasirId = data.KasirId,
                    totalDetail = detailEntities.Count,
                    billingUpdated = affectedBilling
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
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
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                // Cek data MainKasir
                var existingKasir = await _applicationDbContext.MainKasirs
                    .FirstOrDefaultAsync(k => k.KasirId == id && !k.IsDelete);

                if (existingKasir == null)
                {
                    return NotFound(new { message = "Data kasir tidak ditemukan." });
                }

                // Cek kunjungan masih valid
                var datakunjungan = await _applicationDbContext.Kunjungans
                    .FirstOrDefaultAsync(k => k.KunjunganID == vm.KunjunganId && !k.IsDelete);
                if (datakunjungan == null)
                {
                    return NotFound(new { message = "Kunjungan tidak ditemukan atau sudah dihapus." });
                }

                // Update data MainKasir
                existingKasir.KunjunganId = vm.KunjunganId;
                existingKasir.DiskonId = vm.DiskonId;
                existingKasir.GrandTotalPembayaran = vm.GrandTotalPembayaran;
                existingKasir.TotalBiayaObat = vm.TotalBiayaObat;
                existingKasir.Keterangan = vm.Keterangan;
                existingKasir.TglPembayaran = DateTimeOffset.UtcNow;
                existingKasir.UpdateBy = userActiveId;
                existingKasir.UpdateDateTime = DateTimeOffset.UtcNow;

                // Hapus detail lama
                var existingDetails = _applicationDbContext.MainKasirDetails
                    .Where(d => d.MainKasirId == id);
                _applicationDbContext.MainKasirDetails.RemoveRange(existingDetails);

                // Tambahkan detail baru jika ada
                if (vm.Details != null && vm.Details.Any())
                {
                    var newDetails = vm.Details.Select(detail => new MainKasirDetail
                    {
                        MainKasirDetailId = Guid.NewGuid(),
                        MainKasirId = id,
                        MetodePembayaranId = detail.MetodePembayaranId,
                        ReferenceId = detail.ReferenceId,
                        NamaMetode = detail.NamaMetode,
                        NominalPembayaran = detail.NominalPembayaran,
                        Keterangan = detail.Keterangan,
                    }).ToList();

                    _applicationDbContext.MainKasirDetails.AddRange(newDetails);
                }

                await _applicationDbContext.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("Data pembayaran Created", new
                {
                    Action = "update",
                    data = existingKasir.KasirId,
                });

                return Ok(new { message = "Update Data Berhasil || 200 OK" });
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
        public IActionResult PagedKasir(
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
            var query = from a in _applicationDbContext.MainKasirs
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
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
                        };

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
