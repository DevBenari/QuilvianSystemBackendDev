using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Services
{
    public class ObatUnitReserveService : IObatUnitStockService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ObatUnitReserveService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        private async Task<ObatUnit> GetObatUnitForUpdateAsync(
            Guid obatId,
            Guid gudangUnitId,
            CancellationToken ct)
        {
            var obatUnit = await _applicationDbContext.ObatUnits
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM public.""MstObatUnit""
                    WHERE ""ObatId"" = {obatId}
                      AND ""GudangUnitId"" = {gudangUnitId}
                      AND COALESCE(""IsDelete"", false) = false
                    FOR UPDATE
                ")
                .FirstOrDefaultAsync(ct);

            if (obatUnit == null)
                throw new InvalidOperationException("Obat tidak tersedia pada gudang unit ini.");

            return obatUnit;
        }

        private async Task<ObatUnit> GetObatUnitByIdForUpdateAsync(
            Guid obatUnitId,
            CancellationToken ct)
        {
            var obatUnit = await _applicationDbContext.ObatUnits
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM public.""MstObatUnit""
                    WHERE ""ObatUnitId"" = {obatUnitId}
                      AND COALESCE(""IsDelete"", false) = false
                    FOR UPDATE
                ")
                .FirstOrDefaultAsync(ct);

            if (obatUnit == null)
                throw new InvalidOperationException("Data stok obat unit tidak ditemukan.");

            return obatUnit;
        }

        private async Task<Guid> ResolveGudangUnitIdByInstalasiUnitAsync(
            Guid instalasiUnitId,
            CancellationToken ct)
        {
            var gudangUnitId = await _applicationDbContext.GudangUnits
                .AsNoTracking()
                .Where(x =>
                    x.InstalasiUnitId == instalasiUnitId &&
                    (x.IsDelete == false))
                .Select(x => (Guid?)x.GudangUnitId)
                .FirstOrDefaultAsync(ct);

            if (gudangUnitId == null)
            {
                throw new InvalidOperationException(
                    "Gudang unit untuk instalasi unit layanan pasien belum disetting.");
            }

            return gudangUnitId.Value;
        }

        public async Task<Guid> ReserveByInstalasiUnitAsync(
            Guid obatId,
            Guid instalasiUnitId,
            decimal qty,
            Guid userActiveId,
            CancellationToken ct)
        {
            var gudangUnitId = await ResolveGudangUnitIdByInstalasiUnitAsync(
                instalasiUnitId,
                ct);

            return await ReserveAsync(
                obatId,
                gudangUnitId,
                qty,
                userActiveId,
                ct);
        }

        public async Task<Guid> ReserveAsync(
            Guid obatId,
            Guid gudangUnitId,
            decimal qty,
            Guid userActiveId,
            CancellationToken ct)
        {
            if (qty <= 0)
                throw new InvalidOperationException("Qty obat harus lebih dari 0.");

            var obatUnit = await GetObatUnitForUpdateAsync(
                obatId,
                gudangUnitId,
                ct);

            var qtyTersedia = obatUnit.QtyTersedia ?? 0;

            if (qtyTersedia < qty)
            {
                throw new InvalidOperationException(
                    $"Stok obat tidak mencukupi. Qty tersedia: {qtyTersedia}, qty diminta: {qty}.");
            }

            // Saat resep dibuat:
            // Qty tetap
            // QtyAmbil naik
            // QtyTersedia turun
            obatUnit.QtyAmbil = (obatUnit.QtyAmbil ?? 0) + qty;
            obatUnit.QtyTersedia = qtyTersedia - qty;

            obatUnit.UpdateBy = userActiveId;
            obatUnit.UpdateDateTime = DateTimeOffset.UtcNow;

            return obatUnit.ObatUnitId;
        }

        public async Task ReleaseAsync(
            Guid obatUnitId,
            decimal qty,
            Guid userActiveId,
            CancellationToken ct)
        {
            if (qty <= 0)
                throw new InvalidOperationException("Qty release harus lebih dari 0.");

            var obatUnit = await GetObatUnitByIdForUpdateAsync(
                obatUnitId,
                ct);

            if ((obatUnit.QtyAmbil ?? 0) < qty)
                throw new InvalidOperationException("QtyAmbil tidak valid saat release stok.");

            // Saat resep dibatalkan sebelum lunas:
            // Qty tetap
            // QtyAmbil turun
            // QtyTersedia naik
            obatUnit.QtyAmbil = (obatUnit.QtyAmbil ?? 0) - qty;
            obatUnit.QtyTersedia = (obatUnit.QtyTersedia ?? 0) + qty;

            obatUnit.UpdateBy = userActiveId;
            obatUnit.UpdateDateTime = DateTimeOffset.UtcNow;
        }

        public async Task FinalizeReservedAsync(
            Guid obatUnitId,
            decimal qty,
            Guid userActiveId,
            CancellationToken ct)
        {
            if (qty <= 0)
                throw new InvalidOperationException("Qty finalize harus lebih dari 0.");

            var obatUnit = await GetObatUnitByIdForUpdateAsync(
                obatUnitId,
                ct);

            if ((obatUnit.Qty ?? 0) < qty)
                throw new InvalidOperationException("Qty stok fisik tidak mencukupi.");

            if ((obatUnit.QtyAmbil ?? 0) < qty)
                throw new InvalidOperationException("QtyAmbil tidak valid atau kurang dari qty resep.");

            // Saat obat benar-benar keluar:
            // Rawat jalan  : saat lunas di kasir
            // Rawat inap   : saat obat diberikan pagi/siang/malam
            // Resep tebus  : saat lunas di kasir resep tebus
            //
            // Qty turun
            // QtyAmbil turun
            // QtyTersedia tetap
            obatUnit.Qty = (obatUnit.Qty ?? 0) - qty;
            obatUnit.QtyAmbil = (obatUnit.QtyAmbil ?? 0) - qty;

            obatUnit.UpdateBy = userActiveId;
            obatUnit.UpdateDateTime = DateTimeOffset.UtcNow;
        }
    }
}
