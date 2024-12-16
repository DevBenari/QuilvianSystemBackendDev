using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.HealthManagement.Models
{
    [Table("HmnScheduleToday", Schema = "dbo")]
    public class ScheduleToday : UserActivity
    {
        [Key]
        public Guid ScheduleTodayId { get; set; }
        public string KodeJadwal { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DayId { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime TanggalPraktek { get; set; } = DateTime.Now;
        public string JamMulai { get; set; }
        public string JamSelesai { get; set; }
        public string LamaPeriksaPerPasien { get; set; }
        public string PagiSore { get; set; }
        public string Ruangan { get; set; }

        //Relationship
        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }
        [ForeignKey("DepartmentId")]
        public DoctorDepartment? DoctorDepartment { get; set; }
        [ForeignKey("DayId")]
        public Day? Day { get; set; }

    }
}
