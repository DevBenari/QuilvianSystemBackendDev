using QuilvianSystemBackendDev.Services;

namespace QuilvianSystemBackendDev.Interfaces
{
    public interface IAsuransiCoverageService
    {
        Task<AsuransiCoverageResult> ResolveCoverageAsync(
            Guid? kunjunganId,
            string? jenisBilling,
            Guid? itemId = null,
            CancellationToken ct = default);

        Task RefreshCoverageBillingByKunjunganAsync(
            Guid kunjunganId,
            Guid userActiveId,
            CancellationToken ct = default);
    }
}
