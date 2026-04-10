namespace QuilvianSystemBackendDev.Interfaces
{
    public interface IAsuransiCoverageService
    {
        Task<bool?> GetIsCoveredAsync(Guid? kunjunganId, string? jenisBilling, Guid? itemId = null, CancellationToken ct = default);

    }
}
