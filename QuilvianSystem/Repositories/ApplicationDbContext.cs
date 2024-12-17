using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystem.Areas.AccountingAndFinancial.Models;
using QuilvianSystem.Areas.MasterData.Models;
using QuilvianSystem.Models;

namespace QuilvianSystem.Repositories
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<UserActive> UserActives { get; set; }

        #region Areas Pendaftaran
        public DbSet<PendaftaranPasien> PendaftaranPasiens { get; set; }
        #endregion

    }
}
