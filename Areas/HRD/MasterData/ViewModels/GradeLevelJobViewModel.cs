namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class GradeLevelJobViewModel
    {
        public Guid GradeLevelJobId { get; set; }
        //public Guid GradeLevelId { get; set; } // Tidak digunakan
        public Guid PositionId { get; set; }
        public Guid GradeId { get; set; }
        public Guid LevelId { get; set; }
        public string? Keterangan { get; set; }
    }
}
