using System.Text;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.DTO;
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

        public async Task<WhatsAppResultDto> SendWhatsAppAsync(string phoneNumber, string message)
        {
            using var client = new HttpClient();

            var apiKey = _config["WhatsApp:x-api-key"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            var payload = new
            {
                number = normalizedPhone,
                message = message
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiUrl = _config["WhatsApp:ApiUrl"] ?? string.Empty;

            try
            {
                _logger.LogInformation("WA Request URL: {ApiUrl}", apiUrl);
                _logger.LogInformation("WA Request Body: {Json}", json);

                var response = await client.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ WA gagal dikirim ke {PhoneNumber}: {StatusCode} - {ResponseContent}",
                        normalizedPhone, (int)response.StatusCode, responseContent);

                    return new WhatsAppResultDto
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        Message = "WA gagal dikirim",
                        ResponseBody = responseContent,
                        RequestUrl = apiUrl,
                        RequestBody = json,
                        //PhoneNumber = normalizedPhone
                    };
                }

                _logger.LogInformation("✅ WA berhasil dikirim ke {PhoneNumber}: {ResponseContent}",
                    normalizedPhone, responseContent);

                return new WhatsAppResultDto
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Message = "WA berhasil dikirim",
                    ResponseBody = responseContent,
                    RequestUrl = apiUrl,
                    RequestBody = json,
                    //PhoneNumber = normalizedPhone
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception saat kirim WA ke {PhoneNumber}", normalizedPhone);

                return new WhatsAppResultDto
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    ResponseBody = ex.ToString(),
                    RequestUrl = apiUrl,
                    RequestBody = json,
                    //PhoneNumber = normalizedPhone
                };
            }
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            phoneNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

            if (phoneNumber.StartsWith("+62"))
                return "62" + phoneNumber.Substring(3);

            if (phoneNumber.StartsWith("08"))
                return "62" + phoneNumber.Substring(1);

            return phoneNumber;
        }
    }
}
