namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels
{
    public class PendaftaranPasienBaruViewModel
    {

        public Guid PendaftaranPasienBaruId { get; set; }

        // Informasi Penjamin dan Identitas
        public string NoPenjamin { get; set; }
        public string Title { get; set; }
        public string NoRekamMedis { get; set; }
        public string NamaLengkap { get; set; }
        public string NoIdentitas { get; set; } // KTP atau Passport

        // Tempat dan Tanggal Lahir
        public string TempatLahir { get; set; }
        public DateTime TanggalLahir { get; set; }

        // Informasi Pribadi
        public string JenisKelamin { get; set; }
        public string Agama { get; set; }
        public string Suku { get; set; }
        public string Kewarganegaraan { get; set; }
        public string PendidikanTerakhir { get; set; }

        // Informasi Alamat
        public string AlamatDomisili { get; set; }
        public string InformasiAlamat { get; set; }
        public string Kelurahan { get; set; }
        public string Kecamatan { get; set; }

        // Nomor Telepon
        public string NomorHP { get; set; } // Nomor telepon pasien
        public string Email { get; set; }

        // Informasi Pekerjaan
        public string Pekerjaan { get; set; }
        public string NamaKantor { get; set; }
        public string AlamatKantor { get; set; }
        public string NomorTeleponKantor { get; set; }

        // Informasi Medis
        public string GolonganDarah { get; set; }
        public string Alergi { get; set; }

        // Informasi Keluarga
        public string NomorKeluargaTerdekat { get; set; }
        public string HubunganKeluarga { get; set; }
        public string AlamatKeluarga { get; set; }
        public string KelurahanKeluarga { get; set; }
        public string KabupatenKeluarga { get; set; }
        public string NomorTeleponKeluarga { get; set; }
        public string NamaAyah { get; set; }
        public string NamaIbu { get; set; }
        public string NamaSutri { get; set; }
        public string NomorKtpSutri { get; set; }

        // Informasi Lain
        public string DataKaryawanInput { get; set; } // Data karyawan rumah sakit yang input
        public string Foto { get; set; } // Path atau URL foto
        public string QrCode { get; set; }
    }
}
