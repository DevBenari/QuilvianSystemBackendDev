using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.ViewModels
{
    public class UserActiveViewModel
    {
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PlaceOfBirth { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string Handphone { get; set; }
        public string Email { get; set; }
        public Guid? DepartemenId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? TipeUserId { get; set; }
        public Guid? InstalasiUnitId {  get; set; }
        // untuk foto
        //public string? FotoName { get; set; }
        //public string? FotoPath { get; set; }
        //// informasi tambahan untuk data dokter
        //public string? Sip { get; set; }
        //public string? Str { get; set; }
        //public string? TglSip { get;set; }
        //public string? TglStr { get; set; }
        //public string? Spesialis { get; set; }
        //public bool? IsAsuransi { get; set; }

        public string? NoSTR { get; set; }
        public string? StatusPegawai { get; set; }

        public Guid UserActiveId { get; set; }
        public string UserActiveCode { get; set; }
        [MaxLength(64)]
        public string? PinPegawai { get; set; }


        //karyawan
        //public Guid KaryawanId { get; set; }
        public Guid? JabatanId { get; set; }

        public string? NoIdentitas { get; set; }
        public string? KodeKaryawan { get; set; }
        public string? NoRekening { get; set; }
        public string? NoKaryawan { get; set; }
        public string? BankId { get; set; }

        public DateTime? TanggalKontrak { get; set; }
        public DateTime? TanggalAwalKerja { get; set; }
        public DateTime? TanggalAkhirKerja { get; set; }

        public string? NoHandphone { get; set; }
        public string? Alamat { get; set; }
        public string? FotoPath { get; set; }
        public string? FotoName { get; set; }

        public bool? IsKaryawanMedis { get; set; } = false;
        public string? StatusPerkawinan { get; set; }

        public Guid? AgamaId { get; set; }

        public Guid? PendidikanTerakhirId { get; set; }

        // Alamat tinggal sekarang
        public string? AlamatDomisili { get; set; }

        public Guid? ProvinsiId { get; set; }

        public Guid? KotaId { get; set; }

        public Guid? KecId { get; set; }

        public Guid? KewarganegaraanId { get; set; }

        public string? StatusKewarganegaraan { get; set; }
        //karyawan

    }
}
