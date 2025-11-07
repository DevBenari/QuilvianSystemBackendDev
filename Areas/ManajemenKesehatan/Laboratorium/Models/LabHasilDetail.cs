using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabHasilDetail : UserActivity
    {
        [Key]
        public Guid DetailHasilLabId { get; set; }
        public Guid? HasilLabId { get; set; }
        public Guid? PemeriksaanLabId { get; set; }
        public Guid? KelasId { get; set; }
        public  DateTime? TanggalSelesai { get; set; }
        public string? NoPhotoLab { get; set; }
        public string? PhotoLabPath { get; set; }
        public string? HasilLabManual { get; set; }
        public string? HasilLabAI {  get; set; }
        public string? JumlahFilm {  get; set; }
        public string? Keterangan {  get; set; }
    }
}
