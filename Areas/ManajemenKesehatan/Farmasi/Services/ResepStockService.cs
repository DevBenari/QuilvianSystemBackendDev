using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Services
{
    public class ResepStockService : IResepStockService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IObatUnitStockService _obatUnitStockService;

        public ResepStockService(
            ApplicationDbContext applicationDbContext,
            IObatUnitStockService obatUnitStockService)
        {
            _applicationDbContext = applicationDbContext;
            _obatUnitStockService = obatUnitStockService;
        }

        // =====================================================
        // RAJAL / OP
        // Stok dipotong saat kasir lunas.
        // =====================================================
        public async Task FinalizeRajalByKunjunganAsync(
            Guid kunjunganId,
            Guid userActiveId,
            CancellationToken ct)
        {
            var kunjungan = await _applicationDbContext.Kunjungans
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.KunjunganID == kunjunganId &&
                    (x.IsDelete == false || x.IsDelete == null),
                    ct);

            if (kunjungan == null)
                throw new InvalidOperationException("Kunjungan tidak ditemukan.");

            if (!IsRawatJalan(kunjungan.JenisKunjungan))
            {
                // Kalau IP/RANAP, jangan potong stok di kasir.
                // Stok ranap dipotong saat obat diberikan.
                return;
            }

            var reseps = await _applicationDbContext.Reseps
                .Include(x => x.ResepDetails)
                .Where(x =>
                    x.KunjunganId == kunjunganId &&
                    x.IsLunas != true &&
                    x.IsCancelled != true &&
                    (x.IsDelete == false || x.IsDelete == null))
                .ToListAsync(ct);

            foreach (var resep in reseps)
            {
                await FinalizeRajalInternalAsync(
                    resep,
                    userActiveId,
                    ct);
            }
        }

        public async Task FinalizeRajalByResepAsync(
            Guid resepId,
            Guid userActiveId,
            CancellationToken ct)
        {
            var resep = await _applicationDbContext.Reseps
                .Include(x => x.Kunjungan)
                .Include(x => x.ResepDetails)
                .FirstOrDefaultAsync(x =>
                    x.ResepId == resepId &&
                    x.IsLunas != true &&
                    x.IsCancelled != true &&
                    (x.IsDelete == false || x.IsDelete == null),
                    ct);

            if (resep == null)
                throw new InvalidOperationException("Resep tidak ditemukan, sudah lunas, atau sudah dibatalkan.");

            if (resep.Kunjungan == null)
                throw new InvalidOperationException("Data kunjungan pada resep tidak ditemukan.");

            if (!IsRawatJalan(resep.Kunjungan.JenisKunjungan))
                throw new InvalidOperationException("Resep ini bukan resep rawat jalan, stok tidak boleh dipotong melalui kasir rajal.");

            await FinalizeRajalInternalAsync(
                resep,
                userActiveId,
                ct);
        }

        private async Task FinalizeRajalInternalAsync(
            Resep resep,
            Guid userActiveId,
            CancellationToken ct)
        {
            var details = resep.ResepDetails
                .Where(x =>
                    x.IsRacikan != true &&
                    x.IsReturn != true &&
                    (x.IsDelete == false || x.IsDelete == null))
                .ToList();

            if (!details.Any())
                throw new InvalidOperationException("Detail resep tidak ditemukan.");

            foreach (var detail in details)
            {
                if (detail.ObatUnitId == null)
                    throw new InvalidOperationException("ObatUnitId pada detail resep belum terisi.");

                if (detail.Qty == null || detail.Qty <= 0)
                    throw new InvalidOperationException("Qty detail resep tidak valid.");

                await _obatUnitStockService.FinalizeReservedAsync(
                    detail.ObatUnitId.Value,
                    detail.Qty.Value,
                    userActiveId,
                    ct);

                // Kalau di flow Anda pembayaran = obat langsung diambil,
                // boleh true-kan dua status ini.
                // Kalau pengambilan obat punya endpoint terpisah, hapus 2 baris ini.
                detail.StatusPengambilanObat = true;
                detail.StatusDiberikanPasien = true;

                detail.UpdateBy = userActiveId;
                detail.UpdateDateTime = DateTimeOffset.UtcNow;
            }

            resep.IsLunas = true;
            resep.StatusPembuatanResep = "Paid";
            resep.StatusPengambilanResep = true;
            resep.UpdateBy = userActiveId;
            resep.UpdateDateTime = DateTimeOffset.UtcNow;
        }

        // =====================================================
        // RANAP / IP
        // Stok dipotong saat obat diberikan.
        // Bisa pilih pagi, siang, malam sekaligus.
        // =====================================================
        public async Task FinalizeRanapPemberianAsync(
            Guid detailResepId,
            List<string> waktuPengambilan,
            Guid userActiveId,
            CancellationToken ct)
        {
            if (waktuPengambilan == null || !waktuPengambilan.Any())
                throw new InvalidOperationException("Waktu pengambilan wajib dipilih.");

            var normalizedWaktu = waktuPengambilan
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpper())
                .Distinct()
                .ToList();

            var allowed = new[] { "PAGI", "SIANG", "MALAM" };

            if (normalizedWaktu.Any(x => !allowed.Contains(x)))
                throw new InvalidOperationException("Waktu pengambilan hanya boleh PAGI, SIANG, atau MALAM.");

            var detail = await _applicationDbContext.DetailReseps
                .Include(x => x.Resep)
                .FirstOrDefaultAsync(x =>
                    x.DetailResepId == detailResepId &&
                    x.IsRacikan != true &&
                    x.IsReturn != true &&
                    (x.IsDelete == false || x.IsDelete == null),
                    ct);

            if (detail == null)
                throw new InvalidOperationException("Detail resep tidak ditemukan.");

            if (detail.Resep == null)
                throw new InvalidOperationException("Header resep tidak ditemukan.");

            if (detail.Resep.IsCancelled == true)
                throw new InvalidOperationException("Resep sudah dibatalkan.");

            if (detail.ObatUnitId == null)
                throw new InvalidOperationException("ObatUnitId pada detail resep belum terisi.");

            if (detail.Qty == null || detail.Qty <= 0)
                throw new InvalidOperationException("Qty detail resep tidak valid.");

            var signaInfo = ParseSigna(detail.Signa);

            var existingGivenCount = CountExistingGivenTimes(detail);

            var waktuBaru = new List<string>();

            foreach (var waktu in normalizedWaktu)
            {
                if (waktu == "PAGI")
                {
                    if (detail.ObatPagiDiambil == true)
                        continue;

                    detail.ObatPagiDiambil = true;
                    waktuBaru.Add("PAGI");
                }
                else if (waktu == "SIANG")
                {
                    if (detail.ObatSiangDiambil == true)
                        continue;

                    detail.ObatSiangDiambil = true;
                    waktuBaru.Add("SIANG");
                }
                else if (waktu == "MALAM")
                {
                    if (detail.ObatMalamDiambil == true)
                        continue;

                    detail.ObatMalamDiambil = true;
                    waktuBaru.Add("MALAM");
                }
            }

            if (!waktuBaru.Any())
                throw new InvalidOperationException("Semua waktu yang dipilih sudah pernah diberikan.");

            var totalGivenAfter = existingGivenCount + waktuBaru.Count;

            if (totalGivenAfter > signaInfo.FrekuensiPerHari)
            {
                throw new InvalidOperationException(
                    $"Jumlah waktu pemberian melebihi Signa. Signa: {detail.Signa}, frekuensi: {signaInfo.FrekuensiPerHari} kali/hari, total pemberian setelah update: {totalGivenAfter}.");
            }

            var qtyFinal = signaInfo.QtyPerPemberian * waktuBaru.Count;

            if (qtyFinal <= 0)
                throw new InvalidOperationException("Qty pemberian tidak valid.");

            if (qtyFinal > detail.Qty)
            {
                throw new InvalidOperationException(
                    $"Qty pemberian melebihi Qty resep. Qty resep: {detail.Qty}, qty yang akan dipotong: {qtyFinal}.");
            }

            await _obatUnitStockService.FinalizeReservedAsync(
                detail.ObatUnitId.Value,
                qtyFinal,
                userActiveId,
                ct);

            detail.StatusDiberikanPasien = true;

            if (totalGivenAfter >= signaInfo.FrekuensiPerHari)
                detail.StatusPengambilanObat = true;

            detail.UpdateBy = userActiveId;
            detail.UpdateDateTime = DateTimeOffset.UtcNow;

            detail.Resep.UpdateBy = userActiveId;
            detail.Resep.UpdateDateTime = DateTimeOffset.UtcNow;
        }

        // =====================================================
        // RESEP TEBUS
        // Stok dipotong saat kasir resep tebus lunas.
        // =====================================================
        public async Task FinalizeResepTebusAsync(
            Guid resepTebusId,
            Guid userActiveId,
            CancellationToken ct)
        {
            var resepTebus = await _applicationDbContext.ResepTebuss
                .FirstOrDefaultAsync(x =>
                    x.ResepTebusId == resepTebusId &&
                    x.IsLunas != true &&
                    x.IsCancelled != true &&
                    (x.IsDelete == false || x.IsDelete == null),
                    ct);

            if (resepTebus == null)
                throw new InvalidOperationException("Resep tebus tidak ditemukan, sudah lunas, atau sudah dibatalkan.");

            var details = await _applicationDbContext.ResepTebusDetails
                .Where(x =>
                    x.ResepTebusId == resepTebusId &&
                    x.IsRacikan != true &&
                    (x.IsDelete == false || x.IsDelete == null))
                .ToListAsync(ct);

            if (!details.Any())
                throw new InvalidOperationException("Detail resep tebus tidak ditemukan.");

            foreach (var detail in details)
            {
                if (detail.ObatUnitId == null)
                    throw new InvalidOperationException("ObatUnitId pada detail resep tebus belum terisi.");

                if (detail.Qty == null || detail.Qty <= 0)
                    throw new InvalidOperationException("Qty detail resep tebus tidak valid.");

                await _obatUnitStockService.FinalizeReservedAsync(
                    detail.ObatUnitId.Value,
                    detail.Qty.Value,
                    userActiveId,
                    ct);

                detail.UpdateBy = userActiveId;
                detail.UpdateDateTime = DateTimeOffset.UtcNow;
            }

            resepTebus.IsLunas = true;
            resepTebus.StatusPembuatanResep = "Paid";
            resepTebus.TanggalLunas = DateTime.UtcNow;
            resepTebus.UpdateBy = userActiveId;
            resepTebus.UpdateDateTime = DateTimeOffset.UtcNow;
        }

        // =====================================================
        // Helpers
        // =====================================================
        private static bool IsRawatJalan(string? jenisKunjungan)
        {
            var value = (jenisKunjungan ?? string.Empty).Trim().ToUpper();

            return value == "OP" ||
                   value == "RAJAL" ||
                   value == "RAWAT_JALAN" ||
                   value == "RAWAT JALAN";
        }

        private static int CountExistingGivenTimes(ResepDetail detail)
        {
            var count = 0;

            if (detail.ObatPagiDiambil == true)
                count++;

            if (detail.ObatSiangDiambil == true)
                count++;

            if (detail.ObatMalamDiambil == true)
                count++;

            return count;
        }

        private static SignaInfo ParseSigna(string? signa)
        {
            if (string.IsNullOrWhiteSpace(signa))
                throw new InvalidOperationException("Signa belum diisi.");

            var normalized = signa.Trim().ToLower();

            // Format didukung:
            // 3 x 1
            // 3x1
            // 3 X 1
            // 3×1
            var match = Regex.Match(
                normalized,
                @"^\s*(\d+)\s*[x×]\s*(\d+)\s*$");

            if (!match.Success)
            {
                throw new InvalidOperationException(
                    $"Format Signa tidak valid: '{signa}'. Gunakan format seperti '3 x 1', '2 x 1', atau '1 x 1'.");
            }

            var frekuensiPerHari = int.Parse(match.Groups[1].Value);
            var qtyPerPemberian = int.Parse(match.Groups[2].Value);

            if (frekuensiPerHari <= 0)
                throw new InvalidOperationException("Frekuensi pada Signa tidak valid.");

            if (qtyPerPemberian <= 0)
                throw new InvalidOperationException("Qty per pemberian pada Signa tidak valid.");

            return new SignaInfo
            {
                FrekuensiPerHari = frekuensiPerHari,
                QtyPerPemberian = qtyPerPemberian
            };
        }

        private class SignaInfo
        {
            public int FrekuensiPerHari { get; set; }
            public int QtyPerPemberian { get; set; }
        }
    }
}