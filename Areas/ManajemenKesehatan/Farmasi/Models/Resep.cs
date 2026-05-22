using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models
{
    [Table("MstResep", Schema = "public")]
    public class Resep : UserActivity
    {
        [Key]
        public Guid ResepId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? AsuransiId { get; set; }
        public string? NamaAsuransi { get; set; }
        public Guid? PasienId { get; set; }
        public string? NamaPasien { get; set; }
        public Guid? PoliklinikId { get; set; }
        public string? NamaPoliklinik { get; set; }
        public Guid? DokterId { get; set; }
        // Tambahan
        public Guid? KunjunganLayananId { get; set; }
        public Guid? InstalasiUnitId { get; set; }
        public string? JenisLayanan { get; set; }
        public string? NamaDokter { get; set; }
        public int? AntrianResep { get; set; }
        public string? AntrianRegistrasi { get; set; }
        public string? StatusPembuatanResep { get; set; }
        public bool? StatusPengambilanResep { get; set; } = false;
        public bool? IsCancelled { get; set; } = false;
        public bool? IsLunas { get; set; }
        public DateTime? TanggalPembuatanResep { get; set; }
        public Guid? RanapId { get; set; }
        public bool? IsResepPulang { get; set; }
        public bool? IsVerifyByDoctor { get; set; }
        public Guid? PetugasFarmasiId { get; set; }
        public string? PathTTDPetugasFarmasi{ get; set; }
        public string? PathTTDDokter { get; set; }

        // navigation
        public Kunjungan? Kunjungan { get; set; }
        public PendaftaranPasienBaru? Pasien { get; set; }
        public Poliklinik? Poliklinik { get; set; }
        public Dokter? Dokter { get; set; }
        public Asuransi? Asuransi { get; set; }
        public UserActive? PetugasFarmasi { get; set; }

        public ICollection<LogRacikPenerimaan> LogRacikPenerimaans { get; set; } = new HashSet<LogRacikPenerimaan>();
        public ICollection<Racikan> Racikans { get; set; } = new HashSet<Racikan>();
        public ICollection<ResepDetail> ResepDetails { get; set; } = new HashSet<ResepDetail>();



    }
}
