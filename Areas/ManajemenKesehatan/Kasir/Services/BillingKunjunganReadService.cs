using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Repositories;

public sealed class BillingKunjunganReadService : IBillingKunjunganReadService
{
    private readonly ApplicationDbContext _db;

    public BillingKunjunganReadService(ApplicationDbContext db)
    {
        _db = db;
    }

    // ================================
    // LITE BILLING FOR LOOKUP
    // ================================
    private sealed class BillingLite
    {
        public Guid BillingId { get; set; }
        public string? BillingKode { get; set; }
        public Guid? ItemId { get; set; }
        public string? JenisBilling { get; set; }
        public string? NamaItem { get; set; }
        public string? Keterangan { get; set; }

        public int? QtyItem { get; set; }
        public decimal? HargaItem { get; set; }
        public decimal? SubTotalItem { get; set; }

        public bool? StatusBilling { get; set; }
        public DateTimeOffset? CreateDateTime { get; set; }
    }

    private sealed class RacikanDetailRow
    {
        public Guid RacikanId { get; set; }
        public Guid? ObatId { get; set; }
        public string? ObatName { get; set; }
        public int? QtyUsed { get; set; }
        public decimal? KomposisiDosis { get; set; }
        public decimal HTEPrice { get; set; }
    }

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
            join p in _db.PendaftaranPasienBarus.AsNoTracking()
                on k.PasienId equals p.PendaftaranPasienBaruId
            join d in _db.Dokters.AsNoTracking()
                on k.DokterId equals d.DokterId
            join poli in _db.Polikliniks.AsNoTracking()
                on k.PoliklinikId equals poli.PoliklinikId
            join a in _db.Asuransis.AsNoTracking()
                on k.AsuransiId equals a.AsuransiId into ag
            from a in ag.DefaultIfEmpty()
            where k.KunjunganID == kunjunganId && !k.IsDelete
            select new
            {
                k.KunjunganID,
                k.JenisKunjungan,
                TanggalKunjungan = k.TglMasuk,
                k.TipePembayaran,
                k.PasienId,

                // ✅ supaya bisa cek cover obat berdasarkan kunjungan
                k.AsuransiId,

                p.NamaLengkap,
                p.NoRekamMedis,
                p.TanggalLahir,

                d.NmDokter,
                poli.NamaPoliklinik,
                NamaAsuransi = a != null ? a.NamaAsuransi : null
            }
        ).FirstOrDefaultAsync(ct);

        if (header == null) return null;

        var dto = new BillingKunjunganDto
        {
            AsOf = snap,
            KunjunganID = header.KunjunganID,
            JenisKunjungan = header.JenisKunjungan,
            TanggalKunjungan = header.TanggalKunjungan,

            NamaLengkap = header.NamaLengkap,
            NoRekamMedis = header.NoRekamMedis,
            NmDokter = header.NmDokter,
            NamaPoliklinik = header.NamaPoliklinik,
            TipePembayaran = header.TipePembayaran,
            NamaAsuransi = header.NamaAsuransi,
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

        HashSet<Guid> coveredObatIds = new();
        if (isAsuransiCase)
        {
            var coveredList = await _db.ObatAsuransis.AsNoTracking()
                .Where(x => x.AsuransiId == asuransiIdEfektif!.Value && (x.IsDelete == false || x.IsDelete == null))
                .Select(x => x.ObatId)
                .ToListAsync(ct);

            coveredObatIds = coveredList.ToHashSet();

        }

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
                Keterangan = b.Keterangan,
                QtyItem = b.QtyItem,
                HargaItem = b.HargaItem,
                SubTotalItem = b.SubTotalItem,
                StatusBilling = b.StatusBilling,
                CreateDateTime = b.CreateDateTime
            })
            .ToListAsync(ct);

        var billingMap = billings
            .Where(b => b.ItemId.HasValue && !string.IsNullOrWhiteSpace(b.JenisBilling))
            .GroupBy(b => new { Jenis = b.JenisBilling!, Item = b.ItemId!.Value })
            .ToDictionary(
                g => (g.Key.Jenis, g.Key.Item),
                g => g.OrderByDescending(x => x.CreateDateTime ?? DateTime.MinValue).First()
            );

        BillingLite? FindBilling(string jenis, Guid? itemId)
        {
            if (!itemId.HasValue) return null;
            billingMap.TryGetValue((jenis, itemId.Value), out var b);
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
                lbd.DetailBookingLabId,
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
                var qty = bill?.QtyItem ?? 1;
                var subtotal = bill?.SubTotalItem ?? x.HargaPemeriksaan;

                return (object)new
                {
                    x.DetailBookingLabId,
                    x.NamaLab,
                    x.NamaPemeriksaan,
                    HargaPemeriksaan = x.HargaPemeriksaan,
                    Qty = qty,
                    Subtotal = subtotal,
                    BillingId = bill?.BillingId,
                    BillingKode = bill?.BillingKode,
                    StatusBilling = bill?.StatusBilling
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

                var isCoverAsuransi =
                    isAsuransiCase &&
                    x.dr.ObatId != null &&
                    coveredObatIds.Contains(x.dr.ObatId.Value);

                return (object)new
                {
                    x.ResepId,
                    x.dr.DetailResepId,
                    x.dr.ObatId,
                    x.o.ObatName,

                    // ✅ tambahan
                    IsCoverAsuransi = isCoverAsuransi,

                    Qty = qty,
                    Harga = harga,
                    Subtotal = subtotal,
                    BillingId = bill?.BillingId,
                    BillingKode = bill?.BillingKode,
                    StatusBilling = bill?.StatusBilling,
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
                    StatusBilling = bill?.StatusBilling,
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
        var tindakanRows = await (
            from tk in _db.TindakanKunjungans.AsNoTracking()
            where tk.KunjunganId == kunjunganId
            join t in _db.Tindakans.AsNoTracking()
                on tk.TindakanId equals t.TindakanId into tg
            from t in tg.DefaultIfEmpty()
            select new { tk, t }
        ).ToListAsync(ct);

        dto.DaftarTindakan = tindakanRows
            .Where(x => x.tk != null && x.t != null)
            .GroupBy(x => x.tk.TindakanKunjunganId)
            .Select(g =>
            {
                var x = g.First();
                var bill = FindBilling("Tindakan", x.tk.TindakanId);

                var qty = bill?.QtyItem ?? x.tk.Quantity ?? 1;
                var totalTindakan = (decimal?)x.tk.Total ?? 0m;
                var harga = bill?.HargaItem ?? totalTindakan;
                var subtotal = bill?.SubTotalItem ?? ((x.tk.Quantity ?? 1) * totalTindakan);

                return (object)new
                {
                    x.t!.TindakanId,
                    x.t.NamaTindakan,
                    Qty = qty,
                    Harga = harga,
                    Subtotal = subtotal,
                    BillingId = bill?.BillingId,
                    BillingKode = bill?.BillingKode,
                    StatusBilling = bill?.StatusBilling
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
                b.StatusBilling
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
                    b.StatusBilling
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

        if (visitRows.Count > 0)
        {
            var billingVisitMap = billings
                .Where(b => b.ItemId != null && string.Equals(b.JenisBilling, "Visit Dokter", StringComparison.OrdinalIgnoreCase))
                .GroupBy(b => b.ItemId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreateDateTime ?? DateTime.MinValue).First()
                );

            var dokterIds = visitRows.Select(x => x.DokterId).Distinct().ToList();

            var dokterNameMap = await _db.Dokters.AsNoTracking()
                .Where(d => dokterIds.Contains(d.DokterId))
                .Select(d => new { d.DokterId, d.NmDokter })
                .ToDictionaryAsync(x => x.DokterId, x => x.NmDokter, ct);

            var kelasIds = visitRows
                .Where(x => x.KelasId != null)
                .Select(x => x.KelasId!.Value)
                .Distinct()
                .ToList();

            // ✅ pakai DbSet kamu (di kode kamu: TarifKelass)
            var tarifRows = await _db.TarifKelass.AsNoTracking()
                .Where(t => dokterIds.Contains(t.DokterId) && kelasIds.Contains((Guid)t.KelasId))
                .Where(t => (t.IsDelete == false || t.IsDelete == null))
                .Select(t => new
                {
                    t.DokterId,
                    t.KelasId,
                    TarifDokter = (decimal?)t.TarifDokter ?? 0m,
                    t.CreateDateTime
                })
                .ToListAsync(ct);

            var tarifMap = tarifRows
                .GroupBy(x => (x.DokterId, x.KelasId))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreateDateTime).First().TarifDokter
                );

            foreach (var grp in visitRows.GroupBy(x => new { x.DokterId, x.KelasId }))
            {
                var dokterId = grp.Key.DokterId;
                var kelasId = grp.Key.KelasId; // Guid?

                dokterNameMap.TryGetValue((Guid)dokterId, out var nmDokter);

                decimal tarifPerVisit = 0m;
                if (kelasId.HasValue && tarifMap.TryGetValue((dokterId, kelasId.Value), out var t))
                    tarifPerVisit = t;

                var visitDetails = grp
                    .OrderBy(x => x.TanggalVisit ?? DateTime.MinValue)
                    .Select(v =>
                    {
                        billingVisitMap.TryGetValue(v.VisitDokterId, out var bill);

                        var qty = bill?.QtyItem ?? 1;
                        var harga = bill?.HargaItem ?? tarifPerVisit;
                        var subtotal = bill?.SubTotalItem ?? (qty * harga);

                        return new
                        {
                            v.VisitDokterId,
                            v.TanggalVisit,
                            v.WaktuVisit,
                            v.Keterangan,

                            Qty = qty,
                            Harga = harga,
                            Subtotal = subtotal,

                            BillingId = bill?.BillingId,
                            BillingKode = bill?.BillingKode,
                            StatusBilling = bill?.StatusBilling
                        };
                    })
                    .ToList();

                var subtotalGroup = visitDetails.Sum(x => x.Subtotal);

                dto.TotalBiayaVisitDokter += subtotalGroup;

                dto.DaftarVisitDokter.Add(new
                {
                    DokterId = dokterId,
                    NmDokter = nmDokter,
                    KelasId = kelasId,
                    Qty = visitDetails.Count,
                    HargaPerVisit = tarifPerVisit,
                    Subtotal = subtotalGroup,
                    Visits = visitDetails
                });
            }
        }

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
                    StatusBilling = billKamar?.StatusBilling,

                    KamarId = kamarId,
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

        // =========================
        // 10) TOTAL KESELURUHAN
        // =========================
        dto.TotalKeseluruhan =
            dto.TotalPemeriksaanLab +
            dto.TotalObat +
            dto.TotalRacikan +
            dto.TotalTindakan +
            dto.TotalBiayaAdmin +
            dto.TotalAlkes +
            dto.TotalBiayaVisitDokter +
            dto.TotalKamarRanap;

        return dto;
    }


    // =========================
    // HELPERS (punyamu + overload snapshot)
    // =========================
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
}
