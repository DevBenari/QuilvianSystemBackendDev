namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.ViewModels
{
    public class FingerprintRegisterViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty; // base64 dari SDK
    }

    public class FingerprintVerifyViewModel
    {
        public string DeviceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty; // base64 dari SDK
    }
}
