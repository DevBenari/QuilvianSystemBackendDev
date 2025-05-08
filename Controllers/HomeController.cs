using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace QuilvianSystemBackendDev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly serviceMasterData _serviceMasterData;

        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            serviceMasterData serviceMasterData,

            ILogger<HomeController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _serviceMasterData = serviceMasterData;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(emailLogin))
            {
                return Unauthorized(new { message = "User tidak terautentikasi." });
            }

            // Ambil user berdasarkan email
            var user = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (user == null)
            {
                var superadminModel = new UserActive
                {
                    FullName = user.FullName ?? "Superadmin",
                    Email = user.Email,
                    Handphone = "-",
                    Gender = "-",
                    PlaceOfBirth = "Jakarta",
                    DateOfBirth = DateTime.MinValue,
                    Address = "-",
                    //Foto = null,
                    //MstDepartmentUser = null,
                    //MstPositionUser = null
                };

                ViewBag.IsSuperAdmin = true;
                return Ok(new
                {
                    message = "Data superadmin dummy",
                    data = superadminModel
                });
            }

            // Ambil nama tipe user dari tabel MstTipeUser berdasarkan TipeUserId
            var tipeUser = await _applicationDbContext.TipeUsers
                .FirstOrDefaultAsync(t => t.TipeUserId == user.TipeUserId);

            var tipeUserName = tipeUser?.NamaTipeUser ?? "Unknown";

            // Cek apakah tipe user adalah dokter
            if (tipeUserName.ToLower() == "dokter")
            {
                var dokter = await _applicationDbContext.Dokters
                    .FirstOrDefaultAsync(d => d.Email == user.Email);

                if (dokter == null)
                {
                    return NotFound(new { message = "User adalah dokter, tapi data dokter tidak ditemukan." });
                }

                return Ok(new
                {
                    message = "Data dokter ditemukan",
                    data = user,
                    dokter = dokter,
                });
            }

            // Jika bukan dokter
            return Ok(new
            {
                message = "Data user ditemukan",
                data = user,
                TipeUser = tipeUser.NamaTipeUser
            });
        }
        //public async Task<IActionResult> Profile()
        //{
        //    ViewBag.Active = "Profile";

        //    // untuk user tipe umum
        //    var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(EmailLogin))
        //    {
        //        return Unauthorized(new { message = "User tidak terautentikasi!" });
        //    }

        //    var GetUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
        //    var UserActiveId = GetUserActive?.UserActiveId ?? Guid.Empty;

        //    var getUser = await _serviceMasterData.GetCurrentUserByEmail(EmailLogin);

        //    if (getUser != null)
        //    {
        //        return Ok(new
        //        {
        //            message = "Data user ditemukan",
        //            data = getUser
        //        });
        //    }

        //    var userLogin = await _userManager.FindByEmailAsync(EmailLogin);
        //    if (userLogin == null)
        //    {
        //        return NotFound("Data user tidak ditemukan di sistem.");
        //    }

        //    // Buat dummy MstUserActive hanya untuk tampilan super admin
        //    var superadminModel = new UserActive
        //    {
        //        FullName = userLogin.NamaUser ?? "Superadmin",
        //        Email = userLogin.Email,
        //        Handphone = "-",
        //        Gender = "-",
        //        PlaceOfBirth = "Jakarta",
        //        DateOfBirth = DateTime.MinValue,
        //        Address = "-",
        //        //Foto = null,
        //        //MstDepartmentUser = null,
        //        //MstPositionUser = null
        //    };

        //    ViewBag.IsSuperAdmin = true;
        //    return Ok(new
        //    {
        //        message = "Data superadmin dummy",
        //        data = superadminModel
        //    });
        //}
    }
}
