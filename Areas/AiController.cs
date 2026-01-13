using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] PromptRequest request)
    {
        string apiUrl = _configuration["AiSettings:BaseUrl"];
        string apiKey = _configuration["AiSettings:ApiKey"];
        string model = _configuration["AiSettings:Model"];

        // Prompt template yang sudah di-hardcode
        string fullPrompt = $"Saya memiliki diagnosis awal: {request.Prompt}. Tolong buatkan analisis medis lanjutan untuk diagnosis ini dalam format JSON yang terdiri dari: 1. diagnosis_banding (dengan alasan), 2. pemeriksaan_lanjutan, 3. terapi_awal, dan 4. alert_klinis (hal-hal yang perlu diwaspadai dari kondisi ini). Format JSON. Jangan gunakan markdown.";

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = fullPrompt }
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = jsonContent;

        var response = await _httpClient.SendAsync(httpRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, responseContent);
        }

        var contentString = JsonDocument.Parse(responseContent)
                            .RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

        string cleaned = contentString.Trim();

        if (cleaned.StartsWith("```json"))
        {
            cleaned = cleaned.Substring(7);
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(3);
        }

        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }

        cleaned = cleaned.Trim();

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

    public class PromptRequest
    {
        public string Prompt { get; set; }
    }

    [HttpPost("analyze-radiology")]
    public async Task<IActionResult> AnalyzeRadiology([FromForm] RadiologyPromptRequest request)
    {
        string apiUrl = _configuration["AiSettings:BaseUrl"]?.Trim();
        string apiKey = _configuration["AiSettings:ApiKey"]?.Trim();
        string model = _configuration["AiSettings:Model"]?.Trim();

        if (string.IsNullOrWhiteSpace(apiUrl))
            return BadRequest("AiSettings:BaseUrl kosong.");
        if (string.IsNullOrWhiteSpace(apiKey))
            return BadRequest("AiSettings:ApiKey kosong.");
        if (string.IsNullOrWhiteSpace(model))
            model = "gpt-4.1-mini";

        if (request.RadiologyImage == null || request.RadiologyImage.Length == 0)
            return BadRequest("RadiologyImage wajib diisi.");

        // 1) Baca file -> base64 data URL
        using var ms = new MemoryStream();
        await request.RadiologyImage.CopyToAsync(ms);
        var bytes = ms.ToArray();
        var b64 = Convert.ToBase64String(bytes);
        var mime = string.IsNullOrWhiteSpace(request.RadiologyImage.ContentType)
            ? "image/jpeg"
            : request.RadiologyImage.ContentType;

        // 2) Prompt (C# 10 compatible)
        var promptText = @$"
Tolong analisis temuan radiologi dari gambar berikut. Kembalikan JSON valid saja berisi:
1) temuan_utama
2) diagnosis_banding (dengan alasan)
3) pemeriksaan_lanjutan (termasuk radiologi jika relevan)
4) terapi_awal
5) alert_klinis

Info klinis: {request.ClinicalInfo ?? "(tidak ada)"}

WAJIB: Output harus JSON valid saja, tanpa markdown, tanpa ```.";

        // 3) Body untuk /v1/chat/completions (WAJIB messages)
        var requestBody = new
        {
            model = model,
            messages = new object[]
            {
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = promptText },
                    new
                    {
                        type = "image_url",
                        image_url = new { url = $"data:{mime};base64,{b64}" }
                    }
                }
            }
            },
            temperature = 0.2
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(httpRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, responseContent);

        // 4) Ambil output text
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

        // 5) Bersihkan jika model tetap kasih ```json
        string cleaned = contentString.Trim();

        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned.Substring(7);
        else if (cleaned.StartsWith("```"))
            cleaned = cleaned.Substring(3);

        if (cleaned.EndsWith("```"))
            cleaned = cleaned.Substring(0, cleaned.Length - 3);

        cleaned = cleaned.Trim();

        // 6) Parse JSON hasil
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

    public class RadiologyPromptRequest
    {
        public string? ClinicalInfo { get; set; }
        public IFormFile RadiologyImage { get; set; } // Optional
    }

}
