using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("MstKunjunganLayanan", Schema = "public")]
    public class KunjunganLayanan : UserActivity
    {
        [Key]
        public Guid KunjunganLayananId { get; set; }

        public Guid? KunjunganId { get; set; }

        // Unit besar: Rajal, Ranap, IGD, ICU, OK, dll
        public Guid? InstalasiUnitId { get; set; }

        // Kalau rawat jalan, bisa isi PoliklinikId
        public Guid? PoliklinikId { get; set; }

        // Kalau rawat inap, bisa isi RanapId / RuanganId / BedId sesuai model Anda

        public Guid? DokterId { get; set; }

        public string? JenisLayanan { get; set; }
        // contoh: RAJAL, RANAP, IGD, ICU, OK

        public DateTime? TglMasukLayanan { get; set; }

        public DateTime? TglKeluarLayanan { get; set; }

        public bool? IsActive { get; set; } = true;

        [ForeignKey(nameof(KunjunganId))]
        public Kunjungan? Kunjungan { get; set; }

        [ForeignKey(nameof(InstalasiUnitId))]
        public InstalasiUnit? InstalasiUnit { get; set; }

        [ForeignKey(nameof(PoliklinikId))]
        public Poliklinik? Poliklinik { get; set; }

        [ForeignKey(nameof(DokterId))]
        public Dokter? Dokter { get; set; }
    }
}
