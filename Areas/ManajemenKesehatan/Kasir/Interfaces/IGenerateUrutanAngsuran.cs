namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface IGenerateUrutanAngsuran
    {
        Task<int> GenerateAsync(
            Guid kunjunganId,
            decimal? sisaPembayaranSetelahBayar,
            CancellationToken cancellationToken = default);
    }
}
