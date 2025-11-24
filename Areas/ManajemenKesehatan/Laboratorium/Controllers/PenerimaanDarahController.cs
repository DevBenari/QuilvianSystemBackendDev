using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PenerimaanDarahController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<PenerimaanDarahController> _logger;
        private readonly IWebHostEnvironment _env;

        public PenerimaanDarahController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PenerimaanDarahController> logger,
            IWebHostEnvironment env
        )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _env = env;
        }

        private async Task<string> GenerateBatchCode()
        {
            // Ambil batch terakhir dari DB (urut Descending)
            var lastBatch = await _context.StockBatchs
                .OrderByDescending(b => b.KodeBatch)
                .Select(b => b.KodeBatch)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastBatch) && lastBatch.StartsWith("BATCH-"))
            {
                string numberPart = lastBatch.Replace("BATCH-", "");
                if (int.TryParse(numberPart, out int lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"BATCH-{nextNumber.ToString("D3")}";
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from p in _context.PenerimaanDarahs
                        join u in _context.UserActives on p.CreateBy equals u.UserActiveId
                        where p.IsDelete == false
                        orderby p.CreateDateTime descending
                        select new
                        {
                            p.PenerimaanDarahId,
                            p.KodePenerimaan,
                            p.TglPenerimaan,
                            p.TglFaktur,
                            p.NoFaktur,
                            p.NoPO,
                            p.SupplierId,
                            p.PenerimaId,
                            p.JumlahKantong,
                            p.DarahDetailId,
                            p.Keterangan,
                            
                            p.CreateDateTime,
                            CreateByName = u.FullName
                        };

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (!listdata.Any())
                return NotFound(new { message = "Belum ada data || 404 Not Found" });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
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
            var data = await _context.PenerimaanDarahs.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data });
        }

        private async Task<string> GenerateKodePenerimaan(DateTime tglPenerimaan)
        {
            // Format tanggal PDR: ddMMyyyyHHmm
            string dateCode = tglPenerimaan.ToString("ddMMyyyyHHmm");

            // Ambil semua kode pada tanggal yang sama (hanya tanggal, jam tidak dihitung)
            DateTime start = tglPenerimaan.Date;
            DateTime end = tglPenerimaan.Date.AddDays(1).AddTicks(-1);

            var existingCodes = await _context.PenerimaanDarahs
                .Where(x => x.TglPenerimaan >= start && x.TglPenerimaan <= end)
                .Select(x => x.KodePenerimaan)
                .ToListAsync();

            // Cari nomor terakhir untuk tanggal ini
            int nextNumber = 1;

            if (existingCodes.Any())
            {
                var numbers = existingCodes
                    .Select(code =>
                    {
                        var parts = code.Split('-');
                        if (parts.Length == 3 && int.TryParse(parts[2], out int num))
                            return num;
                        return 0;
                    })
                    .Where(n => n > 0)
                    .ToList();

                if (numbers.Any())
                    nextNumber = numbers.Max() + 1;
            }

            // Format running number menjadi 3 digit
            string numberFormatted = nextNumber.ToString("D3");

            return $"PDR-{dateCode}-{numberFormatted}";
        }


        [HttpPost]
        public async Task<IActionResult> CreateFull([FromBody] PenerimaanDarahViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var trx = await _context.Database.BeginTransactionAsync();
            try
            {
                // =============================
                // AMBIL USER LOGIN
                // =============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = _context.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });


                // =============================
                // 1️⃣ INSERT PENERIMAAN DARAH
                // =============================
                string kodeBaru = await GenerateKodePenerimaan((DateTime)vm.TglPenerimaan);

                var penerimaan = new PenerimaanDarah
                {
                    PenerimaanDarahId = Guid.NewGuid(),
                    KodePenerimaan = kodeBaru,
                    TglPenerimaan = vm.TglPenerimaan,
                    TglFaktur = vm.TglFaktur,
                    NoFaktur = vm.NoFaktur,
                    NoPO = vm.NoPO,
                    SupplierId = vm.SupplierId,
                    PenerimaId = vm.PenerimaId,
                    DarahDetailId = vm.DarahDetailId,
                    JumlahKantong = vm.JumlahKantong,
                    Keterangan = vm.Keterangan,
                    CreateBy = user.UserActiveId,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.PenerimaanDarahs.Add(penerimaan);
                await _context.SaveChangesAsync();



                // ===============================================================
                // 2️⃣ PROSES STOCK DARAH (Insert atau Update)
                // ===============================================================
                foreach (var item in vm.StockDarah)
                {
                    var existing = _context.StockDarahs
                        .FirstOrDefault(s =>
                            s.DarahDetailId == item.DarahDetailId &&
                            s.SupplierId == item.SupplierId && !s.IsDelete);

                    if (existing == null)
                    {
                        // ====================
                        // INSERT STOCK DARAH
                        // ====================
                        var newStock = new StockDarah
                        {
                            StockDarahId = Guid.NewGuid(),
                            DarahDetailId = item.DarahDetailId,
                            TipeKomponenId = item.TipeKomponenId,
                            Rhesus = item.Rhesus,
                            Golongan = item.Golongan,
                            Wacc = item.Wacc,
                            JumlahKantong = item.JumlahKantong,
                            Amount = item.Amount,
                            JumlahExpired = item.JumlahExpired,
                            TglExpired = item.TglExpired,
                            SisaStock = item.SisaStock,
                            MinStock = item.MinStock,
                            StatusStock = item.StatusStock,
                            Keterangan = item.Keterangan,
                            SupplierId = item.SupplierId,
                            CreateBy = user.UserActiveId,
                            CreateDateTime = DateTime.UtcNow
                        };
                        _context.StockDarahs.Add(newStock);
                    }
                    else
                    {
                        // ====================
                        // UPDATE STOCK DARAH
                        // ====================
                        existing.JumlahKantong += item.JumlahKantong ?? 0;
                        existing.SisaStock += item.SisaStock ?? 0;

                        if (item.TglExpired != null && item.TglExpired > existing.TglExpired)
                            existing.TglExpired = item.TglExpired;

                        if (item.Amount != null)
                            existing.Amount += item.Amount;

                        if (item.JumlahExpired != null)
                            existing.JumlahExpired += item.JumlahExpired;

                        existing.Keterangan = item.Keterangan ?? existing.Keterangan;

                        _context.StockDarahs.Update(existing);
                    }
                }

                await _context.SaveChangesAsync();



                // ===============================================================
                // 3️⃣ INSERT STOCK BATCH
                // ===============================================================

                foreach (var batch in vm.StockBatch)
                {    
                    // Generate kode batch otomatis
                    string batchCode = await GenerateBatchCode();

                    var newBatch = new StockBatch
                    {
                        StockBatchId = Guid.NewGuid(),
                        KodeBatch = batchCode,
                        ItemId = vm.DarahDetailId,
                        SupplierId = batch.SupplierId,
                        ExpiredDate = batch.ExpiredDate,
                        Keterangan = batch.Keterangan,
                        CreateBy = user.UserActiveId,
                        CreateDateTime = DateTime.UtcNow
                    };

                    _context.StockBatchs.Add(newBatch);
                }

                await _context.SaveChangesAsync();



                // ===============================================================
                // COMMIT TRANSACTION
                // ===============================================================
                await trx.CommitAsync();

                return Created("", new
                {
                    message = "Penerimaan darah, stock darah, dan batch berhasil disimpan (201 Created)"
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Kesalahan internal: {ex.Message}" });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PenerimaanDarahViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                var data = await _context.PenerimaanDarahs
                    .FirstOrDefaultAsync(x => x.PenerimaanDarahId == id && (x.IsDelete == false || x.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                // Ambil user login
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _context.UserActives.FirstOrDefault(u => u.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan." });

                bool needNewKode = data.TglPenerimaan.Value.Date != vm.TglPenerimaan.Value.Date;

                // Update data
                data.TglPenerimaan = vm.TglPenerimaan;
                data.TglFaktur = vm.TglFaktur;
                data.NoFaktur = vm.NoFaktur;
                data.NoPO = vm.NoPO;
                data.SupplierId = vm.SupplierId;
                data.PenerimaId = vm.PenerimaId;
                data.DarahDetailId = vm.DarahDetailId;
                data.JumlahKantong = vm.JumlahKantong;
                data.Keterangan = vm.Keterangan;

                // Jika tanggal berubah → generate kode baru
                if (needNewKode)
                {
                    data.KodePenerimaan = await GenerateKodePenerimaan((DateTime)vm.TglPenerimaan);
                }

                data.UpdateBy = user.UserActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Update Data Berhasil",
                    kodeBaru = data.KodePenerimaan
                });
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
                var data = await _context.PenerimaanDarahs.FindAsync(id);
                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _context.UserActives.FirstOrDefault(u => u.Email == emailLogin);

                data.IsDelete = true;
                data.DeleteBy = (Guid)(user?.UserActiveId);
                data.DeleteDateTime = DateTime.UtcNow;

                _context.PenerimaanDarahs.Update(data);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // GET /paged
        [HttpGet("paged")]
        public IActionResult Paged(
        int page = 1,
        int perPage = 10,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? search = null)
        {
            // ============================
            // 1️⃣ QUERY PARENT (PenerimaanDarah)
            // ============================
            var query =
                from p in _context.PenerimaanDarahs
                join s in _context.Suppliers on p.SupplierId equals s.SupplierId into sJoin
                from sup in sJoin.DefaultIfEmpty()
                where p.IsDelete == null || p.IsDelete == false
                select new
                {
                    p.PenerimaanDarahId,
                    p.KodePenerimaan,
                    p.TglPenerimaan,
                    p.TglFaktur,
                    p.NoFaktur,
                    p.NoPO,
                    p.SupplierId,
                    SupplierNama = sup.SupplierName,
                    p.PenerimaId,
                    p.DarahDetailId,
                    p.JumlahKantong,
                    p.Keterangan,
                    p.CreateDateTime
                };

            // ============================
            // 2️⃣ FILTER DATE
            // ============================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);

                query = query.Where(x =>
                    x.TglPenerimaan >= start &&
                    x.TglPenerimaan <= end);
            }

            // ============================
            // 3️⃣ SEARCH
            // ============================
            if (!string.IsNullOrWhiteSpace(search))
            {
                var sLower = search.ToLower();
                query = query.Where(x =>
                    x.NoFaktur.ToLower().Contains(sLower) ||
                    x.KodePenerimaan.ToLower().Contains(sLower));
            }

            // ============================
            // 4️⃣ TOTAL & PAGING
            // ============================
            int totalRows = query.Count();

            var pagedParents = query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!pagedParents.Any())
            {
                return Ok(new
                {
                    status = "success",
                    data = new { Rows = new List<object>(), TotalRows = 0 }
                });
            }

            var parentIds = pagedParents.Select(x => x.PenerimaanDarahId).ToList();


            // ============================
            // 5️⃣ LOAD STOCK DARAH SEKALI SAJA
            // ============================
            var stockDarah =
                _context.StockDarahs
                .Where(sd => parentIds.Contains((Guid)sd.DarahDetailId))
                .Select(sd => new
                {
                    sd.DarahDetailId,
                    sd.TipeKomponenId,
                    sd.Rhesus,
                    sd.Golongan,
                    sd.JumlahKantong,
                    sd.Amount,
                    sd.JumlahExpired,
                    sd.TglExpired,
                    sd.SisaStock,
                    sd.MinStock,
                    sd.StatusStock,
                    sd.Keterangan,
                    sd.SupplierId
                }).ToList();

            // ============================
            // 6️⃣ LOAD STOCK BATCH
            // ============================
            var stockBatch =
                _context.StockBatchs
                .Where(sb => parentIds.Contains((Guid)sb.ItemId))
                .Select(sb => new
                {
                    sb.StockBatchId,
                    sb.KodeBatch,
                    sb.ItemId,
                    sb.SupplierId,
                    sb.ExpiredDate,
                    sb.Keterangan
                }).ToList();


            // ============================
            // 7️⃣ MERGE DATA
            // ============================
            var merged = pagedParents.Select(parent => new
            {
                Penerimaan = parent,
                StockDarah = stockDarah
                    .Where(s => s.DarahDetailId == parent.DarahDetailId)
                    .ToList(),
                StockBatch = stockBatch
                    .Where(b => b.ItemId == parent.DarahDetailId)
                    .ToList()
            });

            // ============================
            // 8️⃣ RESPONSE
            // ============================
            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage),
                }
            });
        }

        //[HttpGet("paged")]
        //public async Task<IActionResult> GetPaged(
        //    int page = 1,
        //    int perPage = 10,
        //    string? search = null,
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    DateTime? startDate = null,
        //    DateTime? endDate = null
        //)
        //{
        //    try
        //    {
        //        if (!await _context.Database.CanConnectAsync())
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //        var query = from p in _context.PenerimaanDarahs
        //                    join u in _context.UserActives on p.CreateBy equals u.UserActiveId
        //                    where p.IsDelete == false
        //                    select new
        //                    {
        //                        p.PenerimaanDarahId,
        //                        p.KodePenerimaan,
        //                        p.TglPenerimaan,
        //                        p.TglFaktur,
        //                        p.NoFaktur,
        //                        p.NoPO,
        //                        p.SupplierId,
        //                        p.PenerimaId,
        //                        p.JumlahKantong,
        //                        p.DarahDetailId,
        //                        p.Keterangan,
        //                        p.CreateDateTime,
        //                        CreateByName = u.FullName
        //                    };

        //        // Search
        //        if (!string.IsNullOrWhiteSpace(search))
        //        {
        //            search = $"%{search.ToLower()}%";
        //            query = query.Where(p =>
        //                EF.Functions.ILike(p.NoPO, search)
        //            );
        //        }

        //        // Filter tanggal
        //        if (startDate.HasValue && endDate.HasValue)
        //        {
        //            var startUtc = startDate.Value.Date.ToUniversalTime();
        //            var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
        //            query = query.Where(p => p.CreateDateTime >= startUtc && p.CreateDateTime <= endUtc);
        //        }

        //        // Sorting
        //        var sortCol = orderBy?.ToLower() ?? "createdatetime";
        //        bool isDesc = sortDirection?.ToLower() == "desc";

        //        query = sortCol switch
        //        {
        //            "NoPO" => isDesc ? query.OrderByDescending(x => x.NoPO) : query.OrderBy(x => x.NoPO),
        //            _ => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        //        };

        //        int totalRows = await query.CountAsync();
        //        int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
        //        var rows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

        //        if (!rows.Any())
        //            return NotFound(new { message = "Page not found or no data available." });

        //        return Ok(new
        //        {
        //            status = "success",
        //            message = "Data retrieved successfully",
        //            data = new
        //            {
        //                Rows = rows,
        //                TotalRows = totalRows,
        //                CurrentPage = page,
        //                PerPage = perPage,
        //                TotalPages = totalPages
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in GetPaged PenerimaanDarah");
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}
    }
}
