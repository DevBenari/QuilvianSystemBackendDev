using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.OperasiOK.Models
{
    public class PraOperasi
    {
        [Key]
        public Guid PraOperasiId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PasienId { get; set; }
        public Guid? PainAssessmentId { get; set; }
        public Guid? VitalSignId { get; set; }

        public string? StatusMental { get; set; }
        public string? PengobatanSaatIni { get; set; }
        public string? AlatBantu { get; set; }
        public string? JenisOperasi { get; set; }
        public string? WaktuOperasi { get; set; }
        public string? TempatOperasi { get; set; }
        public string? HasilLab { get; set; }

        public bool IsBatukFluDemam { get; set; }
        public bool IsHaid { get; set; }

        public string? ProsedurOperasi { get; set; }
        public DateTime? TanggalOperasi { get; set; }

        public Guid? PerawatBedahId { get; set; }
        public Guid? PerawatRuanganId { get; set; }
        public Guid? DokterId { get; set; }

        public string? Keterangan { get; set; }

        // Relasi ke tabel TTD
        public Guid? TTDPerawatRuanganId { get; set; }
        public Guid? TTDPerawatBedahId { get; set; }
        public Guid? TTDDokterId { get; set; }
        public Guid? TTDPerawatPrimerId { get; set; }

        // Path file tanda tangan
        public string? TTDPerawatRuanganPath { get; set; }
        public string? TTDPerawatBedahPath { get; set; }
        public string? TTDDokterPath { get; set; }
        public string? TTDPerawatPrimerPath { get; set; }
        public string? TTDKeluarga { get; set; }

        //// File upload tanda tangan
        //public IFormFile? FileTTDPerawatRuangan { get; set; }
        //public IFormFile? FileTTDPerawatBedah { get; set; }
        //public IFormFile? FileTTDDokter { get; set; }
        //public IFormFile? FileTTDPerawatPrimer { get; set; }
        //public IFormFile? FileTTDKeluarga { get; set; }

        //// Penandaan Operasi
        //public IFormFile? PenandaanOperasiBag1 { get; set; }
        //public IFormFile? PenandaanOperasiBag2 { get; set; }

        // Tanggal-tanggal penting
        public DateTime? TglCatatan { get; set; }
        public DateTime? TglPernyataanPasien { get; set; }
        public DateTime? TglPernyataanDokter { get; set; }
    }
}
