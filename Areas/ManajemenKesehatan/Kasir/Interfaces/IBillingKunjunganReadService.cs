using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using static BillingKunjunganReadService;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface IBillingKunjunganReadService
    {
        Task<BillingKunjunganDto?> GetBillingKeseluruhanAsync(
            Guid kunjunganId,
            DateTime? asOf = null,
            CancellationToken ct = default);

        Task<PagedResult<object>> GetBillingPagedAsync(
            BillingPagedQuery query,
            CancellationToken ct = default);

        Task<IReadOnlyList<object>> GetMainKasirDanDetailPembayaranAsync(
         Guid kunjunganId,
         CancellationToken ct = default);

        Task<IReadOnlyList<object>> GetRiwayatBillingPasienByNoRmFastAsync(
            string noRekamMedis,
            DateTime? asOf = null,
            CancellationToken ct = default);

        Task<PendapatanKasirHarianDto> GetPendapatanKasirHarianAsync(
            Guid kasirUserId,
            DateTime? tanggal = null,
            CancellationToken ct = default);
    }
}
