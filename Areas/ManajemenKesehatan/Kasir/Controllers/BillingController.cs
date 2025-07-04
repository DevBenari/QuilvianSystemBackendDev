using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<BillingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BillingController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BillingController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBillingById(Guid id)
        {
            var billing = await _applicationDbContext.Billings
                .FirstOrDefaultAsync(b => b.BillingId == id && !b.IsDelete);
            if (billing == null)
                return NotFound(new { message = "Data billing tidak ditemukan!" });
            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = billing
            });
        }

        [HttpGet("GetBillingByKunjunganId/{kunjunganId}")]
        public async Task<IActionResult> GetBillingByKunjunganId(Guid kunjunganId)
        {
            var kunjungan = await _applicationDbContext.Billings.Where(b => b.KunjunganId == kunjunganId && !b.IsDelete).ToListAsync();
            if (kunjungan == null)
                return NotFound(new { message = "Data kunjungan tidak ditemukan!" });

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = kunjungan
            });
        }

        [HttpGet("BillingObat/{kunjunganId}")]
        public async Task<IActionResult> GetResepDetailsByKunjunganIdEntity(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // LEFT JOIN Reseps x DetailReseps
                var resepQuery = await (
                    from r in _applicationDbContext.Reseps
                    where r.KunjunganId == kunjunganId
                    join dr in _applicationDbContext.DetailReseps
                        on r.ResepId equals dr.ResepId into drGroup
                    from dr in drGroup.DefaultIfEmpty() // LEFT JOIN
                    select new
                    {
                        r.KunjunganId,
                        r.AsuransiId,
                        ObatId = dr != null ? dr.ObatId : (Guid?)null,
                        IsRacikan = dr != null ? dr.IsRacikan : (bool?)null,
                        RacikanId = dr != null ? dr.RacikanId : (Guid?)null,
                        Signa = dr != null ? dr.Signa : null,
                        SignaTambahan = dr != null ? dr.SignaTambahan : null
                    }).ToListAsync();

                var result = new List<object>();

                foreach (var item in resepQuery)
                {
                    // Nama Obat
                    var obat = await _applicationDbContext.Obats
                        .Where(o => o.ObatId == item.ObatId)
                        .FirstOrDefaultAsync();

                    // Status cover asuransi (false jika tidak ditemukan)
                    bool isCovered = await _applicationDbContext.ObatAsuransis
                        .AnyAsync(oa => oa.AsuransiId == item.AsuransiId && oa.ObatId == item.ObatId && !oa.IsDelete);

                    // Tentukan ItemId (obat atau racikan)
                    var itemId = (item.IsRacikan == true && item.RacikanId.HasValue)
                        ? item.RacikanId.Value
                        : item.ObatId;

                    var billing = await _applicationDbContext.Billings
                        .Where(b => b.KunjunganId == item.KunjunganId && b.ItemId == itemId)
                        .FirstOrDefaultAsync();

                    var racikan = item.RacikanId.HasValue
                        ? await _applicationDbContext.Racikans
                            .Where(mr => mr.RacikanId == item.RacikanId)
                            .Select(mr => mr.NamaRacikan)
                            .FirstOrDefaultAsync()
                        : null;

                    result.Add(new
                    {
                        billing?.BillingId,
                        item.KunjunganId,
                        item.ObatId,
                        NamaObat = item.IsRacikan == true ? racikan : obat?.ObatName,
                        HargaSatuanObat = billing?.HargaItem,
                        SubTotalObat = item.IsRacikan == true ? billing?.HargaItem : billing?.HargaItem * billing?.QtyItem,
                        item.IsRacikan,
                        item.RacikanId,
                        item.Signa,
                        item.SignaTambahan,
                        IsCoveredByAsuransi = isCovered,
                        BilledQty = billing?.QtyItem,
                        billing?.BillingKode,
                        billing?.JenisBilling,
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("ObatFarmasiByKunjunganId/{kunjunganId}")]
        public async Task<IActionResult> GetObatFarmasiByKunjunganId(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // Ambil data resep utama
                var resep = await _applicationDbContext.Reseps
                    .Where(r => r.KunjunganId == kunjunganId)
                    .OrderByDescending(r => r.CreateDateTime)
                    .FirstOrDefaultAsync();

                if (resep == null)
                    return NotFound(new { message = "Resep tidak ditemukan untuk kunjungan ini." });

                // Ambil detail resep terkait
                var detailList = await (
                    from dr in _applicationDbContext.DetailReseps
                    where dr.ResepId == resep.ResepId
                    select new
                    {
                        dr.ObatId,
                        dr.IsRacikan,
                        dr.RacikanId,
                        dr.Signa,
                        dr.SignaTambahan,
                        dr.StatusPengambilanObat
                    }
                ).ToListAsync();

                var daftarObat = new List<object>();

                foreach (var item in detailList)
                {
                    // Ambil data obat
                    var obat = await _applicationDbContext.Obats
                        .FirstOrDefaultAsync(o => o.ObatId == item.ObatId);

                    // Cek status asuransi
                    bool isCovered = await _applicationDbContext.ObatAsuransis
                        .AnyAsync(oa => oa.AsuransiId == resep.AsuransiId && oa.ObatId == item.ObatId && !oa.IsDelete);

                    var itemId = item.IsRacikan == true ? item.RacikanId : item.ObatId;

                    var billing = await _applicationDbContext.Billings
                        .FirstOrDefaultAsync(b => b.KunjunganId == resep.KunjunganId && b.ItemId == itemId);

                    var namaRacikan = item.IsRacikan == true
                        ? await _applicationDbContext.Racikans
                            .Where(r => r.RacikanId == item.RacikanId)
                            .Select(r => r.NamaRacikan)
                            .FirstOrDefaultAsync()
                        : null;

                    daftarObat.Add(new
                    {
                        billing?.BillingId,
                        item.ObatId,
                        NamaObat = item.IsRacikan == true ? namaRacikan : obat?.ObatName,
                        HargaSatuanObat = billing?.HargaItem,
                        SubTotalObat = item.IsRacikan == true ? billing?.HargaItem : billing?.HargaItem * billing?.QtyItem,
                        item.IsRacikan,
                        item.RacikanId,
                        item.Signa,
                        item.SignaTambahan,
                        IsCoveredByAsuransi = isCovered,
                        BilledQty = billing?.QtyItem,
                        billing?.BillingKode,
                        billing?.JenisBilling,
                        item.StatusPengambilanObat
                    });
                }

                // Return resep + daftar obat
                return Ok(new
                {
                    resep.ResepId,
                    resep.KunjunganId,
                    resep.PasienId,
                    resep.NamaPasien,
                    resep.DokterId,
                    resep.NamaDokter,
                    resep.PoliklinikId,
                    resep.NamaPoliklinik,
                    resep.StatusPembuatanResep,
                    resep.StatusPengambilanResep,
                    resep.IsLunas,
                    resep.IsCancelled,
                    DaftarObat = daftarObat
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("BillingTindakan/{kunjunganId}")]
        public async Task<IActionResult> GetBillingTindakanByKunjunganId(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var tindakanQuery = await (
                    from tk in _applicationDbContext.TindakanKunjungans
                    join k in _applicationDbContext.Kunjungans
                        on tk.KunjunganId equals k.KunjunganID
                    where k.AsuransiId != null // agar aman saat .Value  

                    join mt in _applicationDbContext.Tindakans
                        on tk.TindakanId equals mt.TindakanId

                    join tda in _applicationDbContext.TindakanAsuransis
                        on new { TindakanId = tk.TindakanId, AsuransiId = k.AsuransiId.Value }
                        equals new { TindakanId = tda.TindakanId, AsuransiId = tda.AsuransiId } into tdaGroup
                    from mta in tdaGroup.DefaultIfEmpty()

                    join b in _applicationDbContext.Billings
                        on new { KunjunganId = tk.KunjunganId, ItemId = tk.TindakanId }
                        equals new { KunjunganId = b.KunjunganId.Value, ItemId = b.ItemId.Value } into billingGroup
                    from billing in billingGroup.DefaultIfEmpty()

                    where tk.KunjunganId == kunjunganId && (mta == null || !mta.IsDelete)

                    select new
                    {
                        tk.KunjunganId,
                        tk.TindakanId,
                        NamaTindakan = mt.NamaTindakan,
                        IsCoveredByAsuransi = mta != null,

                        // Info Billing  
                        BillingId = billing != null ? billing.BillingId : (Guid?)null,
                        BillingKode = billing.BillingKode,
                        HargaItem = billing.HargaItem,
                        QtyItem = billing.QtyItem,
                        SubTotalItem = billing.SubTotalItem,
                        BillingDate = billing.BillingDate
                    }
                ).ToListAsync();

                return Ok(tindakanQuery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("BillingAdmin/{kunjunganId}")]
        public async Task<IActionResult> GetBiayaAdministrasiByKunjunganId(Guid kunjunganId)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var billing = await _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == kunjunganId && b.BillingKode == "Biaya Admin" && !b.IsDelete)
                    .Select(b => new
                    {
                        b.BillingId,
                        b.KunjunganId,
                        b.ItemId,
                        b.NamaItem,
                        b.HargaItem,
                        b.QtyItem,
                        b.SubTotalItem,
                        b.BillingKode,
                        b.BillingDate
                    })
                    .FirstOrDefaultAsync();

                if (billing == null)
                {
                    return NotFound(new { message = "Data billing administrasi tidak ditemukan untuk kunjungan ini." });
                }

                return Ok(new
                {
                    message = "Data billing administrasi ditemukan.",
                    data = billing
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBilling(Guid id, [FromBody] BillingViewModel vm)
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

                // cari data
                var billing = await _applicationDbContext.Billings
                    .FirstOrDefaultAsync(b => b.BillingId == id);

                if (billing == null)
                    return NotFound(new { message = "Data billing tidak ditemukan." });

                var kodePrefix = billing.JenisBilling.Trim().ToLower();

                decimal harga = 0;

                switch (kodePrefix)
                {
                    case "obat":
                        var obat = await _applicationDbContext.Obats
                            .FirstOrDefaultAsync(o => o.ObatId == billing.ItemId && !o.IsDelete);
                        if (obat == null)
                            return NotFound(new { message = "Data obat tidak ditemukan." });

                        harga = obat.HargaJual;
                        break;

                    case "tindakan":
                        //// Ambil Tindakan
                        //var tindakan = await _applicationDbContext.Tindakans
                        //    .FirstOrDefaultAsync(t => t.TindakanId == billing.ItemId && !t.IsDelete);
                        //if (tindakan == null)
                        //    return NotFound(new { message = "Data tindakan tidak ditemukan." });

                        //// Ambil kunjungan
                        //var kunjungan = await _applicationDbContext.Kunjungans
                        //    .FirstOrDefaultAsync(k => k.KunjunganID == billing.KunjunganId);
                        //if (kunjungan == null)
                        //    return NotFound(new { message = "Data kunjungan tidak ditemukan." });

                        //// Ambil kelas berdasarkan jenis kunjungan
                        //var kelas = await _applicationDbContext.Kelass
                        //    .FirstOrDefaultAsync(k => k.KodeKelas == kunjungan.JenisKunjungan);
                        //if (kelas == null)
                        //    return NotFound(new { message = "Kelas untuk jenis kunjungan ini tidak ditemukan." });

                        //// Ambil tarif kelas untuk tindakan dan kelas
                        //var tarifKelas = await _applicationDbContext.TarifKelass
                        //    .FirstOrDefaultAsync(t => t.TindakanId == tindakan.TindakanId && t.KelasId == kelas.KelasId);
                        //if (tarifKelas == null)
                        //    return NotFound(new { message = "Tarif untuk tindakan dan kelas ini tidak ditemukan." });

                        //harga = tarifKelas.TarifTotal ?? 0;
                        return Forbid("Tidak bisa mengedit Tindakan.");

                    default:
                        return BadRequest(new { message = "BillingKode tidak dikenali (harus OB atau TD)." });
                }

                // Update billing
                billing.QtyItem = vm.QtyItem;
                billing.HargaItem = harga;
                billing.SubTotalItem = harga * (vm.QtyItem ?? 1); // default 1 jika null
                billing.DiskonId = vm.DiskonId;
                billing.Keterangan = vm.Keterangan;
                billing.UpdateDateTime = DateTimeOffset.UtcNow;
                billing.UpdateBy = userActiveId;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Billing berhasil diperbarui." });
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
    }
}
