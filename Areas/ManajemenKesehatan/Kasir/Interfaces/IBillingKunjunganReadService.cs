using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface IBillingKunjunganReadService
    {
        Task<BillingKunjunganDto?> GetBillingKeseluruhanAsync(
            Guid kunjunganId,
            DateTime? asOf = null,
            CancellationToken ct = default);
    }
}
