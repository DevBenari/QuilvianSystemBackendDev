using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{

    public class LabBookingBatal : UserActivity
    {
        [Key]
        public Guid BatalBookingLabId { get; set; }
        public Guid? LabBookingId { get; set; }
        public Guid? DetailLabBookingId { get; set; }
        public string? JenisPembatalan { get; set; } = string.Empty;
        public DateTime? TglPembatalan { get; set; }
        public string? Keterangan { get; set; }


        public LabBooking? LabBooking { get; set; }
        public LabBookingDetail? LabBookingDetail { get; set; }
    }
}
