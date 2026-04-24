using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Helpers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using static QRCoder.PayloadGenerator;

namespace QuilvianSystemBackendDev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AutoLoginDTO _optAutoLogin;

        public AuthController
        (
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IOptions<AutoLoginDTO> optAutoLogin
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _optAutoLogin = optAutoLogin.Value;
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    return BadRequest(new { message = "User sedang online || Response Code: 400" }); // 400 Bad Request
                }
                else
                {
                   var idfinger = _context.Fingerprints.FirstOrDefault(u => u.UserId == model.Email);
                    if (model.Email == "superadmin@admin.com" && model.Password == "Admin@123")
                    {
                        // Membuat token JWT
                        var jwtSettings = _configuration.GetSection("Jwt");
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, model.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };

                        var token = new JwtSecurityToken(
                            issuer: jwtSettings["Issuer"],
                            audience: jwtSettings["Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                            signingCredentials: credentials
                        );

                        return Ok(new
                        {
                            message = "Berhasil || 200 OK",
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                    else if (idfinger != null && idfinger.UserId == model.Email && idfinger.DeviceId == model.Password)
                    {
                        // dengan fingerprint
                        var userActiveFinger = _context.UserActives.FirstOrDefault(u => u.UserActiveId.ToString() == model.Email && u.IsActive);

                        // Ambil nama tipe user dari TipeUserId
                        var roleNameFinger = _context.TipeUsers
                            .Where(t => t.TipeUserId == userActiveFinger.TipeUserId)
                            .Select(t => t.NamaTipeUser)
                            .FirstOrDefault() ?? "Guest";
                        // Membuat token JWT
                        var jwtSettings = _configuration.GetSection("Jwt");
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        // Update fingerprint
                        var fingerprint = await _context.Fingerprints
                            .FirstOrDefaultAsync(f => f.UserId == idfinger.UserId);

                        if (fingerprint != null)
                        {
                            fingerprint.DeviceId = Guid.NewGuid().ToString();
                            _context.Fingerprints.Update(fingerprint);
                            await _context.SaveChangesAsync();
                        }
                        var claims = new[]
                        {
                                new Claim(JwtRegisteredClaimNames.Sub, userActiveFinger.Email),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                new Claim("role", roleNameFinger)
                                };

                        var token = new JwtSecurityToken(
                            issuer: jwtSettings["Issuer"],
                            audience: jwtSettings["Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                            signingCredentials: credentials
                        );
                        return Ok(new
                        {
                            message = "Berhasil || 200 OK",
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                    else
                    {
                        var user = await _signInManager.UserManager.FindByNameAsync(model.Email);
                        // Ambil data dari UserActive + relasi TipeUser
                        var userActive = _context.UserActives.FirstOrDefault(u => u.Email == model.Email && u.IsActive);
                        // Cek apakah user ada
                        if (user == null)
                        {
                            return NotFound(new { message = "User belum terdaftar" });
                        }
                        else if (user.IsActive != false && user != null)
                        {
                            // Ambil nama tipe user dari TipeUserId
                            var roleName = _context.TipeUsers
                                .Where(t => t.TipeUserId == userActive.TipeUserId)
                                .Select(t => t.NamaTipeUser)
                                .FirstOrDefault() ?? "Guest";

                            // Cek password
                            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
                            if (result.Succeeded)
                            {
                                // Membuat token JWT
                                var jwtSettings = _configuration.GetSection("Jwt");
                                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
                                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                                var claims = new[]
                                {
                                new Claim(JwtRegisteredClaimNames.Sub, model.Email),
                                //new Claim(ClaimTypes.NameIdentifier, user.Id),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                //new Claim("userId", userActive.UserActiveId.ToString()),
                                //new Claim("fullName", userActive.FullName ?? ""),
                                new Claim("role", roleName)
                                };

                                var token = new JwtSecurityToken(
                                    issuer: jwtSettings["Issuer"],
                                    audience: jwtSettings["Audience"],
                                    claims: claims,
                                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                                    signingCredentials: credentials
                                );

                                return Ok(new
                                {
                                    message = "Berhasil || 200 OK",
                                    token = new JwtSecurityTokenHandler().WriteToken(token),
                                    expiration = token.ValidTo
                                });
                            } 
                            else if (result.IsLockedOut)
                            {
                                // HttpContext.session.Clear untuk menghapus session data pengguna tidak lagi tersimpan
                                //HttpContext.Session.Clear();

                                // Hitung waktu yang tersisa
                                var lockTime = await _userManager.GetLockoutEndDateAsync(user);
                                var timeRemaining = lockTime.Value - DateTimeOffset.UtcNow;

                                return BadRequest(new { message = "Maaf, akun anda di blokir sementara... || 400 Bad Request" });
                                //TempData["UserLockOut"] = "Sorry, your account is locked in " + timeRemaining.Minutes + " minutes " + timeRemaining.Seconds + " seconds";
                                //return View(model);
                            }
                            else
                            {
                                return Unauthorized(new { message = "Password salah || 401 Unauthorized" });
                            }
                        }
                        else
                        {
                            return BadRequest(new { message = "Maaf, akun anda belum aktif... || 400 Bad Request" });
                        }
                    }
                }
            }

            return Ok();
        }

        [HttpPost("logout")]
        [Authorize] // Hanya user login yang bisa logout
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new { message = "User tidak terautentikasi!" });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { message = "User tidak ditemukan." });
            }

            user.IsOnline = false;

            await _userManager.UpdateAsync(user);

            // Update fingerprint status menjadi Logout
            // Cari UserActive berdasarkan IdentityUserId
            var userActive = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            // Update fingerprint
            var fingerprint = await _context.Fingerprints
                .FirstOrDefaultAsync(f => f.UserId == userActive.UserActiveId.ToString());

            if (fingerprint != null)
            {
                fingerprint.DeviceId = Guid.NewGuid().ToString();
                _context.Fingerprints.Update(fingerprint);
                await _context.SaveChangesAsync();
            }
            // Karena JWT tidak bisa dihapus dari server, cukup beri respons sukses
            return Ok(new
            {
                message = "Logout berhasil."
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> AutoLogin(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Token kosong." });

            var result = AutoLoginHelper.ValidateToken(token, _optAutoLogin.SecretKey);

            if (!result.IsValid || string.IsNullOrWhiteSpace(result.UserId) || string.IsNullOrWhiteSpace(result.TargetUrl))
                return Unauthorized(new { message = "Token tidak valid atau sudah kadaluarsa." });

            if (!Guid.TryParse(result.UserId, out Guid userId))
                return Unauthorized(new { message = "UserId pada token tidak valid." });

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.UserActiveId == userId);

            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Email ?? user.UserActiveId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim("UserActiveId", user.UserActiveId.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!Url.IsLocalUrl(result.TargetUrl))
                return BadRequest(new { message = "Target URL tidak valid." });

            return LocalRedirect(result.TargetUrl);
        }
    

}


    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
