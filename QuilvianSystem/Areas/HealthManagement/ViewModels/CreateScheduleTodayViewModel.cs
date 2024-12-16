using System.ComponentModel.DataAnnotations;

namespace QuilvianSystem.Areas.HealthManagement.ViewModels
{
    public class CreateScheduleTodayViewModel
    {
        public Guid ScheduleTodayId { get; set; }
        public string KodeJadwal { get; set; }
        public Guid? DoctorId { get; set; }
        public string? Doctor { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DoctorDepartment { get; set; }
        public Guid? DayId { get; set; }
        public string? Day { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime TanggalPraktek { get; set; } = DateTime.Now;
        public string JamMulai { get; set; }
        public string JamSelesai { get; set; }
        public string LamaPeriksaPerPasien { get; set; }
        public string PagiSore { get; set; }
        public string Ruangan { get; set; }
    }
}
