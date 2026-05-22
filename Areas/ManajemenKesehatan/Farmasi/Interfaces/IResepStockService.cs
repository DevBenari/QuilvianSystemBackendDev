namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Interfaces
{
    public interface IResepStockService
    {
        Task FinalizeRajalByKunjunganAsync(
            Guid kunjunganId,
            Guid userActiveId,
            CancellationToken ct);

        Task FinalizeRajalByResepAsync(
            Guid resepId,
            Guid userActiveId,
            CancellationToken ct);

        Task FinalizeRanapPemberianAsync(
            Guid detailResepId,
            string waktuPemberian,
            int qtyDiberikan,
            Guid userActiveId,
            CancellationToken ct);

        Task FinalizeResepTebusAsync(
            Guid resepTebusId,
            Guid userActiveId,
            CancellationToken ct);
    }
}
