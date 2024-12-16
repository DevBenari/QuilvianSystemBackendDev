using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystem.Areas.AccountingAndFinancial.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    [EnableCors("AllowSpecific")]
    public class BankController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BankController
        (
                ApplicationDbContext applicationDbContext,
                UserManager<ApplicationUser> userManager,
                IWebHostEnvironment webHostEnvironment,
                SignInManager<ApplicationUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationDbContext = applicationDbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        // Get all banks
        [HttpGet("banks")]
        public IActionResult GetAllBanks()
        {
            var banks = _applicationDbContext.Banks.ToList();

            return Ok(banks);
        }

        // Get a specific bank by ID
        [HttpGet("banks/{id}")]
        public IActionResult GetBankById(Guid id)
        {
            var bank = _applicationDbContext.Banks
                .FirstOrDefault(b => b.BankId == id);

            if (bank == null)
                return NotFound();

            return Ok(bank);
        }

        // Create a new bank
        [HttpPost("banks")]
        public IActionResult CreateBank([FromBody] Bank bank)
        {
            if (bank == null)
                return BadRequest("Invalid bank data.");

            _applicationDbContext.Banks.Add(bank);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetBankById), new { id = bank.BankId }, bank);
        }

        // Update a bank
        [HttpPut("banks/{id}")]
        public IActionResult UpdateBank(Guid id, [FromBody] Bank updatedBank)
        {
            var existingBank = _applicationDbContext.Banks.Find(id);
            if (existingBank == null)
                return NotFound();

            existingBank.KodeBank = updatedBank.KodeBank;
            existingBank.NamaBank = updatedBank.NamaBank;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a bank
        [HttpDelete("banks/{id}")]
        public IActionResult DeleteBank(Guid id)
        {
            var bank = _applicationDbContext.Banks
                .Include(b => b.BankCabang)
                .FirstOrDefault(b => b.BankId == id);

            if (bank == null)
                return NotFound();

            // Delete related BankCabang entries first
            _applicationDbContext.BankCabangs.RemoveRange(bank.BankCabang);

            // Delete the bank
            _applicationDbContext.Banks.Remove(bank);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Get all bank branches
        [HttpGet("bankcabang")]
        public IActionResult GetAllBankCabangs()
        {
            var bankCabangs = _applicationDbContext.BankCabangs
                .ToList();

            return Ok(bankCabangs);
        }

        // Get a specific bank branch by ID
        [HttpGet("bankcabang/{id}")]
        public IActionResult GetBankCabangById(Guid id)
        {
            var bankCabang = _applicationDbContext.BankCabangs
                .FirstOrDefault(bc => bc.BankCabangId == id);

            if (bankCabang == null)
                return NotFound();

            return Ok(bankCabang);
        }

        // Create a new bank branch
        [HttpPost("bankcabang")]
        public IActionResult CreateBankCabang([FromBody] BankCabang bankCabang)
        {
            if (bankCabang == null)
                return BadRequest("Invalid bank branch data.");

            _applicationDbContext.BankCabangs.Add(bankCabang);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetBankCabangById), new { id = bankCabang.BankCabangId }, bankCabang);
        }

        // Update a bank branch
        [HttpPut("bankcabang/{id}")]
        public IActionResult UpdateBankCabang(Guid id, [FromBody] BankCabang updatedBankCabang)
        {
            var existingBankCabang = _applicationDbContext.BankCabangs.Find(id);
            if (existingBankCabang == null)
                return NotFound();

            existingBankCabang.KodeBankCabang = updatedBankCabang.KodeBankCabang;
            existingBankCabang.NamaCabang = updatedBankCabang.NamaCabang;
            existingBankCabang.BankId = updatedBankCabang.BankId;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a bank branch
        [HttpDelete("bankcabang/{id}")]
        public IActionResult DeleteBankCabang(Guid id)
        {
            var bankCabang = _applicationDbContext.BankCabangs.Find(id);
            if (bankCabang == null)
                return NotFound();

            _applicationDbContext.BankCabangs.Remove(bankCabang);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
