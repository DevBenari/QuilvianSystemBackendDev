using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface IPerkiraanBillingRanapService
    {
        Task<BillingKunjunganDto?> GetPerkiraanBillingIpAsync(
            Guid kunjunganId,
            CancellationToken ct = default);
    }
}
