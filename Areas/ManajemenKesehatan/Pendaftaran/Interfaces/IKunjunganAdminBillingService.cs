using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces
{
    public interface IKunjunganAdminBillingService
    {
        Task ApplyBiayaAdminAsync(
            Guid? kunjunganId,
            string kodeJenis,
            Guid userActiveId,
            CancellationToken cancellationToken = default);

        Task ApplyBiayaKonsultasiDokterAsync(
            Guid? kunjunganId,
            Guid userActiveId,
            CancellationToken cancellationToken = default);

        Task ApplyBillingRawatJalanSaatSimpanSoapAsync(
            Guid? kunjunganId,
            Guid userActiveId,
            CancellationToken cancellationToken = default);

        Task ApplyAdminIGDAsync(
            Guid? kunjunganId,
            Guid userActiveId,
            CancellationToken cancellationToken = default);

        Task ApplyAdminTransferRanapAsync(
            Guid? kunjunganId,
            Guid userActiveId,
            CancellationToken cancellationToken = default);

        Task ApplyBillingAdmisiRanapBaruAsync(
            Guid? kunjunganId,
            Guid userActiveId,
            CancellationToken cancellationToken = default);
    }
}