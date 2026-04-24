using System;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class HasilTestVM
    {
        public string? NamaPeserta { get; set; }
        public decimal? NomorPeserta { get; set; }
        public DateTimeOffset? TglTest { get; set; }
        public string? HasilTest { get; set; }
    }
}
