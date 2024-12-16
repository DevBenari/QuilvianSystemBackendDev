using System.ComponentModel.DataAnnotations;

namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class MultipleDepartmentViewModel
    {
        public Guid  DoctorId { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTimeOffset CreateDateTime { get; set; }
        public string KodeDokter { get; set; }
        public string NamaDokter { get; set; }
        public string TipeDokter { get; set; }
        public string DepartemenDokter { get; set; }
        public string Foto { get; set; }
    }
}
