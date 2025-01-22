using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystem.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystem.Models;

namespace QuilvianSystem.Repositories
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Pegawai> UserActives { get; set; }

        #region Areas Master
        public DbSet<Dokter> Dokters { get; set; }
        public DbSet<DokterPraktek> DokterPrakteks { get; set; }
        public DbSet<Provinsi> Provinsis { get; set; }
        public DbSet<Kabupaten> Kabupatens { get; set; }
        public DbSet<Kecamatan> Kecamatans { get; set; }
        public DbSet<Kelurahan> Kelurahans { get; set; }
        public DbSet<Asuransi> Asuransis { get; set; }
        public DbSet<Keangotaan> Keangotaans { get; set; }
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
