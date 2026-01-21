namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface INoKwitansiService
    {
        Task<string> GenerateNoKwitansiAsync(DateTime tglPembayaran, CancellationToken ct = default);
    }
}
