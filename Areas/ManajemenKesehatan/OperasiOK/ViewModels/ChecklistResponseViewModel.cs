namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels
{
    public class ChecklistResponseViewModel
    {
        public Guid? ChecklistItemId { get; set; }
        public Guid? PraOperasiId { get; set; }

        public bool? RoleAnswers { get; set; }
        public bool? ChecklistAnswers { get; set; }

        public Guid? AnswersId { get; set; }
        public string? Keterangan { get; set; }
    }
}
