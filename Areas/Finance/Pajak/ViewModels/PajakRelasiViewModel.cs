using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Finance.Pajak.ViewModels
{
    public class PajakRelasiViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "PajakId wajib diisi.")]
        public Guid PajakId { get; set; }

        [Required(ErrorMessage = "Jenis relasi wajib diisi.")]
        [MaxLength(30)]
        public string JenisRelasi { get; set; } = string.Empty;

        [Required(ErrorMessage = "RelasiId wajib diisi.")]
        public Guid RelasiId { get; set; }

        [MaxLength(50)]
        public string? JenisTransaksi { get; set; }

        public DateOnly TanggalMulai { get; set; }
        public DateOnly? TanggalBerakhir { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Keterangan { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PajakId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "PajakId tidak boleh Guid kosong.",
                    new[] { nameof(PajakId) });
            }

            if (RelasiId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "RelasiId tidak boleh Guid kosong.",
                    new[] { nameof(RelasiId) });
            }

            var allowedRelations = new[]
            {
                "KARYAWAN",
                "DOKTER",
                "PERUSAHAAN",
                "VENDOR",
                "ASURANSI",
                "BPJS"
            };

            var normalizedRelation = JenisRelasi?.Trim().ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(normalizedRelation) &&
                !allowedRelations.Contains(normalizedRelation))
            {
                yield return new ValidationResult(
                    "JenisRelasi harus KARYAWAN, DOKTER, PERUSAHAAN, VENDOR, ASURANSI, atau BPJS.",
                    new[] { nameof(JenisRelasi) });
            }

            if (TanggalMulai == default)
            {
                yield return new ValidationResult(
                    "Tanggal mulai wajib diisi.",
                    new[] { nameof(TanggalMulai) });
            }

            if (TanggalBerakhir.HasValue && TanggalBerakhir.Value < TanggalMulai)
            {
                yield return new ValidationResult(
                    "Tanggal berakhir tidak boleh lebih kecil dari tanggal mulai.",
                    new[] { nameof(TanggalBerakhir) });
            }
        }
    }
}
