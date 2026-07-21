using QuilvianSystemBackendDev.DTO;

namespace QuilvianSystemBackendDev.Interfaces
{
    public interface INotification
    {
        Task<WhatsAppResultDto> SendWhatsAppAsync(string phoneNumber, string message, CancellationToken ct);
    }
}
