using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PengawasanHarianController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PengawasanHarianController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PengawasanHarianController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PengawasanHarianController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        private DateTime? TryParseTanggalToUtc(string tanggal)
        {
            if (DateTime.TryParseExact(
                    tanggal,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                var now = DateTime.Now; // atau DateTime.UtcNow jika kamu mau jam UTC
                var finalDateTime = new DateTime(
                    parsedDate.Year,
                    parsedDate.Month,
                    parsedDate.Day,
                    now.Hour,
                    now.Minute,
                    now.Second,
                    DateTimeKind.Local
                ); // atau Utc jika perlu

                return finalDateTime.ToUniversalTime(); // simpan dalam UTC
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Step 1: Ambil data Pengawasan Harian
            var query = from a in _applicationDbContext.PengawasanHarians
                        join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()
                        where a.IsDelete == false || a.IsDelete == null
                        orderby a.CreateDateTime descending
                        select new
                        {
                            a.PengawasanHarianId,
                            a.KunjunganId,
                            a.PasienId,
                            a.TglPengawasanHarian,
                            a.WaktuPengawasan,
                            a.IsRelaksasi,
                            a.IsKompres,
                            a.IsDetailKompres,
                            a.IsPijatan,
                            a.IsTens,
                            a.IsIstirahat,
                            a.IsMusik,
                            a.IsTeraphyAktivitas,
                            a.IsLatihanOtot,
                            a.IntakeInfuse,
                            a.IntakeOral,
                            a.IntakeNGT,
                            a.IntakeDarah,
                            a.IntakeObat,
                            a.TotalIntake,
                            a.OutputUrin,
                            a.OutputFeses,
                            a.OutputNGT,
                            a.OutputWL,
                            a.TotalOutput,
                            a.BalanceShift,
                            a.Balance24H,
                            a.GulaDarah,
                            a.AsupanMakanan,
                            a.Diet,
                            a.LingkarPerut,
                            a.MobilisasiPasien,
                            a.Keterangan,
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName
                        };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var pengawasanList = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!pengawasanList.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Step 2: Ambil semua KunjunganId
            var kunjunganIds = pengawasanList.Select(p => p.KunjunganId).Distinct().ToList();

            // ✅ Ambil VitalSign
            var vitalSigns = await _applicationDbContext.VitalSigns
                .Where(v => kunjunganIds.Contains((Guid)v.KunjunganId))
                .Select(v => new
                {
                    v.KunjunganId,
                    v.VitalSignId,
                    v.Suhu,
                    v.HR,
                    v.RR,
                    v.TekananDarahSystolic,
                    v.TekananDarahDiastolic,
                    v.SaturasiOksigen,
                    v.Height,
                    v.Weight,
                    v.BMI,
                    v.LingkarKepalaBayi,
                    v.RanapId,
                    v.Nadi
                }).ToListAsync();

            // ✅ Ambil PainAssessment
            var painAssessments = await _applicationDbContext.PainAssessments
                .Where(p => kunjunganIds.Contains((Guid)p.KunjunganId))
                .Select(p => new
                {
                    p.KunjunganId,
                    p.PainAssessmentId,
                    p.KeluhanUtama,
                    p.IsPain,
                    p.Pemicu,
                    p.Kualitas,
                    p.Lokasi,
                    p.SkalaPainId,
                    p.Frekuensi,
                    p.PainManagement,
                    p.IsInheritedDisease,
                    p.InheritedDisease,
                    p.IsAlergic,
                    p.Alergic,
                    p.NafsuMakan,
                    p.IsMual,
                    p.IsMuntah,
                    p.IsFallRisk,
                    p.FallRisk,
                    p.IsBCGimunisasi,
                    p.IsHepatitisBImunisasi,
                    p.IsPolioImunisasi,
                    p.IsDPTImunisasi,
                    p.IsCampakImunisasi,
                    p.IsAsiEksklusif,
                    p.StatusMpasi,
                    p.IsAtaksia,
                    p.IsPosturalInstability,
                    p.HasilResikoJatuh,
                    p.IsMotorikAktif,
                    p.IsResponsAuditori,
                    p.IsInteraksiSosial,
                    p.RanapId,
                    p.RPD,
                    p.RPS,
                    p.CurrentMedication
                }).ToListAsync();

            // ✅ Ambil Resep (utama)
            var resepList = await _applicationDbContext.Reseps
                .Where(r => kunjunganIds.Contains((Guid)r.KunjunganId))
                .Select(r => new
                {
                    r.ResepId,
                    r.KunjunganId,
                    r.AntrianRegistrasi,
                    r.AntrianResep,
                    r.AsuransiId,
                    r.NamaAsuransi,
                    r.PasienId,
                    r.NamaPasien,
                    r.PoliklinikId,
                    r.NamaPoliklinik,
                    r.DokterId,
                    r.NamaDokter,
                    r.StatusPembuatanResep,
                    r.StatusPengambilanResep,
                    r.IsCancelled,
                    r.IsLunas,
                    r.RanapId,
                    TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ?
                                        r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null
                })
                .ToListAsync();

            var resepIds = resepList.Select(r => r.ResepId).ToList();

            // ✅ Ambil Detail Obat
            var detailObat = await (from d in _applicationDbContext.DetailReseps
                                    join o in _applicationDbContext.Obats
                                        on d.ObatId equals o.ObatId into obatJoin
                                    from o in obatJoin.DefaultIfEmpty()
                                    where resepIds.Contains((Guid)d.ResepId) && (d.IsRacikan == false || d.IsRacikan == null)
                                    select new
                                    {
                                        d.ResepId,
                                        d.DetailResepId,
                                        d.ObatId,
                                        ObatName = o != null ? o.ObatName : null,
                                        d.Qty,
                                        d.HargaObat,
                                        d.TotalHargaObat,
                                        d.Signa,
                                        d.SignaTambahan,
                                        d.TakaranDosis,
                                        d.IsIteratur,
                                        d.JumlahIteratur,
                                        TglMulaiIteratur = d.TglMulaiIteratur.HasValue ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                                        MasaAktifIteratur = d.MasaAktifIteratur.HasValue ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                                        d.CaraPemakaian,
                                        d.EstimasiPemberian,
                                        d.TglStopPemakaian,
                                        d.IsObatDibawaPlg
                                    }).ToListAsync();

            // ✅ Ambil Detail Racikan
            var detailRacikan = await (from d in _applicationDbContext.DetailReseps
                                       join ra in _applicationDbContext.Racikans
                                           on d.RacikanId equals ra.RacikanId
                                       where resepIds.Contains((Guid)d.ResepId) && d.IsRacikan == true
                                       select new
                                       {
                                           d.ResepId,
                                           ra.RacikanId,
                                           ra.NamaRacikan,
                                           d.Qty,
                                           d.Signa,
                                           d.SignaTambahan,
                                           d.HargaObat,
                                           d.TotalHargaObat,
                                           d.CaraPemakaian,
                                           d.EstimasiPemberian,
                                           d.StatusDiberikanPasien,
                                           d.TglStopPemakaian,
                                           ra.Keterangan
                                       }).ToListAsync();

            var racikanIds = detailRacikan.Select(r => r.RacikanId).Distinct().ToList();

            // ✅ Ambil RacikanDetails
            var racikanDetails = await (from rd in _applicationDbContext.RacikanDetails
                                        join o in _applicationDbContext.Obats
                                            on rd.ObatId equals o.ObatId into obatJoin
                                        from o in obatJoin.DefaultIfEmpty()
                                        where racikanIds.Contains((Guid)rd.RacikanId)
                                        select new
                                        {
                                            rd.RacikanId,
                                            rd.DetailRacikanId,
                                            rd.ObatId,
                                            ObatName = o != null ? o.ObatName : null,
                                            rd.KomposisiDosis,
                                        }).ToListAsync();

            // ✅ Grouping hasil akhir
            var data = pengawasanList.Select(p => new
            {
                p.PengawasanHarianId,
                p.KunjunganId,
                p.PasienId,
                p.TglPengawasanHarian,
                p.WaktuPengawasan,
                p.IsRelaksasi,
                p.IsKompres,
                p.IsDetailKompres,
                p.IsPijatan,
                p.IsTens,
                p.IsIstirahat,
                p.IsMusik,
                p.IsTeraphyAktivitas,
                p.IsLatihanOtot,
                p.IntakeInfuse,
                p.IntakeOral,
                p.IntakeNGT,
                p.IntakeDarah,
                p.IntakeObat,
                p.TotalIntake,
                p.OutputUrin,
                p.OutputFeses,
                p.OutputNGT,
                p.OutputWL,
                p.TotalOutput,
                p.BalanceShift,
                p.Balance24H,
                p.GulaDarah,
                p.AsupanMakanan,
                p.Diet,
                p.LingkarPerut,
                p.MobilisasiPasien,
                p.Keterangan,
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,

                VitalSigns = vitalSigns.Where(v => v.KunjunganId == p.KunjunganId).ToList(),
                PainAssessments = painAssessments.Where(pa => pa.KunjunganId == p.KunjunganId).ToList(),

                Reseps = resepList.Where(r => r.KunjunganId == p.KunjunganId)
                                  .Select(r => new
                                  {
                                      r.ResepId,
                                      r.AntrianRegistrasi,
                                      r.AntrianResep,
                                      r.AsuransiId,
                                      r.NamaAsuransi,
                                      r.PasienId,
                                      r.NamaPasien,
                                      r.PoliklinikId,
                                      r.NamaPoliklinik,
                                      r.DokterId,
                                      r.NamaDokter,
                                      r.StatusPembuatanResep,
                                      r.StatusPengambilanResep,
                                      r.IsCancelled,
                                      r.IsLunas,
                                      r.RanapId,
                                      r.TanggalPembuatanResepFormatted,

                                      DaftarObat = detailObat.Where(d => d.ResepId == r.ResepId).ToList(),
                                      DaftarRacikan = detailRacikan.Where(dr => dr.ResepId == r.ResepId)
                                          .Select(dr => new
                                          {
                                              dr.RacikanId,
                                              dr.NamaRacikan,
                                              dr.Qty,
                                              dr.Signa,
                                              dr.SignaTambahan,
                                              dr.HargaObat,
                                              dr.TotalHargaObat,
                                              dr.CaraPemakaian,
                                              dr.EstimasiPemberian,
                                              dr.StatusDiberikanPasien,
                                              dr.TglStopPemakaian,
                                              dr.Keterangan,
                                              DaftarRacikanDetail = racikanDetails.Where(rd => rd.RacikanId == dr.RacikanId).ToList()
                                          }).ToList()
                                  }).ToList()
            });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Ambil data utama Pengawasan Harian
            var pengawasan = await (from a in _applicationDbContext.PengawasanHarians
                                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userJoin
                                    from u in userJoin.DefaultIfEmpty()
                                    where a.PengawasanHarianId == id && (a.IsDelete == false || a.IsDelete == null)
                                    select new
                                    {
                                        a.PengawasanHarianId,
                                        a.KunjunganId,
                                        a.PasienId,
                                        a.TglPengawasanHarian,
                                        a.WaktuPengawasan,
                                        a.IsRelaksasi,
                                        a.IsKompres,
                                        a.IsDetailKompres,
                                        a.IsPijatan,
                                        a.IsTens,
                                        a.IsIstirahat,
                                        a.IsMusik,
                                        a.IsTeraphyAktivitas,
                                        a.IsLatihanOtot,
                                        a.IntakeInfuse,
                                        a.IntakeOral,
                                        a.IntakeNGT,
                                        a.IntakeDarah,
                                        a.IntakeObat,
                                        a.TotalIntake,
                                        a.OutputUrin,
                                        a.OutputFeses,
                                        a.OutputNGT,
                                        a.OutputWL,
                                        a.TotalOutput,
                                        a.BalanceShift,
                                        a.Balance24H,
                                        a.GulaDarah,
                                        a.AsupanMakanan,
                                        a.Diet,
                                        a.LingkarPerut,
                                        a.MobilisasiPasien,
                                        a.Keterangan,
                                        a.CreateDateTime,
                                        a.CreateBy,
                                        CreateByName = u.FullName
                                    }).FirstOrDefaultAsync();

            if (pengawasan == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            // Ambil KunjunganId
            var kunjunganId = pengawasan.KunjunganId;

            // ✅ VitalSigns
            var vitalSigns = await _applicationDbContext.VitalSigns
                .Where(v => v.KunjunganId == kunjunganId)
                .Select(v => new
                {
                    v.VitalSignId,
                    v.Suhu,
                    v.HR,
                    v.RR,
                    v.TekananDarahSystolic,
                    v.TekananDarahDiastolic,
                    v.SaturasiOksigen,
                    v.Height,
                    v.Weight,
                    v.BMI,
                    v.LingkarKepalaBayi,
                    v.RanapId,
                    v.Nadi
                }).ToListAsync();

            // ✅ PainAssessments
            var painAssessments = await _applicationDbContext.PainAssessments
                .Where(p => p.KunjunganId == kunjunganId)
                .Select(p => new
                {
                    p.PainAssessmentId,
                    p.KeluhanUtama,
                    p.IsPain,
                    p.Pemicu,
                    p.Kualitas,
                    p.Lokasi,
                    p.SkalaPainId,
                    p.Frekuensi,
                    p.PainManagement,
                    p.IsInheritedDisease,
                    p.InheritedDisease,
                    p.IsAlergic,
                    p.Alergic,
                    p.NafsuMakan,
                    p.IsMual,
                    p.IsMuntah,
                    p.IsFallRisk,
                    p.FallRisk,
                    p.IsBCGimunisasi,
                    p.IsHepatitisBImunisasi,
                    p.IsPolioImunisasi,
                    p.IsDPTImunisasi,
                    p.IsCampakImunisasi,
                    p.IsAsiEksklusif,
                    p.StatusMpasi,
                    p.IsAtaksia,
                    p.IsPosturalInstability,
                    p.HasilResikoJatuh,
                    p.IsMotorikAktif,
                    p.IsResponsAuditori,
                    p.IsInteraksiSosial,
                    p.RanapId,
                    p.RPD,
                    p.RPS,
                    p.CurrentMedication
                }).ToListAsync();

            // ✅ Resep
            var resepList = await _applicationDbContext.Reseps
                .Where(r => r.KunjunganId == kunjunganId)
                .Select(r => new
                {
                    r.ResepId,
                    r.KunjunganId,
                    r.AntrianRegistrasi,
                    r.AntrianResep,
                    r.AsuransiId,
                    r.NamaAsuransi,
                    r.PasienId,
                    r.NamaPasien,
                    r.PoliklinikId,
                    r.NamaPoliklinik,
                    r.DokterId,
                    r.NamaDokter,
                    r.StatusPembuatanResep,
                    r.StatusPengambilanResep,
                    r.IsCancelled,
                    r.IsLunas,
                    r.RanapId,
                    TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ?
                                        r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null
                })
                .ToListAsync();

            var resepIds = resepList.Select(r => r.ResepId).ToList();

            // ✅ Detail Obat
            var detailObat = await (from d in _applicationDbContext.DetailReseps
                                    join o in _applicationDbContext.Obats on d.ObatId equals o.ObatId into obatJoin
                                    from o in obatJoin.DefaultIfEmpty()
                                    where resepIds.Contains((Guid)d.ResepId) && (d.IsRacikan == false || d.IsRacikan == null)
                                    select new
                                    {
                                        d.ResepId,
                                        d.DetailResepId,
                                        d.ObatId,
                                        ObatName = o != null ? o.ObatName : null,
                                        d.Qty,
                                        d.HargaObat,
                                        d.TotalHargaObat,
                                        d.Signa,
                                        d.SignaTambahan,
                                        d.TakaranDosis,
                                        d.IsIteratur,
                                        d.JumlahIteratur,
                                        TglMulaiIteratur = d.TglMulaiIteratur.HasValue ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                                        MasaAktifIteratur = d.MasaAktifIteratur.HasValue ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                                        d.CaraPemakaian,
                                        d.EstimasiPemberian,
                                        d.TglStopPemakaian,
                                        d.IsObatDibawaPlg
                                    }).ToListAsync();

            // ✅ Racikan
            var detailRacikan = await (from d in _applicationDbContext.DetailReseps
                                       join ra in _applicationDbContext.Racikans on d.RacikanId equals ra.RacikanId
                                       where resepIds.Contains((Guid)d.ResepId) && d.IsRacikan == true
                                       select new
                                       {
                                           d.ResepId,
                                           ra.RacikanId,
                                           ra.NamaRacikan,
                                           d.Qty,
                                           d.Signa,
                                           d.SignaTambahan,
                                           d.HargaObat,
                                           d.TotalHargaObat,
                                           d.CaraPemakaian,
                                           d.EstimasiPemberian,
                                           d.StatusDiberikanPasien,
                                           d.TglStopPemakaian,
                                           ra.Keterangan
                                       }).ToListAsync();

            var racikanIds = detailRacikan.Select(r => r.RacikanId).Distinct().ToList();

            var racikanDetails = await (from rd in _applicationDbContext.RacikanDetails
                                        join o in _applicationDbContext.Obats on rd.ObatId equals o.ObatId into obatJoin
                                        from o in obatJoin.DefaultIfEmpty()
                                        where racikanIds.Contains((Guid)rd.RacikanId)
                                        select new
                                        {
                                            rd.RacikanId,
                                            rd.DetailRacikanId,
                                            rd.ObatId,
                                            ObatName = o != null ? o.ObatName : null,
                                            rd.KomposisiDosis,
                                        }).ToListAsync();

            // ✅ Build final object
            var data = new
            {
                pengawasan.PengawasanHarianId,
                pengawasan.KunjunganId,
                pengawasan.PasienId,
                pengawasan.TglPengawasanHarian,
                pengawasan.WaktuPengawasan,
                pengawasan.IsRelaksasi,
                pengawasan.IsKompres,
                pengawasan.IsDetailKompres,
                pengawasan.IsPijatan,
                pengawasan.IsTens,
                pengawasan.IsIstirahat,
                pengawasan.IsMusik,
                pengawasan.IsTeraphyAktivitas,
                pengawasan.IsLatihanOtot,
                pengawasan.IntakeInfuse,
                pengawasan.IntakeOral,
                pengawasan.IntakeNGT,
                pengawasan.IntakeDarah,
                pengawasan.IntakeObat,
                pengawasan.TotalIntake,
                pengawasan.OutputUrin,
                pengawasan.OutputFeses,
                pengawasan.OutputNGT,
                pengawasan.OutputWL,
                pengawasan.TotalOutput,
                pengawasan.BalanceShift,
                pengawasan.Balance24H,
                pengawasan.GulaDarah,
                pengawasan.AsupanMakanan,
                pengawasan.Diet,
                pengawasan.LingkarPerut,
                pengawasan.MobilisasiPasien,
                pengawasan.Keterangan,
                pengawasan.CreateDateTime,
                pengawasan.CreateBy,
                pengawasan.CreateByName,

                VitalSigns = vitalSigns,
                PainAssessments = painAssessments,

                Reseps = resepList.Select(r => new
                {
                    r.ResepId,
                    r.AntrianRegistrasi,
                    r.AntrianResep,
                    r.AsuransiId,
                    r.NamaAsuransi,
                    r.PasienId,
                    r.NamaPasien,
                    r.PoliklinikId,
                    r.NamaPoliklinik,
                    r.DokterId,
                    r.NamaDokter,
                    r.StatusPembuatanResep,
                    r.StatusPengambilanResep,
                    r.IsCancelled,
                    r.IsLunas,
                    r.RanapId,
                    r.TanggalPembuatanResepFormatted,

                    DaftarObat = detailObat.Where(d => d.ResepId == r.ResepId).ToList(),
                    DaftarRacikan = detailRacikan.Where(dr => dr.ResepId == r.ResepId)
                        .Select(dr => new
                        {
                            dr.RacikanId,
                            dr.NamaRacikan,
                            dr.Qty,
                            dr.Signa,
                            dr.SignaTambahan,
                            dr.HargaObat,
                            dr.TotalHargaObat,
                            dr.CaraPemakaian,
                            dr.EstimasiPemberian,
                            dr.StatusDiberikanPasien,
                            dr.TglStopPemakaian,
                            dr.Keterangan,
                            DaftarRacikanDetail = racikanDetails.Where(rd => rd.RacikanId == dr.RacikanId).ToList()
                        }).ToList()
                }).ToList()
            };

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data
            });
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PengawasanHarianViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                //// **Cek Duplikasi**
                //bool isDuplicate = _applicationDbContext.Diskons
                //                    .Any(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new PengawasanHarian
                {
                    PengawasanHarianId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    TglPengawasanHarian = vm.TglPengawasanHarian,
                    WaktuPengawasan = vm.WaktuPengawasan,
                    IsRelaksasi = vm.IsRelaksasi,
                    IsKompres = vm.IsKompres,
                    IsDetailKompres = vm.IsDetailKompres,
                    IsPijatan = vm.IsPijatan,
                    IsTens = vm.IsTens,
                    IsIstirahat = vm.IsIstirahat,
                    IsMusik = vm.IsMusik,
                    IsTeraphyAktivitas = vm.IsTeraphyAktivitas,
                    IntakeInfuse = vm.IntakeInfuse,
                    IntakeOral = vm.IntakeOral,
                    IntakeNGT = vm.IntakeNGT,
                    IntakeDarah = vm.IntakeDarah,
                    IntakeObat = vm.IntakeObat,
                    TotalIntake = vm.TotalIntake,
                    OutputUrin = vm.OutputUrin,
                    OutputFeses = vm.OutputFeses,
                    OutputNGT = vm.OutputNGT,
                    OutputWL = vm.OutputWL,
                    TotalOutput = vm.TotalOutput,
                    Balance24H = vm.Balance24H,
                    BalanceShift = vm.BalanceShift,
                    GulaDarah = vm.GulaDarah,
                    AsupanMakanan = vm.AsupanMakanan,
                    Diet = vm.Diet,
                    LingkarPerut = vm.LingkarPerut,
                    MobilisasiPasien = vm.MobilisasiPasien,
                    Keterangan = vm.Keterangan,
    
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.PengawasanHarians.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PengawasanHarianViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data**
                var data = await _applicationDbContext.PengawasanHarians.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.TglPengawasanHarian = vm.TglPengawasanHarian;
                data.WaktuPengawasan = vm.WaktuPengawasan;

                data.IsRelaksasi = vm.IsRelaksasi;
                data.IsKompres = vm.IsKompres;
                data.IsDetailKompres = vm.IsDetailKompres;
                data.IsPijatan = vm.IsPijatan;
                data.IsTens = vm.IsTens;
                data.IsIstirahat = vm.IsIstirahat;
                data.IsMusik = vm.IsMusik;
                data.IsTeraphyAktivitas = vm.IsTeraphyAktivitas;
                data.IsLatihanOtot = vm.IsLatihanOtot;

                data.IntakeInfuse = vm.IntakeInfuse;
                data.IntakeOral = vm.IntakeOral;
                data.IntakeNGT = vm.IntakeNGT;
                data.IntakeDarah = vm.IntakeDarah;
                data.IntakeObat = vm.IntakeObat;
                data.TotalIntake = vm.TotalIntake;

                data.OutputUrin = vm.OutputUrin;
                data.OutputFeses = vm.OutputFeses;
                data.OutputNGT = vm.OutputNGT;
                data.OutputWL = vm.OutputWL;
                data.TotalOutput = vm.TotalOutput;

                data.BalanceShift = vm.BalanceShift;
                data.Balance24H = vm.Balance24H;
                data.GulaDarah = vm.GulaDarah;
                data.AsupanMakanan = vm.AsupanMakanan;
                data.Diet = vm.Diet;
                data.LingkarPerut = vm.LingkarPerut;
                data.MobilisasiPasien = vm.MobilisasiPasien;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.PengawasanHarians.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data**
                var data = await _applicationDbContext.PengawasanHarians.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.PengawasanHarians.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // ✅ Query utama PengawasanHarian
            var baseQuery = from a in _applicationDbContext.PengawasanHarians
                            join u in _applicationDbContext.UserActives
                                on a.CreateBy equals u.UserActiveId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where a.IsDelete == false || a.IsDelete == null
                            orderby a.CreateDateTime descending
                            select new
                            {
                                a.PengawasanHarianId,
                                a.KunjunganId,
                                a.PasienId,
                                a.TglPengawasanHarian,
                                a.WaktuPengawasan,
                                a.IsRelaksasi,
                                a.IsKompres,
                                a.IsDetailKompres,
                                a.IsPijatan,
                                a.IsTens,
                                a.IsIstirahat,
                                a.IsMusik,
                                a.IsTeraphyAktivitas,
                                a.IsLatihanOtot,
                                a.IntakeInfuse,
                                a.IntakeOral,
                                a.IntakeNGT,
                                a.IntakeDarah,
                                a.IntakeObat,
                                a.TotalIntake,
                                a.OutputUrin,
                                a.OutputFeses,
                                a.OutputNGT,
                                a.OutputWL,
                                a.TotalOutput,
                                a.BalanceShift,
                                a.Balance24H,
                                a.GulaDarah,
                                a.AsupanMakanan,
                                a.Diet,
                                a.LingkarPerut,
                                a.MobilisasiPasien,
                                a.Keterangan,
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u.FullName
                            };

            // ✅ Hitung total untuk pagination
            var totalRows = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var pengawasanList = await baseQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!pengawasanList.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // ✅ Ambil semua KunjunganId untuk batch query
            var kunjunganIds = pengawasanList.Select(x => x.KunjunganId).ToList();

            // ✅ Ambil VitalSigns
            var vitalSigns = await _applicationDbContext.VitalSigns
                .Where(v => kunjunganIds.Contains((Guid)v.KunjunganId))
                .Select(v => new
                {
                    v.VitalSignId,
                    v.KunjunganId,
                    v.Suhu,
                    v.HR,
                    v.RR,
                    v.TekananDarahSystolic,
                    v.TekananDarahDiastolic,
                    v.SaturasiOksigen,
                    v.Height,
                    v.Weight,
                    v.BMI,
                    v.LingkarKepalaBayi,
                    v.RanapId,
                    v.Nadi
                }).ToListAsync();

            // ✅ Ambil PainAssessments
            var painAssessments = await _applicationDbContext.PainAssessments
                .Where(p => kunjunganIds.Contains((Guid)p.KunjunganId))
                .Select(p => new
                {
                    p.PainAssessmentId,
                    p.KunjunganId,
                    p.KeluhanUtama,
                    p.IsPain,
                    p.Pemicu,
                    p.Kualitas,
                    p.Lokasi,
                    p.SkalaPainId,
                    p.Frekuensi,
                    p.PainManagement,
                    p.IsInheritedDisease,
                    p.InheritedDisease,
                    p.IsAlergic,
                    p.Alergic,
                    p.NafsuMakan,
                    p.IsMual,
                    p.IsMuntah,
                    p.IsFallRisk,
                    p.FallRisk,
                    p.IsBCGimunisasi,
                    p.IsHepatitisBImunisasi,
                    p.IsPolioImunisasi,
                    p.IsDPTImunisasi,
                    p.IsCampakImunisasi,
                    p.IsAsiEksklusif,
                    p.StatusMpasi,
                    p.IsAtaksia,
                    p.IsPosturalInstability,
                    p.HasilResikoJatuh,
                    p.IsMotorikAktif,
                    p.IsResponsAuditori,
                    p.IsInteraksiSosial,
                    p.RanapId,
                    p.RPD,
                    p.RPS,
                    p.CurrentMedication
                }).ToListAsync();

            // ✅ Ambil Resep
            var resepList = await _applicationDbContext.Reseps
                .Where(r => kunjunganIds.Contains((Guid)r.KunjunganId))
                .Select(r => new
                {
                    r.ResepId,
                    r.KunjunganId,
                    r.AntrianRegistrasi,
                    r.AntrianResep,
                    r.AsuransiId,
                    r.NamaAsuransi,
                    r.PasienId,
                    r.NamaPasien,
                    r.PoliklinikId,
                    r.NamaPoliklinik,
                    r.DokterId,
                    r.NamaDokter,
                    r.StatusPembuatanResep,
                    r.StatusPengambilanResep,
                    r.IsCancelled,
                    r.IsLunas,
                    r.RanapId,
                    TanggalPembuatanResepFormatted = r.TanggalPembuatanResep.HasValue ?
                                                      r.TanggalPembuatanResep.Value.ToString("yyyy-MM-dd") : null
                }).ToListAsync();

            var resepIds = resepList.Select(r => r.ResepId).ToList();

            // ✅ Ambil Detail Obat (non-racikan)
            var detailObat = await (from d in _applicationDbContext.DetailReseps
                                    join o in _applicationDbContext.Obats
                                        on d.ObatId equals o.ObatId into obatJoin
                                    from o in obatJoin.DefaultIfEmpty()
                                    where resepIds.Contains((Guid)d.ResepId) && (d.IsRacikan == false || d.IsRacikan == null)
                                    select new
                                    {
                                        d.ResepId,
                                        d.DetailResepId,
                                        d.ObatId,
                                        ObatName = o != null ? o.ObatName : null,
                                        d.Qty,
                                        d.HargaObat,
                                        d.TotalHargaObat,
                                        d.Signa,
                                        d.SignaTambahan,
                                        d.TakaranDosis,
                                        d.IsIteratur,
                                        d.JumlahIteratur,
                                        TglMulaiIteratur = d.TglMulaiIteratur.HasValue ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                                        MasaAktifIteratur = d.MasaAktifIteratur.HasValue ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                                        d.CaraPemakaian,
                                        d.EstimasiPemberian,
                                        d.TglStopPemakaian,
                                        d.IsObatDibawaPlg
                                    }).ToListAsync();

            // ✅ Ambil Racikan
            var detailRacikan = await (from d in _applicationDbContext.DetailReseps
                                       join ra in _applicationDbContext.Racikans
                                           on d.RacikanId equals ra.RacikanId
                                       where resepIds.Contains((Guid)d.ResepId) && d.IsRacikan == true
                                       select new
                                       {
                                           d.ResepId,
                                           ra.RacikanId,
                                           ra.NamaRacikan,
                                           d.Qty,
                                           d.Signa,
                                           d.SignaTambahan,
                                           d.HargaObat,
                                           d.TotalHargaObat,
                                           d.CaraPemakaian,
                                           d.EstimasiPemberian,
                                           d.StatusDiberikanPasien,
                                           d.TglStopPemakaian,
                                           ra.Keterangan
                                       }).ToListAsync();

            var racikanIds = detailRacikan.Select(r => r.RacikanId).Distinct().ToList();

            // ✅ Ambil RacikanDetail
            var racikanDetails = await (from rd in _applicationDbContext.RacikanDetails
                                        join o in _applicationDbContext.Obats
                                            on rd.ObatId equals o.ObatId into obatJoin
                                        from o in obatJoin.DefaultIfEmpty()
                                        where racikanIds.Contains((Guid)rd.RacikanId)
                                        select new
                                        {
                                            rd.RacikanId,
                                            rd.DetailRacikanId,
                                            rd.ObatId,
                                            ObatName = o != null ? o.ObatName : null,
                                            rd.KomposisiDosis,
                                        }).ToListAsync();

            // ✅ Build final data per pengawasan
            var data = pengawasanList.Select(p => new
            {
                p.PengawasanHarianId,
                p.KunjunganId,
                p.PasienId,
                p.TglPengawasanHarian,
                p.WaktuPengawasan,
                p.IsRelaksasi,
                p.IsKompres,
                p.IsDetailKompres,
                p.IsPijatan,
                p.IsTens,
                p.IsIstirahat,
                p.IsMusik,
                p.IsTeraphyAktivitas,
                p.IsLatihanOtot,
                p.IntakeInfuse,
                p.IntakeOral,
                p.IntakeNGT,
                p.IntakeDarah,
                p.IntakeObat,
                p.TotalIntake,
                p.OutputUrin,
                p.OutputFeses,
                p.OutputNGT,
                p.OutputWL,
                p.TotalOutput,
                p.BalanceShift,
                p.Balance24H,
                p.GulaDarah,
                p.AsupanMakanan,
                p.Diet,
                p.LingkarPerut,
                p.MobilisasiPasien,
                p.Keterangan,
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,

                VitalSigns = vitalSigns.Where(v => v.KunjunganId == p.KunjunganId).ToList(),
                PainAssessments = painAssessments.Where(v => v.KunjunganId == p.KunjunganId).ToList(),
                Reseps = resepList.Where(r => r.KunjunganId == p.KunjunganId).Select(r => new
                {
                    r.ResepId,
                    r.AntrianRegistrasi,
                    r.AntrianResep,
                    r.AsuransiId,
                    r.NamaAsuransi,
                    r.PasienId,
                    r.NamaPasien,
                    r.PoliklinikId,
                    r.NamaPoliklinik,
                    r.DokterId,
                    r.NamaDokter,
                    r.StatusPembuatanResep,
                    r.StatusPengambilanResep,
                    r.IsCancelled,
                    r.IsLunas,
                    r.RanapId,
                    r.TanggalPembuatanResepFormatted,
                    DaftarObat = detailObat.Where(d => d.ResepId == r.ResepId).ToList(),
                    DaftarRacikan = detailRacikan.Where(dr => dr.ResepId == r.ResepId)
                        .Select(dr => new
                        {
                            dr.RacikanId,
                            dr.NamaRacikan,
                            dr.Qty,
                            dr.Signa,
                            dr.SignaTambahan,
                            dr.HargaObat,
                            dr.TotalHargaObat,
                            dr.CaraPemakaian,
                            dr.EstimasiPemberian,
                            dr.StatusDiberikanPasien,
                            dr.TglStopPemakaian,
                            dr.Keterangan,
                            DaftarRacikanDetail = racikanDetails.Where(rd => rd.RacikanId == dr.RacikanId).ToList()
                        }).ToList()
                }).ToList()
            });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }


    }
}
