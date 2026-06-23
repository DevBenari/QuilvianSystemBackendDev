using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class SupplierController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Helper
        private async Task<Guid?> GetUserId()
        {
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(email)) return null;

            return await _context.UserActives
                .Where(x => x.Email == email)
                .Select(x => x.UserActiveId)
                .FirstOrDefaultAsync();
        }
        #endregion

        // ========================= GET ALL =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await (
                from s in _context.Suppliers
                join m in _context.MataUangs
                    on s.MataUangId equals m.MataUangId
                where !s.IsDelete
                orderby s.CreateDateTime descending
                select new
                {
                    Supplier = s,
                    MataUang = m
                }
            ).ToListAsync();

            return Ok(new
            {
                message = "OK",
                data
            });
        }

        // ========================= GET BY ID =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Suppliers
                .FirstOrDefaultAsync(x => x.SupplierId == id && !x.IsDelete);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(data);
        }

        // ========================= CREATE =========================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupplierViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = await GetUserId();
            if (userId == null)
                return Unauthorized();

            var duplicate = await _context.Suppliers
                .AnyAsync(x => x.SupplierName.ToLower() == vm.SupplierName.ToLower()
                            && !x.IsDelete);

            if (duplicate)
                return Conflict(new { message = "Supplier sudah ada" });

            var today = DateTime.UtcNow.ToString("yyMMdd");
            var lastCode = await _context.Suppliers
                .Where(x => x.SupplierCode.StartsWith($"SPL{today}"))
                .OrderByDescending(x => x.SupplierCode)
                .Select(x => x.SupplierCode)
                .FirstOrDefaultAsync();

            var newCode = lastCode == null
                ? $"SPL{today}0001"
                : $"SPL{today}{(int.Parse(lastCode.Substring(9)) + 1):D4}";

            var data = new Supplier
            {
                SupplierId = Guid.NewGuid(),
                SupplierCode = newCode,
                SupplierName = vm.SupplierName,
                ContactPerson = vm.ContactPerson,
                TermOfPayment = vm.TermOfPayment,
                LeadTime = vm.LeadTime,
                Address = vm.Address,
                City = vm.City,
                PhoneNumber = vm.PhoneNumber,
                Email = vm.Email,
                IsPKS = vm.IsPKS,
                IsActive = vm.IsActive,
                BankId = vm.BankId,
                NoRekening = vm.NoRekening,
                AccountHolderName = vm.AccountHolderName,
                IsFullPaid = vm.IsFullPaid,
                IsBloodBankSupplier = vm.IsBloodBankSupplier,
                PaymentMethod = vm.PaymentMethod,
                PPN = vm.PPN,
                Note = vm.Note,
                CreateBy = userId.Value,
                CreateDateTime = DateTimeOffset.UtcNow
            };

            _context.Suppliers.Add(data);
            await _context.SaveChangesAsync();

            return Created("", new { message = "Data berhasil ditambahkan" });
        }

        // ========================= UPDATE =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SupplierViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = await GetUserId();
            if (userId == null)
                return Unauthorized();

            var data = await _context.Suppliers.FindAsync(id);
            if (data == null || data.IsDelete)
                return NotFound();

            data.SupplierName = vm.SupplierName;
            data.ContactPerson = vm.ContactPerson;
            data.TermOfPayment = vm.TermOfPayment;
            data.LeadTime = vm.LeadTime;
            data.Address = vm.Address;
            data.City = vm.City;
            data.PhoneNumber = vm.PhoneNumber;
            data.Email = vm.Email;
            data.IsPKS = vm.IsPKS;
            data.IsActive = vm.IsActive;
            data.BankId = vm.BankId;
            data.NoRekening = vm.NoRekening;
            data.AccountHolderName = vm.AccountHolderName;
            data.IsFullPaid = vm.IsFullPaid;
            data.IsBloodBankSupplier = vm.IsBloodBankSupplier;
            data.PaymentMethod = vm.PaymentMethod;
            data.PPN = vm.PPN;
            data.Note = vm.Note;

            data.UpdateBy = userId.Value;
            data.UpdateDateTime = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil diupdate" });
        }

        // ========================= DELETE (SOFT) =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = await GetUserId();
            if (userId == null)
                return Unauthorized();

            var data = await _context.Suppliers.FindAsync(id);
            if (data == null || data.IsDelete)
                return NotFound();

            data.IsDelete = true;
            data.DeleteBy = userId.Value;
            data.DeleteDateTime = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus" });
        }

        // ========================= PAGED =========================
        [HttpGet("paged")]
        public async Task<IActionResult> PagedSupplier(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc")
        {
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;

            var query = _context.Suppliers
                .Where(x => !x.IsDelete)
                .Select(s => new
                {
                    s.SupplierId,
                    s.SupplierCode,
                    s.SupplierName,
                    s.ContactPerson,
                    s.TermOfPayment,
                    s.LeadTime,
                    s.Address,
                    s.City,
                    s.PhoneNumber,
                    s.Email,
                    s.IsPKS,
                    s.IsActive,
                    s.BankId,
                    s.NoRekening,
                    s.AccountHolderName,
                    s.IsFullPaid,
                    s.IsBloodBankSupplier,
                    s.PaymentMethod,
                    s.PPN,
                    s.Note,
                    s.CreateDateTime
                });

            // ================= SEARCH =================
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.SupplierName.Contains(search) ||
                    x.ContactPerson.Contains(search) ||
                    x.PhoneNumber.Contains(search));
            }

            // ================= SORTING =================
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "SupplierName" => query.OrderByDescending(x => x.SupplierName),
                    "City" => query.OrderByDescending(x => x.City),
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "SupplierName" => query.OrderBy(x => x.SupplierName),
                    "City" => query.OrderBy(x => x.City),
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    _ => query.OrderBy(x => x.CreateDateTime)
                };

            // ================= PAGINATION =================
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (page > totalPages && totalRows > 0)
                return NotFound(new { message = "Page not found." });

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = rows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }
}
