using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Interfaces
{
    public interface IObatUnitStockService
    {
        Task<Guid> ReserveAsync(
            Guid obatId,
            Guid gudangUnitId,
            decimal qty,
            Guid userActiveId,
            CancellationToken ct);

        Task<Guid> ReserveByInstalasiUnitAsync(
            Guid obatId,
            Guid instalasiUnitId,
            decimal qty,
            Guid userActiveId,
            CancellationToken ct);

        Task ReleaseAsync(
            Guid obatUnitId,
            decimal qty,
            Guid userActiveId,
            CancellationToken ct);

        Task FinalizeReservedAsync(
            Guid obatUnitId,
            decimal qty,
            Guid userActiveId,
            CancellationToken ct);
    }
}
