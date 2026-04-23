using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Services;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
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

        //[HttpGet]
        //public async Task<IActionResult> Profile()
        //{
        //    var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(emailLogin))
        //    {
        //        return Unauthorized(new { message = "User tidak terautentikasi." });
        //    }

        //    // Ambil user berdasarkan email
        //    var user = await _applicationDbContext.UserActives
        //        .FirstOrDefaultAsync(u => u.Email == emailLogin);

        //    if (user == null)
        //    {
        //        var superadminModel = new UserActive
        //        {
        //            FullName = user.FullName ?? "Superadmin",
        //            Email = user.Email,
        //            Handphone = "-",
        //            Gender = "-",
        //            PlaceOfBirth = "Jakarta",
        //            DateOfBirth = DateTime.MinValue,
        //            Address = "-",
        //            //Foto = null,
        //            //MstDepartmentUser = null,
        //            //MstPositionUser = null
        //        };

        //        ViewBag.IsSuperAdmin = true;
        //        return Ok(new
        //        {
        //            message = "Data superadmin",
        //            data = superadminModel
        //        });
        //    }

        //    // Ambil nama tipe user dari tabel MstTipeUser berdasarkan TipeUserId
        //    var tipeUser = await _applicationDbContext.TipeUsers
        //        .FirstOrDefaultAsync(t => t.TipeUserId == user.TipeUserId);

        //    var tipeUserName = tipeUser?.NamaTipeUser ?? "Unknown";

        //    // Cek apakah tipe user adalah dokter
        //    if (tipeUserName.ToLower() == "dokter")
        //    {
        //        var dokter = await _applicationDbContext.Dokters
        //            .FirstOrDefaultAsync(d => d.Email == user.Email);

        //        if (dokter == null)
        //        {
        //            return NotFound(new { message = "User adalah dokter, tapi data dokter tidak ditemukan." });
        //        }

        //        return Ok(new
        //        {
        //            message = "Data dokter ditemukan",
        //            data = user,
        //            dokter = dokter,
        //        });
        //    }

        //    // Jika bukan dokter
        //    return Ok(new
        //    {
        //        message = "Data user ditemukan",
        //        data = user,
        //        TipeUser = tipeUser.NamaTipeUser
        //    });
        //}

        [HttpGet]
        public async Task<IActionResult> Profile(CancellationToken ct)
        {
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(emailLogin))
            {
                return Unauthorized(new { message = "User tidak terautentikasi." });
            }

            var userData = await
                (from u in _applicationDbContext.UserActives.AsNoTracking()
                 join t in _applicationDbContext.TipeUsers.AsNoTracking()
                     on u.TipeUserId equals t.TipeUserId into tipeJoin
                 from tipe in tipeJoin.DefaultIfEmpty()

                 join td in _applicationDbContext.MasterTTDs.AsNoTracking()
                 on u.UserActiveId equals td.UserActiveId into tdG
                 from td in tdG.DefaultIfEmpty()

                 join d in _applicationDbContext.Departements.AsNoTracking()
                 on u.DepartemenId equals d.DepartementId into dG
                 from d in dG.DefaultIfEmpty()

                 join p in _applicationDbContext.Positions.AsNoTracking()
                 on u.PositionId equals p.PositionId into pG
                 from p in pG.DefaultIfEmpty()

                 where u.Email == emailLogin && (u.IsDelete == false || u.IsDelete == null)
                 select new
                 {
                     u.UserActiveId,
                     u.UserActiveCode,
                     u.FullName,
                     u.IdentityNumber,
                     u.PlaceOfBirth,
                     u.DateOfBirth,
                     u.Gender,
                     u.Address,
                     u.Handphone,
                     u.Email,
                     u.IsActive,
                     u.DepartemenId,
                     NamaDepartement = d != null ? d.NamaDepartement : null,
                     u.PositionId,
                     NamaPosisi = p != null ? p.PositionName : null,
                     u.TipeUserId,
                     NamaTipeUser = tipe != null ? tipe.NamaTipeUser : null,
                     u.NoSTR,
                     u.StatusPegawai,
                     u.FotoName,
                     u.FotoPath,
                     u.CreateDateTime,
                     u.CreateBy,
                     u.UpdateDateTime,
                     u.UpdateBy,
                     u.DeleteDateTime,
                     u.DeleteBy,
                     u.IsDelete
                 })
                .FirstOrDefaultAsync(ct);

            if (userData == null)
            {
                return NotFound(new { message = "Data user tidak ditemukan." });
            }

            var namaTipeUser = (userData.NamaTipeUser ?? string.Empty).Trim();
            var isSuperAdmin = namaTipeUser.Equals("Super Admin", StringComparison.OrdinalIgnoreCase);
            var isDokter = namaTipeUser.Equals("Dokter", StringComparison.OrdinalIgnoreCase);

            if (isDokter)
            {
                var dokter = await _applicationDbContext.Dokters
                    .AsNoTracking()
                    .Where(d =>
                        d.UserActiveId == userData.UserActiveId &&
                        (d.IsDelete == false || d.IsDelete == null))
                    .Select(d => new
                    {
                        d.DokterId,
                        d.UserActiveId,
                        d.KdDokter,
                        d.NmDokter,
                        d.Email,
                        d.Nohp,
                        d.Alamat,
                        d.Sip,
                        d.Str,
                        d.Spesialis,
                        d.TglSip,
                        d.TglStr,
                        //d.TTDPath,
                        //d.TTDName,
                        d.CreateDateTime,
                        d.CreateBy,
                        d.UpdateDateTime,
                        d.UpdateBy
                    })
                    .FirstOrDefaultAsync(ct);

                if (dokter == null)
                {
                    return NotFound(new
                    {
                        message = "User bertipe Dokter, tetapi data dokter tidak ditemukan."
                    });
                }

                return Ok(new
                {
                    message = isSuperAdmin
                        ? "Data profile Super Admin (Dokter) ditemukan"
                        : "Data profile dokter ditemukan",
                    data = new
                    {
                        IsSuperAdmin = isSuperAdmin,
                        TipeUser = namaTipeUser,
                        UserActive = new
                        {
                            userData.UserActiveId,
                            userData.UserActiveCode,
                            userData.FullName,
                            userData.IdentityNumber,
                            userData.PlaceOfBirth,
                            userData.DateOfBirth,
                            userData.Gender,
                            userData.Address,
                            userData.Handphone,
                            userData.Email,
                            userData.IsActive,
                            userData.DepartemenId,
                            userData.PositionId,
                            userData.TipeUserId,
                            NamaTipeUser = namaTipeUser,
                            userData.NoSTR,
                            userData.StatusPegawai,
                            userData.FotoName,
                            userData.FotoPath,
                            userData.CreateDateTime,
                            userData.CreateBy,
                            userData.UpdateDateTime,
                            userData.UpdateBy,
                            userData.DeleteDateTime,
                            userData.DeleteBy,
                            userData.IsDelete
                        },
                        ProfileDetail = dokter
                    }
                });
            }

            var karyawan = await _applicationDbContext.Karyawans
                .AsNoTracking()
                .Where(k =>
                    k.UserActiveId == userData.UserActiveId &&
                    (k.IsDelete == false || k.IsDelete == null))
                .Select(k => new
                {
                    k.KaryawanId,
                    k.UserActiveId,
                    k.NoIdentitas,
                    k.Email,
                    k.NoHandphone,
                    k.TanggalAkhirKerja,
                    k.TanggalAwalKerja,
                    k.Alamat,
                    k.DepartementId,
                    //k.Po,
                    k.FotoPath,
                    k.FotoName,
                    //k.TTDPath,
                    //k.TTDName,
                    k.CreateDateTime,
                    k.CreateBy,
                    k.UpdateDateTime,
                    k.UpdateBy
                })
                .FirstOrDefaultAsync(ct);

            return Ok(new
            {
                message = isSuperAdmin
                    ? "Data profile Super Admin ditemukan"
                    : "Data profile user ditemukan",
                data = new
                {
                    IsSuperAdmin = isSuperAdmin,
                    TipeUser = namaTipeUser,
                    UserActive = new
                    {
                        userData.UserActiveId,
                        userData.UserActiveCode,
                        userData.FullName,
                        userData.IdentityNumber,
                        userData.PlaceOfBirth,
                        userData.DateOfBirth,
                        userData.Gender,
                        userData.Address,
                        userData.Handphone,
                        userData.Email,
                        userData.IsActive,
                        userData.DepartemenId,
                        userData.PositionId,
                        userData.TipeUserId,
                        NamaTipeUser = namaTipeUser,
                        userData.NoSTR,
                        userData.StatusPegawai,
                        userData.FotoName,
                        userData.FotoPath,
                        userData.CreateDateTime,
                        userData.CreateBy,
                        userData.UpdateDateTime,
                        userData.UpdateBy,
                        userData.DeleteDateTime,
                        userData.DeleteBy,
                        userData.IsDelete
                    },
                    ProfileDetail = karyawan
                }
            });
        }


    }
}
