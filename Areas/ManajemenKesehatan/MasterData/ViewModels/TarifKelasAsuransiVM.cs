namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class TarifKelasAsuransiVM
    {
        public Guid? AsuransiId { get; set; }
        public Guid? TarifKelasId { get; set; }
        public DateTime? TanggalPemakaian { get; set; }
        public DateTime? Keterangan { get; set; }
    }
}
