using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Data;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class UserActiveController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        private readonly ILogger<UserActiveController> _logger;

        public UserActiveController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<UserActiveController> logger
            )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: api/Pegawai
        [HttpGet]
        public IActionResult GetAllUserActive()
        {
            var user = _applicationDbContext.UserActives.ToList();
            if (user == null || !user.Any())
            {
                return NotFound(new { message = "Belum ada data. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = user
            });
        }

        // GET: api/Pegawai/{id}
        [HttpGet("{id}")]
        public IActionResult GetUserById(Guid id)
        {
            var user = _applicationDbContext.UserActives.Find(id);
            if (user == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = user
            });
        }

        // POST: api/Pegawai
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserActiveViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            else
            {
                var dateNow = DateTimeOffset.Now;
                var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

                // Generate UserActiveCode
                var lastCode = _applicationDbContext.UserActives.Where(d => d.CreateDateTime.ToString("yyMMdd") == dateNow.ToString("yyMMdd")).OrderByDescending(k => k.UserActiveCode).FirstOrDefault();                

                if (lastCode == null)
                {
                    vm.UserActiveCode = "USR" + setDateNow + "0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.UserActiveCode.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        vm.UserActiveCode = "USR" + setDateNow + "0001";
                    }
                    else
                    {
                        vm.UserActiveCode = "USR" + setDateNow +
                            (Convert.ToInt32(lastCode.UserActiveCode.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    var userLogin = new ApplicationUser
                    {
                        KodeUser = vm.UserActiveCode,
                        NamaUser = vm.FullName,
                        Email = vm.Email,
                        UserName = vm.Email,
                        PhoneNumber = vm.Handphone,
                        IsActive = true
                    };

                    var user = new UserActive
                    {
                        CreateDateTime = DateTimeOffset.Now,
                        CreateBy = Guid.NewGuid(),
                        UpdateDateTime = DateTimeOffset.MinValue,
                        UpdateBy = new Guid("00000000-0000-0000-0000-000000000000"),
                        DeleteDateTime = DateTimeOffset.MinValue,
                        DeleteBy = new Guid("00000000-0000-0000-0000-000000000000"),
                        UserActiveId = Guid.NewGuid(),
                        UserActiveCode = vm.UserActiveCode,
                        FullName = vm.FullName,
                        IdentityNumber = vm.IdentityNumber,
                        PlaceOfBirth = vm.PlaceOfBirth,
                        DateOfBirth = vm.DateOfBirth,
                        Gender = vm.Gender,
                        Address = vm.Address,
                        Handphone = vm.Handphone,
                        Email = vm.Email,
                        IsActive = true
                    };

                    var passTglLahir = vm.DateOfBirth.ToString("ddMMMyyyy");

                    var checkDuplicate = _applicationDbContext.UserActives.Where(c => c.UserActiveCode == vm.UserActiveCode && c.FullName == vm.FullName).ToList();

                    if (checkDuplicate.Count == 0)
                    {
                        var result = _applicationDbContext.UserActives.Where(c => c.UserActiveCode == vm.UserActiveCode && c.FullName == vm.FullName).FirstOrDefault();
                        if (result == null)
                        {
                            var resultLogin = await _userManager.CreateAsync(userLogin, passTglLahir);

                            if (resultLogin.Succeeded)
                            {
                                _applicationDbContext.UserActives.Add(user);
                                _applicationDbContext.SaveChanges();
                                return CreatedAtAction(nameof(GetAllUserActive), new { message = "Tambah Data Berhasil || 201 Created" }, vm);
                            }
                            else
                            {
                                return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                            }
                        }
                        else
                        {
                            return BadRequest(new { message = "Data tidak dapat di input !!! || 400 Bad Request" });
                        }
                    }
                    else
                    {
                        return Conflict(new { message = "Terdapat duplikasi data !!! || 409 Conflict Data" });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(Guid id, [FromBody] UserActiveViewModel update)
        {
            if (update == null)
            {
                return BadRequest("Data tidak boleh kosong. || 400 Bad Request");
            }

            // Cari data berdasarkan ID
            var user = _applicationDbContext.UserActives.Find(id);
            if (user == null)
            {
                return NotFound($"User dengan ID {id} tidak ditemukan. || 404 Not Found");
            }

            try
            {
                // Perbarui data user
                user.FullName = update.FullName;
                user.IdentityNumber = update.IdentityNumber;
                user.PlaceOfBirth = update.PlaceOfBirth;
                user.DateOfBirth = update.DateOfBirth;
                user.Gender = update.Gender;
                user.Address = update.Address;
                user.Handphone = update.Handphone;
                user.Email = update.Email;
                user.IsActive = update.IsActive;

                // Tandai data sebagai telah diubah
                _applicationDbContext.UserActives.Update(user);

                // Simpan perubahan ke database
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Berhasil Update || 200 OK" });
            }
            catch (Exception ex)
            {
                // Tangani error jika terjadi masalah
                return StatusCode(500, $"Terjadi kesalahan saat memperbarui data: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(Guid id)
        {
            // Cari data berdasarkan ID
            var user = _applicationDbContext.UserActives.Find(id);
            if (user == null)
            {
                return NotFound($"User dengan ID {id} tidak ditemukan. || 404 Not Found");
            }

            try
            {
                // Hapus Akun Login
                var userLogin = _signInManager.UserManager.Users.FirstOrDefault(s => s.KodeUser == user.UserActiveCode);
                _applicationDbContext.Attach(userLogin);
                _applicationDbContext.Entry(userLogin).State = EntityState.Deleted;
                _applicationDbContext.SaveChanges();

                // Hapus entitas dari database
                _applicationDbContext.UserActives.Remove(user);

                // Simpan perubahan
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Berhasil Hapus || 200 OK" });
            }
            catch (Exception ex)
            {
                // Tangani error jika ada masalah
                return StatusCode(500, $"Terjadi kesalahan saat menghapus data: {ex.Message}");
            }
        }

        [HttpGet("paged")]
        public IActionResult GetPagedUsers(int page = 1, int perPage = 2, string? search = null, string? orderBy = "CreateDateTime", string? sortDirection = "asc")
        {
            if (page <= 0 || perPage <= 0)
            {
                return BadRequest(new { status = "error", message = "Page and perPage must be greater than 0." });
            }

            // Query dasar
            var query = _applicationDbContext.UserActives.AsQueryable();

            // Filter berdasarkan search jika ada
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.UserActiveCode.Contains(search) || u.FullName.Contains(search) || u.Email.Contains(search)); // Sesuaikan properti dengan kebutuhan
            }

            // Tambahkan order by
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(e => EF.Property<object>(e, orderBy))
                    : query.OrderBy(e => EF.Property<object>(e, orderBy));
            }

            // Total Rows
            var totalRows = query.Count();

            // Total Pages
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil Data Berdasarkan Pagination
            var rows = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { status = "error", message = "Page not found." });
            }

            // Buat Respons
            var response = new ApiResponse<PaginatedData<UserActive>>
            {
                Status = "success",
                Message = "Data retrieved successfully",
                Data = new PaginatedData<UserActive>
                {
                    Rows = rows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            };

            return Ok(response);
        }        
    }
}
