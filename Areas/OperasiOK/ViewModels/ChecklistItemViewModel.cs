namespace QuilvianSystemBackendDev.Areas.Operasi.ViewModels
{
    public class ChecklistItemViewModel
    {
        public Guid? ChecklistTemplateId { get; set; }
        public string? KodeChecklistItem { get; set; }
        public string? NamaChecklistItem { get; set; }
        public string? Keterangan { get; set; }
    }
}
