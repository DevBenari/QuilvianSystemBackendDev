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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
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
        public async Task<IActionResult> GetBillingByKunjunganId(Guid kunjunganId)
        {
            try
            {
                // ================================
                // LOAD BILLINGS
                // ================================
                var billings = await _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == kunjunganId && (b.IsDelete == false || b.IsDelete == null))
                    .ToListAsync();

                // ================================
                // QUERY UTAMA (EFISIEN)
                // ================================
                var query =
                    from k in _applicationDbContext.Kunjungans

                    join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                    join a in _applicationDbContext.Asuransis on k.AsuransiId equals a.AsuransiId into asuransiTempGroup
                    from a in asuransiTempGroup.DefaultIfEmpty()

                    join ap in _applicationDbContext.AsuransiPasiens
                        on p.PendaftaranPasienBaruId equals ap.PasienId into asuransiPasienGroup
                    from ap in asuransiPasienGroup.DefaultIfEmpty()

                    join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                    join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId

                    // LAB
                    join lbd in _applicationDbContext.LabBookingDetails
                        on k.PasienId equals lbd.PasienId into labGroup
                    from lbd in labGroup.DefaultIfEmpty()

                    join lp in _applicationDbContext.LabPemeriksaans
                        on lbd.PemeriksaanLabId equals lp.PemeriksaanLabId into pemeriksaanGroup
                    from lp in pemeriksaanGroup.DefaultIfEmpty()

                    join la in _applicationDbContext.Labs
                        on lbd.LabId equals la.LabId into laGroup
                    from la in laGroup.DefaultIfEmpty()

                        // RESEP
                    join r in _applicationDbContext.Reseps.Where(x => !x.IsDelete)
                        on k.KunjunganID equals r.KunjunganId into resepGroup
                    from r in resepGroup.DefaultIfEmpty()

                    join dr in _applicationDbContext.DetailReseps.Where(x => !x.IsDelete)
                        on r.ResepId equals dr.ResepId into detailResepGroup
                    from dr in detailResepGroup.DefaultIfEmpty()

                    join o in _applicationDbContext.Obats
                        on dr.ObatId equals o.ObatId into obatGroup
                    from o in obatGroup.DefaultIfEmpty()

                    join rc in _applicationDbContext.Racikans
                        on dr.RacikanId equals rc.RacikanId into racikanGroup
                    from rc in racikanGroup.DefaultIfEmpty()

                        // TINDAKAN
                    join tobj in _applicationDbContext.TindakanKunjungans
                        on k.KunjunganID equals tobj.KunjunganId into tindakanGroup
                    from tobj in tindakanGroup.DefaultIfEmpty()

                    join t in _applicationDbContext.Tindakans
                        on tobj.TindakanId equals t.TindakanId into tindakanMasterGroup
                    from t in tindakanMasterGroup.DefaultIfEmpty()

                        // ADMIN + KASIR
                    join adm in _applicationDbContext.BiayaAdministrasis
                        on k.JenisKunjungan equals adm.BiayaAdministrasiKode into admGroup
                    from adm in admGroup.DefaultIfEmpty()

                    join kasir in _applicationDbContext.MainKasirs
                        on k.KunjunganID equals kasir.KunjunganId into kasirGroup
                    from kasir in kasirGroup.DefaultIfEmpty()

                    join dk in _applicationDbContext.MainKasirDetails
                        on kasir.KasirId equals dk.MainKasirId into kasirDetailGroup
                    from dk in kasirDetailGroup.DefaultIfEmpty()

                    join mp in _applicationDbContext.MetodePembayarans
                        on dk.MetodePembayaranId equals mp.MetodePembayaranId into metodeGroup
                    from mp in metodeGroup.DefaultIfEmpty()

                    where k.KunjunganID == kunjunganId && !k.IsDelete

                    select new { k, p, a, ap, d, poli, r, dr, o, rc, tobj, t, adm, kasir, dk, mp, lbd, lp, la };

                var result = await query.ToListAsync();
                if (!result.Any())
                    return NotFound(new { message = "Data billing tidak ditemukan." });

                // ============================================================
                // AMBIL RACIKAN IDs
                // ============================================================
                var racikanIds = result
                    .Where(x => x.dr?.IsRacikan == true && x.dr.RacikanId != null)
                    .Select(x => x.dr!.RacikanId!.Value)
                    .Distinct()
                    .ToList();

                // ============================================================
                // LOAD KOMPOSISI RACIKAN (EFISIEN)
                // ============================================================
                var racikanDetails = racikanIds.Any()
                    ? await (
                        from rd in _applicationDbContext.RacikanDetails
                        join ob in _applicationDbContext.Obats on rd.ObatId equals ob.ObatId
                        where racikanIds.Contains(rd.RacikanId.Value)
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
                    )
                    .Select(x => (object)x)   // 🔥 CAST ke object
                    .ToListAsync()
                    : new List<object>();      // 🔥 SAMAKAN TIPE

                var racikanMap = racikanDetails
                    .Cast<dynamic>()                 // 🔥 Convert kembali agar property bisa diakses
                    .GroupBy(x => (Guid)x.RacikanId) // 🔥 property sudah bisa dibaca
                    .ToDictionary(g => g.Key, g => g.ToList());

                // ============================================================
                // GROUPING KUNJUNGAN
                // ============================================================
                var data = result
                    .GroupBy(x => x.k.KunjunganID)
                    .Select(group =>
                    {
                        var first = group.First();

                        // ================= LAB =================
                        var daftarPemeriksaanLab = group
                            .Where(x => x.lbd != null)
                            .GroupBy(x => x.lbd.DetailBookingLabId)
                            .Select(g =>
                            {
                                var x = g.First();
                                var bill = billings.FirstOrDefault(b =>
                                    b.ItemId == x.lbd.DetailBookingLabId &&
                                    b.JenisBilling == "Pemeriksaan Lab");

                                return new
                                {
                                    x.lbd.DetailBookingLabId,
                                    x.la.NamaLab,
                                    x.lp?.NamaPemeriksaan,
                                    x.lp?.HargaPemeriksaan,
                                    Qty = bill?.QtyItem ?? 1,
                                    Subtotal = bill?.SubTotalItem ?? x.lp?.HargaPemeriksaan ?? 0,
                                    BillingId = bill?.BillingId,
                                    BillingKode = bill?.BillingKode
                                };
                            }).ToList();

                        var totalLab = daftarPemeriksaanLab.Sum(x => x.Subtotal);

                        // ================= OBAT NON RACIKAN =================
                        var daftarObat = group
                            .Where(x => x.dr != null && x.o != null && x.dr.IsRacikan != true)
                            .GroupBy(x => x.dr.DetailResepId)
                            .Select(g =>
                            {
                                var x = g.First();
                                var bill = billings.FirstOrDefault(b => b.ItemId == x.dr.ObatId && b.JenisBilling == "Obat");

                                return new
                                {
                                    x.r?.ResepId,
                                    x.dr.DetailResepId,
                                    x.dr.ObatId,
                                    x.o.ObatName,
                                    Qty = bill?.QtyItem ?? x.dr.Qty,
                                    Harga = bill?.HargaItem ?? x.o.HTEPrice,
                                    Subtotal = bill?.SubTotalItem ?? (x.dr.Qty * x.o.HTEPrice),
                                    BillingId = bill?.BillingId,
                                    BillingKode = bill?.BillingKode,
                                    x.dr.Signa,
                                    x.dr.SignaTambahan,
                                    x.dr.StatusPengambilanObat
                                };
                            }).ToList();

                        var totalObat = daftarObat.Sum(x => x.Subtotal);

                        // ================= RACIKAN =================
                        var daftarRacikan = group
                            .Where(x => x.dr != null && x.dr.IsRacikan == true && x.rc != null)
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
                                    x.rc.NamaRacikan,
                                    x.rc.KodeRacikan,
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

                        var totalRacikan = daftarRacikan.Sum(x => x.Subtotal ?? 0);

                        // ================= TINDAKAN =================
                        var daftarTindakan = group
                            .Where(x => x.tobj != null && x.t != null)
                            .GroupBy(x => x.tobj.TindakanKunjunganId)
                            .Select(g =>
                            {
                                var x = g.First();
                                var bill = billings.FirstOrDefault(b =>
                                    b.ItemId == x.tobj.TindakanId && b.JenisBilling == "Tindakan");

                                return new
                                {
                                    x.t.TindakanId,
                                    x.t.NamaTindakan,
                                    Qty = bill?.QtyItem ?? x.tobj.Quantity ?? 1,
                                    Harga = bill?.HargaItem ?? x.tobj.Total ?? 0,
                                    Subtotal = bill?.SubTotalItem ?? ((x.tobj.Quantity ?? 1) * (x.tobj.Total ?? 0)),
                                    BillingId = bill?.BillingId,
                                    BillingKode = bill?.BillingKode
                                };
                            }).ToList();

                        var totalTindakan = daftarTindakan.Sum(x => x.Subtotal);

                        // ================= ADMIN =================
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

                        // ================= FINAL =================
                        return new
                        {
                            first.k.KunjunganID,
                            first.k.JenisKunjungan,
                            TanggalKunjungan = first.k?.TglMasuk,
                            first.kasir?.KasirId,
                            first.p?.NamaLengkap,
                            first.p?.NoRekamMedis,
                            first.d?.NmDokter,
                            first.poli?.NamaPoliklinik,
                            first.k.TipePembayaran,
                            first.a?.NamaAsuransi,
                            Umur = HitungUmurLengkap(first.p?.TanggalLahir),
                            DaftarPemeriksaanLab = daftarPemeriksaanLab,
                            DaftarObat = daftarObat,
                            DaftarRacikan = daftarRacikan,
                            DaftarTindakan = daftarTindakan,
                            DaftarBiayaAdmin = daftarAdmin,

                            TotalPemeriksaanLab = totalLab,
                            TotalObat = totalObat,
                            TotalRacikan = totalRacikan,
                            TotalTindakan = totalTindakan,
                            TotalBiayaAdmin = totalAdmin,

                            TotalKeseluruhan =
                                totalLab + totalObat + totalRacikan + totalTindakan + totalAdmin
                        };
                    })
                    .FirstOrDefault();

                return Ok(new { status = "success", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("perkiraan-billing-ip/{kunjunganId}")]
        public async Task<IActionResult> GetPerkiraanBillingRawatInap(Guid kunjunganId)
        {
            try
            {
                // =========================
                // 1) HEADER
                // =========================
                var header = await _applicationDbContext.Kunjungans
                    .AsNoTracking()
                    .Where(k => k.KunjunganID == kunjunganId && !k.IsDelete)
                    .Select(k => new
                    {
                        k.KunjunganID,
                        k.JenisKunjungan,
                        k.TipePembayaran,
                        Pasien = _applicationDbContext.PendaftaranPasienBarus
                            .Where(p => p.PendaftaranPasienBaruId == k.PasienId)
                            .Select(p => new { p.NamaLengkap, p.NoRekamMedis })
                            .FirstOrDefault(),
                        Dokter = _applicationDbContext.Dokters
                            .Where(d => d.DokterId == k.DokterId)
                            .Select(d => new { d.NmDokter })
                            .FirstOrDefault(),
                        Poli = _applicationDbContext.Polikliniks
                            .Where(p => p.PoliklinikId == k.PoliklinikId)
                            .Select(p => new { p.NamaPoliklinik })
                            .FirstOrDefault(),
                        Asuransi = _applicationDbContext.Asuransis
                            .Where(a => a.AsuransiId == k.AsuransiId)
                            .Select(a => new { a.NamaAsuransi })
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (header == null)
                    return NotFound(new { status = "failed", message = "Data kunjungan tidak ditemukan." });

                // =========================
                // 2) VALIDASI IP
                // =========================
                if (!IsRawatInapIP(header.JenisKunjungan))
                {
                    return BadRequest(new
                    {
                        status = "failed",
                        message = BuildJenisKunjunganMessage(header.JenisKunjungan)
                    });
                }

                // =========================
                // 3) BILLING
                // =========================
                var billings = await _applicationDbContext.Billings
                    .AsNoTracking()
                    .Where(b => b.KunjunganId == kunjunganId && (b.IsDelete == false || b.IsDelete == null))
                    .ToListAsync();

                // =========================
                // 4) SOURCE OF TRUTH RANAP
                // =========================
                var bookingRanaps = await _applicationDbContext.BookingBedRanaps
                    .AsNoTracking()
                    .Where(x => x.KunjunganId == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
                    .ToListAsync();

                var transfers = await _applicationDbContext.TransferPasiens
                    .AsNoTracking()
                    .Where(x => x.KunjunganId == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
                    .ToListAsync();

                // Transfer per bedId (cepat untuk lookup)
                var transferByBed = transfers
                    .Where(t => t.BedId != null)
                    .GroupBy(t => t.BedId!.Value)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(x => x.TglMasuk ?? x.TglPindah ?? DateTime.MinValue).ToList()
                    );

                // =========================
                // 5) GROUPING + BUILD ITEMS
                // =========================
                var groups = billings
                    .GroupBy(b => NormalizeGroup(b.JenisBilling, b.NamaItem))
                    .Select(g =>
                    {
                        var items = new List<object>();
                        decimal total = 0m;

                        foreach (var b in g)
                        {
                            var jenis = (b.JenisBilling ?? "").Trim();

                            // default
                            int qty = b.QtyItem ?? 1;
                            decimal harga = b.HargaItem ?? 0m;
                            decimal subtotal = b.SubTotalItem ?? (qty * harga);

                            // =========================
                            // KHUSUS KAMAR RANAP
                            // Billing.ItemId = KamarId
                            // Qty tetap 1; Harga = tarif/hari * totalHari
                            // Tampilkan tanggal Booking & Transfer
                            // =========================
                            if (jenis.Equals("Kamar Ranap", StringComparison.OrdinalIgnoreCase))
                            {
                                var tarifPerHari = b.HargaItem ?? 0m;

                                int totalHari = 0;
                                DateTime? tglMasukAwal = null;
                                DateTime? tglKeluarAkhir = null;

                                // detail per segmen booking (berisi tanggal booking + daftar transfer yang relevan)
                                var segments = new List<object>();

                                if (b.ItemId != null)
                                {
                                    var kamarId = b.ItemId.Value;

                                    var bookingsKamar = bookingRanaps
                                        .Where(x => x.KamarId == kamarId)
                                        .OrderBy(x => x.TglMasuk ?? DateTime.MinValue)
                                        .ToList();

                                    foreach (var bk in bookingsKamar)
                                    {
                                        tglMasukAwal ??= bk.TglMasuk;

                                        // Tentukan end-date final segmen: booking.TglKeluar > transfer end > null(=masih dirawat)
                                        var endTransfer = ResolveEndFromTransfer(bk.BedId, bk.TglMasuk, transferByBed);
                                        var endFinal = bk.TglKeluar ?? endTransfer;

                                        var hari = HitungJumlahHariRanap(bk.TglMasuk, endFinal);
                                        totalHari += hari;

                                        if (endFinal.HasValue)
                                        {
                                            if (!tglKeluarAkhir.HasValue || endFinal.Value > tglKeluarAkhir.Value)
                                                tglKeluarAkhir = endFinal;
                                        }

                                        // ambil daftar transfer yang relevan untuk segmen ini (bed yang sama, setelah start segmen)
                                        var transfersSegmen = GetTransfersForSegment(bk.BedId, bk.TglMasuk, endFinal, transferByBed);

                                        segments.Add(new
                                        {
                                            bk.BookingBedRanapId,
                                            bk.KamarId,
                                            bk.BedId,

                                            // tanggal dari BookingBedRanap
                                            TglMasukBooking = bk.TglMasuk,
                                            TglKeluarBooking = bk.TglKeluar,

                                            // tanggal final yg dipakai hitung hari
                                            TglKeluarTransfer = endTransfer,
                                            TglKeluarFinal = endFinal,

                                            JumlahHari = hari,

                                            // daftar transfer pasien (tanggal masuk/pindah/keluar)
                                            TransferPasien = transfersSegmen.Select(t => new
                                            {
                                                t.TransferPasienId,
                                                t.BedId,
                                                t.TglMasuk,
                                                t.TglPindah,
                                                t.TglKeluar,
                                                t.Keterangan
                                            }).ToList()
                                        });
                                    }
                                }

                                // Kalau tidak ada booking untuk kamar ini -> fallback aman (pakai billing apa adanya)
                                if (totalHari <= 0)
                                {
                                    qty = 1;
                                    harga = b.HargaItem ?? 0m;
                                    subtotal = b.SubTotalItem ?? harga;

                                    total += subtotal;

                                    items.Add(new
                                    {
                                        b.BillingId,
                                        b.BillingKode,
                                        b.ItemId, // KamarId
                                        b.JenisBilling,
                                        b.NamaItem,
                                        b.Keterangan,
                                        Qty = qty,
                                        Harga = harga,
                                        Subtotal = subtotal,
                                        Note = "Tidak ditemukan BookingBedRanap untuk KamarId ini. Pastikan BookingBedRanap.KamarId == Billing.ItemId."
                                    });

                                    continue;
                                }

                                // Qty tetap 1; harga total berdasarkan hari
                                qty = 1;
                                harga = tarifPerHari * totalHari;
                                subtotal = harga;

                                total += subtotal;

                                items.Add(new
                                {
                                    b.BillingId,
                                    b.BillingKode,
                                    b.ItemId, // KamarId
                                    b.JenisBilling,
                                    b.NamaItem,
                                    b.Keterangan,
                                    Qty = qty,
                                    Harga = harga,
                                    Subtotal = subtotal,

                                    // info khusus kamar
                                    JumlahHari = totalHari,
                                    HargaPerHari = tarifPerHari,
                                    TglMasukBookingAwal = tglMasukAwal,
                                    TglKeluarFinalAkhir = tglKeluarAkhir,

                                    // detail segmen booking + transfer
                                    Segments = segments
                                });

                                continue;
                            }

                            // =========================
                            // SELAIN KAMAR RANAP
                            // =========================
                            total += subtotal;

                            items.Add(new
                            {
                                b.BillingId,
                                b.BillingKode,
                                b.ItemId,
                                b.JenisBilling,
                                b.NamaItem,
                                b.Keterangan,
                                Qty = qty,
                                Harga = harga,
                                Subtotal = subtotal
                            });
                        }

                        return new
                        {
                            Group = g.Key,
                            Items = items,
                            Total = total
                        };
                    })
                    .ToList();

                // =========================
                // 6) ORDER GROUPS
                // =========================
                var order = new[]
                {
                "Tindakan Operasi",
                "Tindakan Rawat Inap",
                "Alkes Khusus",
                "Biaya Kamar Rawat",
                "Pemeriksaan Lab",
                "Biaya Lain-lain",
                "Obat",
                "Lain-lain"
            };

                var orderedGroups = groups
                    .OrderBy(x => Array.IndexOf(order, x.Group) == -1 ? int.MaxValue : Array.IndexOf(order, x.Group))
                    .ToList();

                var grandTotal = orderedGroups.Sum(x => x.Total);

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        Header = header,
                        Groups = orderedGroups,
                        GrandTotal = grandTotal
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "failed", message = ex.Message });
            }
        }

        // =====================================================
        // HELPERS PERKIRAAN BILLING
        // =====================================================
        private static bool IsRawatInapIP(string? jenisKunjungan)
        {
            var j = (jenisKunjungan ?? "").Trim().ToUpperInvariant();
            return j == "IP" || j == "RAWAT INAP" || j == "INAP";
        }

        private static string BuildJenisKunjunganMessage(string? jenisKunjungan)
        {
            var j = (jenisKunjungan ?? "").Trim().ToUpperInvariant();
            var readable = j switch
            {
                "OP" => "Rawat Jalan (OP)",
                "RAWAT JALAN" => "Rawat Jalan",
                "" => "bukan Rawat Inap (IP)",
                _ => jenisKunjungan ?? "bukan Rawat Inap (IP)"
            };

            return $"Maaf, prakiraan billing ini hanya untuk Rawat Inap (IP). Kunjungan yang dipilih adalah {readable}.";
        }

        private static string NormalizeGroup(string? jenisBilling, string? namaItem)
        {
            var jb = (jenisBilling ?? "").Trim();

            if (jb.Equals("Operasi", StringComparison.OrdinalIgnoreCase)) return "Tindakan Operasi";
            if (jb.Equals("Tindakan", StringComparison.OrdinalIgnoreCase)) return "Tindakan Rawat Inap";
            if (jb.Equals("Alkes", StringComparison.OrdinalIgnoreCase)) return "Alkes Khusus";
            if (jb.Equals("Kamar Ranap", StringComparison.OrdinalIgnoreCase)) return "Biaya Kamar Rawat";
            if (jb.Equals("Biaya Admin", StringComparison.OrdinalIgnoreCase)) return "Biaya Lain-lain";
            if (jb.Equals("Obat", StringComparison.OrdinalIgnoreCase)) return "Obat";
            if (jb.Equals("Pemeriksaan Lab", StringComparison.OrdinalIgnoreCase)) return "Pemeriksaan Lab";

            return "Lain-lain";
        }

        private static int HitungJumlahHariRanap(DateTime? tglMasuk, DateTime? tglKeluarFinal)
        {
            if (!tglMasuk.HasValue) return 1;

            var start = tglMasuk.Value;
            var end = tglKeluarFinal ?? DateTime.Now; // kalau belum pulang -> sekarang
            if (end < start) end = start;

            var durasi = end - start;
            var hari = (int)Math.Ceiling(durasi.TotalDays);
            if (hari < 1) hari = 1;

            return hari;
        }

        // End-date dari transfer: ambil transfer pertama (anchor>=start) untuk bed tsb,
        // end = TglKeluar ?? TglPindah
        private static DateTime? ResolveEndFromTransfer(
            Guid? bedId,
            DateTime? start,
            Dictionary<Guid, List<TransferPasien>> transferByBed)
        {
            if (!bedId.HasValue) return null;
            if (!transferByBed.TryGetValue(bedId.Value, out var list) || list.Count == 0) return null;

            var s = start ?? DateTime.MinValue;

            var candidate = list
                .Select(t => new { Transfer = t, Anchor = t.TglMasuk ?? t.TglPindah })
                .Where(x => x.Anchor.HasValue && x.Anchor.Value >= s)
                .OrderBy(x => x.Anchor!.Value)
                .FirstOrDefault();

            if (candidate == null) return null;

            return candidate.Transfer.TglKeluar ?? candidate.Transfer.TglPindah;
        }

        // Transfer yang relevan untuk segmen booking (bed sama, dalam range start..end jika end ada)
        private static List<TransferPasien> GetTransfersForSegment(
            Guid? bedId,
            DateTime? segStart,
            DateTime? segEnd,
            Dictionary<Guid, List<TransferPasien>> transferByBed)
        {
            var result = new List<TransferPasien>();
            if (!bedId.HasValue) return result;
            if (!transferByBed.TryGetValue(bedId.Value, out var list) || list.Count == 0) return result;

            var start = segStart ?? DateTime.MinValue;

            return list
                .Where(t =>
                {
                    var anchor = t.TglMasuk ?? t.TglPindah ?? DateTime.MinValue;
                    if (anchor < start) return false;
                    if (segEnd.HasValue && anchor > segEnd.Value) return false;
                    return true;
                })
                .OrderBy(t => t.TglMasuk ?? t.TglPindah ?? DateTime.MinValue)
                .ToList();
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
    }
}
