namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class AssesmentEdukasiViewModel
    {
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public string? BahasaDigunakan { get; set; }
        public bool? IsKebutuhanPenerjemah { get; set; }
        public bool? IsBacaTulis { get; set; }
        public string? TipePembelajaran { get; set; }
        public string? NilaiKepercayaan { get; set; }
        public Guid? PendidikanId { get; set; }
        public string? HambatanEdukasi { get; set; }
        public bool? IsMenerimaEdukasi { get; set; }
        public string? KebutuhanEdukasi { get; set; }
        public string? Keterangan { get; set; }
    }
}
