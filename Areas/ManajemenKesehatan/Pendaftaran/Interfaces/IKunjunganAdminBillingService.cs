using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces
{
    public interface IKunjunganAdminBillingService
    {
        Task ApplyBiayaAdminAsync(
                Guid? kunjunganId,
                string kodeJenis,
                Guid userActiveId,
                CancellationToken cancellationToken = default
            );
    }
}
