namespace QuilvianSystemBackendDev.DTO
{
    public class WhatsAppResultDto
    {
        public bool Success { get; set; }
        public int? StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
