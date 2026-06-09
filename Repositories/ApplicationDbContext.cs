using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.Finance.AR.Models;
using QuilvianSystemBackendDev.Areas.Finance.AyatSilangs.Models;
using QuilvianSystemBackendDev.Areas.Finance.COA.Models;
using QuilvianSystemBackendDev.Areas.Finance.Faktur.Models;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Controllers;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models;
using QuilvianSystemBackendDev.Areas.Finance.Po.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Hemodialisa.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MCU.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,   // ❗ jangan camelCase
                //Converters =
                //{
                //    new TimeOnlyEfConverter(),
                //    new NullableTimeOnlyEfConverter()
                //}
            };

            #region Dictionary Hemodialisa
            modelBuilder.Entity<HemodialisaHasil>()
                .Property(x => x.LaporanNaCl)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, LaporanNaCLEntry>>(v, jsonOptions)
                );

            modelBuilder.Entity<HemodialisaHasil>()
                .Property(x => x.UF)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, jsonOptions)
                );
            #endregion

            #region Unique No Rekam Medis
            modelBuilder.Entity<PendaftaranPasienBaru>()
                .HasIndex(x => x.NoRekamMedis)
                .IsUnique();

            modelBuilder.Entity<LabHasilDetail>(entity =>
            {
                entity.HasKey(e => e.DetailHasilLabId);

                // Kolom string (PostgreSQL: text)
                entity.Property(e => e.HasilImunoHistokimiaJson)
                      .HasColumnType("text")
                      .HasColumnName("HasilImunoHistokimia"); // <-- nama kolom DB yang kamu minta

                // Wrapper tidak dimapping ke DB
                entity.Ignore(e => e.HasilImunoHistokimia);
            });
            #endregion

            #region Unique NoKwitansi Deposit
            modelBuilder.Entity<PendaftaranPasienBaru>()
                .HasIndex(x => x.NoRekamMedis)
                .IsUnique();

            modelBuilder.Entity<DepositRanap>(entity =>
            {
                entity.HasKey(e => e.DepositRanapId);

                entity.Property(e => e.NoKwitansi)
                    .IsRequired();

                entity.HasIndex(e => e.NoKwitansi)
                    .IsUnique();
            });
            #endregion

            #region Anti-Race Citext Agama
            // enable extension citext (dibantu oleh provider Npgsql)
            modelBuilder.HasPostgresExtension("citext");

            modelBuilder.Entity<Agama>(e =>
            {
                e.HasKey(x => x.AgamaId);

                // kolom Nama jadi citext (case-insensitive)
                e.Property(x => x.NamaAgama)
                 .HasColumnType("citext")
                 .IsRequired();

                // Unique hanya untuk data aktif (IsDelete=false atau null)
                e.HasIndex(x => x.NamaAgama)
                 .IsUnique()
                 .HasDatabaseName("UX_Agamas_Nama_Active")
                 .HasFilter(@"""IsDelete"" = false OR ""IsDelete"" IS NULL");
            });
            #endregion

            #region Icollection Restriction
            #region Alat Pemakaian
            modelBuilder.Entity<AlatPemakaian>()
                .HasOne(x => x.Kunjungan)
                .WithMany(x => x.AlatPemakaians)
                .HasForeignKey(x => x.KunjunganId)
                .HasPrincipalKey(x=>x.KunjunganID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AlatPemakaian>()
                .HasOne(x => x.Pasien)
                .WithMany(x => x.AlatPemakaians)
                .HasForeignKey(x => x.PasienId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AlatPemakaian>()
                .HasMany(x => x.Details)
                .WithOne(x => x.AlatPemakaian)
                .HasForeignKey(x => x.PemakaianAlatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AlatPemakaianDetail>()
                .HasOne(x => x.Peralatan)
                .WithMany(x => x.AlatPemakaianDetails)
                .HasForeignKey(x => x.PeralatanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AlatPemakaianDetail>()
                .HasOne(x => x.Kelas)
                .WithMany(x => x.AlatPemakaianDetails)
                .HasForeignKey(x => x.KelasId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Penerimaan dan Permintaan Unit + Farmasi RJ
            // =========================================================
            // PERMINTAAN UNIT -> DETAIL PERMINTAAN UNIT
            // 1 header : many details
            // =========================================================
            modelBuilder.Entity<PermintaanUnit>()
                .HasMany(x => x.DetailPermintaanUnits)
                .WithOne(x => x.PermintaanUnit)
                .HasForeignKey(x => x.PermintaanUnitId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================================
            // PENERIMAAN UNIT -> DETAIL PENERIMAAN UNIT
            // 1 header : many details
            // =========================================================
            modelBuilder.Entity<PenerimaanUnit>()
                .HasMany(x => x.DetailPenerimaanUnits)
                .WithOne(x => x.PenerimaanUnit)
                .HasForeignKey(x => x.PenerimaanUnitId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================================
            // PERMINTAAN UNIT -> INSTALASI UNIT (ASAL)
            // UnitId -> InstalasiUnit.UnitId
            // =========================================================
            modelBuilder.Entity<PermintaanUnit>()
                .HasOne(x => x.Unit)
                .WithMany(x => x.PermintaanUnitsAsal)
                .HasForeignKey(x => x.UnitId)
                .HasPrincipalKey(x => x.InstalasiUnitId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // PERMINTAAN UNIT -> INSTALASI UNIT (TUJUAN)
            // TujuanUnitId -> InstalasiUnit.UnitId
            // =========================================================
            modelBuilder.Entity<PermintaanUnit>()
                .HasOne(x => x.TujuanUnit)
                .WithMany(x => x.PermintaanUnitsTujuan)
                .HasForeignKey(x => x.TujuanUnitId)
                .HasPrincipalKey(x => x.InstalasiUnitId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // PENERIMAAN UNIT -> INSTALASI UNIT
            // UnitId -> InstalasiUnit.UnitId
            // =========================================================
            modelBuilder.Entity<PenerimaanUnit>()
                .HasOne(x => x.Unit)
                .WithMany(x => x.PenerimaanUnits)
                .HasForeignKey(x => x.UnitId)
                .HasPrincipalKey(x => x.InstalasiUnitId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // DETAIL PERMINTAAN UNIT -> OBAT
            // =========================================================
            modelBuilder.Entity<DetailPermintaanUnit>()
                .HasOne(x => x.Obat)
                .WithMany(x => x.DetailPermintaanUnits)
                .HasForeignKey(x => x.ObatId)
                .HasPrincipalKey(x => x.ObatId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // DETAIL PENERIMAAN UNIT -> OBAT
            // =========================================================
            modelBuilder.Entity<DetailPenerimaanUnit>()
                .HasOne(x => x.Obat)
                .WithMany(x => x.DetailPenerimaanUnits)
                .HasForeignKey(x => x.ObatId)
                .HasPrincipalKey(x => x.ObatId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // FARMASI RJ -> OBAT
            // =========================================================
            modelBuilder.Entity<FarmasiRJ>()
                .HasOne(x => x.Obat)
                .WithMany(x => x.FarmasiRJs)
                .HasForeignKey(x => x.ObatId)
                .HasPrincipalKey(x => x.ObatId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // FARMASI RJ -> KONVERSI SATUAN
            // =========================================================
            modelBuilder.Entity<FarmasiRJ>()
                .HasOne(x => x.KonversiSatuan)
                .WithMany(x => x.FarmasiRJs)
                .HasForeignKey(x => x.KonversiSatuanId)
                .HasPrincipalKey(x => x.KonversiSatuanId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Log Racik Penerimaan + Obat Return + Obat Rute

            // =========================================================
            // KUNJUNGAN -> LOG RACIK PENERIMAAN
            // 1 kunjungan : many log racik penerimaan
            // =========================================================
            modelBuilder.Entity<LogRacikPenerimaan>()
                .HasOne(x => x.Kunjungan)
                .WithMany(x => x.LogRacikPenerimaans)
                .HasForeignKey(x => x.KunjunganId)
                .HasPrincipalKey(x => x.KunjunganID)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // RESEP -> LOG RACIK PENERIMAAN
            // 1 resep : many log racik penerimaan
            // =========================================================
            modelBuilder.Entity<LogRacikPenerimaan>()
                .HasOne(x => x.Resep)
                .WithMany(x => x.LogRacikPenerimaans)
                .HasForeignKey(x => x.ResepId)
                .HasPrincipalKey(x => x.ResepId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // LOG RACIK PENERIMAAN -> USER ACTIVE FARMASI
            // UserActiveFarmasiId -> UserActive.UserActiveId
            // =========================================================
            modelBuilder.Entity<LogRacikPenerimaan>()
                .HasOne(x => x.UserActiveFarmasi)
                .WithMany()
                .HasForeignKey(x => x.UserActiveFarmasiId)
                .HasPrincipalKey(x => x.UserActiveId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // LOG RACIK PENERIMAAN -> USER ACTIVE PERAWAT
            // UserActivePerawatId -> UserActive.UserActiveId
            // =========================================================
            modelBuilder.Entity<LogRacikPenerimaan>()
                .HasOne(x => x.UserActivePerawat)
                .WithMany()
                .HasForeignKey(x => x.UserActivePerawatId)
                .HasPrincipalKey(x => x.UserActiveId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // OBAT RETURN -> OBAT RETURN DETAIL
            // 1 header : many details
            // =========================================================
            modelBuilder.Entity<ObatReturn>()
                .HasMany(x => x.ObatReturnDetails)
                .WithOne(x => x.ObatReturn)
                .HasForeignKey(x => x.ObatReturnId)
                .HasPrincipalKey(x => x.ObatReturnId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================================
            // OBAT RETURN DETAIL -> OBAT
            // ObatId -> Obat.ObatId
            // =========================================================
            modelBuilder.Entity<ObatReturnDetail>()
                .HasOne(x => x.Obat)
                .WithMany(x => x.ObatReturnDetails)
                .HasForeignKey(x => x.ObatId)
                .HasPrincipalKey(x => x.ObatId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // OBAT RUTE -> OBAT RUTE DETAIL
            // 1 header/master rute : many details
            // =========================================================
            modelBuilder.Entity<ObatRute>()
                .HasMany(x => x.ObatRuteDetails)
                .WithOne(x => x.ObatRute)
                .HasForeignKey(x => x.RuteObatId)
                .HasPrincipalKey(x => x.RuteObatId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Obat dan satuannya
            // =========================================================
            // OBAT -> SATUAN
            // =========================================================
            modelBuilder.Entity<Obat>()
                .HasOne(x => x.Satuan)
                .WithMany(x => x.Obats)
                .HasForeignKey(x => x.SatuanId)
                .HasPrincipalKey(x => x.SatuanId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // OBAT -> BENTUK OBAT
            // =========================================================
            modelBuilder.Entity<Obat>()
                .HasOne(x => x.BentukObat)
                .WithMany(x => x.Obats)
                .HasForeignKey(x => x.BentukObatId)
                .HasPrincipalKey(x => x.BentukSatuanId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Resep + Racikan
            // =========================================================
            // RESEP -> RACIKAN
            // 1 resep : many racikan
            // =========================================================
            modelBuilder.Entity<Racikan>()
                .HasOne(x => x.Resep)
                .WithMany(x => x.Racikans)
                .HasForeignKey(x => x.ResepId)
                .HasPrincipalKey(x => x.ResepId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // RACIKAN BENTUK -> RACIKAN
            // 1 bentuk racikan : many racikan
            // =========================================================
            modelBuilder.Entity<Racikan>()
                .HasOne(x => x.BentukRacikan)
                .WithMany(x => x.Racikans)
                .HasForeignKey(x => x.BentukRacikanId)
                .HasPrincipalKey(x => x.BentukRacikanId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // RACIKAN -> RACIKAN DETAIL
            // 1 racikan : many detail racikan
            // =========================================================
            modelBuilder.Entity<Racikan>()
                .HasMany(x => x.RacikanDetails)
                .WithOne(x => x.Racikan)
                .HasForeignKey(x => x.RacikanId)
                .HasPrincipalKey(x => x.RacikanId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================================
            // RACIKAN DETAIL -> OBAT
            // ObatId -> Obat.ObatId
            // =========================================================
            modelBuilder.Entity<RacikanDetail>()
                .HasOne(x => x.Obat)
                .WithMany(x => x.RacikanDetails)
                .HasForeignKey(x => x.ObatId)
                .HasPrincipalKey(x => x.ObatId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // RESEP -> RESEP DETAIL
            // 1 resep : many resep detail
            // =========================================================
            modelBuilder.Entity<Resep>()
                .HasMany(x => x.ResepDetails)
                .WithOne(x => x.Resep)
                .HasForeignKey(x => x.ResepId)
                .HasPrincipalKey(x => x.ResepId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================================
            // RACIKAN -> RESEP DETAIL
            // 1 racikan : many resep detail
            // =========================================================
            modelBuilder.Entity<ResepDetail>()
                .HasOne(x => x.Racikan)
                .WithMany(x => x.ResepDetails)
                .HasForeignKey(x => x.RacikanId)
                .HasPrincipalKey(x => x.RacikanId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================================================
            // RESEP DETAIL -> OBAT
            // ObatId -> Obat.ObatId
            // =========================================================
            modelBuilder.Entity<ResepDetail>()
                .HasOne(x => x.Obat)
                .WithMany(x => x.ResepDetails)
                .HasForeignKey(x => x.ObatId)
                .HasPrincipalKey(x => x.ObatId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Kunjungan
            modelBuilder.Entity<Kunjungan>(entity =>
            {
                entity.ToTable("MstKunjungan", "public");

                entity.HasKey(e => e.KunjunganID);

                // =========================
                // RELASI KE ASURANSI
                // =========================

                entity.HasOne(e => e.Asuransi)
                    .WithMany()
                    .HasForeignKey(e => e.AsuransiId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MstKunjungan_MstAsuransi_AsuransiId");

                entity.HasOne(e => e.AsuransiExcess)
                    .WithMany()
                    .HasForeignKey(e => e.AsuransiExcessId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MstKunjungan_MstAsuransi_AsuransiExcessId");

                entity.HasOne(e => e.AsuransiPasien)
                    .WithMany()
                    .HasForeignKey(e => e.AsuransiPasienId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienId");

                entity.HasOne(e => e.AsuransiPasienExcess)
                    .WithMany()
                    .HasForeignKey(e => e.AsuransiPasienExcessId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienExcessId");

                // =========================
                // RELASI KE POLIKLINIK, DOKTER, PASIEN
                // =========================

                entity.HasOne(e => e.Poliklinik)
                    .WithMany()
                    .HasForeignKey(e => e.PoliklinikId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MstKunjungan_MstPoliklinik_PoliklinikId");

                entity.HasOne(e => e.Dokter)
                    .WithMany()
                    .HasForeignKey(e => e.DokterId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MstKunjungan_MstDokter_DokterId");

                entity.HasOne(e => e.Pasien)
                    .WithMany()
                    .HasForeignKey(e => e.PasienId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MstKunjungan_MstPendaftaranPasienBaru_PasienId");

            });
            #endregion

            #region Lab Booking + detail
            modelBuilder.Entity<LabBooking>(entity =>
            {
                entity.ToTable("LabBooking", "public");

                entity.HasKey(e => e.BookingLabId);

                // =========================
                // INDEX UNTUK GET DATA BESAR
                // =========================

                entity.HasIndex(e => e.KunjunganId)
                    .HasDatabaseName("IX_LabBooking_KunjunganId");

                entity.HasIndex(e => e.PasienId)
                    .HasDatabaseName("IX_LabBooking_PasienId");

                entity.HasIndex(e => e.AsuransiId)
                    .HasDatabaseName("IX_LabBooking_AsuransiId");

                entity.HasIndex(e => e.KelasId)
                    .HasDatabaseName("IX_LabBooking_KelasId");

                entity.HasIndex(e => e.DokterKonsulenId)
                    .HasDatabaseName("IX_LabBooking_DokterKonsulenId");

                entity.HasIndex(e => e.TerapisId)
                    .HasDatabaseName("IX_LabBooking_TerapisId");

                entity.HasIndex(e => e.NoOrder)
                    .HasDatabaseName("IX_LabBooking_NoOrder");

                entity.HasIndex(e => e.NoLab)
                    .HasDatabaseName("IX_LabBooking_NoLab");

                entity.HasIndex(e => e.NoPA)
                    .HasDatabaseName("IX_LabBooking_NoPA");

                entity.HasIndex(e => e.StatusPemeriksaan)
                    .HasDatabaseName("IX_LabBooking_StatusPemeriksaan");

                entity.HasIndex(e => e.IsLunas)
                    .HasDatabaseName("IX_LabBooking_IsLunas");

                entity.HasIndex(e => e.TglBooking)
                    .HasDatabaseName("IX_LabBooking_TglBooking");

                entity.HasIndex(e => e.TglPemeriksaan)
                    .HasDatabaseName("IX_LabBooking_TglPemeriksaan");

                entity.HasIndex(e => new { e.IsDelete, e.CreateDateTime })
                    .HasDatabaseName("IX_LabBooking_IsDelete_CreateDateTime");

                entity.HasIndex(e => new { e.PasienId, e.CreateDateTime })
                    .HasDatabaseName("IX_LabBooking_PasienId_CreateDateTime");

                entity.HasIndex(e => new { e.KunjunganId, e.CreateDateTime })
                    .HasDatabaseName("IX_LabBooking_KunjunganId_CreateDateTime");

                // =========================
                // RELASI
                // =========================

                entity.HasOne(e => e.Kunjungan)
                    .WithMany()
                    .HasForeignKey(e => e.KunjunganId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBooking_MstKunjungan_KunjunganId");

                entity.HasOne(e => e.Pasien)
                    .WithMany()
                    .HasForeignKey(e => e.PasienId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBooking_MstPasien_PasienId");

                entity.HasOne(e => e.Asuransi)
                    .WithMany()
                    .HasForeignKey(e => e.AsuransiId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBooking_MstAsuransi_AsuransiId");

                entity.HasOne(e => e.Kelas)
                    .WithMany()
                    .HasForeignKey(e => e.KelasId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBooking_MstKelas_KelasId");

                entity.HasOne(e => e.DokterKonsulen)
                    .WithMany()
                    .HasForeignKey(e => e.DokterKonsulenId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBooking_MstDokter_DokterKonsulenId");

                entity.HasMany(e => e.LabBookingDetails)
                    .WithOne(e => e.LabBooking)
                    .HasForeignKey(e => e.BookingLabId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBookingDetail_LabBooking_BookingLabId");
            });

            modelBuilder.Entity<LabBookingDetail>(entity =>
            {
                entity.ToTable("LabBookingDetail", "public");

                entity.HasKey(e => e.DetailBookingLabId);

                // =========================
                // INDEX UNTUK GET DATA BESAR
                // =========================

                entity.HasIndex(e => e.BookingLabId)
                    .HasDatabaseName("IX_LabBookingDetail_BookingLabId");

                entity.HasIndex(e => e.PasienId)
                    .HasDatabaseName("IX_LabBookingDetail_PasienId");

                entity.HasIndex(e => e.PemeriksaanLabId)
                    .HasDatabaseName("IX_LabBookingDetail_PemeriksaanLabId");


                entity.HasIndex(e => e.AsalSpecimenId)
                    .HasDatabaseName("IX_LabBookingDetail_AsalSpecimenId");

                entity.HasIndex(e => e.StatusPemeriksaan)
                    .HasDatabaseName("IX_LabBookingDetail_StatusPemeriksaan");

                entity.HasIndex(e => e.StatusVerifikasi)
                    .HasDatabaseName("IX_LabBookingDetail_StatusVerifikasi");

                entity.HasIndex(e => e.TanggalSelesai)
                    .HasDatabaseName("IX_LabBookingDetail_TanggalSelesai");

                entity.HasIndex(e => new { e.IsDelete, e.CreateDateTime })
                    .HasDatabaseName("IX_LabBookingDetail_IsDelete_CreateDateTime");

                entity.HasIndex(e => new { e.BookingLabId, e.CreateDateTime })
                    .HasDatabaseName("IX_LabBookingDetail_BookingLabId_CreateDateTime");

                // Jika pakai PostgreSQL array uuid[]
                entity.Property(e => e.SpecimenJenisId)
                    .HasColumnType("uuid[]");

                entity.Property(e => e.SpecimenMethodId)
                    .HasColumnType("uuid[]");

                // =========================
                // RELASI
                // =========================

                entity.HasOne(e => e.LabBooking)
                    .WithMany(e => e.LabBookingDetails)
                    .HasForeignKey(e => e.BookingLabId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBookingDetail_LabBooking_BookingLabId");

                entity.HasOne(e => e.Pasien)
                    .WithMany()
                    .HasForeignKey(e => e.PasienId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBookingDetail_MstPasien_PasienId");

                entity.HasOne(e => e.PemeriksaanLab)
                    .WithMany()
                    .HasForeignKey(e => e.PemeriksaanLabId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBookingDetail_MstPemeriksaanLab_PemeriksaanLabId");

                entity.HasOne(e => e.AsalSpecimen)
                    .WithMany()
                    .HasForeignKey(e => e.AsalSpecimenId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabBookingDetail_MstAsalSpecimen_AsalSpecimenId");
            });
            #endregion

            #region Billing
            modelBuilder.Entity<Billing>(entity =>
            {
                entity.HasKey(e => e.BillingId);

                entity.HasOne(e => e.Kunjungan)
                    .WithMany(e => e.Billings)
                    .HasForeignKey(e => e.KunjunganId)
                    .HasPrincipalKey(e => e.KunjunganID)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Billing_MstKunjungan_KunjunganId");
            });
            #endregion

            #region Lab Persiapan
            modelBuilder.Entity<LabPemeriksaanPersiapan>(entity =>
            {
                entity.HasKey(e => e.LabPemeriksaanPersiapanId);

                entity.HasOne(e => e.Lab)
                    .WithMany()
                    .HasForeignKey(e => e.LabId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LabPemeriksaan)
                    .WithMany()
                    .HasForeignKey(e => e.PemeriksaanLabId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LabPersiapanPemeriksaan)
                    .WithMany(e => e.LabPemeriksaanPersiapans)
                    .HasForeignKey(e => e.LabPersiapanPemeriksaanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<LabJawabanPersiapan>(entity =>
            {
                entity.HasKey(e => e.LabJawabanPersiapanId);

                entity.HasOne(e => e.Kunjungan)
                    .WithMany()
                    .HasForeignKey(e => e.KunjunganId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Pasien)
                    .WithMany()
                    .HasForeignKey(e => e.PasienId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PemeriksaanLab)
                    .WithMany()
                    .HasForeignKey(e => e.PemeriksaanLabId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LabPersiapanPemeriksaan)
                    .WithMany(e => e.LabJawabanPersiapans)
                    .HasForeignKey(e => e.LabPersiapanPemeriksaanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<RiwayatBendaMedisPasien>(entity =>
            {
                entity.HasKey(e => e.RiwayatBendaMedisPasienId);

                entity.HasOne(e => e.Kunjungan)
                    .WithMany()
                    .HasForeignKey(e => e.KunjunganId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Pasien)
                    .WithMany()
                    .HasForeignKey(e => e.PasienId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<RiwayatOperasiPasien>(entity =>
            {
                entity.HasKey(e => e.RiwayatOperasiPasienId);

                entity.HasOne(e => e.Kunjungan)
                    .WithMany()
                    .HasForeignKey(e => e.KunjunganId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Pasien)
                    .WithMany()
                    .HasForeignKey(e => e.PasienId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region Tarif Film
            modelBuilder.Entity<Film>(entity =>
            {
                entity.HasKey(e => e.FilmId);

                entity.Property(e => e.NamaFilm)
                    .IsRequired();

                entity.Property(e => e.IsDelete)
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<TarifFilm>(entity =>
            {
                entity.HasKey(e => e.TarifFilmId);

                entity.Property(e => e.TarifDokter).HasColumnType("numeric");
                entity.Property(e => e.TarifRs).HasColumnType("numeric");
                entity.Property(e => e.TarifJp).HasColumnType("numeric");
                entity.Property(e => e.TarifBahp).HasColumnType("numeric");
                entity.Property(e => e.TarifLain).HasColumnType("numeric");
                entity.Property(e => e.TarifTotal).HasColumnType("numeric");
                entity.Property(e => e.KSO).HasColumnType("numeric");

                entity.Property(e => e.IsDelete)
                    .HasDefaultValue(false);

                entity.HasOne(e => e.Film)
                    .WithMany(e => e.TarifFilms)
                    .HasForeignKey(e => e.FilmId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Kelas)
                    .WithMany()
                    .HasForeignKey(e => e.KelasId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion
            #endregion
        }

        public DbSet<UserActive> UserActives { get; set; }
        public DbSet<Setting> Settings { get; set; }

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
        public DbSet<AsuransiPasien> AsuransiPasiens { get; set; }
        public DbSet<KodePos> KodePoss { get; set; }
        public DbSet<Suku> Sukus { get; set; }
        public DbSet<FasilitasPasien> FasilitasPasiens { get; set; }
        public DbSet<RegistFasilitasPasien> RegistFasilitasPasiens { get; set; }
        public DbSet<DokterAsuransi> DokterAsuransis { get; set; }
        public DbSet<Kunjungan> Kunjungans { get; set; }
        public DbSet<KategoriObat> KategoriObats { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<TermOfPayment> TermOfPayments { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<WarehouseLocation> WarehouseLocations { get; set; }
        public DbSet<Obat> Obats { get; set; }
        public DbSet<CoveranObatAsuransi> CoveranObatAsuransis { get; set; }
        public DbSet<CurrentMedication> CurrentMedications { get; set; }
        public DbSet<SkalaPain> SkalaPains { get; set; }
        public DbSet<PainAssessment> PainAssessments { get; set; }
        public DbSet<VitalSign> VitalSigns { get; set; }
        public DbSet<SOAP> SOAPs { get; set; }
        public DbSet<ICD10> ICD10s { get; set; }
        public DbSet<DetailICD> DetailICDs { get; set; }
        public DbSet<Resep> Reseps { get; set; }
        public DbSet<ResepTemplate> ResepTemplates { get; set; }
        public DbSet<ResepTemplateDetail> ResepTemplateDetails { get; set; }
        public DbSet<ResepDetail> DetailReseps { get; set; }
        public DbSet<Kelas> Kelass { get; set; }
        public DbSet<TarifKelas> TarifKelass { get; set; } 
        public DbSet<CoveranTindakanAsuransi> CoveranTindakanAsuransis { get; set; }
        public DbSet<Satuan> Satuans { get; set; }
        public DbSet<Tindakan> Tindakans { get; set; }
        public DbSet<TindakanPoli> TindakanPolis { get; set; }
        public DbSet<TindakanAsuransi> TindakanAsuransis { get; set; }
        public DbSet<TindakanKunjungan> TindakanKunjungans { get; set; }
        public DbSet<ObatAsuransi> ObatAsuransis { get; set; }
        public DbSet<ObatKandungan> ObatKandungans { get; set; }
        public DbSet<Kandungan> Kandungans { get; set; }
        public DbSet<BentukObat> BentukObats { get; set; }
        public DbSet<TipeUser> TipeUsers { get; set; }
        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<Membership> Membership { get; set; }
        public DbSet<BenefitEntitiyMapping> BenefitEntitiyMappings { get; set; }
        public DbSet<DetailMembership> DetailMembership { get; set; }
        public DbSet<PasienBenefitAsign> PasienBenefitAsigns { get; set; }
        public DbSet<PasienBenefitOverride> PasienBenefitOverrides { get; set; }
        public DbSet<KonversiSatuan> KonversiSatuans { get; set; }
        public DbSet<FarmasiRJ> FarmasiRJs { get; set; }
        public DbSet<Racikan> Racikans { get; set; }
        public DbSet<RacikanAddon> RacikanAddons { get; set; }
        public DbSet<ResepTebus> ResepTebuss { get; set; }
        public DbSet<ResepTebusDetail> ResepTebusDetails { get; set; }
        public DbSet<PPN> PPNs { get; set; }
        public DbSet<ObatReturn> ObatReturns { get; set; }
        public DbSet<ObatHarga> ObatHargas { get; set; }
        public DbSet<ObatReturnDetail> ObatReturnDetails { get; set; }
        public DbSet<RacikanDetail> RacikanDetails { get; set; }
        public DbSet<Kamar> Kamars { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<KamarAsuransi> KamarAsuransis { get; set; }
        public DbSet<Delegasi> Delegasis { get; set; }
        public DbSet<ICDPlanning> ICDPlannings { get; set; }
        public DbSet<SOAPPlanning> SOAPPlannings { get; set; }
        public DbSet<DetailPlanning> DetailPlannings { get; set; }
        public DbSet<Gudang> Gudangs { get; set; }
        public DbSet<GudangUnit> GudangUnits { get; set; }
        public DbSet<TindakanPerawat> TindakanPerawats { get; set; }
        public DbSet<KategoriIndikator> KategoriIndikators { get; set; }
        public DbSet<IntervensiResikoJatuh> IntervensiResikoJatuhs { get; set; }
        public DbSet<ObatRute> ObatRutes { get; set; }
        public DbSet<ItemKategori> ItemKategoris { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<RacikanBentuk> RacikanBentuks { get; set; }
        public DbSet<OperasiTipe> OperasiTipes { get; set; }
        public DbSet<AnastesiTipe> AnastesiTipes { get; set; }
        public DbSet<ASATipe> ASATipes { get; set; }
        public DbSet<OperasiJenis> OperasiJeniss { get; set; }
        public DbSet<StockDarah> StockDarahs { get; set; }
        public DbSet<PenerimaanDarah> PenerimaanDarahs { get; set; }
        public DbSet<PAS> PASs { get; set; }
        public DbSet<PemeriksaanLabAsuransi> PemeriksaanLabAsuransis {  get; set; }
        public DbSet<TarifKelasAsuransi> TarifKelasAsuransis { get; set; }
        public DbSet<CatatanKIE> CatatanKIEs { get; set; }
        public DbSet<InformasiPenundaan> InformasiPenundaans {  get; set; }
        public DbSet<PelunasanDeposit> PelunasanDeposits { get; set; }
        public DbSet<TarifKelasKamar> TarifKelasKamars { get; set; }
        public DbSet<Layanan> Layanans { get; set; }
        public DbSet<PaketLayanan> PaketLayanans { get; set; }
        public DbSet<PaketLayananDetail> PaketLayananDetails { get; set; }
        public DbSet<PaketLayananAsuransi> PaketLayananAsuransis { get; set; }
        public DbSet<PaketLayananDiskon> PaketLayananDiskons { get; set; }
        public DbSet<LoketKasir> LoketKasirs { get; set; }
        public DbSet<DokumenPasien> DokumenPasiens { get; set; }
        public DbSet<BarangKategori> BarangKategoris { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<KelasResiko> KelasResikos { get; set; }
        public DbSet<Lantai> Lantais { get; set; }
        public DbSet<LokasiPenyimpanan> LokasiPenyimpanans {  get; set; }
        public DbSet<Barang> Barangs { get; set; }
        public DbSet<BarangHarga> BarangHargas { get; set; }
        public DbSet<BarangStok> BarangStoks { get; set; }
        public DbSet<TarifVisit> TarifVisits { get; set; }
        public DbSet<TarifKamar> TarifKamars { get; set; }
        public DbSet<TarifPatologiKlinik> TarifPatologiKliniks { get; set; }
        public DbSet<TarifPatalogiAnatomi> TarifPatalogiAnatomis { get; set; }
        public DbSet<TarifMicrobiologi> TarifMicrobiologis { get; set; }
        public DbSet<TarifRehabMedik> TarifRehabMediks { get; set; }
        public DbSet<TarifHemodialisa> TarifHemodialisas { get; set; }
        public DbSet<TarifAlkes> TarifAlkess { get; set; }
        public DbSet<TarifOperasi> TarifOperasis { get; set; }
        public DbSet<TarifPaketLayanan> TarifPaketLayanans { get; set; }
        public DbSet<TarifRadiologi> TarifRadiologis { get; set; }
        public DbSet<KategoriTerapeutik> KategoriTerapeutiks { get; set; }
        public DbSet<SubKategoriTerapeutik> SubKategoriTerapeutiks { get; set; }
        public DbSet<JenisProkes> JenisProkess { get; set; }
        public DbSet<KodeKFA> KodeKFAs { get; set; }
        public DbSet<Komoditas> Komoditas { get; set; }
        public DbSet<Principal> Principals { get; set; }
        public DbSet<GolonganObat> GolonganObats { get; set; }
        public DbSet<GroupObatAlkes> GroupObatAlkess { get; set; }
        public DbSet<ObatAlkes> ObatAlkess { get; set; }
        public DbSet<SupplierObatAlkes> SupplierObatAlkess { get; set; }
        public DbSet<DokterInstalasiUnit> DokterInstalasiUnits { get; set; }
        public DbSet<KunjunganLayanan> KunjunganLayanans { get; set; }
        public DbSet<Film> Films { get; set; }
        public DbSet<TarifFilm> TarifFilms { get; set; }


        #endregion

        #region Areas Keuangan
        public DbSet<MetodePembayaran> MetodePembayarans { get; set; }
        public DbSet<Diskon> Diskons { get; set; }
        public DbSet<DiskonDetail> DiskonDetails { get; set; }
        public DbSet<DiskonDireksi> DiskonDireksis { get; set; }
        public DbSet<DiskonTagihan> DiskonTagihans { get; set; }
        public DbSet<BiayaAdministrasi> BiayaAdministrasis { get; set; }
        public DbSet<MainKasir> MainKasirs { get; set; }
        public DbSet<MainKasirDetail> MainKasirDetails { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<MasterDenominasi> MasterDenominasies { get; set; }
        public DbSet<ShiftDenominasi> ShiftDenominasies { get; set; }
        public DbSet<PergantianShift> PergantianShifts { get; set; }
        public DbSet<VoucherPettyCash> VoucherPettyCashes { get; set; }
        public DbSet<DepositRanap> DepositRanaps { get; set; }
        public DbSet<DepositPersentase> DepositPersentases { get; set; }
        public DbSet<DiskonPersentase> DiskonPersentases { get; set; }
        public DbSet<DiskonDokter> DiskonDokters { get; set; }


        #endregion

        #region Areas Hrd
        public DbSet<JenisCuti> JenisCutis { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<GradePay> GradePays { get; set; }
        public DbSet<JenisLembur> JenisLemburs { get; set; }
        public DbSet<JenisTiketing> JenisTiketings { get; set; }
        public DbSet<SubLevel> SubLevels { get; set; }

        public DbSet<PengajuanCuti> PengajuanCutis { get; set; }
        public DbSet<PengajuanLembur> PengajuanLemburs { get; set; }
        public DbSet<PengajuanTiketing> PengajuanTiketings { get; set; }
        public DbSet<PengajuanResign> PengajuanResigns { get; set; }
        public DbSet<GradeLevelJob> GradeLevelJobs { get; set; }
        public DbSet<CounterOffer> CounterOffers { get; set; }
        public DbSet<PengajuanRekrutmen> PengajuanRekrutmens { get; set; }
        public DbSet<RiwayatPendidikan> RiwayatPendidikans { get; set; }
        public DbSet<RiwayatSertifikat> RiwayatSertifikats { get; set; }
        public DbSet<DokumenDetailKaryawan> DokumenDetailKaryawans { get; set; }
        public DbSet<MasterKeahlian> MasterKeahlians { get; set; }
        public DbSet<DetailKeahlian> DetailKeahlians { get; set; }
        public DbSet<MasterTTD> MasterTTDs { get; set; }
        public DbSet<MasterSoal> MasterSoals { get; set; }
        public DbSet<HasilTest> HasilTests { get; set; }
        public DbSet<InstalasiUnit> InstalasiUnits { get; set; }
        public DbSet<Karyawan> Karyawans { get; set; }
        public DbSet<MappingPosisi> MappingPosisis { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }


        #endregion

        #region Areas Administrator
        public DbSet<JenisUser> JenisUsers { get; set; }
        public DbSet<JenisPembayaran> JenisPembayarans { get; set; }
        public DbSet<Pembayaran> Pembayarans { get; set; }
        public DbSet<Fingerprint> Fingerprints { get; set; }


        #endregion

        #region Areas ManagementKesehatan
            #region Areas Pendaftaran
            public DbSet<PendaftaranPasienBaru> PendaftaranPasienBarus { get; set; }
            public DbSet<PendaftaranPasien> PendaftaranPasiens { get; set; }
            public DbSet<PendaftaranPasienUGD> PendaftaranPasienUGDs { get; set; }
            public DbSet<PendaftaranPasienOptik> PendaftaranPasienOptiks { get; set; }
            public DbSet<PendaftaranPasienAmbulan> PendaftaranPasienAmbulans { get; set; }
            public DbSet<PendaftaranPasienRehabMedik> PendaftaranPasienRehabMediks { get; set; }
            public DbSet<PendaftaranPasienMCU> PendaftaranPasienMCUs { get; set; }
            public DbSet<PendaftaranPasienRadiologi> PendaftaranPasienRadiologis { get; set; }
            //public DbSet<PendaftaranPasienLaboratorium> PendaftaranPasienLaboratoriums { get; set; }
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

            #region Areas Rawat Inap
            public DbSet<SuratPengantarRawatInap> SuratPengantarRawatInaps { get; set; }
            public DbSet<BookingBedRanap> BookingBedRanaps { get; set; }
            public DbSet<PerawatObjective> PerawatObjectives { get; set; }
            public DbSet<PerawatSubjective> PerawatSubjectives { get; set; }
            public DbSet<PerawatIntervensi> PerawatIntervensis { get; set; }
            public DbSet<SDKIKolaborasi> SDKIKolaborasis { get; set; }
            public DbSet<SDKIEdukasi> SDKIEdukasis { get; set; }
            public DbSet<SDKITeraupetik> SDKITeraupetiks { get; set; }
            public DbSet<SDKIEtiologi> SDKIEtiologis { get; set; }
            public DbSet<SDKIDiagnosa> SDKIDiagnosas { get; set; }
            public DbSet<SDKIObservasi> SDKIObservasis { get; set; }
            public DbSet<SDKIEvaluasi> SDKIEvaluasis { get; set; }
            public DbSet<KajianPasien> KajianPasiens { get; set; }
            public DbSet<SDKIGroup> SDKIGroups { get; set; }
            public DbSet<Indikator> Indikators { get; set; }
            public DbSet<IndikatorScore> IndikatorScores { get; set; }
            public DbSet<IndikatorPengkajian> IndikatorPengkajians { get; set; }
            public DbSet<SkriningNutrisi> SkriningNutrisis { get; set; }
            public DbSet<PengkajianEliminasi> PengkajianEliminasis { get; set; }
            public DbSet<PengkajianKetergantungan> PengkajianKetergantungans { get; set; }
            public DbSet<PengkajianKulit> PengkajianKulits { get; set; }
            public DbSet<PengkajianPernapasan> PengkajianPernapasans { get; set; }
            public DbSet<PengkajianPerawat> PengkajianPerawats { get; set; }
            public DbSet<DetailKetergantungan> DetailKetergantungans { get; set; }
            public DbSet<ResumePulang> ResumePulangs { get; set; }
            public DbSet<ResumePulangDetail> ResumePulangDetails { get; set; }
            public DbSet<CttPemberianObat> CttPemberianObats { get; set; }
            public DbSet<CatatanESO> CatatanESOs { get; set; }
            public DbSet<ObservasiCairan> ObservasiCairans { get; set; }
            public DbSet<ObservasiCairanWsd> ObservasiCairanWsds { get; set; }
            public DbSet<SlidingScale> SlidingScales { get; set; }
            public DbSet<ChecklistTemplate> ChecklistTemplates { get; set; }
            public DbSet<ChecklistItem> ChecklistItems { get; set; }
            public DbSet<ChecklistResponse> ChecklistResponses { get; set; }
            public DbSet<EvaluasiAwal> EvaluasiAwals { get; set; }
            public DbSet<EvaluasiAwalDetail> EvaluasiAwalDetails { get; set; }
            public DbSet<CatatanDiet> CatatanDiets { get; set; }
            public DbSet<TindakanHarian> TindakanHarians { get; set; }
            public DbSet<PengawasanHarian> PengawasanHarians { get; set; }
            public DbSet<TransferPasien> TransferPasiens { get; set; }
            public DbSet<TransferPasienDetail> TransferPasienDetails { get; set; }
            public DbSet<VisitDokter> VisitDokters { get; set; }
            public DbSet<TopikEdukasi> TopikEdukasis { get; set; }
            public DbSet<AssesmentEdukasi> AssesmentEdukasis { get; set; }
            public DbSet<AssesmentEdukasiDetail> AssesmentEdukasiDetails { get; set; }
            public DbSet<ResikoJatuh> ResikoJatuhs { get; set; }
            public DbSet<PenilaianResikoJatuhDetail> PenilaianResikoJatuhDetails { get; set; }
            public DbSet<CatatanPerawat> CatatanPerawats { get; set; }
            public DbSet<MonitoringNyeri> MonitoringNyeris { get; set; }
            public DbSet<GeneralConsent> GeneralConsents { get; set; }
            public DbSet<PermintaanPrivasi> PermintaanPrivasis { get; set; }
            public DbSet<NilaiKepercayaan> NilaiKepercayaans { get; set; }
            public DbSet<HandoverPasien> HandoverPasiens { get; set; }
            public DbSet<HandoverPasienDetail> HandoverPasienDetails { get; set; }
            public DbSet<SelisihBiaya> SelisihBiayas { get; set; }
            #endregion

            #region Farmasi
            public DbSet<LogRacikPenerimaan> LogRacikPenerimaans { get; set; }
            public DbSet<PermintaanUnit> PermintaanUnits { get; set; }
            public DbSet<DetailPermintaanUnit> DetailPermintaanUnits { get; set; }
            public DbSet<DetailPenerimaanUnit> DetailPenerimaanUnits { get; set; }
            public DbSet<PenerimaanUnit> PenerimaanUnits { get; set; }
            public DbSet<StockBatch> StockBatchs { get; set; }
            public DbSet<StockKartu> StockKartus { get; set; }
            public DbSet<ResepTelaah> ResepTelaahs { get; set; }
            public DbSet<ObatSubstitusi> ObatSubstitusis { get; set; }
            public DbSet<ObatTelaah> ObatTelaahs { get; set; }
            public DbSet<ObatRuteDetail> ObatRuteDetails { get; set; }
            public DbSet<ObatUnit> ObatUnits { get; set; }

            #endregion

            #region OperasiOK

            public DbSet<PraOperasi> PraOperasis { get; set; }
            public DbSet<OperasiTindakan> OperasiTindakans { get; set; }
            public DbSet<RuangBedahBooking> RuangBedahBookings { get; set; }
            public DbSet<RuangBedahBookingDetail> RuangBedahBookingDetails { get; set; }
            public DbSet<CatatanBedah> CatatanBedahs { get; set; }
            public DbSet<CatatanBedahLokal> CatatanBedahLokals { get; set; }
            public DbSet<CatatanPemulihan> CatatanPemulihans { get; set; }
            public DbSet<CatatanPemulihanDetail> CatatanPemulihanDetails { get; set; }
            public DbSet<LaporanBedah> LaporanBedahs { get; set; }
            public DbSet<LaporanAnestesi> LaporanAnestesis { get; set; }
            public DbSet<LaporanAnestesiDetail> LaporanAnestesiDetails { get; set; }
            #endregion

            #region Laborat
            public DbSet<Lab> Labs { get; set; }
            public DbSet<LabKategoriPemeriksaan> LabKategoriPemeriksaans { get; set; }
            public DbSet<LabPemeriksaan> LabPemeriksaans { get; set; }
            public DbSet<LabBooking> LabBookings { get; set; }
            public DbSet<LabBookingDetail> LabBookingDetails { get; set; }
            public DbSet<SpecimenAsal> SpecimenAsals { get; set; }
            public DbSet<SpecimenJenis> SpecimenJeniss { get; set; }
            public DbSet<SpecimenMethod> SpecimenMethods { get; set; }
            public DbSet<Darah> Darahs { get; set; }
            public DbSet<DarahPermintaan> DarahPermintaans { get; set; }
            public DbSet<PemeriksaanAsuransi> PemeriksaanAsuransis { get; set; }
            public DbSet<LabHasil> LabHasils { get; set; }
            public DbSet<LabHasilDetail> LabHasilDetails { get; set; }
            public DbSet<PenerimaDarahPasien> PenerimaDarahPasiens { get; set; }
            public DbSet<DarahDetail> DarahDetails { get; set; }
            public DbSet<LabBookingDetailSpecimenJenis> LabBookingDetailSpecimenJenises { get; set; }
            public DbSet<LabBookingDetailSpecimenMethod> LabBookingDetailSpecimenMethods { get; set; }
            public DbSet<LabPersiapanPemeriksaan> LabPersiapanPemeriksaans { get; set; }
            public DbSet<LabPemeriksaanPersiapan> LabPemeriksaanPersiapans { get; set; }
            public DbSet<LabJawabanPersiapan> LabJawabanPersiapans { get; set; }
            public DbSet<RiwayatBendaMedisPasien> RiwayatBendaMedisPasiens { get; set; }
            public DbSet<RiwayatOperasiPasien> RiwayatOperasiPasiens { get; set; }


        #endregion

        #region IGD
        public DbSet<IGDTriage> IGDTriages { get; set; }
            public DbSet<IGDTriageDetail> IGDTriageDetails { get; set; }
            public DbSet<IGDPasienDetail> IGDPasienDetails { get; set; }
            public DbSet<IGDTindakanDetail> IGDTindakanDetails { get; set; }
            public DbSet<IGDAssessmentAwal> IGDAssessmentAwals { get; set; }
            public DbSet<Nosokomial> Nosokomials { get; set; }
            public DbSet<PindahRuangan> PindahRuangans { get; set; }
            public DbSet<InfeksiADP> InfeksiADPs { get; set; }
            public DbSet<InfeksiLO> InfeksiLOs { get; set; }
            public DbSet<InfeksiSK> InfeksiSKs { get; set; }
            public DbSet<InfeksiTD> InfeksiTDs { get; set; }
            public DbSet<InfeksiDetail> InfeksiDetails { get; set; }
            public DbSet<UlkusDebitus> UlkusDebituss { get; set; }
            public DbSet<Pneumonia> Pneumonias { get; set; }
            public DbSet<KulturDarah> KulturDarahs { get; set; }
            public DbSet<IGDObservasi> IGDObservasis { get; set; }
            public DbSet<IGDObservasiDetail> IGDObservasiDetails { get; set; }
            public DbSet<IGDTindakLanjut> IGDTindakLanjuts { get; set; }

            #endregion

            #region HemodialisaDD
            public DbSet<HemodialisaHasil> HemodialisaHasils { get; set; }
            public DbSet<MonitoringHD> MonitoringHDs { get; set; }

            #endregion

            #region Gizi
            public DbSet<GiziDiagnosa> GiziDiagnosas { get; set; }
            public DbSet<GiziEvaluasi> GiziEvaluasis { get; set; }
            public DbSet<GiziAssessment> GiziAssessments { get; set; }
            public DbSet<GiziKonsultasi> GiziKonsultasis { get; set; }
            public DbSet<Recall> Recalls { get; set; }
            public DbSet<RecallDetail> RecallDetails { get; set; }
            #endregion

            #region MCU
            public DbSet<PaketMCU> PaketMCUs { get; set; }
            public DbSet<ModulMCU> ModulMCUs { get; set; }
            #endregion

            #region ALKES
            public DbSet<AlatPemakaian> AlatPemakaians { get; set; }
            public DbSet<AlatPemakaianDetail> AlatPemakaianDetails { get; set; }

        #endregion
        #endregion

        #region Finance


            #region AR
            public DbSet<ARHeader> ARHeaders { get; set; }
            public DbSet<ARDetail> ARDetails { get; set; }
            public DbSet<ARDokumen> ARDokumens { get; set; }
            //public DbSet<ARSettlement> ARSettlements { get; set; }
            //public DbSet<ARSettlementDetail> ARSettlementDetails { get; set; }
            public DbSet<ARCanceled> ARCanceleds { get; set; }
            #endregion

            #region Po
            public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
                public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

                #endregion

            #region COA
            public DbSet<MasterCoa> MasterCoas { get; set; }
            public DbSet<MasterGrup> MasterGrups { get; set; }
            public DbSet<TipeAkun> TipeAkuns { get; set; }

            #endregion

            #region Pembayaran
            public DbSet<DetailDokumenReceived> DetailDokumenReceiveds { get; set; }
            public DbSet<DetailInvoiceReceived> DetailInvoiceReceiveds { get; set; }
            public DbSet<DetailReceivedPayment> DetailReceivedPayments { get; set; }
            public DbSet<ReceivedPayment> ReceivedPayments { get; set; }

        #endregion

            #region AyatSilang
            public DbSet<AyatSilang> AyatSilangs { get; set; }
            public DbSet<DokAyatSilang> DokAyatSilangs { get; set; }
            public DbSet<TransaksiAyatSilang> TransaksiAyatSilangs { get; set; }
            public DbSet<CanceledReceivedPayment> CanceledReceivedPayments { get; set; }

            #endregion

        public DbSet<MasterBank> MasterBanks { get; set; }
        public DbSet<TukarFaktur> TukarFakturs { get; set; }
        public DbSet<DetailTukarFaktur> DetailTukarFakturs { get; set; }
        #endregion
    }
}