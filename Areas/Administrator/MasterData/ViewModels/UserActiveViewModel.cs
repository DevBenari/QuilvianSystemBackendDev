namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.ViewModels
{
    public class UserActiveViewModel
    {
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PlaceOfBirth { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string Handphone { get; set; }
        public string Email { get; set; }
        public Guid? DepartemenId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? TipeUserId { get; set; }
        public Guid? InstalasiUnitId {  get; set; }
        // untuk foto
        //public string? FotoName { get; set; }
        //public string? FotoPath { get; set; }
        //// informasi tambahan untuk data dokter
        //public string? Sip { get; set; }
        //public string? Str { get; set; }
        //public string? TglSip { get;set; }
        //public string? TglStr { get; set; }
        //public string? Spesialis { get; set; }
        //public bool? IsAsuransi { get; set; }

        public string? NoSTR { get; set; }
        public string? StatusPegawai { get; set; }
    }
}
