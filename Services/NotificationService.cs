using System.Text;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.Interfaces;

namespace QuilvianSystemBackendDev.Services
{
    public class NotificationService : INotification
    {
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IConfiguration config, ILogger<NotificationService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendWhatsAppAsync(string phoneNumber, string message)
        {
            using var client = new HttpClient();

            var apiKey = _config["WhatsApp:x-api-key"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }

            // ✅ Gunakan "number" sesuai Postman yang berhasil
            var payload = new
            {
                number = phoneNumber,
                message = message
            };

            var json = JsonConvert.SerializeObject(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiUrl = _config["WhatsApp:ApiUrl"];
            var response = await client.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"❌ WA gagal dikirim ke {phoneNumber}: {response.StatusCode} - {responseContent}");
                return false;
            }

            _logger.LogInformation($"✅ WA berhasil dikirim ke {phoneNumber}: {responseContent}");
            return true;
        }
    }
}
