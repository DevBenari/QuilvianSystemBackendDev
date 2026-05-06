namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces
{
    public interface IDepositRanapNumberService
    {
        Task<string> GenerateNoKwitansiAsync(CancellationToken cancellationToken = default);
    }
}
