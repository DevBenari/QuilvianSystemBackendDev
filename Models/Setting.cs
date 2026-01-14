using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Models
{
    [Table("Setting", Schema = "public")]
    public class Setting : UserActivity
    {
        public Guid SettingId { get; set; }
        public string BaseUrlAi { get; set; } = string.Empty;
        public string ApiKeyAi { get; set; } = string.Empty;
        public string ModelAi { get; set; } = string.Empty;
        public bool StatusAi { get; set; } = false;
    }
}
