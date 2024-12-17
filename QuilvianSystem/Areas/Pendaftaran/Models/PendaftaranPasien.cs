
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Models
{
    [Table("PdfPasienBaru", Schema = "dbo")]
    public class PendaftaranPasien : UserActivity
    {
        [Key]
        public Guid PendaftaranPasienId { get; set; }
        public string Penjamin { get; set; }
        public string Title { get; set; }
        public string NoRekamMedis { get; set; }
        public string NamaLengkap { get; set; }
        public string NoIdentitas { get; set; }
        public string TempatLahir { get; set; }
        public DateTime TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string Agama { get; set; }
        public string Suku { get; set; }
        public string Kewarganegaraan { get; set; }
        public string PendidikanTerakhir { get; set; }
        public string AlamatDomisili { get; set; }
        public string InformasiAlamat { get; set; }
        public string Kelurahan { get; set; }
        public string Kecamatan { get; set; }
        public string NoTeleponRumah { get; set; }
        public string Email { get; set; }
        public string Pekerjaan { get; set; }
        public string NamaKantor { get; set; }
        public string AlamatKantor { get; set; }
        public string NomorTeleponKantor { get; set; }
        public string GolonganDarah { get; set; }
        public string Alergi { get; set; }
        public string NomorKeluargaTerdekat { get; set; }
        public string HubunganKeluarga { get; set; }
        public string DataKaryawanInput { get; set; }
        public string AlamatKeluarga { get; set; }
        public string KelurahanKeluarga { get; set; }
        public string KabupatenKeluarga { get; set; }
        public string NomorTeleponKeluarga { get; set; }
        public string NamaAyah { get; set; }
        public string NamaIbu { get; set; }
        public string NamaSutri { get; set; }
        public string NomorKtpSutri { get; set; }
        public string Foto { get; set; }

    }

}
