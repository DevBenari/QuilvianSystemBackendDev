namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces
{
    public interface INoRMGeneratorService
    {
        Task<string> GenerateNoRekamMedisAsync(CancellationToken ct = default);
    }
}
