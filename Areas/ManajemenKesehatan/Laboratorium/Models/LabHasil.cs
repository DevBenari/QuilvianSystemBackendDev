using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabHasil : UserActivity
    {
        [Key]
        public Guid HasilLabId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? LabId { get; set; }
        public Guid? LabBookingId { get; set; }
        public Guid? DokterPerujukId { get; set; }
        public Guid? DokterKonfirmatorId { get; set; }
        public string? NoPhoneKonfirmator {  get; set; }
        public bool? IsKonfirmatorDPJP {  get; set; }
        public List<Guid>? UserActiveId { get; set; } = new List<Guid>();
        public Guid? PenanggungJawabId { get; set; }
        public Guid? PenanggungJawabAnalisId { get; set; }
        public DateTime? TanggalPemeriksaan {  get; set; }
        public string? Keterangan {  get; set; }

        // navigatiom
        public Kunjungan? Kunjungan { get; set; }
        public Lab? Lab { get; set; }
        public LabBooking? LabBooking { get; set; }
        public Dokter? DokterPerujuk { get; set; }
        public Dokter? DokterKonfirmator { get; set; }
        public UserActive? PenanggungJawab { get; set; }
        public UserActive? PenanggungJawabAnalis {  get; set; }

    }
}
