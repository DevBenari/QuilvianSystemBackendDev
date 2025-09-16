using System;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class DetailKeahlianVM
    {
        public Guid? UserActiveId { get; set; }
        public Guid? KeahlianId { get; set; }
        public string? LevelKeahlian { get; set; }
        public Guid? Penilai { get; set; }
    }
}
