namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces
{
    public interface IKunjunganTransactionGuard
    {
        Task EnsureCanAddTransactionAsync(
            Guid kunjunganId,
            CancellationToken cancellationToken = default);
    }
}
