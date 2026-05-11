namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces
{
    public interface IKunjunganNoRegistrasiService
    {
        Task<string> GenerateNoRegistrasiAsync(
            CancellationToken cancellationToken = default);
    }
}
