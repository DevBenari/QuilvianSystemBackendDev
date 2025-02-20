namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class PeralatanViewModel
    {
        public string NamaPeralatan { get; set; }
        public string Manufacturer { get; set; }
        public string Purchase_date { get; set; }
        public string Maintenance_status { get; set; }
        public string Operational_status { get; set; }
        public string Department_name { get; set; }
        public string Location { get; set; }

        public Guid KategoriPeralatanId { get; set; }
    }
}
