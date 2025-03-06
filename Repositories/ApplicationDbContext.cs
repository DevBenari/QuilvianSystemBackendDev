using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Repositories
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=160.20.104.98;Port=5432;Database=QuilvianSystemBackendDev;Username=sa;Password=Admin@1234;");
            }
        }

        public DbSet<UserActive> UserActives { get; set; }

        #region Areas Master
        public DbSet<Agama> Agamas { get; set; }
        public DbSet<GolonganDarah> GolonganDarahs { get; set; }
        public DbSet<Pendidikan> Pendidikans { get; set; }
        public DbSet<Pekerjaan> Pekerjaans { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<Dokter> Dokters { get; set; }
        public DbSet<Provinsi> Provinsis { get; set; }
        public DbSet<KabupatenKota> KabupatenKotas { get; set; }
        public DbSet<Kecamatan> Kecamatans { get; set; }
        public DbSet<Kelurahan> Kelurahans { get; set; }
        public DbSet<Asuransi> Asuransis { get; set; }
        public DbSet<Keanggotaan> Keanggotaans { get; set; }
        public DbSet<Negara> Negaras { get; set; }
        public DbSet<RolePosition> RolePositions { get; set; }
        public DbSet<RoleUser> RoleUsers { get; set; }
        public DbSet<Jabatan> Jabatans { get; set; }
        public DbSet<Identitas> Identitass { get; set; }
        public DbSet<Peralatan> Peralatans { get; set; }
        public DbSet<KategoriPeralatan> KategoriPeralatans { get; set; }
        public DbSet<Departement> Departements { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Poliklinik> Polikliniks { get; set; }
        public DbSet<Persalinan> Persalinans { get; set; }
        public DbSet<SubPoli> SubPolis { get; set; }
        public DbSet<JadwalPraktek> JadwalPrakteks { get; set; }
        public DbSet<DokterPoli> DokterPolis { get; set; }
        public DbSet<CoveranAsuransi> CoveranAsuransis { get; set; }
        public DbSet<Operasi> Operasis { get; set; }
        public DbSet<DokterPraktek> DokterPrakteks { get; set; }

        #endregion

        #region Areas Pendaftaran
        public DbSet<PendaftaranPasienBaru> PendaftaranPasienBarus { get; set; }
        public DbSet<PendaftaranPasien> PendaftaranPasiens { get; set; }
        #endregion

        #region Areas Tindakan
        //public DbSet<TindakanPasienAmbulan> TindakanPasienAmbulans { get; set; }
        //public DbSet<TindakanPasienFasilitas> TindakanPasienFasilitass { get; set; }
        ////public DbSet<TindakanPasienGizi> TindakanPasienGizis { get; set; }
        //public DbSet<TindakanPasienLaboratorium> TindakanPasienLaboratoriums { get; set; }
        //public DbSet<TindakanPasienMcu> TindakanPasienMcus { get; set; }
        //public DbSet<TindakanPasienOptik> TindakanPasienOptiks { get; set; }
        //public DbSet<TindakanPasienRadiologi> TindakanPasienRadiologis { get; set; }
        //public DbSet<TindakanPasienRehabilitas> TindakanPasienRehabilitass { get; set; }
        #endregion        
    }
}
