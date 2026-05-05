using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableCors("AllowSpecific")]
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

    [HttpPost("all2")]
    public async Task<IActionResult> All2([FromBody] PromptRequest request)
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
    private async Task<Setting> GetActiveAiSettingAsync()
    {
        var setting = await _context.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StatusAi == true);

        if (setting == null)
            throw new InvalidOperationException("Setting AI aktif tidak ditemukan.");

        if (string.IsNullOrWhiteSpace(setting.Prompt))
            throw new InvalidOperationException("Prompt AI kosong di database.");

        return setting;
    }

    [HttpPost("analyze-radiology")]
    public async Task<IActionResult> AnalyzeRadiology([FromForm] RadiologyPromptRequest request)
    {
        if (request?.RadiologyImages == null || request.RadiologyImages.Count == 0)
            return BadRequest("Minimal 1 RadiologyImages wajib diisi.");

        Setting setting;
        try
        {
            setting = await GetActiveAiSettingAsync();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        // 🔹 Build image contents
        var imageContents = new List<object>();

        foreach (var file in request.RadiologyImages)
        {
            if (file.Length == 0) continue;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var b64 = Convert.ToBase64String(ms.ToArray());
            var mime = string.IsNullOrWhiteSpace(file.ContentType)
                ? "image/jpeg"
                : file.ContentType;

            imageContents.Add(new
            {
                type = "image_url",
                image_url = new { url = $"data:{mime};base64,{b64}" }
            });
        }

        if (imageContents.Count == 0)
            return BadRequest("File gambar tidak valid.");

        // 🔹 Prompt dari DB
        var promptText = setting.Prompt
            .Replace("{clinical_info}", request.ClinicalInfo ?? "(tidak ada)");

        // 🔹 Gabungkan text + multi image
        var content = new List<object>
    {
        new { type = "text", text = promptText }
    };
        content.AddRange(imageContents);

        var requestBody = new
        {
            model = string.IsNullOrWhiteSpace(setting.ModelAi)
                ? "gpt-4.1-mini"
                : setting.ModelAi,
            messages = new object[]
            {
            new
            {
                role = "user",
                content = content
            }
            },
            temperature = 0.2
        };

        return await SendChatCompletions(
            setting.BaseUrlAi,
            setting.ApiKeyAi,
            requestBody,
            parseAsJson: true
        );
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
        public List<IFormFile> RadiologyImages { get; set; } = new();
    }

}
