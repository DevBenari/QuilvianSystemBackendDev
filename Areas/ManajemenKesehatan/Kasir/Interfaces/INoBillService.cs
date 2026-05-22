namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface INoBillService
    {
        Task<string> GenerateNoBillAsync(
            Guid kunjunganId,
            CancellationToken cancellationToken = default);
    }
}
