using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories; 

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AiController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _context = context;
    }

    // Helper: ambil setting AI sekali, dipakai semua endpoint
    private async Task<(string ApiUrl, string ApiKey, string Model)> GetAiConfigAsync()
    {
        var dbSetting = await _context.Settings.AsNoTracking().FirstOrDefaultAsync();

        var apiUrl = (dbSetting?.BaseUrlAi ?? _configuration["AiSettings:BaseUrl"])?.Trim();
        var apiKey = (dbSetting?.ApiKeyAi ?? _configuration["AiSettings:ApiKey"])?.Trim();
        var model = (dbSetting?.ModelAi ?? _configuration["AiSettings:Model"])?.Trim();

        // default model kalau kosong
        if (string.IsNullOrWhiteSpace(model))
            model = "gpt-4.1-mini";

        if (string.IsNullOrWhiteSpace(apiUrl))
            throw new InvalidOperationException("AiSettings:BaseUrl kosong (DB/appsettings).");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("AiSettings:ApiKey kosong (DB/appsettings).");

        return (apiUrl!, apiKey!, model!);
    }

    [HttpPost("all")]
    public async Task<IActionResult> All([FromBody] PromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Prompt))
            return BadRequest("Prompt wajib diisi.");

        (string apiUrl, string apiKey, string model) cfg;
        try
        {
            cfg = await GetAiConfigAsync();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        string fullPrompt = request.Prompt + " Jawab secara singkat dan jelas.";

        var requestBody = new
        {
            model = cfg.model,
            messages = new[] { new { role = "user", content = fullPrompt } },
            temperature = 0.7
        };

        return await SendChatCompletions(cfg.apiUrl, cfg.apiKey, requestBody, parseAsJson: false);
    }

    [HttpPost("extract-ktp")]
    public async Task<IActionResult> ExtractKtp([FromForm] KtpRequest request)
    {
        if (request?.KtpImage == null || request.KtpImage.Length == 0)
            return BadRequest("KtpImage wajib diisi.");

        (string apiUrl, string apiKey, string model) cfg;
        try
        {
            cfg = await GetAiConfigAsync();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        using var ms = new MemoryStream();
        await request.KtpImage.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        var mime = string.IsNullOrWhiteSpace(request.KtpImage.ContentType) ? "image/jpeg" : request.KtpImage.ContentType;

        var promptText = @"
Tolong ekstrak semua informasi dari KTP berikut dan kembalikan dalam format JSON:
- Nama
- Tempat/Tanggal Lahir
- Jenis Kelamin
- Alamat
- RT/RW
- Kelurahan/Desa
- Kecamatan
- Agama
- Status Perkawinan
- Pekerjaan
- Kewarganegaraan
- Nomor KTP
- Berlaku Hingga

Output WAJIB JSON valid, tanpa markdown, tanpa ```.
".Trim();

        var requestBody = new
        {
            model = cfg.model,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = promptText },
                        new { type = "image_url", image_url = new { url = $"data:{mime};base64,{base64}" } }
                    }
                }
            },
            temperature = 0
        };

        return await SendChatCompletions(cfg.apiUrl, cfg.apiKey, requestBody, parseAsJson: true);
    }

    [HttpPost("analyze-radiology")]
    public async Task<IActionResult> AnalyzeRadiology([FromForm] RadiologyPromptRequest request)
    {
        if (request?.RadiologyImage == null || request.RadiologyImage.Length == 0)
            return BadRequest("RadiologyImage wajib diisi.");

        (string apiUrl, string apiKey, string model) cfg;
        try
        {
            cfg = await GetAiConfigAsync();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        using var ms = new MemoryStream();
        await request.RadiologyImage.CopyToAsync(ms);
        var b64 = Convert.ToBase64String(ms.ToArray());
        var mime = string.IsNullOrWhiteSpace(request.RadiologyImage.ContentType) ? "image/jpeg" : request.RadiologyImage.ContentType;

        var promptText = $@"
Tolong analisis temuan radiologi dari gambar berikut. Kembalikan JSON valid saja berisi:
1) temuan_utama
2) diagnosis_banding (dengan alasan)
3) pemeriksaan_lanjutan (termasuk radiologi jika relevan)
4) terapi_awal
5) alert_klinis

Info klinis: {request.ClinicalInfo ?? "(tidak ada)"}

WAJIB: Output harus JSON valid saja, tanpa markdown, tanpa ```.
".Trim();

        var requestBody = new
        {
            model = cfg.model,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = promptText },
                        new { type = "image_url", image_url = new { url = $"data:{mime};base64,{b64}" } }
                    }
                }
            },
            temperature = 0.2
        };

        return await SendChatCompletions(cfg.apiUrl, cfg.apiKey, requestBody, parseAsJson: true);
    }

    // Helper kirim request + parse response
    private async Task<IActionResult> SendChatCompletions(string apiUrl, string apiKey, object requestBody, bool parseAsJson)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, responseContent);

        string contentString;
        try
        {
            contentString = JsonDocument.Parse(responseContent)
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
        catch
        {
            return Ok(new { raw = responseContent, note = "Tidak bisa parse choices/message/content" });
        }

        var cleaned = CleanTripleBackticks(contentString);

        if (!parseAsJson)
            return Ok(new { answer = cleaned.Trim() });

        try
        {
            var json = JsonDocument.Parse(cleaned);
            return Ok(json.RootElement);
        }
        catch (JsonException ex)
        {
            return Ok(new { raw = cleaned, error = "Gagal parsing JSON", detail = ex.Message });
        }
    }

    private static string CleanTripleBackticks(string input)
    {
        var s = (input ?? "").Trim();

        if (s.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(7);
        else if (s.StartsWith("```"))
            s = s.Substring(3);

        if (s.EndsWith("```"))
            s = s.Substring(0, s.Length - 3);

        return s.Trim();
    }

    public class PromptRequest
    {
        public string Prompt { get; set; }
    }

    public class KtpRequest
    {
        public IFormFile KtpImage { get; set; }
    }

    public class RadiologyPromptRequest
    {
        public string? ClinicalInfo { get; set; }
        public IFormFile RadiologyImage { get; set; }
    }
}

//using Microsoft.AspNetCore.Mvc;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;

//[ApiController]
//[Route("api/[controller]")]
//public class AiController : ControllerBase
//{
//    private readonly HttpClient _httpClient;
//    private readonly IConfiguration _configuration;

//    public AiController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//    {
//        _httpClient = httpClientFactory.CreateClient();
//        _configuration = configuration;
//    }

//    [HttpPost("all")]
//    public async Task<IActionResult> All([FromBody] PromptRequest request)
//    {
//        string apiUrl = _configuration["AiSettings:BaseUrl"]?.Trim();
//        string apiKey = _configuration["AiSettings:ApiKey"]?.Trim();
//        string model = _configuration["AiSettings:Model"]?.Trim();

//        if (string.IsNullOrWhiteSpace(apiUrl))
//            return BadRequest("AiSettings:BaseUrl kosong.");
//        if (string.IsNullOrWhiteSpace(apiKey))
//            return BadRequest("AiSettings:ApiKey kosong.");
//        if (string.IsNullOrWhiteSpace(model))
//            model = "gpt-4.1-mini";

//        if (string.IsNullOrWhiteSpace(request.Prompt))
//            return BadRequest("Prompt wajib diisi.");

//        // Tambahkan instruksi untuk menjawab secara singkat
//        string fullPrompt = request.Prompt + " Jawab secara singkat dan jelas.";

//        var requestBody = new
//        {
//            model = model,
//            messages = new[]
//            {
//            new { role = "user", content = fullPrompt }
//        },
//            temperature = 0.7
//        };

//        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
//        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
//        httpRequest.Content = new StringContent(
//            JsonSerializer.Serialize(requestBody),
//            Encoding.UTF8,
//            "application/json"
//        );

//        var response = await _httpClient.SendAsync(httpRequest);
//        var responseContent = await response.Content.ReadAsStringAsync();

//        if (!response.IsSuccessStatusCode)
//            return StatusCode((int)response.StatusCode, responseContent);

//        string contentString;
//        try
//        {
//            contentString = JsonDocument.Parse(responseContent)
//                .RootElement
//                .GetProperty("choices")[0]
//                .GetProperty("message")
//                .GetProperty("content")
//                .GetString() ?? "";
//        }
//        catch
//        {
//            return Ok(new { raw = responseContent, note = "Tidak bisa parse choices/message/content" });
//        }

//        return Ok(new
//        {
//            answer = contentString.Trim()
//        });
//    }

//    public class PromptRequest
//    {
//        public string Prompt { get; set; }
//    }

//    [HttpPost("extract-ktp")]
//    public async Task<IActionResult> ExtractKtp([FromForm] KtpRequest request)
//    {
//        var dbSetting = await _context.Settings.AsNoTracking().FirstOrDefaultAsync();

//        // fallback ke appsettings kalau DB belum ada / field kosong
//        var apiUrl = (dbSetting?.BaseUrlAi ?? _configuration["AiSettings:BaseUrl"])?.Trim();
//        var apiKey = (dbSetting?.ApiKeyAi ?? _configuration["AiSettings:ApiKey"])?.Trim();
//        var model = (dbSetting?.ModelAi ?? _configuration["AiSettings:Model"])?.Trim();

//        if (request.KtpImage == null || request.KtpImage.Length == 0)
//            return BadRequest("KtpImage wajib diisi.");

//        // Baca gambar -> Base64
//        using var ms = new MemoryStream();
//        await request.KtpImage.CopyToAsync(ms);
//        var bytes = ms.ToArray();
//        var base64 = Convert.ToBase64String(bytes);
//        var mime = string.IsNullOrWhiteSpace(request.KtpImage.ContentType)
//            ? "image/jpeg"
//            : request.KtpImage.ContentType;

//        // Prompt untuk ekstraksi data KTP
//        var promptText = @$"
//            Tolong ekstrak semua informasi dari KTP berikut dan kembalikan dalam format JSON:
//            - Nama
//            - Tempat/Tanggal Lahir
//            - Jenis Kelamin
//            - Alamat
//            - RT/RW
//            - Kelurahan/Desa
//            - Kecamatan
//            - Agama
//            - Status Perkawinan
//            - Pekerjaan
//            - Kewarganegaraan
//            - Nomor KTP
//            - Berlaku Hingga

//            Output WAJIB JSON valid, tanpa markdown, tanpa ```.
//        ";

//        var requestBody = new
//        {
//            model = model,
//            messages = new object[]
//            {
//            new
//            {
//                role = "user",
//                content = new object[]
//                {
//                    new { type = "text", text = promptText },
//                    new { type = "image_url", image_url = new { url = $"data:{mime};base64,{base64}" } }
//                }
//            }
//            },
//            temperature = 0
//        };

//        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
//        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
//        httpRequest.Content = new StringContent(
//            JsonSerializer.Serialize(requestBody),
//            Encoding.UTF8,
//            "application/json"
//        );

//        var response = await _httpClient.SendAsync(httpRequest);
//        var responseContent = await response.Content.ReadAsStringAsync();

//        if (!response.IsSuccessStatusCode)
//            return StatusCode((int)response.StatusCode, responseContent);

//        string contentString;
//        try
//        {
//            contentString = JsonDocument.Parse(responseContent)
//                .RootElement
//                .GetProperty("choices")[0]
//                .GetProperty("message")
//                .GetProperty("content")
//                .GetString() ?? "";
//        }
//        catch
//        {
//            return Ok(new { raw = responseContent, note = "Tidak bisa parse choices/message/content" });
//        }

//        // Bersihkan ``` jika ada
//        string cleaned = contentString.Trim();
//        if (cleaned.StartsWith("```")) cleaned = cleaned.Substring(3);
//        if (cleaned.EndsWith("```")) cleaned = cleaned.Substring(0, cleaned.Length - 3);
//        cleaned = cleaned.Trim();

//        try
//        {
//            var json = JsonDocument.Parse(cleaned);
//            return Ok(json.RootElement);
//        }
//        catch (JsonException ex)
//        {
//            return Ok(new { raw = cleaned, error = "Gagal parsing JSON", detail = ex.Message });
//        }
//    }

//    // Tambahkan class untuk request
//    public class KtpRequest
//    {
//        public IFormFile KtpImage { get; set; }
//    }

//    [HttpPost("analyze-radiology")]
//    public async Task<IActionResult> AnalyzeRadiology([FromForm] RadiologyPromptRequest request)
//    {
//        string apiUrl = _configuration["AiSettings:BaseUrl"]?.Trim();
//        string apiKey = _configuration["AiSettings:ApiKey"]?.Trim();
//        string model = _configuration["AiSettings:Model"]?.Trim();

//        if (string.IsNullOrWhiteSpace(apiUrl))
//            return BadRequest("AiSettings:BaseUrl kosong.");
//        if (string.IsNullOrWhiteSpace(apiKey))
//            return BadRequest("AiSettings:ApiKey kosong.");
//        if (string.IsNullOrWhiteSpace(model))
//            model = "gpt-4.1-mini";

//        if (request.RadiologyImage == null || request.RadiologyImage.Length == 0)
//            return BadRequest("RadiologyImage wajib diisi.");

//        // 1) Baca file -> base64 data URL
//        using var ms = new MemoryStream();
//        await request.RadiologyImage.CopyToAsync(ms);
//        var bytes = ms.ToArray();
//        var b64 = Convert.ToBase64String(bytes);
//        var mime = string.IsNullOrWhiteSpace(request.RadiologyImage.ContentType)
//            ? "image/jpeg"
//            : request.RadiologyImage.ContentType;

//        // 2) Prompt (C# 10 compatible)
//        var promptText = @$"
//            Tolong analisis temuan radiologi dari gambar berikut. Kembalikan JSON valid saja berisi:
//            1) temuan_utama
//            2) diagnosis_banding (dengan alasan)
//            3) pemeriksaan_lanjutan (termasuk radiologi jika relevan)
//            4) terapi_awal
//            5) alert_klinis

//            Info klinis: {request.ClinicalInfo ?? "(tidak ada)"}

//            WAJIB: Output harus JSON valid saja, tanpa markdown, tanpa ```.";

//        // 3) Body untuk /v1/chat/completions (WAJIB messages)
//        var requestBody = new
//        {
//            model = model,
//            messages = new object[]
//            {
//            new
//            {
//                role = "user",
//                content = new object[]
//                {
//                    new { type = "text", text = promptText },
//                    new
//                    {
//                        type = "image_url",
//                        image_url = new { url = $"data:{mime};base64,{b64}" }
//                    }
//                }
//            }
//            },
//            temperature = 0.2
//        };

//        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
//        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
//        httpRequest.Content = new StringContent(
//            JsonSerializer.Serialize(requestBody),
//            Encoding.UTF8,
//            "application/json"
//        );

//        var response = await _httpClient.SendAsync(httpRequest);
//        var responseContent = await response.Content.ReadAsStringAsync();

//        if (!response.IsSuccessStatusCode)
//            return StatusCode((int)response.StatusCode, responseContent);

//        // 4) Ambil output text
//        string contentString;
//        try
//        {
//            contentString = JsonDocument.Parse(responseContent)
//                .RootElement
//                .GetProperty("choices")[0]
//                .GetProperty("message")
//                .GetProperty("content")
//                .GetString() ?? "";
//        }
//        catch
//        {
//            return Ok(new { raw = responseContent, note = "Tidak bisa parse choices/message/content" });
//        }

//        // 5) Bersihkan jika model tetap kasih ```json
//        string cleaned = contentString.Trim();

//        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
//            cleaned = cleaned.Substring(7);
//        else if (cleaned.StartsWith("```"))
//            cleaned = cleaned.Substring(3);

//        if (cleaned.EndsWith("```"))
//            cleaned = cleaned.Substring(0, cleaned.Length - 3);

//        cleaned = cleaned.Trim();

//        // 6) Parse JSON hasil
//        try
//        {
//            var json = JsonDocument.Parse(cleaned);
//            return Ok(json.RootElement);
//        }
//        catch (JsonException ex)
//        {
//            return Ok(new { raw = cleaned, error = "Gagal parsing JSON", detail = ex.Message });
//        }
//    }

//    public class RadiologyPromptRequest
//    {
//        public string? ClinicalInfo { get; set; }
//        public IFormFile RadiologyImage { get; set; } // Optional
//    }

//}
