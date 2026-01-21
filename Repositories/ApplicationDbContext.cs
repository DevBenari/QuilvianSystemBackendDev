using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
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

            modelBuilder.Entity<PendaftaranPasienBaru>()
                .HasIndex(x => x.NoRekamMedis)
                .IsUnique();
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
        #endregion

        #region Areas Keuangan
        public DbSet<MetodePembayaran> MetodePembayarans { get; set; }
        public DbSet<Diskon> Diskons { get; set; }
        public DbSet<BiayaAdministrasi> BiayaAdministrasis { get; set; }
        public DbSet<MainKasir> MainKasirs { get; set; }
        public DbSet<KasirTebusResep> KasirTebusReseps { get; set; }
        public DbSet<MainKasirDetail> MainKasirDetails { get; set; }
        public DbSet<Billing> Billings { get; set; }
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

        #endregion

        #region Areas Administrator
        public DbSet<JenisUser> JenisUsers { get; set; }
        public DbSet<JenisPembayaran> JenisPembayarans { get; set; }
        public DbSet<Pembayaran> Pembayarans { get; set; }
        public DbSet<Fingerprint> Fingerprints { get; set; }


        #endregion

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

        #endregion

        #region OperasiOK

        public DbSet<PraOperasi> PraOperasis { get; set; }
        public DbSet<OperasiTindakan> OperasiTindakans { get; set; }
        public DbSet<RuangBedahBooking> RuangBedahBookings { get; set; }
        public DbSet<RuangBedahBookingDetail> RuangBedahBookingDetails { get; set; }

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
        #endregion

        #region IGD
        public DbSet<IGDTriage> IGDTriages { get; set; }
        public DbSet<IGDTriageDetail> IGDTriageDetails { get; set; }
        public DbSet<IGDPasienDetail> IGDPasienDetails { get; set; }
        public DbSet<IGDTindakanDetail> IGDTindakanDetails { get;set; } 
        public DbSet<IGDAssessmentAwal> IGDAssessmentAwals { get;set; } 
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
        public DbSet<GiziDiagnosa> GiziDiagnosas {  get; set; }
        public DbSet<GiziEvaluasi> GiziEvaluasis {  get; set; }
        public DbSet<GiziAssessment> GiziAssessments {  get; set; }
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
    }
}