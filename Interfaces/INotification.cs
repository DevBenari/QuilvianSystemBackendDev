namespace QuilvianSystemBackendDev.Interfaces
{
    public interface INotification
    {
        Task<bool> SendWhatsAppAsync(string phoneNumber, string message);
    }
}
