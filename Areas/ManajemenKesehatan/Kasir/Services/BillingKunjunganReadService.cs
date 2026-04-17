using System.Linq;
using System.Security.Cryptography;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Repositories;

public sealed class BillingKunjunganReadService : IBillingKunjunganReadService
{
    private readonly ApplicationDbContext _db;

    public BillingKunjunganReadService(ApplicationDbContext db)
    {
        _db = db;
    }

    #region DTO CLASS AND LIST
    // ================================
    // DTO LITE BILLING FOR LOOKUP
    // ================================
    private sealed class BillingLite
    {
        public Guid BillingId { get; set; }
        public string? BillingKode { get; set; }
        public Guid? ItemId { get; set; }
        public string? JenisBilling { get; set; }
        public string? NamaItem { get; set; }
        public bool? IsCovered { get; set; }
        public bool? IsCoveredExcess { get; set; }
        public string? Keterangan { get; set; }

        public int? QtyItem { get; set; }
        public decimal? HargaItem { get; set; }
        public decimal? SubTotalItem { get; set; }

        public bool? StatusBilling { get; set; }
        public DateTimeOffset? CreateDateTime { get; set; }
        public Guid KunjunganId { get; internal set; }
        public DateTime? TanggalInvoice { get; set; }
        public DateTime? TanggalJatuhTempo { get; set; }
    }

    // ================================
    // DTO LITE BILLING BAGIAN RACIKAN
    // ================================
    private sealed class RacikanDetailRow
    {
        public Guid RacikanId { get; set; }
        public Guid? ObatId { get; set; }
        public string? ObatName { get; set; }
        public int? QtyUsed { get; set; }
        public decimal? KomposisiDosis { get; set; }
        public decimal HTEPrice { get; set; }
    }

    // ================================
    // DTO PAGED BILLING
    // ================================
    public sealed class PagedResult<T>
    {
        public string Status { get; set; } = "success";
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalKunjungan { get; set; }
        public int TotalPages { get; set; }
        public T[] Data { get; set; } = Array.Empty<T>();
    }

    public sealed class BillingPagedQuery
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? PetugasId { get; set; }
        public StatusBayarEnum? sb {  get; set; }
        public EnumJenisKunjungan? jk {  get; set; }
        public string? Search { get; set; }
        public bool? isClosed { get; set; }
        public bool? isPks { get; set; }
        public bool? isCovered { get; set; }
        public bool? isCoveredExcess { get; set; }
        public string? asal { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PeriodeFilter? Periode { get; set; }

        public DateTime? AsOf { get; set; } // untuk kamar ranap sampai waktu ini
    }

    // ======================
    // DTO COVERAGE LIST
    // =====================
    private sealed class CoverageLookup
    {
        public HashSet<Guid> ObatIds { get; init; } = new();
        public Dictionary<Guid, decimal> KamarMarkup { get; init; } = new();      // key: KamarId
        public Dictionary<Guid, decimal> LabMarkup { get; init; } = new();        // key: PemeriksaanLabId
        public Dictionary<Guid, decimal> TindakanMarkup { get; init; } = new();   // key: TindakanId
    }


    // =================================
    // DTO PAGED PENDAPATAN HARIAN KASIR
    // =================================
    public sealed class PendapatanHarianPagedQuery
    {
        public DateTime? StartDate { get; set; }   // inclusive
        public DateTime? EndDate { get; set; }     // inclusive
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public sealed class PagedRekapResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRows { get; set; }     // jumlah hari
        public int TotalPages { get; set; }
        public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
    }
    #endregion

    // ============================
    // FUNCTION GET BILLING BY ID 
    // ============================
    public async Task<BillingKunjunganDto?> GetBillingKeseluruhanAsync(
        Guid kunjunganId,
        DateTime? asOf = null,
        CancellationToken ct = default)
    {
        var snap = asOf ?? DateTime.Now;

        // =========================
        // 1) HEADER
        // =========================
        var header = await (
            from k in _db.Kunjungans.AsNoTracking()
            where k.KunjunganID == kunjunganId && !k.IsDelete

            join p0 in _db.PendaftaranPasienBarus.AsNoTracking()
                on k.PasienId equals p0.PendaftaranPasienBaruId into pg
            from p in pg.DefaultIfEmpty()

            join d0 in _db.Dokters.AsNoTracking()
                on k.DokterId equals d0.DokterId into dg
            from d in dg.DefaultIfEmpty()

            join poli0 in _db.Polikliniks.AsNoTracking()
                on k.PoliklinikId equals poli0.PoliklinikId into polig
            from poli in polig.DefaultIfEmpty()

            join a0 in _db.Asuransis.AsNoTracking()
                on k.AsuransiId equals a0.AsuransiId into ag
            from a in ag.DefaultIfEmpty()

                // JOIN berdasarkan AsuransiPasienId yang tersimpan di kunjungan
            join ap0 in _db.AsuransiPasiens.AsNoTracking()
                on k.AsuransiPasienId equals (Guid?)ap0.AsuransiPasienId into apg
            from ap in apg.DefaultIfEmpty()

            select new
            {
                k.KunjunganID,
                k.JenisKunjungan,
                k.AsalKunjungan,
                k.IsClosed,
                TanggalKunjungan = k.TglMasuk,
                k.TipePembayaran,
                k.PasienId,
                k.AsuransiId,

                NamaLengkap = p != null ? p.NamaLengkap : null,
                NoHp = p != null ? p.NoPasien : null,
                NoRekamMedis = p != null ? p.NoRekamMedis : null,
                TanggalLahir = p != null ? p.TanggalLahir : (DateTime?)null,

                NmDokter = d != null ? d.NmDokter : null,
                NamaPoliklinik = poli != null ? poli.NamaPoliklinik : null,
                NamaAsuransi = a != null ? a.NamaAsuransi : null,
                NoPolis = ap != null ? ap.NoPolis : null,
                IsPKS = a != null ? a.IsPKS : null,
            }
        ).FirstOrDefaultAsync(ct);

        if (header == null) return null;


        var dto = new BillingKunjunganDto
        {
            AsOf = snap,
            PasienId = header.PasienId,
            KunjunganID = header.KunjunganID,
            JenisKunjungan = header.JenisKunjungan,
            TanggalKunjungan = header.TanggalKunjungan,
            AsalKunjungan = header.AsalKunjungan,
            IsClosed = header.IsClosed,
            NamaLengkap = header.NamaLengkap,
            NoHP = header.NoHp,
            NoRekamMedis = header.NoRekamMedis,
            NmDokter = header.NmDokter,
            NamaPoliklinik = header.NamaPoliklinik,
            TipePembayaran = header.TipePembayaran,
            NamaAsuransi = header.NamaAsuransi,
            NoPolis = header.NoPolis,
            IsPKS = header.IsPKS,
            Umur = HitungUmurLengkap(header.TanggalLahir)
        };

        // init list (jaga-jaga kalau DTO belum init)
        dto.DaftarVisitDokter ??= new List<object>();
        dto.DaftarKamarRanap ??= new List<object>();

        // KasirId (opsional)
        dto.KasirId = await _db.MainKasirs.AsNoTracking()
            .Where(x => x.KunjunganId == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
            .Select(x => (Guid?)x.KasirId)
            .FirstOrDefaultAsync(ct);

        // =========================
        // 1B) COVER OBAT ASURANSI (HANYA DARI KUNJUNGAN)
        // RULE: kalau Kunjungan.AsuransiId kosong -> tidak pakai asuransi saat kunjungan ini
        // =========================
        Guid? asuransiIdEfektif = header.AsuransiId;

        // pakai asuransi hanya kalau AsuransiId terisi (kunjungan) dan tipe pembayaran memang "Asuransi"
        // kalau kamu mau cukup cek AsuransiId saja, ganti jadi: var isAsuransiCase = asuransiIdEfektif.HasValue;
        var isAsuransiCase =
            asuransiIdEfektif.HasValue &&
            string.Equals(dto.TipePembayaran, "Asuransi", StringComparison.OrdinalIgnoreCase);

        CoverageLookup cover = new();
        if (isAsuransiCase)
            cover = await LoadCoverageLookupAsync(asuransiIdEfektif!.Value, snap, ct);

        // =========================
        // 2) LOAD BILLINGS + MAP
        // =========================
        var billings = await _db.Billings.AsNoTracking()
            .Where(b => b.KunjunganId == kunjunganId && (b.IsDelete == false || b.IsDelete == null))
            .Select(b => new BillingLite
            {
                BillingId = b.BillingId,
                BillingKode = b.BillingKode,
                ItemId = b.ItemId,
                JenisBilling = b.JenisBilling,
                NamaItem = b.NamaItem,
                IsCovered = b.IsCovered,
                IsCoveredExcess = b.IsCoveredExcess,
                Keterangan = b.Keterangan,
                QtyItem = b.QtyItem,
                HargaItem = b.HargaItem,
                SubTotalItem = b.SubTotalItem,
                StatusBilling = b.StatusBilling,
                TanggalInvoice = b.TanggalInvoice,
                TanggalJatuhTempo = b.TanggalJatuhTempo,


                CreateDateTime = b.CreateDateTime
            })
            .ToListAsync(ct);

        var billingMap = billings
        .Where(b => !string.IsNullOrWhiteSpace(b.JenisBilling))
        .GroupBy(b => new { Jenis = b.JenisBilling!, Item = b.ItemId }) // Item = Guid? (boleh null)
        .ToDictionary(
            g => (g.Key.Jenis, g.Key.Item), // key = (string, Guid?)
            g => g.OrderByDescending(x => x.CreateDateTime ?? DateTimeOffset.MinValue).First()
        );

         // ✅ FindBilling sekarang boleh itemId null
        BillingLite? FindBilling(string jenis, Guid? itemId)
        {
           billingMap.TryGetValue((jenis, itemId), out var b);
           return b;
         }

        // =========================
        // 3) LAB
        // =========================
        var labRows = await (
            from lbd in _db.LabBookingDetails.AsNoTracking()
            where lbd.PasienId == header.PasienId
            join lp in _db.LabPemeriksaans.AsNoTracking()
                on lbd.PemeriksaanLabId equals lp.PemeriksaanLabId into pg
            from lp in pg.DefaultIfEmpty()
            join la in _db.Labs.AsNoTracking()
                on lbd.LabId equals la.LabId into lg
            from la in lg.DefaultIfEmpty()
            select new
            {
                lbd.BookingLabId,
                lbd.DetailBookingLabId,
                PemeriksaanLabId = lbd.PemeriksaanLabId,
                NamaLab = la != null ? la.NamaLab : null,
                NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                HargaPemeriksaan = (decimal?)lp.HargaPemeriksaan ?? 0m
            }
        ).ToListAsync(ct);

        dto.DaftarPemeriksaanLab = labRows
            .GroupBy(x => x.DetailBookingLabId)
            .Select(g =>
            {
                var x = g.First();
                var bill = FindBilling("Pemeriksaan Lab", x.DetailBookingLabId);
                //var isCovered =
                //    isAsuransiCase &&
                //    x.PemeriksaanLabId != null &&
                //    cover.LabMarkup.ContainsKey(x.PemeriksaanLabId.Value);
                var qty = bill?.QtyItem ?? 1;
                var subtotal = bill?.SubTotalItem ?? x.HargaPemeriksaan;

                return (object)new
                {
                    x.BookingLabId,
                    x.DetailBookingLabId,
                    x.NamaLab,
                    x.NamaPemeriksaan,
                    IsCovered = bill?.IsCovered,
                    IsCoveredExcess = bill?.IsCoveredExcess,
                    HargaPemeriksaan = x.HargaPemeriksaan,
                    Qty = qty,
                    Subtotal = subtotal,
                    BillingId = bill?.BillingId,
                    BillingKode = bill?.BillingKode,
                    StatusBilling = bill?.StatusBilling,
                    jenisBilling = bill?.JenisBilling,
                    TanggalInvoice = bill?.TanggalInvoice,
                    TanggalJatuhTempo = bill?.TanggalJatuhTempo,

                    // ✅ dpd lokal
                    DPD = HitungDpd(bill?.TanggalJatuhTempo, snap)
                };
            })
            .ToList();

        dto.TotalPemeriksaanLab = dto.DaftarPemeriksaanLab.Sum(x => (decimal)((dynamic)x).Subtotal);

        // =========================
        // 4) RESEP: OBAT + RACIKAN
        // =========================
        var resepRows = await (
            from r in _db.Reseps.AsNoTracking()
            where r.KunjunganId == kunjunganId && !r.IsDelete
            join dr in _db.DetailReseps.AsNoTracking()
                on r.ResepId equals dr.ResepId
            where !dr.IsDelete
            join o in _db.Obats.AsNoTracking()
                on dr.ObatId equals o.ObatId into og
            from o in og.DefaultIfEmpty()
            join rc in _db.Racikans.AsNoTracking()
                on dr.RacikanId equals rc.RacikanId into rg
            from rc in rg.DefaultIfEmpty()
            select new { r.ResepId, dr, o, rc }
        ).ToListAsync(ct);

        // 4A) OBAT non-racikan  ✅ tambahkan IsCoverAsuransi
        dto.DaftarObat = resepRows
            .Where(x => x.dr != null && x.o != null && x.dr.IsRacikan != true)
            .GroupBy(x => x.dr.DetailResepId)
            .Select(g =>
            {
                var x = g.First();
                var bill = FindBilling("Obat", x.dr.ObatId);

                var hte = (decimal?)x.o!.HTEPrice ?? 0m;
                var qty = bill?.QtyItem ?? x.dr.Qty;
                var harga = bill?.HargaItem ?? hte;
                var subtotal = bill?.SubTotalItem ?? (x.dr.Qty * hte);

                //var isCovered =
                //    isAsuransiCase &&
                //    x.dr.ObatId != null &&
                //    cover.ObatIds.Contains(x.dr.ObatId.Value);

                return (object)new
                {
                    x.ResepId,
                    x.dr.DetailResepId,
                    x.dr.ObatId,
                    x.o.ObatName,

                    // ✅ tambahan
                    IsCovered = bill?.IsCovered,
                    IsCoveredExcess = bill?.IsCoveredExcess,

                    Qty = qty,
                    Harga = harga,
                    Subtotal = subtotal,
                    BillingId = bill?.BillingId,
                    BillingKode = bill?.BillingKode,
                    StatusBilling = bill?.StatusBilling,
                    jenisBilling = bill?.JenisBilling,
                    TanggalInvoice = bill?.TanggalInvoice,
                    TanggalJatuhTempo = bill?.TanggalJatuhTempo,

                    // ✅ dpd lokal
                    DPD = HitungDpd(bill?.TanggalJatuhTempo, snap),
                    x.dr.Signa,
                    x.dr.SignaTambahan,
                    x.dr.StatusPengambilanObat
                };
            })
            .ToList();

        dto.TotalObat = dto.DaftarObat.Sum(x => (decimal)((dynamic)x).Subtotal);

        // 4B) RACIKAN (tanpa cover asuransi)
        var racikanIds = resepRows
            .Where(x => x.dr?.IsRacikan == true && x.dr.RacikanId != null)
            .Select(x => x.dr!.RacikanId!.Value)
            .Distinct()
            .ToList();

        List<RacikanDetailRow> racikanDetails;
        if (racikanIds.Any())
        {
            racikanDetails = await (
                from rd in _db.RacikanDetails.AsNoTracking()
                join ob in _db.Obats.AsNoTracking() on rd.ObatId equals ob.ObatId
                where rd.RacikanId != null && racikanIds.Contains(rd.RacikanId.Value)
                select new RacikanDetailRow
                {
                    RacikanId = rd.RacikanId!.Value,
                    ObatId = rd.ObatId,
                    ObatName = ob.ObatName,
                    QtyUsed = rd.QtyUsed,
                    KomposisiDosis = rd.KomposisiDosis,
                    HTEPrice = (decimal?)ob.HTEPrice ?? 0m
                }
            ).ToListAsync(ct);
        }
        else
        {
            racikanDetails = new List<RacikanDetailRow>();
        }

        var racikanMap = racikanDetails
            .GroupBy(x => x.RacikanId)
            .ToDictionary(g => g.Key, g => g.ToList());

        dto.DaftarRacikan = resepRows
            .Where(x => x.dr != null && x.dr.IsRacikan == true && x.rc != null && x.dr.RacikanId != null)
            .GroupBy(x => x.dr.RacikanId)
            .Select(g =>
            {
                var x = g.First();
                var bill = FindBilling("Obat", x.dr.RacikanId);

                racikanMap.TryGetValue(x.dr.RacikanId!.Value, out var komps);

                return (object)new
                {
                    x.ResepId,
                    x.dr.RacikanId,
                    x.rc!.NamaRacikan,
                    x.rc.KodeRacikan,
                    Qty = bill?.QtyItem,
                    Harga = bill?.HargaItem,
                    Subtotal = bill?.SubTotalItem ?? 0m,
                    BillingId = bill?.BillingId,
                    BillingKode = bill?.BillingKode,
                    jenisBilling = bill?.JenisBilling,
                    StatusBilling = bill?.StatusBilling,
                    TanggalInvoice = bill?.TanggalInvoice,
                    TanggalJatuhTempo = bill?.TanggalJatuhTempo,

                    // ✅ dpd lokal
                    DPD = HitungDpd(bill?.TanggalJatuhTempo, snap),
                    x.dr.Signa,
                    x.dr.SignaTambahan,
                    x.dr.StatusPengambilanObat,
                    Komposisi = komps?.Select(k => new
                    {
                        k.ObatId,
                        k.ObatName,
                        k.QtyUsed,
                        k.KomposisiDosis,
                        k.HTEPrice
                    })
                };
            })
            .ToList();

        dto.TotalRacikan = dto.DaftarRacikan.Sum(x => (decimal)((dynamic)x).Subtotal);

        // =========================
        // 5) TINDAKAN
        // =========================
        var tindakanLogs = await (
            from tk in _db.TindakanKunjungans.AsNoTracking()
            where tk.KunjunganId == kunjunganId
            join t in _db.Tindakans.AsNoTracking()
                on tk.TindakanId equals t.TindakanId into tg
            from t in tg.DefaultIfEmpty()
            select new
            {
                tk.TindakanId,
                NamaTindakan = t != null ? t.NamaTindakan : null,
                Qty = tk.Quantity ?? 1,
                HargaLog = (decimal?)tk.Total ?? 0m, // asumsi: harga satuan (sesuai pola lama kamu)
                tk.CreateDateTime
            }
        ).ToListAsync(ct);

        dto.DaftarTindakan = tindakanLogs
            .Where(x => x.TindakanId != null)
            .GroupBy(x => x.TindakanId) // ✅ 1 baris per master tindakan
            .Select(g =>
            {
                var tindakanId = g.Key;
                var nama = g.First().NamaTindakan;

                // total tindakan yang sama dilakukan berapa kali
                var qtyLogTotal = g.Sum(x => x.Qty);

                // harga log terbaru (fallback kalau billing & markup tidak ada)
                var hargaLogTerbaru = g
                    .OrderByDescending(x => x.CreateDateTime)
                    .First().HargaLog;

                // billing hanya 1x per tindakan master
                var bill = FindBilling("Tindakan", tindakanId);

                // cover: markup per tindakan master
                //var isCovered =
                //    isAsuransiCase &&
                //    cover.TindakanMarkup.TryGetValue(tindakanId, out var markup);

                // harga: Billing > Markup cover > harga log
                var hargaEfektif =
                    bill?.HargaItem;
                    //?? (isCovered ? markup : hargaLogTerbaru);

                // qty final: billing qty kalau ada, kalau tidak pakai hasil agregasi log
                var qtyFinal = bill?.QtyItem ?? qtyLogTotal;

                // subtotal final: billing subtotal kalau ada, kalau tidak hitung sendiri
                var subtotal = bill?.SubTotalItem ?? (qtyFinal * hargaEfektif);

                return (object)new
                {
                    TindakanId = tindakanId,
                    NamaTindakan = nama,

                    IsCovered = bill?.IsCovered,
                    IsCoveredExcess = bill?.IsCoveredExcess,

                    Qty = qtyFinal,
                    Harga = hargaEfektif,
                    Subtotal = subtotal,
                    BillingId = bill?.BillingId,
                    BillingKode = bill?.BillingKode,
                    StatusBilling = bill?.StatusBilling,
                    jenisBilling = bill?.JenisBilling,
                    TanggalInvoice = bill?.TanggalInvoice,
                    TanggalJatuhTempo = bill?.TanggalJatuhTempo,
                    DPD = HitungDpd(bill?.TanggalJatuhTempo, snap)
                };
            })
            .ToList();

        dto.TotalTindakan = dto.DaftarTindakan.Sum(x => (decimal)((dynamic)x).Subtotal);


        // =========================
        // 6) BIAYA ADMIN (dari billings)
        // =========================
        dto.DaftarBiayaAdmin = billings
            .Where(b => string.Equals(b.JenisBilling, "Biaya Admin", StringComparison.OrdinalIgnoreCase))
            .Select(b => (object)new
            {
                b.BillingId,
                b.NamaItem,
                b.HargaItem,
                b.QtyItem,
                b.SubTotalItem,
                b.BillingKode,
                b.StatusBilling,
                b.JenisBilling,
                TanggalInvoice = b?.TanggalInvoice,
                TanggalJatuhTempo = b?.TanggalJatuhTempo,

                // ✅ dpd lokal
                DPD = HitungDpd(b?.TanggalJatuhTempo, snap)
            })
            .ToList();

        dto.TotalBiayaAdmin = dto.DaftarBiayaAdmin.Sum(x => (decimal?)((dynamic)x).SubTotalItem ?? 0m);

        // =========================
        // 7) ALKES (dari billings)
        // =========================
        dto.DaftarAlkes = billings
            .Where(b => string.Equals(b.JenisBilling, "Alkes", StringComparison.OrdinalIgnoreCase))
            .Select(b =>
            {
                var qty = b.QtyItem ?? 1;
                var harga = b.HargaItem ?? 0m;
                var subtotal = b.SubTotalItem ?? (qty * harga);

                return (object)new
                {
                    b.BillingId,
                    b.BillingKode,
                    b.ItemId,
                    b.NamaItem,
                    b.Keterangan,
                    Qty = qty,
                    Harga = harga,
                    Subtotal = subtotal,
                    b.StatusBilling,
                    TanggalInvoice = b?.TanggalInvoice,
                    TanggalJatuhTempo = b?.TanggalJatuhTempo,

                    // ✅ dpd lokal
                    DPD = HitungDpd(b?.TanggalJatuhTempo, snap)
                };
            })
            .ToList();

        dto.TotalAlkes = dto.DaftarAlkes.Sum(x => (decimal)((dynamic)x).Subtotal);

        // =========================
        // 8) VISIT DOKTER (TarifKelas)
        // =========================
        var visitRows = await _db.VisitDokters.AsNoTracking()
            .Where(v => v.KunjunganId == kunjunganId && (v.IsDelete == false || v.IsDelete == null))
            .Select(v => new
            {
                v.VisitDokterId,
                v.DokterId,
                v.KelasId,
                v.TanggalVisit,
                v.WaktuVisit,
                v.Keterangan
            })
            .ToListAsync(ct);

        dto.DaftarVisitDokter = new List<object>();
        dto.TotalBiayaVisitDokter = 0m;

        //if (visitRows.Count > 0)
        //{
        //    var billingVisitMap = billings
        //        .Where(b => b.ItemId != null && string.Equals(b.JenisBilling, "Visit Dokter", StringComparison.OrdinalIgnoreCase))
        //        .GroupBy(b => b.ItemId!.Value)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.OrderByDescending(x => x.CreateDateTime ?? DateTime.MinValue).First()
        //        );

        //    var dokterIds = visitRows.Select(x => x.DokterId).Distinct().ToList();

        //    var dokterNameMap = await _db.Dokters.AsNoTracking()
        //        .Where(d => dokterIds.Contains(d.DokterId))
        //        .Select(d => new { d.DokterId, d.NmDokter })
        //        .ToDictionaryAsync(x => x.DokterId, x => x.NmDokter, ct);

        //    var kelasIds = visitRows
        //        .Where(x => x.KelasId != null)
        //        .Select(x => x.KelasId!.Value)
        //        .Distinct()
        //        .ToList();

        //    // ✅ pakai DbSet kamu (di kode kamu: TarifKelass)
        //    var tarifRows = await _db.TarifKelass.AsNoTracking()
        //        .Where(t => dokterIds.Contains(t.DokterId) && kelasIds.Contains((Guid)t.KelasId))
        //        .Where(t => (t.IsDelete == false || t.IsDelete == null))
        //        .Select(t => new
        //        {
        //            t.DokterId,
        //            t.KelasId,
        //            TarifDokter = (decimal?)t.TarifDokter ?? 0m,
        //            t.CreateDateTime
        //        })
        //        .ToListAsync(ct);

        //    var tarifMap = tarifRows
        //        .GroupBy(x => (x.DokterId, x.KelasId))
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.OrderByDescending(x => x.CreateDateTime).First().TarifDokter
        //        );

        //    foreach (var grp in visitRows.GroupBy(x => new { x.DokterId, x.KelasId }))
        //    {
        //        var dokterId = grp.Key.DokterId;
        //        var kelasId = grp.Key.KelasId; // Guid?

        //        dokterNameMap.TryGetValue((Guid)dokterId, out var nmDokter);

        //        decimal tarifPerVisit = 0m;
        //        if (kelasId.HasValue && tarifMap.TryGetValue((dokterId, kelasId.Value), out var t))
        //            tarifPerVisit = t;

        //        var visitDetails = grp
        //            .OrderBy(x => x.TanggalVisit ?? DateTime.MinValue)
        //            .Select(v =>
        //            {
        //                billingVisitMap.TryGetValue(v.VisitDokterId, out var bill);

        //                var qty = bill?.QtyItem ?? 1;
        //                var harga = bill?.HargaItem ?? tarifPerVisit;
        //                var subtotal = bill?.SubTotalItem ?? (qty * harga);

        //                return new
        //                {
        //                    v.VisitDokterId,
        //                    v.TanggalVisit,
        //                    v.WaktuVisit,
        //                    v.Keterangan,

        //                    Qty = qty,
        //                    Harga = harga,
        //                    Subtotal = subtotal,

        //                    BillingId = bill?.BillingId,
        //                    BillingKode = bill?.BillingKode,
        //                    StatusBilling = bill?.StatusBilling,
        //                    TanggalInvoice = bill?.TanggalInvoice,
        //                    TanggalJatuhTempo = bill?.TanggalJatuhTempo,

        //                    // ✅ dpd lokal
        //                    DPD = HitungDpd(bill?.TanggalJatuhTempo, snap)
        //                };
        //            })
        //            .ToList();

        //        var subtotalGroup = visitDetails.Sum(x => x.Subtotal);

        //        dto.TotalBiayaVisitDokter += subtotalGroup;

        //        dto.DaftarVisitDokter.Add(new
        //        {
        //            DokterId = dokterId,
        //            NmDokter = nmDokter,
        //            KelasId = kelasId,
        //            Qty = visitDetails.Count,
        //            HargaPerVisit = tarifPerVisit,
        //            Subtotal = subtotalGroup,
        //            Visits = visitDetails
        //        });
        //    }
        //}

        // =========================
        // 9) KAMAR RANAP (IP): hitung sampai SNAPSHOT asOf
        // =========================
        if (IsRawatInapIP(dto.JenisKunjungan))
        {
            var bookingRanaps = await _db.BookingBedRanaps.AsNoTracking()
                .Where(x => x.KunjunganId == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
                .ToListAsync(ct);

            var transfers = await _db.TransferPasiens.AsNoTracking()
                .Where(x => x.KunjunganId == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
                .ToListAsync(ct);

            var transferByBed = transfers
                .Where(t => t.BedId != null)
                .GroupBy(t => t.BedId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.TglMasuk ?? x.TglPindah ?? DateTime.MinValue).ToList()
                );

            var billingKamarMap = billings
                .Where(b => b.ItemId != null && string.Equals(b.JenisBilling, "Kamar Ranap", StringComparison.OrdinalIgnoreCase))
                .GroupBy(b => b.ItemId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreateDateTime ?? DateTime.MinValue).First()
                );

            foreach (var kamarGroup in bookingRanaps.GroupBy(x => x.KamarId))
            {
                var kamarId = kamarGroup.Key;
                billingKamarMap.TryGetValue((Guid)kamarId, out var billKamar);

                var tarifPerHari = billKamar?.HargaItem ?? 0m;

                int totalHari = 0;
                DateTime? tglMasukAwal = null;
                DateTime? tglKeluarAkhir = null;

                var segments = new List<object>();

                var bookingsKamar = kamarGroup.OrderBy(x => x.TglMasuk ?? DateTime.MinValue).ToList();
                // ✅ cover kamar + markup
                //var isCoveredKamar =
                //    isAsuransiCase &&
                //    kamarId.HasValue &&
                //    cover.KamarMarkup.TryGetValue(kamarId.Value, out var kamarMarkup);
                foreach (var bk in bookingsKamar)
                {
                    tglMasukAwal ??= bk.TglMasuk;

                    var endTransfer = ResolveEndFromTransfer(bk.BedId, bk.TglMasuk, transferByBed);

                    // endFinal selalu DateTime (bukan nullable), dibatasi <= snap
                    DateTime endFinal = bk.TglKeluar ?? endTransfer ?? snap;
                    if (endFinal > snap) endFinal = snap;

                    var hari = HitungJumlahHariRanap(bk.TglMasuk, endFinal, snap);
                    totalHari += hari;

                    if (!tglKeluarAkhir.HasValue || endFinal > tglKeluarAkhir.Value)
                        tglKeluarAkhir = endFinal;

                    var transfersSegmen = GetTransfersForSegment(bk.BedId, bk.TglMasuk, endFinal, transferByBed);

                    segments.Add(new
                    {
                        bk.BookingBedRanapId,
                        bk.KamarId,
                        bk.BedId,
                        TglMasukBooking = bk.TglMasuk,
                        TglKeluarBooking = bk.TglKeluar,
                        TglKeluarTransfer = endTransfer,
                        TglKeluarFinal = endFinal,
                        JumlahHari = hari,
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

                var subtotal = tarifPerHari * totalHari;
                dto.TotalKamarRanap += subtotal;

                dto.DaftarKamarRanap.Add(new
                {
                    BillingId = billKamar?.BillingId,
                    BillingKode = billKamar?.BillingKode,
                    jenisBilling = billKamar?.JenisBilling,
                    StatusBilling = billKamar?.StatusBilling,
                    TanggalInvoice = billKamar?.TanggalInvoice,
                    TanggalJatuhTempo = billKamar?.TanggalJatuhTempo,

                    // ✅ dpd lokal
                    DPD = HitungDpd(billKamar?.TanggalJatuhTempo, snap),

                    KamarId = kamarId,
                    IsCovered = billKamar?.IsCovered,
                    IsCoveredExcess = billKamar?.IsCoveredExcess,
                    NamaItem = billKamar?.NamaItem,

                    HargaPerHari = tarifPerHari,
                    JumlahHari = totalHari,
                    Subtotal = subtotal,

                    TglMasukBookingAwal = tglMasukAwal,
                    TglKeluarFinalAkhir = tglKeluarAkhir,
                    Segments = segments,

                    AsOf = snap
                });
            }
        }

        // =============================
        // 10) Deposito Ranap
        // ==============================
        var lastSaldoDp =
            await GetLastSaldoByKunjunganIdAsync(_db,kunjunganId,ct);

        dto.TotalSaldoDeposito = lastSaldoDp.SaldoDeposit;
        dto.NominalKeluar = lastSaldoDp.NominalKeluar;
        dto.NominalMasuk = lastSaldoDp.NominalMasuk;

        // ================
        // 11) Biaya lain2
        // ================
        dto.DaftarBiayaLain = billings
            .Where(b => string.Equals(b.JenisBilling, "Biaya Lain - Lain", StringComparison.OrdinalIgnoreCase))
            .Select(b => (object)new
            {
                b.BillingId,
                b.NamaItem,
                b.HargaItem,
                b.QtyItem,
                b.SubTotalItem,
                b.BillingKode,
                b.StatusBilling,
                b.JenisBilling,
                TanggalInvoice = b?.TanggalInvoice,
                TanggalJatuhTempo = b?.TanggalJatuhTempo,

                // ✅ dpd lokal
                DPD = HitungDpd(b?.TanggalJatuhTempo, snap)
            })
            .ToList();

        dto.TotalBiayaLain = dto.DaftarBiayaLain.Sum(x => (decimal?)((dynamic)x).SubTotalItem ?? 0m);

        // =========================
        // 11) TOTAL KESELURUHAN (asuransi dan mandiri)
        // =========================
        var DepositRanap = dto.TotalSaldoDeposito;

        var asuransi =
            SumCovered(dto.DaftarPemeriksaanLab) +
            SumCovered(dto.DaftarObat) +
            SumCovered(dto.DaftarTindakan) +
            SumCovered(dto.DaftarKamarRanap);

        var asuransiExcess =
            SumCoveredExcess(dto.DaftarPemeriksaanLab) +
            SumCoveredExcess(dto.DaftarObat) +
            SumCoveredExcess(dto.DaftarTindakan) +
            SumCoveredExcess(dto.DaftarKamarRanap);

        var mandiri =
            SumUncovered(dto.DaftarPemeriksaanLab) +
            SumUncovered(dto.DaftarObat) +
            SumUncovered(dto.DaftarTindakan) +
            SumUncovered(dto.DaftarKamarRanap)
            // komponen yang memang tidak punya IsCovered → anggap mandiri
            + dto.TotalRacikan
            + dto.TotalBiayaAdmin
            + dto.TotalAlkes
            + dto.TotalBiayaVisitDokter
            + dto.TotalBiayaLain;

        var ppnRate = dto.PPN / 100m;
        dto.SubTotalAsuransi = asuransi;
        dto.SubTotalAsuransiExcess = asuransiExcess;

        dto.SebelumTaxTotalMandiri = Math.Round(mandiri, 2, MidpointRounding.AwayFromZero);
        dto.PajakTotalMandiri = Math.Round((dto.SebelumTaxTotalMandiri ?? 0m) * ppnRate, 2, MidpointRounding.AwayFromZero);
        dto.SubTotalMandiri = Math.Round((dto.SebelumTaxTotalMandiri ?? 0m) + (dto.PajakTotalMandiri ?? 0m), 2, MidpointRounding.AwayFromZero);
        
        //dto.TotalKeseluruhan = mandiri + Math.Round(mandiri * 0.11m);
        //dto.TotalKeseluruhan =
        //    dto.TotalPemeriksaanLab +
        //    dto.TotalObat +
        //    dto.TotalRacikan +
        //    dto.TotalTindakan +
        //    dto.TotalBiayaAdmin +
        //    dto.TotalAlkes +
        //    dto.TotalBiayaVisitDokter +
        //    dto.TotalKamarRanap;

        return dto;
    }

    // ================================
    // FUNCTION GET ALL BILLING PAGED
    // ================================
    public async Task<PagedResult<object>> GetBillingPagedAsync(BillingPagedQuery query, CancellationToken ct = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var snap = query.AsOf ?? DateTime.Now;

        // ============================================================
        // BASE QUERY KUNJUNGAN (FILTER)
        // ============================================================
        var baseQuery = _db.Kunjungans.AsNoTracking().Where(k => !k.IsDelete);

        if (query.KunjunganId.HasValue && query.KunjunganId.Value != Guid.Empty)
            baseQuery = baseQuery.Where(k => k.KunjunganID == query.KunjunganId.Value);

        if (query.PasienId.HasValue && query.PasienId.Value != Guid.Empty)
            baseQuery = baseQuery.Where(k => k.PasienId == query.PasienId.Value);

        if (query.jk.HasValue)
            baseQuery = baseQuery.Where(k => k.JenisKunjungan == query.jk.Value.ToString());

        if (query.isClosed.HasValue)
            baseQuery = baseQuery.Where(k => k.IsClosed == query.isClosed);

        if (query.sb.HasValue)
        {
            // sesuaikan string di DB kamu
            var wantedStatus = query.sb.Value switch
            {
                StatusBayarEnum.Lunas => "Lunas",
                StatusBayarEnum.Cicil => "Cicil",
                StatusBayarEnum.BelumBayar => "Belum Lunas", 
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(wantedStatus))
            {
                wantedStatus = wantedStatus.Trim();

                // ✅ perbaiki precedence + pastikan KunjunganId non-null
                var kasirQ = _db.MainKasirs.AsNoTracking()
                    .Where(m => (m.IsDelete == false || m.IsDelete == null) && m.KunjunganId != null);

                if (query.sb.Value == StatusBayarEnum.BelumBayar)
                {
                    // Belum bayar = belum ada kasir sama sekali
                    // (+ opsional: atau ada status "Belum Lunas" kalau memang dipakai)
                    baseQuery = baseQuery.Where(k =>
                        !kasirQ.Any(m => m.KunjunganId == k.KunjunganID)
                        || kasirQ.Any(m => m.KunjunganId == k.KunjunganID && (m.StatusPembayaran ?? "").Trim() == wantedStatus)
                    );
                }
                else
                {
                    // Lunas/Cicil = harus ada kasir dengan status tsb
                    baseQuery = baseQuery.Where(k =>
                        kasirQ.Any(m => m.KunjunganId == k.KunjunganID && (m.StatusPembayaran ?? "").Trim() == wantedStatus)
                    );
                }
            }
        }

        if (query.isPks.HasValue)
        {
            var wanted = query.isPks.Value;

            // IsPKS hanya relevan kalau ada AsuransiId (opsional: juga TipePembayaran == "Asuransi")
            baseQuery = baseQuery.Where(k =>
                k.AsuransiId != null &&
                _db.Asuransis.AsNoTracking().Any(a =>
                    a.AsuransiId == k.AsuransiId.Value
                    // kalau Asuransi punya IsDelete, pakai ini (kalau tidak ada, hapus baris ini)
                    && (a.IsDelete == false || a.IsDelete == null)
                    // treat null as false biar aman
                    && (a.IsPKS ?? false) == wanted
                )
            );
        }

        //if (query.PetugasId.HasValue && query.PetugasId.Value != Guid.Empty)
        //{
        //    var petugasId = query.PetugasId.Value;

        //    baseQuery = baseQuery.Where(k =>
        //        _db.MainKasirs.AsNoTracking()
        //            .Where(m =>
        //                m.KunjunganId == k.KunjunganID &&
        //                (m.IsDelete == false || m.IsDelete == null))
        //            .OrderByDescending(m => m.CreateDateTime)
        //            .Select(m => m.CreateBy)
        //            .FirstOrDefault() == petugasId
        //    );
        //}

        if (!string.IsNullOrWhiteSpace(query.asal))
        {
            var asalQ = query.asal.Trim();
            baseQuery = baseQuery.Where(k =>
                k.AsalKunjungan != null &&
                EF.Functions.ILike(k.AsalKunjungan, $"%{asalQ}%"));
        }

        // ============================================================
        // FILTER NAMA & NO HP 
        // ============================================================
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            var isNumeric = s.All(char.IsDigit);

            baseQuery =
                from k in baseQuery
                join p0 in _db.PendaftaranPasienBarus.AsNoTracking()
                    on k.PasienId equals p0.PendaftaranPasienBaruId into pg
                from p in pg.DefaultIfEmpty()
                where p != null
                      && (
                          isNumeric
                              ? (p.NoPasien != null && EF.Functions.ILike(p.NoPasien, $"%{s}%"))
                              : ((p.NamaLengkap != null && EF.Functions.ILike(p.NamaLengkap, $"%{s}%"))
                                 || (p.NoPasien != null && EF.Functions.ILike(p.NoPasien, $"%{s}%")))
                         )
                select k;
        }

        // date range
        if (query.StartDate.HasValue && query.EndDate.HasValue)
        {
            var startUtc = new DateTimeOffset(DateTime.SpecifyKind(query.StartDate.Value.Date, DateTimeKind.Utc));
            var endUtc = new DateTimeOffset(DateTime.SpecifyKind(query.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc));
            baseQuery = baseQuery.Where(k => k.CreateDateTime >= startUtc && k.CreateDateTime <= endUtc);
        }

        // periode filter
        if (query.Periode.HasValue)
            baseQuery = (IQueryable<Kunjungan>)ApplyPeriodeFilter(baseQuery, query.Periode.Value);

        // ============================================================
        // PAGING IDs
        // ============================================================
        var totalKunjungan = await baseQuery.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalKunjungan / (double)pageSize);

        var pageIds = await baseQuery
            .OrderByDescending(k => k.CreateDateTime)
            .ThenByDescending(k => k.KunjunganID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(k => k.KunjunganID)
            .ToListAsync(ct);

        if (!pageIds.Any())
        {
            return new PagedResult<object>
            {
                Page = page,
                PageSize = pageSize,
                TotalKunjungan = totalKunjungan,
                TotalPages = totalPages,
                Data = Array.Empty<object>()
            };
        }

        // ============================================================
        // DEPOSIT RANAP TERBARU bulk per kunjungan
        // ============================================================
        var depositMap = await GetLatestSaldoByKunjunganIdsAsync(
            _db,
            pageIds,
            ct);

        // ============================================================
        // HEADERS bulk (seperti GetBillingKeseluruhanAsync)
        // ============================================================
        var headers = await (
            from k in _db.Kunjungans.AsNoTracking()
            where pageIds.Contains(k.KunjunganID) && !k.IsDelete

            join p0 in _db.PendaftaranPasienBarus.AsNoTracking()
                on k.PasienId equals p0.PendaftaranPasienBaruId into pg
            from p in pg.DefaultIfEmpty()

            join d0 in _db.Dokters.AsNoTracking()
                on k.DokterId equals d0.DokterId into dg
            from d in dg.DefaultIfEmpty()

            join poli0 in _db.Polikliniks.AsNoTracking()
                on k.PoliklinikId equals poli0.PoliklinikId into polig
            from poli in polig.DefaultIfEmpty()

            join a0 in _db.Asuransis.AsNoTracking()
                on k.AsuransiId equals a0.AsuransiId into ag
            from a in ag.DefaultIfEmpty()

                // ✅ LEFT JOIN AsuransiPasien, tapi khusus yang match AsuransiId kunjungan
                // JOIN berdasarkan AsuransiPasienId yang tersimpan di kunjungan
            join ap0 in _db.AsuransiPasiens.AsNoTracking()
                on k.AsuransiPasienId equals (Guid?)ap0.AsuransiPasienId into apg
            from ap in apg.DefaultIfEmpty()

            select new
            {
                k.KunjunganID,
                k.CreateDateTime,
                k.JenisKunjungan,
                k.AsalKunjungan,
                k.IsClosed,
                TanggalKunjungan = k.TglMasuk,
                k.TipePembayaran,
                k.PasienId,
                k.AsuransiId,
                NamaLengkap = p != null ? p.NamaLengkap : null,
                NoRekamMedis = p != null ? p.NoRekamMedis : null,
                TanggalLahir = p != null ? p.TanggalLahir : (DateTime?)null,
                NoHp = p != null ? p.NoPasien : null,
                NmDokter = d != null ? d.NmDokter : null,
                NamaPoliklinik = poli != null ? poli.NamaPoliklinik : null,
                NamaAsuransi = a != null ? a.NamaAsuransi : null,
                IsPKS = a != null ? a.IsPKS : null,
                NoPolis = ap != null ? ap.NoPolis : null,
            }
        ).ToListAsync(ct);


        var headerById = headers.ToDictionary(x => x.KunjunganID, x => x);
        var pasienIds = headers.Select(x => x.PasienId).Distinct().ToList();

        // ============================================================
        // KasirId bulk
        // ============================================================
        var kasirMap = await _db.MainKasirs.AsNoTracking()
            .Where(x => pageIds.Contains((Guid)x.KunjunganId) && (x.IsDelete == false || x.IsDelete == null))
            .GroupBy(x => x.KunjunganId)
            .Select(g => new
            {
                KunjunganId = g.Key,
                KasirId = (Guid?)g.OrderByDescending(x => x.CreateDateTime).Select(x => x.KasirId).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.KunjunganId, x => x.KasirId, ct);

        // ============================================================
        // BILLINGS bulk + latest map per (kunjungan, jenis, item)
        // ============================================================
        var billings = await _db.Billings.AsNoTracking()
            .Where(b => b.KunjunganId != null
                        && pageIds.Contains(b.KunjunganId.Value)
                        && (b.IsDelete == false || b.IsDelete == null))
            .Select(b => new BillingLite
            {
                KunjunganId = b.KunjunganId!.Value,
                BillingId = b.BillingId,
                BillingKode = b.BillingKode,
                ItemId = b.ItemId,
                JenisBilling = b.JenisBilling,
                NamaItem = b.NamaItem,
                IsCovered = b.IsCovered,
                IsCoveredExcess = b.IsCoveredExcess,
                Keterangan = b.Keterangan,
                QtyItem = b.QtyItem,
                HargaItem = b.HargaItem,
                SubTotalItem = b.SubTotalItem,
                StatusBilling = b.StatusBilling,
                TanggalInvoice = b.TanggalInvoice,
                TanggalJatuhTempo = b.TanggalJatuhTempo,
                CreateDateTime = b.CreateDateTime
            })
            .ToListAsync(ct);

        var billingLatestMap = billings
            .Where(b => b.ItemId.HasValue && !string.IsNullOrWhiteSpace(b.JenisBilling))
            .GroupBy(b => new { b.KunjunganId, Jenis = b.JenisBilling!, Item = b.ItemId!.Value })
            .ToDictionary(
                g => (g.Key.KunjunganId, g.Key.Jenis, g.Key.Item),
                g => g.OrderByDescending(x => x.CreateDateTime ?? DateTime.MinValue).First()
            );

        BillingLite? FindBilling(Guid kid, string jenis, Guid? itemId)
        {
            if (!itemId.HasValue) return null;
            billingLatestMap.TryGetValue((kid, jenis, itemId.Value), out var b);
            return b;
        }

        // Set Coveran Asuransi
        var asuransiAktifIds = headers
            .Where(h => h.AsuransiId.HasValue
                        && string.Equals(h.TipePembayaran, "Asuransi", StringComparison.OrdinalIgnoreCase))
            .Select(h => h.AsuransiId!.Value)
            .Distinct()
            .ToList();

        var coverageByAsuransi = new Dictionary<Guid, CoverageLookup>();
        foreach (var aid in asuransiAktifIds)
        {
            coverageByAsuransi[aid] = await LoadCoverageLookupAsync(aid, snap, ct);
        }

        // ============================================================
        // LAB bulk (filter by PasienId seperti function kamu)
        // ============================================================
        var labRows = await (
            from lbd in _db.LabBookingDetails.AsNoTracking()
            where pasienIds.Contains(lbd.PasienId)
            join lp in _db.LabPemeriksaans.AsNoTracking()
                on lbd.PemeriksaanLabId equals lp.PemeriksaanLabId into pg
            from lp in pg.DefaultIfEmpty()
            join la in _db.Labs.AsNoTracking()
                on lbd.LabId equals la.LabId into lg
            from la in lg.DefaultIfEmpty()
            select new
            {
                lbd.PasienId,
                lbd.BookingLabId,
                lbd.DetailBookingLabId,
                PemeriksaanLabId = lbd.PemeriksaanLabId, 
                NamaLab = la != null ? la.NamaLab : null,
                NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                HargaPemeriksaan = (decimal?)lp.HargaPemeriksaan ?? 0m
            }
        ).ToListAsync(ct);

        var labByPasien = labRows
            .GroupBy(x => x.PasienId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ============================================================
        // RESEP bulk per kunjungan
        // ============================================================
        var resepRows = await (
            from r in _db.Reseps.AsNoTracking()
            where pageIds.Contains((Guid)r.KunjunganId) && !r.IsDelete
            join dr in _db.DetailReseps.AsNoTracking() on r.ResepId equals dr.ResepId
            where !dr.IsDelete
            join o in _db.Obats.AsNoTracking() on dr.ObatId equals o.ObatId into og
            from o in og.DefaultIfEmpty()
            join rc in _db.Racikans.AsNoTracking() on dr.RacikanId equals rc.RacikanId into rg
            from rc in rg.DefaultIfEmpty()
            select new { r.KunjunganId, r.ResepId, dr, o, rc }
        ).ToListAsync(ct);

        var resepByKunjungan = resepRows.GroupBy(x => x.KunjunganId).ToDictionary(g => g.Key, g => g.ToList());

        // racikan details bulk
        var racikanIds = resepRows
            .Where(x => x.dr != null && x.dr.IsRacikan == true && x.dr.RacikanId != null)
            .Select(x => x.dr.RacikanId!.Value)
            .Distinct()
            .ToList();

        var racikanDetails = new List<RacikanDetailRow>();
        if (racikanIds.Any())
        {
            racikanDetails = await (
                from rd in _db.RacikanDetails.AsNoTracking()
                join ob in _db.Obats.AsNoTracking() on rd.ObatId equals ob.ObatId
                where rd.RacikanId != null && racikanIds.Contains(rd.RacikanId.Value)
                select new RacikanDetailRow
                {
                    RacikanId = rd.RacikanId!.Value,
                    ObatId = rd.ObatId,
                    ObatName = ob.ObatName,
                    QtyUsed = rd.QtyUsed,
                    KomposisiDosis = rd.KomposisiDosis,
                    HTEPrice = (decimal?)ob.HTEPrice ?? 0m
                }
            ).ToListAsync(ct);
        }

        var racikanMap = racikanDetails.GroupBy(x => x.RacikanId).ToDictionary(g => g.Key, g => g.ToList());

        // ============================================================
        // TINDAKAN bulk
        // ============================================================
        var tindakanRows = await (
            from tk in _db.TindakanKunjungans.AsNoTracking()
            where pageIds.Contains(tk.KunjunganId)
            join t in _db.Tindakans.AsNoTracking()
                on tk.TindakanId equals t.TindakanId into tg
            from t in tg.DefaultIfEmpty()
            select new { tk, t }
        ).ToListAsync(ct);

        var tindakanByKunjungan = tindakanRows.GroupBy(x => x.tk.KunjunganId).ToDictionary(g => g.Key, g => g.ToList());

        // ============================
        // VISIT DOKTER BULK (untuk semua pageIds)
        // ============================
        var visitRows = await _db.VisitDokters.AsNoTracking()
            .Where(v => pageIds.Contains((Guid)v.KunjunganId)
                        && (v.IsDelete == false || v.IsDelete == null)
                        && v.DokterId != null)
            .Select(v => new
            {
                v.KunjunganId,
                v.VisitDokterId,
                DokterId = v.DokterId!.Value, // Guid (non-null)
                v.KelasId,                    // Guid?
                v.TanggalVisit,
                v.WaktuVisit,
                v.Keterangan
            })
            .ToListAsync(ct);

        var visitByKunjungan = visitRows
            .GroupBy(x => x.KunjunganId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // nama dokter bulk
        Dictionary<Guid, string?> dokterNameMap = new();
        var dokterIds = visitRows.Select(x => x.DokterId).Distinct().ToList();
        if (dokterIds.Any())
        {
            dokterNameMap = await _db.Dokters.AsNoTracking()
                .Where(d => dokterIds.Contains(d.DokterId))
                .Select(d => new { d.DokterId, d.NmDokter })
                .ToDictionaryAsync(x => x.DokterId, x => x.NmDokter, ct);
        }

        // tarif kelas bulk (kunci selalu Guid,Guid)
        //Dictionary<(Guid DokterId, Guid KelasId), decimal> tarifMap = new();

        //var kelasIds = visitRows
        //    .Where(x => x.KelasId != null)
        //    .Select(x => x.KelasId!.Value)
        //    .Distinct()
        //    .ToList();

        //if (dokterIds.Any() && kelasIds.Any())
        //{
        //    var tarifRows = await _db.TarifKelass.AsNoTracking()
        //        .Where(t => (t.IsDelete == false || t.IsDelete == null))
        //        .Where(t => t.DokterId != null && dokterIds.Contains(t.DokterId.Value))
        //        .Where(t => t.KelasId != null && kelasIds.Contains(t.KelasId.Value))
        //        .Select(t => new
        //        {
        //            DokterId = t.DokterId!.Value, // Guid
        //            KelasId = t.KelasId!.Value,   // Guid
        //            TarifDokter = (decimal?)t.TarifDokter ?? 0m,
        //            t.CreateDateTime
        //        })
        //        .ToListAsync(ct);

        //    tarifMap = tarifRows
        //        .GroupBy(x => (x.DokterId, x.KelasId))
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.OrderByDescending(x => x.CreateDateTime).First().TarifDokter
        //        );
        //}

        // ============================================================
        // KAMAR RANAP bulk
        // ============================================================
        var bookingRanaps = await _db.BookingBedRanaps.AsNoTracking()
            .Where(x => pageIds.Contains((Guid)x.KunjunganId) && (x.IsDelete == false || x.IsDelete == null))
            .ToListAsync(ct);

        var bookingByKunjungan = bookingRanaps.GroupBy(x => x.KunjunganId).ToDictionary(g => g.Key, g => g.ToList());

        var transfers = await _db.TransferPasiens.AsNoTracking()
            .Where(x => pageIds.Contains((Guid)x.KunjunganId) && (x.IsDelete == false || x.IsDelete == null))
            .ToListAsync(ct);

        var transferByKunjunganBed = transfers
            .Where(t => t.BedId != null)
            .GroupBy(t => (t.KunjunganId, BedId: t.BedId!.Value))
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.TglMasuk ?? x.TglPindah ?? DateTime.MinValue).ToList()
            );

        // ============================================================
        // BUILD OUTPUT per kunjungan (urutan sama dengan pageIds)
        // ============================================================
        var output = new List<object>(pageIds.Count);

        foreach (var kid in pageIds)
        {
            if (!headerById.TryGetValue(kid, out var h))
                continue;

            var isAsuransiCase =
                h.AsuransiId.HasValue &&
                string.Equals(h.TipePembayaran, "Asuransi", StringComparison.OrdinalIgnoreCase);

            CoverageLookup cover = new();
            if (isAsuransiCase && h.AsuransiId.HasValue && coverageByAsuransi.TryGetValue(h.AsuransiId.Value, out var c))
                cover = c;

            var dto = new BillingKunjunganDto
            {
                AsOf = snap,
                KunjunganID = h.KunjunganID,
                PasienId = h.PasienId,
                JenisKunjungan = h.JenisKunjungan,
                AsalKunjungan = h.AsalKunjungan,
                IsClosed = h.IsClosed,
                TanggalKunjungan = h.TanggalKunjungan,
                KasirId = kasirMap.TryGetValue(kid, out var kasirId) ? kasirId : null,

                NamaLengkap = h.NamaLengkap,
                NoRekamMedis = h.NoRekamMedis,
                NoHP = h.NoHp,
                NmDokter = h.NmDokter,
                NamaPoliklinik = h.NamaPoliklinik,
                TipePembayaran = h.TipePembayaran,
                NamaAsuransi = h.NamaAsuransi,
                NoPolis = h.NoPolis,
                IsPKS = h.IsPKS,
                Umur = HitungUmurLengkap(h.TanggalLahir),

                DaftarPemeriksaanLab = new List<object>(),
                DaftarObat = new List<object>(),
                DaftarRacikan = new List<object>(),
                DaftarTindakan = new List<object>(),
                DaftarBiayaAdmin = new List<object>(),
                DaftarAlkes = new List<object>(),
                DaftarVisitDokter = new List<object>(),
                DaftarKamarRanap = new List<object>(),

            };

            // DEPOSIT TERBARU
            if (depositMap.TryGetValue(kid, out var deposit))
            {
                dto.TotalSaldoDeposito = deposit.SaldoDeposit;
                dto.NominalMasuk = deposit.NominalMasuk;
                dto.NominalKeluar = deposit.NominalKeluar;
            }
            else
            {
                dto.TotalSaldoDeposito = 0m;
                dto.NominalMasuk = 0m;
                dto.NominalKeluar = 0m;
            }

            // LAB
            if (labByPasien.TryGetValue(h.PasienId, out var labsForPasien))
            {
                dto.DaftarPemeriksaanLab = labsForPasien
                    .GroupBy(x => x.DetailBookingLabId)
                    .Select(g =>
                    {
                        var x = g.First();
                        var bill = FindBilling(kid, "Pemeriksaan Lab", x.DetailBookingLabId);
                        var qty = bill?.QtyItem ?? 1;
                        var subtotal = bill?.SubTotalItem ?? x.HargaPemeriksaan;

                        //var isCovered =
                        //    isAsuransiCase &&
                        //    x.PemeriksaanLabId.HasValue &&
                        //    cover.LabMarkup.ContainsKey(x.PemeriksaanLabId.Value);

                        return (object)new
                        {
                            x.BookingLabId,
                            x.DetailBookingLabId,
                            x.NamaLab,
                            x.NamaPemeriksaan,
                            IsCovered = bill?.IsCovered,
                            IsCoveredExcess = bill?.IsCoveredExcess,
                            HargaPemeriksaan = x.HargaPemeriksaan,
                            Qty = qty,
                            Subtotal = subtotal,
                            BillingId = bill?.BillingId,
                            BillingKode = bill?.BillingKode,
                            StatusBilling = bill?.StatusBilling,
                            jenisBilling = bill?.JenisBilling,

                        };
                    })
                    .ToList();
            }

            dto.TotalPemeriksaanLab = dto.DaftarPemeriksaanLab.Sum(x => (decimal)((dynamic)x).Subtotal);

            // RESEP
            if (resepByKunjungan.TryGetValue(kid, out var resepForKunjungan))
            {
                dto.DaftarObat = resepForKunjungan
                    .Where(x => x.dr != null && x.o != null && x.dr.IsRacikan != true)
                    .GroupBy(x => x.dr.DetailResepId)
                    .Select(g =>
                    {
                        var x = g.First();
                        var bill = FindBilling(kid, "Obat", x.dr.ObatId);

                        var hte = (decimal?)x.o!.HTEPrice ?? 0m;
                        var qty = bill?.QtyItem ?? x.dr.Qty;
                        var harga = bill?.HargaItem ?? hte;
                        var subtotal = bill?.SubTotalItem ?? (x.dr.Qty * hte);

                        //var isCover =
                        //    isAsuransiCase &&
                        //    x.dr.ObatId != null &&
                        //    cover.ObatIds.Contains(x.dr.ObatId.Value);

                        return (object)new
                        {
                            x.ResepId,
                            x.dr.DetailResepId,
                            x.dr.ObatId,
                            x.o.ObatName,
                            IsCovered = bill?.IsCovered,
                            IsCoveredExcess = bill?.IsCoveredExcess,
                            Qty = qty,
                            Harga = harga,
                            Subtotal = subtotal,
                            BillingId = bill?.BillingId,
                            BillingKode = bill?.BillingKode,
                            StatusBilling = bill?.StatusBilling,
                            jenisBilling = bill?.JenisBilling,
                            x.dr.Signa,
                            x.dr.SignaTambahan,
                            x.dr.StatusPengambilanObat
                        };
                    })
                    .ToList();

                dto.TotalObat = dto.DaftarObat.Sum(x => (decimal)((dynamic)x).Subtotal);

                dto.DaftarRacikan = resepForKunjungan
                    .Where(x => x.dr != null && x.dr.IsRacikan == true && x.rc != null && x.dr.RacikanId != null)
                    .GroupBy(x => x.dr.RacikanId)
                    .Select(g =>
                    {
                        var x = g.First();
                        var bill = FindBilling(kid, "Obat", x.dr.RacikanId);

                        racikanMap.TryGetValue(x.dr.RacikanId!.Value, out var komps);

                        return (object)new
                        {
                            x.ResepId,
                            x.dr.RacikanId,
                            x.rc!.NamaRacikan,
                            x.rc.KodeRacikan,
                            Qty = bill?.QtyItem,
                            Harga = bill?.HargaItem,
                            Subtotal = bill?.SubTotalItem ?? 0m,
                            BillingId = bill?.BillingId,
                            BillingKode = bill?.BillingKode,
                            StatusBilling = bill?.StatusBilling,
                            jenisBilling = bill?.JenisBilling,
                            x.dr.Signa,
                            x.dr.SignaTambahan,
                            x.dr.StatusPengambilanObat,
                            Komposisi = komps?.Select(k => new
                            {
                                k.ObatId,
                                k.ObatName,
                                k.QtyUsed,
                                k.KomposisiDosis,
                                k.HTEPrice
                            })
                        };
                    })
                    .ToList();

                dto.TotalRacikan = dto.DaftarRacikan.Sum(x => (decimal)((dynamic)x).Subtotal);
            }

            // TINDAKAN
            if (tindakanByKunjungan.TryGetValue(kid, out var tindakanForKunjungan))
            {
                dto.DaftarTindakan = tindakanForKunjungan
                    .Where(x => x.tk != null && x.t != null)
                    .GroupBy(x => x.tk.TindakanKunjunganId)
                    .Select(g =>
                    {
                        var x = g.First();
                        var bill = FindBilling(kid, "Tindakan", x.tk.TindakanId);

                        var qty = bill?.QtyItem ?? x.tk.Quantity ?? 1;
                        var totalTindakan = (decimal?)x.tk.Total ?? 0m;
                        var harga = bill?.HargaItem ?? totalTindakan;
                        var subtotal = bill?.SubTotalItem ?? ((x.tk.Quantity ?? 1) * totalTindakan);

                        //var isCovered =
                        //    isAsuransiCase &&
                        //    cover.TindakanMarkup.ContainsKey(x.tk.TindakanId);
                        return (object)new
                        {
                            x.t!.TindakanId,
                            x.t.NamaTindakan,
                            IsCovered = bill?.IsCovered,
                            IsCoveredExcess = bill?.IsCoveredExcess,
                            Qty = qty,
                            Harga = harga,
                            Subtotal = subtotal,
                            BillingId = bill?.BillingId,
                            BillingKode = bill?.BillingKode,
                            StatusBilling = bill?.StatusBilling,
                            jenisBilling = bill?.JenisBilling,
                        };
                    })
                    .ToList();
            }

            dto.TotalTindakan = dto.DaftarTindakan.Sum(x => (decimal)((dynamic)x).Subtotal);

            // ADMIN (dari billings)
            dto.DaftarBiayaAdmin = billings
                .Where(b => b.KunjunganId == kid && string.Equals(b.JenisBilling, "Biaya Admin", StringComparison.OrdinalIgnoreCase))
                .Select(b => (object)new
                {
                    b.BillingId,
                    b.NamaItem,
                    b.HargaItem,
                    b.QtyItem,
                    b.SubTotalItem,
                    b.BillingKode,
                    b.StatusBilling,
                    b.JenisBilling,
                })
                .ToList();

            dto.TotalBiayaAdmin = dto.DaftarBiayaAdmin.Sum(x => (decimal?)((dynamic)x).SubTotalItem ?? 0m);

            // ALKES
            dto.DaftarAlkes = billings
                .Where(b => b.KunjunganId == kid && string.Equals(b.JenisBilling, "Alkes", StringComparison.OrdinalIgnoreCase))
                .Select(b =>
                {
                    var qty = b.QtyItem ?? 1;
                    var harga = b.HargaItem ?? 0m;
                    var subtotal = b.SubTotalItem ?? (qty * harga);

                    return (object)new
                    {
                        b.BillingId,
                        b.BillingKode,
                        b.ItemId,
                        b.NamaItem,
                        b.Keterangan,
                        Qty = qty,
                        Harga = harga,
                        Subtotal = subtotal,
                        b.StatusBilling,
                        b.JenisBilling
                    };
                })
                .ToList();

            dto.TotalAlkes = dto.DaftarAlkes.Sum(x => (decimal)((dynamic)x).Subtotal);

            // VISIT DOKTER
            //dto.DaftarVisitDokter = new List<object>();
            //dto.TotalBiayaVisitDokter = 0m;

            //if (visitByKunjungan.TryGetValue(kid, out var visits))
            //{
            //    // Billing Visit per kunjungan: JenisBilling = "Visit Dokter", ItemId = VisitDokterId
            //    var billingVisitMap = billings
            //        .Where(b => b.KunjunganId == kid
            //                    && b.ItemId != null
            //                    && string.Equals(b.JenisBilling, "Visit Dokter", StringComparison.OrdinalIgnoreCase))
            //        .GroupBy(b => b.ItemId!.Value)
            //        .ToDictionary(
            //            g => g.Key,
            //            g => g.OrderByDescending(x => x.CreateDateTime ?? DateTime.MinValue).First()
            //        );

            //    foreach (var grp in visits.GroupBy(x => new { x.DokterId, x.KelasId }))
            //    {
            //        var dokterId = grp.Key.DokterId; // Guid
            //        var kelasId = grp.Key.KelasId;   // Guid?

            //        dokterNameMap.TryGetValue(dokterId, out var nmDokter);

            //        decimal tarifPerVisit = 0m;
            //        if (kelasId.HasValue && tarifMap.TryGetValue((dokterId, kelasId.Value), out var tarif))
            //            tarifPerVisit = tarif;

            //        var visitDetails = grp
            //            .OrderBy(x => x.TanggalVisit ?? DateTime.MinValue)
            //            .Select(v =>
            //            {
            //                billingVisitMap.TryGetValue(v.VisitDokterId, out var bill);

            //                var qty = bill?.QtyItem ?? 1;
            //                var harga = bill?.HargaItem ?? tarifPerVisit;
            //                var subtotal = bill?.SubTotalItem ?? (qty * harga);

            //                return new
            //                {
            //                    v.VisitDokterId,
            //                    v.TanggalVisit,
            //                    v.WaktuVisit,
            //                    v.Keterangan,

            //                    Qty = qty,
            //                    Harga = harga,
            //                    Subtotal = subtotal,

            //                    BillingId = bill?.BillingId,
            //                    BillingKode = bill?.BillingKode,
            //                    StatusBilling = bill?.StatusBilling
            //                };
            //            })
            //            .ToList();

            //        var subtotalGroup = visitDetails.Sum(x => x.Subtotal);
            //        dto.TotalBiayaVisitDokter += subtotalGroup;

            //        dto.DaftarVisitDokter.Add(new
            //        {
            //            DokterId = dokterId,
            //            NmDokter = nmDokter,
            //            KelasId = kelasId,
            //            Qty = visitDetails.Count,
            //            HargaPerVisit = tarifPerVisit,
            //            Subtotal = subtotalGroup,
            //            Visits = visitDetails
            //        });
            //    }
            //}

            // KAMAR RANAP
            dto.TotalKamarRanap = 0m;
            if (IsRawatInapIP(dto.JenisKunjungan) && bookingByKunjungan.TryGetValue(kid, out var bookings))
            {
                var billingKamarMap = billings
                    .Where(b => b.KunjunganId == kid
                                && b.ItemId != null
                                && string.Equals(b.JenisBilling, "Kamar Ranap", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(b => b.ItemId!.Value)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(x => x.CreateDateTime ?? DateTime.MinValue).First()
                    );

                var transferByBed = transferByKunjunganBed
                    .Where(kv => kv.Key.KunjunganId == kid)
                    .ToDictionary(kv => kv.Key.BedId, kv => kv.Value);

                foreach (var kamarGroup in bookings.GroupBy(x => x.KamarId))
                {
                    var kamarId = kamarGroup.Key;

                    billingKamarMap.TryGetValue((Guid)kamarId, out var billKamar);
                    var tarifPerHari = billKamar?.HargaItem ?? 0m;

                    int totalHari = 0;
                    DateTime? tglMasukAwal = null;
                    DateTime? tglKeluarAkhir = null;
                    var segments = new List<object>();
                    // ✅ cover kamar + markup
                    //var isCoveredKamar =
                    //    isAsuransiCase &&
                    //    cover.KamarMarkup.TryGetValue(kamarId.Value, out var kamarMarkup);

                    //if (isCoveredKamar && kamarMarkup > 0m)
                    //    tarifPerHari = kamarMarkup; // ✅ pakai markup total kamar untuk harga asuransi

                    foreach (var bk in kamarGroup.OrderBy(x => x.TglMasuk ?? DateTime.MinValue))
                    {
                        tglMasukAwal ??= bk.TglMasuk;

                        var endTransfer = ResolveEndFromTransfer(bk.BedId, bk.TglMasuk, transferByBed);
                        DateTime endFinal = bk.TglKeluar ?? endTransfer ?? snap;
                        if (endFinal > snap) endFinal = snap;

                        var hari = HitungJumlahHariRanap(bk.TglMasuk, endFinal, snap);
                        totalHari += hari;

                        if (!tglKeluarAkhir.HasValue || endFinal > tglKeluarAkhir.Value)
                            tglKeluarAkhir = endFinal;

                        var transfersSegmen = GetTransfersForSegment(bk.BedId, bk.TglMasuk, endFinal, transferByBed);

                        segments.Add(new
                        {
                            bk.BookingBedRanapId,
                            bk.KamarId,
                            bk.BedId,
                            TglMasukBooking = bk.TglMasuk,
                            TglKeluarBooking = bk.TglKeluar,
                            TglKeluarTransfer = endTransfer,
                            TglKeluarFinal = endFinal,
                            JumlahHari = hari,
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

                    var subtotal = tarifPerHari * totalHari;
                    dto.TotalKamarRanap += subtotal;

                    dto.DaftarKamarRanap.Add(new
                    {
                        BillingId = billKamar?.BillingId,
                        BillingKode = billKamar?.BillingKode,
                        StatusBilling = billKamar?.StatusBilling,
                        KamarId = kamarId,
                        IsCovered = billKamar?.IsCovered,
                        IsCoveredExcess = billKamar?.IsCoveredExcess,
                        NamaItem = billKamar?.NamaItem,
                        HargaPerHari = tarifPerHari,
                        JumlahHari = totalHari,
                        Subtotal = subtotal,
                        TglMasukBookingAwal = tglMasukAwal,
                        TglKeluarFinalAkhir = tglKeluarAkhir,
                        Segments = segments,
                        AsOf = snap
                    });
                }
            }


            // Biaya lain2 (dari billings)
            dto.DaftarBiayaLain = billings
                .Where(b => b.KunjunganId == kid && string.Equals(b.JenisBilling, "Biaya Lain - Lain", StringComparison.OrdinalIgnoreCase))
                .Select(b => (object)new
                {
                    b.BillingId,
                    b.NamaItem,
                    b.HargaItem,
                    b.QtyItem,
                    b.SubTotalItem,
                    b.BillingKode,
                    b.StatusBilling,
                    b.JenisBilling,
                })
                .ToList();

            dto.TotalBiayaLain = dto.DaftarBiayaLain.Sum(x => (decimal?)((dynamic)x).SubTotalItem ?? 0m);

            var DepositRanap = dto.TotalSaldoDeposito;

            var asuransi =
                SumCovered(dto.DaftarPemeriksaanLab) +
                SumCovered(dto.DaftarObat) +
                SumCovered(dto.DaftarTindakan) +
                SumCovered(dto.DaftarKamarRanap);

            var asuransiExcess =
                SumCoveredExcess(dto.DaftarPemeriksaanLab) +
                SumCoveredExcess(dto.DaftarObat) +
                SumCoveredExcess(dto.DaftarTindakan) +
                SumCoveredExcess(dto.DaftarKamarRanap);

            var mandiri =
                SumUncovered(dto.DaftarPemeriksaanLab) +
                SumUncovered(dto.DaftarObat) +
                SumUncovered(dto.DaftarTindakan) +
                SumUncovered(dto.DaftarKamarRanap)
                // komponen yang memang tidak punya IsCovered → anggap mandiri
                + dto.TotalRacikan
                + dto.TotalBiayaAdmin
                + dto.TotalBiayaLain
                + dto.TotalAlkes
                + dto.TotalBiayaVisitDokter;

            var ppnRate = dto.PPN / 100m;
            dto.SubTotalAsuransi = asuransi;
            dto.SubTotalAsuransiExcess = asuransiExcess;
            dto.SebelumTaxTotalMandiri = Math.Round(mandiri, 2, MidpointRounding.AwayFromZero);
            dto.PajakTotalMandiri = Math.Round((dto.SebelumTaxTotalMandiri ?? 0m) * ppnRate, 2, MidpointRounding.AwayFromZero);
            dto.SubTotalMandiri = Math.Round((dto.SebelumTaxTotalMandiri ?? 0m) + (dto.PajakTotalMandiri ?? 0m), 2, MidpointRounding.AwayFromZero);

            output.Add(new
            {
                dto.KunjunganID,
                dto.PasienId,
                CreateDateTime = h.CreateDateTime,
                dto.JenisKunjungan,
                dto.AsalKunjungan,
                dto.IsClosed,
                dto.TanggalKunjungan,
                dto.KasirId,
                dto.NamaLengkap,
                dto.NoHP,
                dto.NoRekamMedis,
                dto.NmDokter,
                dto.NamaPoliklinik,
                dto.TipePembayaran,
                dto.NamaAsuransi,
                dto.NoPolis,
                dto.IsPKS,
                dto.Umur,
                dto.AsOf,

                dto.DaftarPemeriksaanLab,
                dto.DaftarObat,
                dto.DaftarRacikan,
                dto.DaftarTindakan,
                dto.DaftarBiayaAdmin,
                dto.DaftarAlkes,
                dto.DaftarVisitDokter,
                dto.DaftarKamarRanap,
                dto.DaftarBiayaLain,
                dto.DPRanap,
                dto.TotalSaldoDeposito,
                dto.NominalMasuk,
                dto.NominalKeluar,
                dto.TotalPemeriksaanLab,
                dto.TotalObat,
                dto.TotalRacikan,
                dto.TotalTindakan,
                dto.TotalBiayaAdmin,
                dto.TotalAlkes,
                dto.TotalBiayaVisitDokter,
                dto.TotalKamarRanap,
                dto.TotalBiayaLain,
                dto.SubTotalAsuransi,
                dto.SubTotalAsuransiExcess,
                dto.SebelumTaxTotalMandiri,
                dto.PajakTotalMandiri,
                dto.SubTotalMandiri,
                dto.PPN,
            });
        }

        return new PagedResult<object>
        {
            Page = page,
            PageSize = pageSize,
            TotalKunjungan = totalKunjungan,
            TotalPages = totalPages,
            Data = output
                .OrderByDescending(x => (DateTimeOffset)((dynamic)x).CreateDateTime)
                .ToArray()
        };
    }

    // ======================================================
    // FUNCTION GET BY ID MAIN KASIR (pembayaran dan detailnya)
    // =======================================================
    public async Task<IReadOnlyList<object>> GetMainKasirDanDetailPembayaranAsync(
        Guid kunjunganId,
        //Guid? petugasId = null,
        CancellationToken ct = default)
    {
        // =========================
        // 1) Headers (SEMUA MainKasir) + LEFT JOIN Pasien
        // =========================
        var headers = await (
            from x in _db.MainKasirs.AsNoTracking()

            join p0 in _db.PendaftaranPasienBarus.AsNoTracking()
                on x.PasienId equals p0.PendaftaranPasienBaruId into pGroup
            from p in pGroup.DefaultIfEmpty()

            join k0 in _db.Kunjungans.AsNoTracking()
                on x.KunjunganId equals k0.KunjunganID into kGroup
            from k in kGroup.DefaultIfEmpty()

            join a0 in _db.Asuransis.AsNoTracking()
                on k.AsuransiId equals a0.AsuransiId into aGroup
            from a in aGroup.DefaultIfEmpty()

            where x.KunjunganId == kunjunganId && x.IsDelete != true
            orderby x.CreateDateTime descending
            select new
            {
                x.KasirId,
                x.KunjunganId,
                x.PasienId,

                NamaLengkap = p != null ? p.NamaLengkap : null,
                NoRekamMedis = p != null ? p.NoRekamMedis : null,

                AsuransiId = (Guid?)k.AsuransiId,
                NamaAsuransi = a != null ? a.NamaAsuransi : null,

                x.InvoiceBilling,
                x.JumlahAngsuran,
                x.StatusPembayaran,
                x.IsVerified,
                x.TTDUserVerfiedId,
                x.PathUserVerified,
                x.JumlahPajak,
                x.Deposito,
                x.SisaDeposito,
                x.SubTotalAsuransi,
                x.SubTotalMandiri,
                x.SubTotalAsuransiExcess,
                x.HargaDiskon,
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
        ).ToListAsync(ct);

        if (headers.Count == 0)
            return Array.Empty<object>();

        var kasirIds = headers.Select(h => h.KasirId).ToList();

        // =========================
        // 2) Details (ONE query) + filter petugasId
        // =========================
        var detailQuery = _db.MainKasirDetails
            .AsNoTracking()
            .Where(d => d.MainKasirId != null
                        && kasirIds.Contains(d.MainKasirId.Value)
                        && d.IsDelete != true);

        //if (petugasId.HasValue && petugasId.Value != Guid.Empty)
        //{
        //    detailQuery = detailQuery.Where(d => d.CreateBy == petugasId.Value);
        //}

        var tmpDetails = await detailQuery
            .OrderBy(d => d.TglPembayaran ?? DateTime.MaxValue)
            .ThenBy(d => d.CreateDateTime)
            .Select(d => new
            {
                d.MainKasirDetailId,
                MainKasirId = d.MainKasirId!.Value,

                d.MetodePembayaranId,
                d.ReferenceId,
                d.KunjunganId,
                d.PasienId,
                TotalPembayaran = (decimal?)d.TotalPembayaran,
                SisaPembayaran = (decimal?)d.SisaPembayaran,
                d.NoKwitansi,
                AngsuranKe = (decimal?)d.AngsuranKe,
                d.NamaMetode,
                NominalPembayaran = (decimal?)d.NominalPembayaran,
                d.Keterangan,
                d.TglPembayaran,

                d.CreateDateTime,
                CreateBy = (Guid?)d.CreateBy,
                d.UpdateDateTime,
                UpdateBy = (Guid?)d.UpdateBy
            })
            .ToListAsync(ct);

        var detailLookup = tmpDetails.ToLookup(d => d.MainKasirId);

        var paymentSummaryByKasirId = tmpDetails
            .GroupBy(d => d.MainKasirId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var latestDetail = g
                        .OrderByDescending(x => x.TglPembayaran ?? DateTime.MinValue)
                        .ThenByDescending(x => x.CreateDateTime)
                        .First();

                    return new
                    {
                        LatestAngsuran = latestDetail.AngsuranKe ?? 0m,
                        TotalSudahDibayar = g.Sum(x => x.NominalPembayaran ?? 0m)
                    };
                });

        decimal GetLatestAngsuran(Guid kasirId)
            => paymentSummaryByKasirId.TryGetValue(kasirId, out var d)
                ? d.LatestAngsuran
                : 0m;

        decimal GetSisaPembayaran(Guid kasirId, decimal? nominalTagihan)
        {
            var totalTagihan = nominalTagihan ?? 0m;

            var totalSudahDibayar = paymentSummaryByKasirId.TryGetValue(kasirId, out var d)
                ? d.TotalSudahDibayar
                : 0m;

            var sisa = totalTagihan - totalSudahDibayar;

            return sisa <= 0 ? 0m : sisa;
        }

        // =========================
        // 3) Load nama user sekali
        // =========================
        var userIds = new HashSet<Guid>();

        foreach (var h in headers)
        {
            if (h.CreateBy.HasValue) userIds.Add(h.CreateBy.Value);
            if (h.UpdateBy.HasValue) userIds.Add(h.UpdateBy.Value);
            if (h.TTDUserVerfiedId.HasValue) userIds.Add(h.TTDUserVerfiedId.Value);
        }

        foreach (var d in tmpDetails)
        {
            if (d.CreateBy.HasValue) userIds.Add(d.CreateBy.Value);
            if (d.UpdateBy.HasValue) userIds.Add(d.UpdateBy.Value);
        }

        var userDict = userIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.UserActives
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserActiveId))
                .Select(u => new { u.UserActiveId, u.FullName })
                .ToDictionaryAsync(x => x.UserActiveId, x => x.FullName, ct);

        string? GetUserName(Guid? userId)
            => userId.HasValue && userDict.TryGetValue(userId.Value, out var name) ? name : null;

        // =========================
        // 4) Compose response
        // =========================
        var kasirs = headers.Select(h =>
        {
            var totalTagihan = h.GrandTotalPembayaran ?? 0m;
            var jumlahAngsuranHitung = GetLatestAngsuran(h.KasirId);
            var sisaPembayaranHitung = GetSisaPembayaran(h.KasirId, totalTagihan);

            return (object)new
            {
                Header = new
                {
                    h.KasirId,
                    h.KunjunganId,
                    h.PasienId,
                    h.NamaLengkap,
                    h.NoRekamMedis,
                    h.AsuransiId,
                    h.NamaAsuransi,
                    h.InvoiceBilling,
                    JumlahAngsuran = jumlahAngsuranHitung,
                    SisaPembayaran = sisaPembayaranHitung,
                    h.StatusPembayaran,
                    h.IsVerified,
                    h.TTDUserVerfiedId,
                    VerifiedByName = GetUserName(h.TTDUserVerfiedId),
                    h.PathUserVerified,
                    h.JumlahPajak,
                    h.Deposito,
                    h.SisaDeposito,
                    h.SubTotalAsuransi,
                    h.SubTotalAsuransiExcess,
                    h.SubTotalMandiri,
                    h.TotalPembayaran,
                    h.GrandTotalPembayaran,
                    h.TotalBiayaObat,
                    h.TotalBiayaTindakan,
                    h.HargaDiskon,
                    h.Keterangan,
                    h.TglPembayaran,
                    h.DiskonId,
                    h.CreateDateTime,
                    h.CreateBy,
                    CreateByName = GetUserName(h.CreateBy),
                    h.UpdateDateTime,
                    h.UpdateBy,
                    UpdateByName = GetUserName(h.UpdateBy),
                },

                Details = detailLookup[h.KasirId]
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
                        d.CreateBy,
                        CreateByName = GetUserName(d.CreateBy),
                        d.UpdateDateTime,
                        d.UpdateBy,
                        UpdateByName = GetUserName(d.UpdateBy),
                    })
                    .ToList()
            };
        }).ToList();

        return kasirs;
    }

    // =====================
    // GET BILLING BY NO RM
    // =====================
    public async Task<IReadOnlyList<object>>
        GetRiwayatBillingPasienByNoRmFastAsync(
            string noRekamMedis,
            DateTime? asOf = null,
            CancellationToken ct = default)
    {
        var snap = asOf ?? DateTime.Now;

        // =========================
        // 1️⃣ Ambil semua kunjungan pasien (DESC)
        // =========================
        var kunjungans = await (
            from p in _db.PendaftaranPasienBarus.AsNoTracking()
            join k in _db.Kunjungans.AsNoTracking()
                on p.PendaftaranPasienBaruId equals k.PasienId
                into kj
            from k in kj.DefaultIfEmpty()
            where p.NoRekamMedis == noRekamMedis
                  && (k == null || !k.IsDelete)
            orderby k.CreateDateTime descending
            select new
            {
                KunjunganID = k != null ? k.KunjunganID : (Guid?)null,
                CreateDateTime = k.CreateDateTime,
                PasienId = p.PendaftaranPasienBaruId
            }
        ).ToListAsync(ct);

        if (kunjungans.Count == 0)
            return Array.Empty<object>();

        var kunjunganIds = kunjungans.Select(x => x.KunjunganID).ToList();

        // =========================
        // 2️⃣ Ambil semua Billing sekaligus
        // =========================
        var billings = await _db.Billings
            .AsNoTracking()
            .Where(b => kunjunganIds.Contains((Guid)b.KunjunganId)
                        && b.StatusBilling != true
                        && (b.IsDelete == false || b.IsDelete == null))
            .ToListAsync(ct);

        var billingLookup = billings
            .GroupBy(b => b.KunjunganId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // =========================
        // 3️⃣ Ambil semua MainKasir sekaligus
        // =========================
        var mainKasirs = await _db.MainKasirs
            .AsNoTracking()
            .Where(x => kunjunganIds.Contains((Guid)x.KunjunganId)
                        && x.IsDelete != true)
            .ToListAsync(ct);

        var kasirLookup = mainKasirs
            .GroupBy(x => x.KunjunganId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var kasirIds = mainKasirs.Select(x => x.KasirId).ToList();

        // =========================
        // 4️⃣ Ambil semua Detail Kasir sekaligus
        // =========================
        var kasirDetails = await _db.MainKasirDetails
            .AsNoTracking()
            .Where(d => d.MainKasirId != null
                        && kasirIds.Contains(d.MainKasirId.Value)
                        && d.IsDelete != true)
            .ToListAsync(ct);

        var detailLookup = kasirDetails
            .GroupBy(d => d.MainKasirId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // =========================
        // 5️⃣ Compose Result
        // =========================
        var result = new List<object>();

        foreach (var k in kunjungans)
        {
            billingLookup.TryGetValue(k.KunjunganID, out var billingPerKunjungan);
            kasirLookup.TryGetValue(k.KunjunganID, out var kasirPerKunjungan);

            var kasirResult = new List<object>();

            if (kasirPerKunjungan != null)
            {
                foreach (var kasir in kasirPerKunjungan)
                {
                    detailLookup.TryGetValue(kasir.KasirId, out var details);

                    kasirResult.Add(new
                    {
                        Header = kasir,
                        Details = details ?? new List<MainKasirDetail>()
                    });
                }
            }

            result.Add(new
            {
                KunjunganId = k.KunjunganID,
                TanggalKunjungan = k.CreateDateTime,
                Billings = billingPerKunjungan ?? new List<Billing>(),
                Kasir = kasirResult
            });
        }

        return result;
    }

    // =======================
    // Pendapatan Kasir Harian
    // =======================
    public async Task<PendapatanKasirHarianDto> GetPendapatanKasirHarianAsync(
    Guid kasirUserId,
    DateTime? tanggal = null,
    CancellationToken ct = default)
    {
        var day = (tanggal ?? DateTime.Now).Date;

        // pakai offset server saat ini (biasanya +07:00 kalau server di WIB)
        var offset = DateTimeOffset.Now.Offset;
        var start = new DateTimeOffset(day, offset);
        var end = start.AddDays(1);

        // get nama petugas kasir
        var namaKasir = await _db.UserActives.AsNoTracking()
            .Where(u => u.UserActiveId == kasirUserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct);

        // 1) Pendapatan Tunai & Non-Tunai dari MainKasirDetail (berdasarkan CreateBy kasir & tanggal)
        // NOTE: kalau kamu mau pakai TglPembayaran sebagai acuan, ganti d.CreateDateTime -> d.TglPembayaran (jika tipe sama & nullable)
        var payAgg = await _db.MainKasirDetails.AsNoTracking()
            .Where(d => d.CreateBy == kasirUserId && d.IsDelete != true)
            .Where(d => d.CreateDateTime >= start && d.CreateDateTime < end)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Tunai = g.Sum(d =>
                    (d.NamaMetode != null && EF.Functions.ILike(d.NamaMetode, "Tunai"))
                        ? (d.NominalPembayaran ?? 0m)
                        : 0m),

                NonTunai = g.Sum(d =>
                    (d.NamaMetode != null && !EF.Functions.ILike(d.NamaMetode, "Tunai"))
                        ? (d.NominalPembayaran ?? 0m)
                        : 0m)
            })
            .FirstOrDefaultAsync(ct);

        var pendapatanTunai = payAgg?.Tunai ?? 0m;
        var pendapatanNonTunai = payAgg?.NonTunai ?? 0m;

        // 2) Piutang Asuransi dari MainKasir header (SubTotalAsuransi)
        // filter by CreateBy kasir & tanggal (shift harian)
        var piutangAsuransi = await _db.MainKasirs.AsNoTracking()
            .Where(h => h.CreateBy == kasirUserId && h.IsDelete != true)
            .Where(h => h.CreateDateTime >= start && h.CreateDateTime < end)
            .SumAsync(h => (decimal?)(h.SubTotalAsuransi ?? 0m), ct) ?? 0m;

        return new PendapatanKasirHarianDto
        {
            KasirUserId = kasirUserId,
            PetugasKasir = namaKasir,
            Tanggal = day,

            PendapatanTunai = pendapatanTunai,
            PendapatanNonTunai = pendapatanNonTunai,
            PiutangAsuransi = piutangAsuransi,

            TotalPendapatan = pendapatanTunai + pendapatanNonTunai + piutangAsuransi,
        };
    }


    // =============================
    // Pendapatan Kasir Harian Paged
    // =============================
    public async Task<PagedRekapResult<PendapatanKasirHarianDto>> GetPendapatanHarianPagedAsync(
    PendapatanHarianPagedQuery q,
    CancellationToken ct = default)
    {
        var page = q.Page <= 0 ? 1 : q.Page;
        var pageSize = q.PageSize <= 0 ? 10 : q.PageSize;
        if (pageSize > 100) pageSize = 100;

        // default: hari ini
        var startDay = (q.StartDate ?? DateTime.Now).Date;
        var endDay = (q.EndDate ?? startDay).Date;

        // sargable range: >= start && < endExclusive
        var offset = DateTimeOffset.Now.Offset;
        var start = new DateTimeOffset(startDay, offset);
        var endExclusive = new DateTimeOffset(endDay.AddDays(1), offset);

        // ==========================
        // 1) AGG MainKasirDetail: Tunai & NonTunai per hari
        // ==========================
        var detailAgg = await _db.MainKasirDetails.AsNoTracking()
            .Where(d => d.IsDelete != true)
            .Where(d => d.CreateDateTime >= start && d.CreateDateTime < endExclusive)
            .GroupBy(d => d.CreateDateTime.Date)
            .Select(g => new
            {
                Tanggal = g.Key,
                Tunai = g.Sum(d =>
                    (d.NamaMetode != null && EF.Functions.ILike(d.NamaMetode, "Tunai"))
                        ? (d.NominalPembayaran ?? 0m)
                        : 0m),
                NonTunai = g.Sum(d =>
                    (d.NamaMetode != null && !EF.Functions.ILike(d.NamaMetode, "Tunai"))
                        ? (d.NominalPembayaran ?? 0m)
                        : 0m)
            })
            .ToListAsync(ct);

        // ==========================
        // 2) AGG MainKasir: Piutang Asuransi per hari
        // ==========================
        var asuransiAgg = await _db.MainKasirs.AsNoTracking()
            .Where(h => h.IsDelete != true)
            .Where(h => h.CreateDateTime >= start && h.CreateDateTime < endExclusive)
            .GroupBy(h => h.CreateDateTime.Date)
            .Select(g => new
            {
                Tanggal = g.Key,
                Piutang = g.Sum(x => (decimal?)(x.SubTotalAsuransi ?? 0m)) ?? 0m
            })
            .ToListAsync(ct);

        // ==========================
        // 3) MERGE per hari + paging
        // ==========================
        var detailMap = detailAgg.ToDictionary(x => x.Tanggal, x => x);
        var asuMap = asuransiAgg.ToDictionary(x => x.Tanggal, x => x.Piutang);

        var allDays = detailMap.Keys
            .Union(asuMap.Keys)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var totalRows = allDays.Count;
        var totalPages = (int)Math.Ceiling(totalRows / (double)pageSize);

        var pagedDays = allDays
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var data = pagedDays.Select(day =>
        {
            detailMap.TryGetValue(day, out var d);
            asuMap.TryGetValue(day, out var piutang);

            var tunai = d?.Tunai ?? 0m;
            var nonTunai = d?.NonTunai ?? 0m;
            var asuransi = piutang;

            return new PendapatanKasirHarianDto
            {
                Tanggal = day,
                PendapatanTunai = tunai,
                PendapatanNonTunai = nonTunai,
                PiutangAsuransi = asuransi,
                TotalPendapatan = tunai + nonTunai + asuransi
            };
        }).ToList();

        return new PagedRekapResult<PendapatanKasirHarianDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalRows = totalRows,
            TotalPages = totalPages,
            Data = data.ToArray()
        };
    }

    #region HELPERS

    #region Hitung Biaya Ranap
    private static bool IsRawatInapIP(string? jenisKunjungan)
    {
        var j = (jenisKunjungan ?? "").Trim().ToUpperInvariant();
        return j == "IP" || j == "RAWAT INAP" || j == "INAP";
    }

    private static int HitungJumlahHariRanap(DateTime? tglMasuk, DateTime? tglKeluarFinal, DateTime asOf)
    {
        if (!tglMasuk.HasValue) return 1;

        var start = tglMasuk.Value;
        var end = tglKeluarFinal ?? asOf;
        if (end > asOf) end = asOf;
        if (end < start) end = start;

        var durasi = end - start;
        var hari = (int)Math.Ceiling(durasi.TotalDays);
        if (hari < 1) hari = 1;
        return hari;
    }

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

    private static List<TransferPasien> GetTransfersForSegment(
        Guid? bedId,
        DateTime? segStart,
        DateTime? segEnd,
        Dictionary<Guid, List<TransferPasien>> transferByBed)
    {
        if (!bedId.HasValue) return new List<TransferPasien>();
        if (!transferByBed.TryGetValue(bedId.Value, out var list) || list.Count == 0) return new List<TransferPasien>();

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
    #endregion

    #region Hitung Umur
    private static string HitungUmurLengkap(DateTime? tanggalLahir)
    {
        if (!tanggalLahir.HasValue) return "-";

        var today = DateTime.Today;
        var dob = tanggalLahir.Value.Date;

        int years = today.Year - dob.Year;
        int months = today.Month - dob.Month;
        int days = today.Day - dob.Day;

        if (days < 0)
        {
            var prevMonth = today.AddMonths(-1);
            days += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            months--;
        }

        if (months < 0)
        {
            months += 12;
            years--;
        }

        return $"{years} tahun {months} bulan {days} hari";
    }
    #endregion

    #region Periode Filter Paged
    private static IQueryable<Kunjungan> ApplyPeriodeFilter(
        IQueryable<Kunjungan> baseQuery,
        PeriodeFilter periode)
    {
        var today = DateTime.UtcNow.Date;

        DateTimeOffset rangeStartUtc;
        DateTimeOffset rangeEndUtc;

        switch (periode)
        {
            case PeriodeFilter.Today:
                rangeStartUtc = new DateTimeOffset(DateTime.SpecifyKind(today, DateTimeKind.Utc));
                rangeEndUtc = new DateTimeOffset(DateTime.SpecifyKind(today.AddDays(1).AddTicks(-1), DateTimeKind.Utc));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc && k.CreateDateTime <= rangeEndUtc);

            case PeriodeFilter.Yesterday:
                var y = today.AddDays(-1);
                rangeStartUtc = new DateTimeOffset(DateTime.SpecifyKind(y, DateTimeKind.Utc));
                rangeEndUtc = new DateTimeOffset(DateTime.SpecifyKind(y.AddDays(1).AddTicks(-1), DateTimeKind.Utc));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc && k.CreateDateTime <= rangeEndUtc);

            case PeriodeFilter.ThisWeek:
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                rangeStartUtc = new DateTimeOffset(DateTime.SpecifyKind(weekStart, DateTimeKind.Utc));
                rangeEndUtc = new DateTimeOffset(DateTime.SpecifyKind(today.AddDays(1).AddTicks(-1), DateTimeKind.Utc));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc && k.CreateDateTime <= rangeEndUtc);

            case PeriodeFilter.LastWeek:
                var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                var lastWeekEnd = lastWeekStart.AddDays(6);
                rangeStartUtc = new DateTimeOffset(DateTime.SpecifyKind(lastWeekStart, DateTimeKind.Utc));
                rangeEndUtc = new DateTimeOffset(DateTime.SpecifyKind(lastWeekEnd.AddDays(1).AddTicks(-1), DateTimeKind.Utc));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc && k.CreateDateTime <= rangeEndUtc);

            case PeriodeFilter.ThisMonth:
                var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeStartUtc = new DateTimeOffset(thisMonthStart);
                rangeEndUtc = new DateTimeOffset(thisMonthStart.AddMonths(1).AddTicks(-1));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc && k.CreateDateTime <= rangeEndUtc);

            case PeriodeFilter.LastMonth:
                var lm = today.AddMonths(-1);
                var lastMonthStart = new DateTime(lm.Year, lm.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeStartUtc = new DateTimeOffset(lastMonthStart);
                rangeEndUtc = new DateTimeOffset(lastMonthStart.AddMonths(1).AddTicks(-1));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc && k.CreateDateTime <= rangeEndUtc);

            case PeriodeFilter.ThisYear:
                var thisYearStart = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeStartUtc = new DateTimeOffset(thisYearStart);
                rangeEndUtc = new DateTimeOffset(thisYearStart.AddYears(1).AddTicks(-1));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc && k.CreateDateTime <= rangeEndUtc);

            case PeriodeFilter.LastYear:
                var lastYearStart = new DateTime(today.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeStartUtc = new DateTimeOffset(lastYearStart);
                rangeEndUtc = new DateTimeOffset(lastYearStart.AddYears(1).AddTicks(-1));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc && k.CreateDateTime <= rangeEndUtc);

            case PeriodeFilter.Last3Months:
                rangeStartUtc = new DateTimeOffset(DateTime.SpecifyKind(today.AddMonths(-3), DateTimeKind.Utc));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc);

            case PeriodeFilter.Last6Months:
                rangeStartUtc = new DateTimeOffset(DateTime.SpecifyKind(today.AddMonths(-6), DateTimeKind.Utc));
                return baseQuery.Where(k => k.CreateDateTime >= rangeStartUtc);

            default:
                return baseQuery;
        }
    }
    #endregion

    #region Hitung DPD
    private static int HitungDpd(DateTimeOffset? jatuhTempo, DateTimeOffset asOf)
    {
        if (!jatuhTempo.HasValue) return 0;

        var diff = (asOf.Date - jatuhTempo.Value.Date).Days;
        return diff > 0 ? diff : 0;
    }
    #endregion

    #region Hitung Sisa Pembayaran + Get Angsuran
    private static decimal? GetSisaPembayaranFromLatest(object? latestDetail)
    {
        if (latestDetail == null) return null;

        dynamic d = latestDetail;
        return (decimal?)d.SisaPembayaran;
    }

    private static decimal? GetAngsuranKeFromLatest(object? latestDetail)
    {
        if (latestDetail == null) return null;

        dynamic d = latestDetail;
        return (decimal?)d.AngsuranKe;
    }
    #endregion

    #region List Covered Asuransi
    private async Task<CoverageLookup> LoadCoverageLookupAsync(
    Guid asuransiId,
    DateTime snap,
    CancellationToken ct)
    {
        // OBAT (cukup membership)
        var obatIds = (await _db.ObatAsuransis.AsNoTracking()
                .Where(x => x.AsuransiId == asuransiId && (x.IsDelete == false || x.IsDelete == null))
                .Select(x => x.ObatId)
                .ToListAsync(ct))
            .ToHashSet();

        //// KAMAR (ambil MarkupTotal, kalau ada banyak versi ambil yang terbaru)
        var kamarRows = await _db.KamarAsuransis.AsNoTracking()
            .Where(x => x.AsuransiId == asuransiId && (x.IsDelete == false || x.IsDelete == null))
            .Where(x => x.KamarId != null)
            .Select(x => new
            {
                Id = x.KamarId!.Value,                         // ✅ jadi Guid
                MarkupTotal = (decimal?)x.MarkupTotal ?? 0m,    // ✅ pastikan kolom ada
                x.CreateDateTime
            })
            .ToListAsync(ct);

        var kamarMarkup = kamarRows
            .GroupBy(x => x.Id)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(r => r.CreateDateTime).First().MarkupTotal
            );

        // LAB (MstPemeriksaanAsuransi): key PemeriksaanLabId
        var labRows = await _db.PemeriksaanLabAsuransis.AsNoTracking()
               .Where(x => x.AsuransiId == asuransiId && (x.IsDelete == false || x.IsDelete == null))
               .Where(x => x.PemeriksaanLabId != null)
               .Select(x => new
               {
                   Id = x.PemeriksaanLabId!.Value,
                   MarkupTotal = (decimal?)x.MarkupTotal ?? 0m,
                   x.CreateDateTime
               })
               .ToListAsync(ct);

        var labMarkup = labRows
            .GroupBy(x => x.Id)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(r => r.CreateDateTime).First().MarkupTotal
            );

        //  TINDAKAN: ambil markup terbaru per TindakanId (hindari key Guid?)
        var tindakanRows = await _db.TindakanAsuransis.AsNoTracking()
            .Where(x => x.AsuransiId == asuransiId && (x.IsDelete == false || x.IsDelete == null))
            .Where(x => x.TindakanId != null)
            .Select(x => new
            {
                Id = x.TindakanId,
                MarkupTotal = (decimal?)x.MarkupTotal ?? 0m,
                x.CreateDateTime
            })
            .ToListAsync(ct);

        var tindakanMarkup = tindakanRows
            .GroupBy(x => x.Id)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(r => r.CreateDateTime).First().MarkupTotal
            );

        return new CoverageLookup
        {
            ObatIds = obatIds,
            KamarMarkup = kamarMarkup,
            LabMarkup = labMarkup,
            TindakanMarkup = tindakanMarkup
        };
    }

    #endregion

    #region Hitung Sub total mandiri + asuransi + excess
    private static bool GetBoolProp(object o, string propName)
    {
        var p = o.GetType().GetProperty(propName);
        if (p == null) return false;
        var v = p.GetValue(o);
        return v is bool b && b;
    }

    private static decimal GetDecimalProp(object o, string propName)
    {
        var p = o.GetType().GetProperty(propName);
        if (p == null) return 0m;
        var v = p.GetValue(o);
        if (v == null) return 0m;
        return Convert.ToDecimal(v);
    }

    private static decimal SumCovered(IEnumerable<object>? rows)
    {
        if (rows == null) return 0m;
        decimal sum = 0m;
        foreach (var r in rows)
            if (GetBoolProp(r, "IsCovered"))
                sum += GetDecimalProp(r, "Subtotal");
        return sum;
    }

    private static decimal SumUncovered(IEnumerable<object>? rows)
    {
        if (rows == null) return 0m;
        decimal sum = 0m;
        foreach (var r in rows)
            if (!GetBoolProp(r, "IsCovered")) // kalau prop tidak ada => false => masuk mandiri
                sum += GetDecimalProp(r, "Subtotal");
        return sum;
    }

    private static decimal SumCoveredExcess(IEnumerable<object>? rows)
    {
        if (rows == null) return 0m;
        decimal sum = 0m;
        foreach (var r in rows)
            if (GetBoolProp(r, "IsCoveredExcess"))
                sum += GetDecimalProp(r, "Subtotal");
        return sum;
    }
    #endregion

    #region Get Latest Saldo Deposit Ranap
    public static async Task<(decimal SaldoDeposit, decimal NominalMasuk, decimal NominalKeluar)> GetLastSaldoByKunjunganIdAsync(
        ApplicationDbContext dbContext,
        Guid kunjunganId,
        CancellationToken cancellationToken = default)
    {
        var lastData = await dbContext.DepositRanaps
            .AsNoTracking()
            .Where(x => x.KunjunganId == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
            .OrderByDescending(x => x.TglTransaksi)
            .ThenByDescending(x => x.CreateDateTime)
            .Select(x => new
            {
                SaldoDeposit = (decimal?)x.SaldoDeposit,
                NominalMasuk = (decimal?)x.NominalMasuk,
                NominalKeluar = (decimal?)x.NominalKeluar
            })
            .FirstOrDefaultAsync(cancellationToken);

        return (
            SaldoDeposit: lastData?.SaldoDeposit ?? 0m,
            NominalMasuk: lastData?.NominalMasuk ?? 0m,
            NominalKeluar: lastData?.NominalKeluar ?? 0m
        );
    }

    public static async Task<Dictionary<Guid, (decimal SaldoDeposit, decimal NominalMasuk, decimal NominalKeluar)>> GetLatestSaldoByKunjunganIdsAsync(
        ApplicationDbContext dbContext,
        IEnumerable<Guid> kunjunganIds,
        CancellationToken cancellationToken = default)
    {
        var ids = kunjunganIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!ids.Any())
        {
            return new Dictionary<Guid, (decimal SaldoDeposit, decimal NominalMasuk, decimal NominalKeluar)>();
        }

        var latestRows = await dbContext.DepositRanaps
            .AsNoTracking()
            .Where(x => x.KunjunganId.HasValue && ids.Contains(x.KunjunganId.Value))
            .Where(x => x.IsDelete == false || x.IsDelete == null)
            .GroupBy(x => x.KunjunganId)
            .Select(g => g
                .OrderByDescending(x => x.TglTransaksi)
                .ThenByDescending(x => x.CreateDateTime)
                .ThenByDescending(x => x.DepositRanapId)
                .Select(x => new
                {
                    x.KunjunganId,
                    SaldoDeposit = x.SaldoDeposit ?? 0m,
                    NominalMasuk = x.NominalMasuk ?? 0m,
                    NominalKeluar = x.NominalKeluar ?? 0m
                })
                .FirstOrDefault())
            .ToListAsync(cancellationToken);

        return latestRows
            .Where(x => x != null && x.KunjunganId.HasValue)
            .ToDictionary(
                x => x!.KunjunganId!.Value,
                x => (
                    SaldoDeposit: x!.SaldoDeposit,
                    NominalMasuk: x!.NominalMasuk,
                    NominalKeluar: x!.NominalKeluar
                )
            );
    }

    #endregion


    #endregion




}


