using Microsoft.AspNetCore.Mvc;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels
{
    public class PraOperasiViewModel
    {
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
        public string? TanggalOperasi { get; set; }

        public Guid? PerawatBedahId { get; set; }

        public string? Keterangan { get; set; }

        // Relasi ke tabel TTD
        public Guid? TTDPerawatRuanganId { get; set; } //ke-1

        //// Penandaan Operasi
        //public string? PenandaanOperasiBag1 { get; set; }
        //public string? PenandaanOperasiBag2 { get; set; }

        // Tanggal-tanggal penting
        public string? TglCatatan { get; set; }

        //// File upload tanda tangan
        //public IFormFile? FileTTDPerawatRuangan { get; set; }
        [FromForm]
        public IFormFile? FilePenandaanOperasiBag1 { get; set; }

        [FromForm]
        public IFormFile? FilePenandaanOperasiBag2 { get; set; }
    }
}
