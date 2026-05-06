namespace QuilvianSystemBackendDev.Interfaces
{
    public interface ITTDService
    {
        Task<TTDResult> CheckTTDAsync(Guid userActiveId);
    }

    public class TTDResult
    {
        public bool? HasTTD { get; set; }
        public string? Path { get; set; }
        public Guid? TTDId { get; set; }
        public string? Message { get; set; } = "";
    }
}
