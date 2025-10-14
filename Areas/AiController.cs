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

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
            new { role = "user", content = request.Prompt }
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

        // Ambil string isi pesan dari GPT
        var contentString = JsonDocument.Parse(responseContent)
                            .RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

        // Bersihkan jika ada ```json ... ```
        string cleaned = contentString.Trim();

        if (cleaned.StartsWith("```json"))
        {
            cleaned = cleaned.Substring(7); // Hapus ```json
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(3); // Hapus ```
        }

        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }

        // Trim dan parse JSON hasilnya
        cleaned = cleaned.Trim();

        try
        {
            var json = JsonDocument.Parse(cleaned);
            return Ok(json.RootElement); // return as raw JSON
        }
        catch (JsonException ex)
        {
            // Jika parsing gagal, kirim raw content
            return Ok(new { raw = cleaned, error = "Gagal parsing JSON", detail = ex.Message });
        }
    }


    public class PromptRequest
    {
        public string Prompt { get; set; }
    }
}
