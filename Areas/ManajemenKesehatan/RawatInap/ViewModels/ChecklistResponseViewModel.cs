namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class ChecklistResponseViewModel
    {
        public Guid? ChecklistItemId { get; set; }
        public Guid? PraOperasiId { get; set; }

        public string? RoleAnswers { get; set; }
        public bool? ChecklistAnswers { get; set; }

        public Guid? AnswersId { get; set; }
        public string? Keterangan { get; set; }
    }
}
