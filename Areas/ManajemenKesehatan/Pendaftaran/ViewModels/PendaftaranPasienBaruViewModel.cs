using Microsoft.Data.SqlClient.Server;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels
{
    public class PendaftaranPasienBaruViewModel
    {        
        // Informasi Pasien
        public string? TipePasien { get; set; }
        public string? TipePendaftaran { get; set; } // Rawat Jalan, Rawat Inap, Darurat, dll.
        public Guid? TitleId { get; set; }
        public string NamaLengkap { get; set; }
        public Guid IdentitasId { get; set; }
        public string NoIdentitas { get; set; } // KTP atau Passport
        public string? TempatLahir { get; set; }
        public string? TanggalLahir { get; set; }
        public string? JenisKelamin { get; set; }
        public string? StatusPerkawinan { get; set; }
        public Guid? AgamaId { get; set; }
        public Guid? PendidikanTerakhirId { get; set; }

        // Informasi Alamat
        public string? AlamatIdentitas { get; set; }
        public string? AlamatDomisili { get; set; }
        public Guid? NegaraId { get; set; }
        public Guid? ProvinsiId { get; set; }
        public Guid? KotaId { get; set; }
        public Guid? KecKabId { get; set; }
        public Guid? KelurahanId { get; set; }
        public string? KodePos { get; set; }
        public string? Email { get; set; }
        public string? NoPasien { get; set; }
        public string? NoWali2 { get; set; }
        public string? NoWali3 { get; set; }

        // Informasi Grafis
        public string? Kewarganegaraan { get; set; }
        public string? Suku { get; set; }
        public string? StatusKewarganegaraan { get; set; }

        // Informasi Pekerjaan
        public Guid? PekerjaanId { get; set; }
        public string? NamaPerusahaan { get; set; }
        public string? AlamatPerusahaan { get; set; }
        public string? NoTeleponPerusahaan { get; set; }

        // Informasi Kesehatan
        public Guid? GolonganDarahId { get; set; }
        public string? Alergi { get; set; }
        public string? RiwayatPenyakit { get; set; }
        public string? RiwayatOperasi { get; set; }
        public string? RiwayatPenyakitKeluarga { get; set; }

        // Informasi Darurat
        public string? NamaWali2 { get; set; }
        public string? NamaWali3 { get; set; }
        public string? HubunganKeluarga1 { get; set; }
        public string? NoIdentitasDarurat { get; set; }
        public string? AlamatDarurat { get; set; }
        public string? NoTeleponDarurat { get; set; }

        // Informasi Pasien Anak
        public string? NamaOrangTua { get; set; } // Data karyawan rumah sakit yang input
        public string? IdentitasOrangTua { get; set; }
        public string? PekerjaanOrangTua { get; set; }
        public string? HubunganKeluarga2 { get; set; }
        public string? HubunganKeluarga3 { get; set; } // Hubungan dengan pasien, misal: Ayah, Ibu, Kakek, Nenek, dll.

        //// Informasi Membership
        public Guid? MembershipId { get; set; }

        public IFormFile? Foto { get; set; }

        //public IFormFile? QR { get; set; } = null;
        //public string? QrCode { get; set; }
        //public string? FotoName { get; set; }
        //public string? FotoPath { get; set; }
        //public List<byte>? FotoByte { get; set; }
        //public byte[]? ImageBytes { get; set; }

    }
}
