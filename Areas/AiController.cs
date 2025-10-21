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
        string apiUrl = _configuration["AiSettings:BaseUrl"];
        string apiKey = _configuration["AiSettings:ApiKey"];

        using var formData = new MultipartFormDataContent();

        // 1️⃣ Gabungkan teks pesan
        string fullPrompt = $@"
        Tolong buatkan analisis medis lanjutan untuk data ini dalam format JSON yang terdiri dari:
        1. temuan_utama (deskripsi temuan radiologi utama),
        2. diagnosis_banding (dengan alasan),
        3. pemeriksaan_lanjutan (termasuk radiologi jika relevan),
        4. terapi_awal, dan
        5. alert_klinis (hal-hal yang perlu diwaspadai dari kondisi ini).

        Gunakan format JSON. Jangan gunakan markdown.

        Informasi klinis tambahan:
        {request.ClinicalInfo ?? "(tidak ada data tambahan)"}";

        formData.Add(new StringContent(fullPrompt), "message");

        // 2️⃣ Tambahkan file radiologi (jika ada)
        if (request.RadiologyImage != null && request.RadiologyImage.Length > 0)
        {
            var stream = request.RadiologyImage.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.RadiologyImage.ContentType);
            formData.Add(fileContent, "image", request.RadiologyImage.FileName);
        }

        // 3️⃣ Buat request HTTP ke Node.js API
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        httpRequest.Headers.Add("x-api-key", apiKey); // sesuai header di Postman kamu
        httpRequest.Content = formData;

        var response = await _httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, new { error = responseBody });
        }

        // 4️⃣ Kembalikan hasil JSON yang diterima dari Perplex City
        try
        {
            var json = JsonDocument.Parse(responseBody);
            if (json.RootElement.TryGetProperty("reply", out var replyProp))
            {
                string rawReply = replyProp.GetString();

                try
                {
                    // Jika reply berisi JSON valid, parse ulang agar tampil rapi
                    var innerJson = JsonDocument.Parse(rawReply);
                    return Ok(innerJson.RootElement);
                }
                catch
                {
                    // Kalau bukan JSON valid, tampilkan apa adanya
                    return Ok(new { reply = rawReply });
                }
            }

            return Ok(json.RootElement);

        }
        catch
        {
            // Jika balasan bukan JSON valid (misal plain text), kirim apa adanya
            return Ok(new { raw = responseBody });
        }
    }

    //[HttpPost("analyze-radiology")]
    //public async Task<IActionResult> AnalyzeRadiology([FromForm] RadiologyPromptRequest request)
    //{
    //    string apiUrl = _configuration["AiSettings:BaseUrl"];
    //    string apiKey = _configuration["AiSettings:ApiKey"];
    //    string model = _configuration["AiSettings:Model"];

    //    string imageDescription = "";

    //    if (request.RadiologyImage != null && request.RadiologyImage.Length > 0)
    //    {
    //        // Konversi gambar ke base64 (opsional, jika mau kirim ke LLM)
    //        using var ms = new MemoryStream();
    //        await request.RadiologyImage.CopyToAsync(ms);
    //        byte[] imageBytes = ms.ToArray();
    //        string base64Image = Convert.ToBase64String(imageBytes);

    //        imageDescription = "(Catatan: Gambar radiologi telah diunggah. Deskripsikan secara visual jika memungkinkan.)";

    //        // Bisa tambahkan call ke vision API jika AI yang kamu gunakan mendukung analisis gambar
    //        // Untuk GPT-4-vision, kamu perlu endpoint khusus yang bisa handle base64 image
    //    }

    //    // Prompt akhir gabungan teks klinis dan info gambar
    //    string fullPrompt = $@"
    //    Saya memiliki data klinis berikut: {request.ClinicalInfo}.
    //    {imageDescription}
    //    Tolong buatkan analisis medis lanjutan untuk data ini dalam format JSON yang terdiri dari:
    //    1. temuan_utama (deskripsi temuan radiologi utama),
    //    2. diagnosis_banding (dengan alasan),
    //    3. pemeriksaan_lanjutan (termasuk radiologi jika relevan),
    //    4. terapi_awal, dan
    //    5. alert_klinis (hal-hal yang perlu diwaspadai dari kondisi ini).

    //    Gunakan format JSON. Jangan gunakan markdown.";

    //    var requestBody = new
    //    {
    //        model = model,
    //        messages = new[]
    //        {
    //        new { role = "user", content = fullPrompt }
    //    }
    //    };

    //    var jsonContent = new StringContent(
    //        JsonSerializer.Serialize(requestBody),
    //        Encoding.UTF8,
    //        "application/json"
    //    );

    //    var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
    //    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    //    httpRequest.Content = jsonContent;

    //    var response = await _httpClient.SendAsync(httpRequest);
    //    var responseContent = await response.Content.ReadAsStringAsync();

    //    if (!response.IsSuccessStatusCode)
    //    {
    //        return StatusCode((int)response.StatusCode, responseContent);
    //    }

    //    var contentString = JsonDocument.Parse(responseContent)
    //                        .RootElement
    //                        .GetProperty("choices")[0]
    //                        .GetProperty("message")
    //                        .GetProperty("content")
    //                        .GetString();

    //    string cleaned = contentString.Trim();

    //    if (cleaned.StartsWith("```json"))
    //        cleaned = cleaned[7..];
    //    else if (cleaned.StartsWith("```"))
    //        cleaned = cleaned[3..];

    //    if (cleaned.EndsWith("```"))
    //        cleaned = cleaned[..^3];

    //    cleaned = cleaned.Trim();

    //    try
    //    {
    //        var json = JsonDocument.Parse(cleaned);
    //        return Ok(json.RootElement);
    //    }
    //    catch (JsonException ex)
    //    {
    //        return Ok(new { raw = cleaned, error = "Gagal parsing JSON", detail = ex.Message });
    //    }
    //}

    public class RadiologyPromptRequest
    {
        public string? ClinicalInfo { get; set; }
        public IFormFile RadiologyImage { get; set; } // Optional
    }

}
