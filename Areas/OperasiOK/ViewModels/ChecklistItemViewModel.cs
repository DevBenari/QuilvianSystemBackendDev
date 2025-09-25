namespace QuilvianSystemBackendDev.Areas.Operasi.ViewModels
{
    public class ChecklistItemViewModel
    {
        public Guid? ChecklistTemplateId { get; set; }
        public decimal? UrutanChecklistItem { get; set; }
        public string? KodeChecklistItem { get; set; }
        public string? KodeChecklistItemName { get; set; }
        public string? Keterangan { get; set; }
    }
}
