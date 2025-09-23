namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class CurrentMedicationViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PendaftaranPasienBaruId { get; set; }
        public string? NoRekamMedis { get; set; }
        public string? NamaObat { get; set; }
        public string? Dosis { get; set; }
        public string? Frekuensi { get; set; }
        public string? LamaKonsumsi { get; set; }
        public string? Status { get; set; }

    }
}
