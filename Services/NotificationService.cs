using System.Text;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.DTO;
using QuilvianSystemBackendDev.Interfaces;

namespace QuilvianSystemBackendDev.Services
{
    public class NotificationService : INotification
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<WhatsAppResultDto> SendWhatsAppAsync(
            string phoneNumber,
            string message,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new WhatsAppResultDto
                {
                    Success = false,
                    Message = "Nomor WhatsApp wajib diisi."
                };
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return new WhatsAppResultDto
                {
                    Success = false,
                    Message = "Pesan WhatsApp wajib diisi."
                };
            }

            var apiUrl = _config["WhatsApp:ApiUrl"];
            var apiKey = _config["WhatsApp:x-api-key"];

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                return new WhatsAppResultDto
                {
                    Success = false,
                    Message = "Konfigurasi WhatsApp:ApiUrl belum tersedia."
                };
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new WhatsAppResultDto
                {
                    Success = false,
                    Message = "Konfigurasi WhatsApp:x-api-key belum tersedia."
                };
            }

            var payload = new
            {
                // Tidak perlu normalisasi di backend.
                // WebJS sudah menangani 08, +62, dan 62.
                number = phoneNumber.Trim(),
                message = message.Trim()
            };

            var json = JsonConvert.SerializeObject(payload);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                apiUrl);

            httpRequest.Headers.TryAddWithoutValidation(
                "x-api-key",
                apiKey);

            httpRequest.Content = content;

            try
            {
                _logger.LogInformation(
                    "Mengirim WhatsApp melalui {ApiUrl}.",
                    apiUrl);

                using var response =
                    await _httpClient.SendAsync(
                        httpRequest,
                        cancellationToken);

                var responseContent =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "WhatsApp gagal dikirim. StatusCode: {StatusCode}. Response: {Response}",
                        (int)response.StatusCode,
                        responseContent);

                    return new WhatsAppResultDto
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        Message = "WhatsApp gagal dikirim.",
                        ResponseBody = responseContent,
                        RequestUrl = apiUrl
                    };
                }

                var webJsResult =
                    JsonConvert.DeserializeObject<WebJsResponseDto>(
                        responseContent);

                if (webJsResult?.Success == false)
                {
                    return new WhatsAppResultDto
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        Message =
                            webJsResult.Message ??
                            "WebJS gagal mengirim WhatsApp.",
                        ResponseBody = responseContent,
                        RequestUrl = apiUrl
                    };
                }

                _logger.LogInformation(
                    "WhatsApp berhasil dikirim. StatusCode: {StatusCode}.",
                    (int)response.StatusCode);

                return new WhatsAppResultDto
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Message =
                        webJsResult?.Message ??
                        "WhatsApp berhasil dikirim.",
                    ResponseBody = responseContent,
                    RequestUrl = apiUrl
                };
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    "Request ke WebJS mengalami timeout.");

                return new WhatsAppResultDto
                {
                    Success = false,
                    Message = "Request ke WebJS mengalami timeout.",
                    RequestUrl = apiUrl
                };
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Tidak dapat terhubung ke WebJS.");

                return new WhatsAppResultDto
                {
                    Success = false,
                    Message =
                        $"Tidak dapat terhubung ke WebJS: {exception.Message}",
                    RequestUrl = apiUrl
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Exception saat mengirim WhatsApp.");

                return new WhatsAppResultDto
                {
                    Success = false,
                    Message = $"Exception: {exception.Message}",
                    RequestUrl = apiUrl
                };
            }
        }

        private sealed class WebJsResponseDto
        {
            public bool Success { get; set; }

            public string? Message { get; set; }

            public string? Error { get; set; }
        }
    }
}