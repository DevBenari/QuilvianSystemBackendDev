namespace QuilvianSystemBackendDev.Interfaces
{
    public interface IGenerateInvoiceBillingService
    {
        Task<string> GetOrCreateAsync(
            Guid kunjunganId,
            DateTime tanggalPembayaran,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateIsListWhiteOffAsync(
            Guid kunjunganId,
            DateTime today,
            CancellationToken cancellationToken = default);
    }
}
