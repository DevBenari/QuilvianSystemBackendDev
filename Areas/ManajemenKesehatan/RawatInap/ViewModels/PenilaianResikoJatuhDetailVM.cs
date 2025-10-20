namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class PenilaianResikoJatuhDetailVM
    {
        public Guid? IndikatorPengkajianId { get; set; }
        public Guid? IntervensiResikoJatuhId { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsIntervensiChecklist { get; set; }
    }
}
