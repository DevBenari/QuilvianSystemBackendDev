namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface ICountAngsuran
    {
        Task<int> CountAsync(Guid kunjunganId, CancellationToken cancellationToken = default);

    }
}
