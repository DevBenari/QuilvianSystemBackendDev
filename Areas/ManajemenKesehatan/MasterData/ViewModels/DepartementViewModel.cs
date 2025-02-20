namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class DepartementViewModel
    {
        public string NamaDepartement { get; set; }
        public string KepalaDepartement { get; set; }
        public string Lokasi { get; set; }
        public string Telepon { get; set; }
        public string Email { get; set; }
        public DateTime? JamBuka { get; set; }
        public DateTime? JamTutup { get; set; }
        public string Layanan { get; set; }
    }
}
