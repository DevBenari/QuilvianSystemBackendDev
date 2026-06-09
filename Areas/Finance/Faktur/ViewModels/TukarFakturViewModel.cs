namespace QuilvianSystemBackendDev.Areas.Finance.Faktur.ViewModels
{
    public class TukarFakturViewModel
    {
        public Guid SupplierId { get; set; }

        public DateTime TglRegistrasi { get; set; }
        public DateTime? TglTerimaFaktur { get; set; }
        public DateTime? TglJatuhTempo { get; set; }

        public string Keterangan { get; set; }

        public List<DetailTukarFakturViewModel>? Details { get; set; }
    }
}
