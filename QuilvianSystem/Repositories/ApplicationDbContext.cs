using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystem.Areas.AccountingAndFinancial.Models;
using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Areas.HealthManagement.Models;
using QuilvianSystem.Areas.MasterData.Models;
using QuilvianSystem.Areas.PatientRegistration.Models;
using QuilvianSystem.Models;

namespace QuilvianSystem.Repositories
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<UserActive> UserActives { get; set; }


        #region Areas Administration
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<SubDistrict> SubDistricts { get; set; }
        public DbSet<LastEducation> LastEducations { get; set; }
        public DbSet<Religion> Religions { get; set; }
        public DbSet<Working> Workings { get; set; }
        public DbSet<Promo> Promos { get; set; }
        #endregion

        #region Areas Patient Registration
        public DbSet<NewPatient> NewPatients { get; set; }
        public DbSet<ExternalPatientLaboratorium> ExternalPatientLaboratoriums { get; set; }
        public DbSet<ExternalPatientRadiologi> ExternalPatientRadiologis { get; set; }
        public DbSet<ExternalPatientRehabilitasiMedik> ExternalPatientRehabilitasiMediks { get; set; }
        public DbSet<ExternalPatientMedicalCheckUp> ExternalPatientMedicalCheckUps { get; set; }
        public DbSet<ExternalPatientFasilitas> ExternalPatientFasilitas { get; set; }
        public DbSet<ExternalPatientAmbulance> ExternalPatientAmbulances { get; set; }
        public DbSet<ExternalPatientOptik> ExternalPatientOptiks { get; set; }
        public DbSet<Reference> References { get; set; }
        public DbSet<ReferenceType> ReferenceTypes { get; set; }
        public DbSet<ReferenceDetail> ReferenceDetails { get; set; }
        #endregion

        #region Areas Accounting And Financial
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankCabang> BankCabangs { get; set; }
        #endregion

        #region Areas Health Management    
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorType> DoctorTypes { get; set; }
        public DbSet<DoctorTitle> DoctorTitles { get; set; }
        public DbSet<DoctorQueueType> DoctorQueueTypes { get; set; }
        public DbSet<DoctorDepartment> DoctorDepartments { get; set; }
        public DbSet<DoctorDepartmentLocation> DepartmentLocations { get; set; }
        public DbSet<BloodType> BloodTypes { get; set; }
        public DbSet<MultipleDoctorDepartment> MultipleDoctorDepartments { get; set; }
        public DbSet<ScheduleToday> ScheduleTodays { get; set; }
        public DbSet<Day> Days { get; set; }
        #endregion
    }
}
