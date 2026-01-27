namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface INoKwitansiService
    {
        Task<string> GenerateNoKwitansiAsync(DateTimeOffset tglPembayaran, CancellationToken ct = default);
    }
}
