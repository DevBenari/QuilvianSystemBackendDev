using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models
{
    [Table("MstKunjungan", Schema = "public")]
    public class Kunjungan : UserActivity
    {
        [Key]
        public Guid KunjunganID { get; set; }
        public Guid? AsuransiId { get; set; }
        public Guid? AsuransiExcessId { get; set; }
        public Guid? AsuransiPasienExcessId { get; set; }
        public Guid? AsuransiPasienId { get; set; }
        public Guid? PoliklinikId { get; set; }
        public Guid? DokterId { get; set; }
        public Guid? PasienId { get; set; }
        public string NoRekamMedis { get; set; }
        public string? TipePasien { get; set; }
        public string? NoRegistrasi { get; set; }
        public string? TipePembayaran { get; set; }
        public bool? IsFinished { get; set; } = false;
        public string JenisKunjungan { get; set; }
        public string? DokterPerujuk { get; set; }
        public string? RSPerujuk { get; set; }
        public string? Antrian { get; set; }
        public bool? IsScreening { get; set; }
        public bool? IsPresent { get; set; }
        public bool? IsFinishedKasir { get; set; }
        public DateTime? TglFinishedKasir { get; set; }
        public string? StatusPengkajian { get; set; }
        public string? AsalKunjungan { get; set; }
        public string? CaraMasukRS { get; set; }
        public string? KondisiKeluar {  get; set; }
        public bool? IsTriage {  get; set; }
        public bool? IsCTTPasienIGD { get; set; }
        public Guid? KelasId { get; set; }
        public bool? IsClosed { get; set; }
        public bool? KunjunganLab { get; set; }
        public string? KategoriPendaftaran { get; set; }
        public string? NoHandphone { get; set; }
        public string? Email { get; set; }

        // Navigation
        public Poliklinik? Poliklinik { get; set; }
        public Dokter? Dokter { get; set; }
        public PendaftaranPasienBaru? Pasien { get; set; }
        public Kelas? Kelas { get; set; }
        public Asuransi? Asuransi { get; set; }
        public Asuransi? AsuransiExcess { get; set; }
        public AsuransiPasien? AsuransiPasien { get; set; }
        public AsuransiPasien? AsuransiPasienExcess { get; set; }

        public ICollection<AlatPemakaian> AlatPemakaians { get; set; } = new List<AlatPemakaian>();
        public ICollection<LogRacikPenerimaan> LogRacikPenerimaans { get; set; } = new HashSet<LogRacikPenerimaan>();

        public ICollection<Billing> Billings { get; set; } = new List<Billing>();

        public ICollection<KunjunganLayanan> KunjunganLayanans { get; set; } = new List<KunjunganLayanan>();
    }

}
