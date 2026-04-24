using System;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class DokumenDetailKaryawanVM
    {
        public Guid? UserActiveId { get; set; }
        public string? NamaPeserta { get; set; }
        public string? NoPeserta { get; set; }
        public DateTimeOffset? TglUpload { get; set; }
        public string? NamaDokumen { get; set; }
        public string? FilePath { get; set; }
        public string? StatusKepemilikan { get; set; }
    }
}
