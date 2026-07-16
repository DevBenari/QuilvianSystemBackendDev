using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces
{
    public interface IKunjunganAdminBillingService
    {
        Task ApplyBiayaAdminAsync(
            Guid? kunjunganId,
            string? jenisKunjungan,
            string? asalKunjungan,
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
    }
}