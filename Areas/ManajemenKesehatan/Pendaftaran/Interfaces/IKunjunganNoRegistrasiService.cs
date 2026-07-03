namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces
{
    public interface IKunjunganNoRegistrasiService
    {
        Task<string> GenerateNoRegistrasiAsync(
            CancellationToken cancellationToken = default);

        Task<string?> GenerateNoAntrianAsync(
            string kodeJenis,
            string asal,
            Guid? poliklinikId,
            CancellationToken cancellationToken = default);

        string ValidasiJenisKunjungan(
            string? jenisKunjungan,
            Guid? poliklinikId,
            decimal? depositRanap);
    }
}
