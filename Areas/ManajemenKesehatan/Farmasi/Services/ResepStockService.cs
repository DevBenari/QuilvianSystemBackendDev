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

        // ======================================================
        // RAJAL: stok dipotong saat kasir lunas
        // ======================================================
        public async Task FinalizeRajalByKunjunganAsync(
            Guid kunjunganId,
            Guid userActiveId,
            CancellationToken ct)
        {
            var reseps = await _applicationDbContext.Reseps
                .Include(x => x.ResepDetails)
                .Where(x =>
                    x.KunjunganId == kunjunganId &&
                    x.IsLunas != true &&
                    x.IsCancelled != true &&
                    (x.IsDelete == false || x.IsDelete == null))
                .ToListAsync(ct);

            if (!reseps.Any())
                return;

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
                .Include(x => x.ResepDetails)
                .FirstOrDefaultAsync(x =>
                    x.ResepId == resepId &&
                    x.IsLunas != true &&
                    x.IsCancelled != true &&
                    (x.IsDelete == false || x.IsDelete == null),
                    ct);

            if (resep == null)
                throw new InvalidOperationException("Resep tidak ditemukan, sudah lunas, atau sudah dibatalkan.");

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
            if (resep.ResepDetails.Any(x =>
                    x.IsRacikan == true &&
                    (x.IsDelete == false || x.IsDelete == null)))
            {
                throw new InvalidOperationException(
                    "Finalisasi stok racikan belum didukung. RacikanDetail perlu menyimpan ObatUnitId komposisi racikan.");
            }

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

                // Untuk RAJAL, kalau bayar = obat langsung diambil,
                // status ini boleh dibuat true.
                //detail.StatusPengambilanObat = true;
                //detail.StatusDiberikanPasien = true;

                detail.UpdateBy = userActiveId;
                detail.UpdateDateTime = DateTimeOffset.UtcNow;
            }

            resep.IsLunas = true;
            resep.StatusPembuatanResep = "Paid";
            resep.StatusPengambilanResep = true;
            resep.UpdateBy = userActiveId;
            resep.UpdateDateTime = DateTimeOffset.UtcNow;
        }

        // ======================================================
        // RANAP: stok dipotong saat obat diberikan
        // ======================================================
        public async Task FinalizeRanapPemberianAsync(
            Guid detailResepId,
            string waktuPemberian,
            int qtyDiberikan,
            Guid userActiveId,
            CancellationToken ct)
        {
            if (qtyDiberikan <= 0)
                throw new InvalidOperationException("QtyDiberikan wajib lebih dari 0.");

            var waktu = (waktuPemberian ?? string.Empty).Trim().ToUpper();

            if (waktu != "PAGI" && waktu != "SIANG" && waktu != "MALAM")
                throw new InvalidOperationException("Waktu pemberian hanya boleh PAGI, SIANG, atau MALAM.");

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

            if (waktu == "PAGI")
            {
                if (detail.ObatPagiDiambil == true)
                    throw new InvalidOperationException("Obat pagi sudah pernah diberikan.");

                detail.ObatPagiDiambil = true;
            }
            else if (waktu == "SIANG")
            {
                if (detail.ObatSiangDiambil == true)
                    throw new InvalidOperationException("Obat siang sudah pernah diberikan.");

                detail.ObatSiangDiambil = true;
            }
            else if (waktu == "MALAM")
            {
                if (detail.ObatMalamDiambil == true)
                    throw new InvalidOperationException("Obat malam sudah pernah diberikan.");

                detail.ObatMalamDiambil = true;
            }

            await _obatUnitStockService.FinalizeReservedAsync(
                detail.ObatUnitId.Value,
                qtyDiberikan,
                userActiveId,
                ct);

            detail.StatusDiberikanPasien = true;

            if (detail.ObatPagiDiambil == true &&
                detail.ObatSiangDiambil == true &&
                detail.ObatMalamDiambil == true)
            {
                detail.StatusPengambilanObat = true;
            }

            detail.UpdateBy = userActiveId;
            detail.UpdateDateTime = DateTimeOffset.UtcNow;

            detail.Resep.UpdateBy = userActiveId;
            detail.Resep.UpdateDateTime = DateTimeOffset.UtcNow;
        }

        // ======================================================
        // RESEP TEBUS: stok dipotong saat kasir resep tebus lunas
        // ======================================================
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
    }
}