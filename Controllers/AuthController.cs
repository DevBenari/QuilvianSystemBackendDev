using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Helpers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private const string IdentityScheme = "Identity.Application";

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AutoLoginDTO _optAutoLogin;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IOptions<AutoLoginDTO> optAutoLogin,
            ILogger<AuthController> logger
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _optAutoLogin = optAutoLogin.Value;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Model login tidak valid" });

            var idfinger = await _context.Fingerprints
                .FirstOrDefaultAsync(u => u.UserId == model.Email);

            var setCookie = true;
            // 1. SUPERADMIN
            if (model.Email == "superadmin@admin.com" && model.Password == "Admin@123")
            {
                var jwt = BuildJwtToken(
                    model.Email,
                    "Superadmin",
                    null,
                    "Superadmin"
                );

                if (setCookie)
                {
                    await SignInIdentityCookieAsync(
                        model.Email,
                        "Superadmin",
                        null,
                        "Superadmin"
                    );
                }

                SetSession(
                    model.Email,
                    "Superadmin",
                    null,
                    "Superadmin"
                );

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    token = jwt.Token,
                    expiration = jwt.ExpirationUtc,
                    sessionDurationMinutes = GetSessionTimeoutMinutes(),
                    cookieCreated = setCookie
                });
            }

            // 2. LOGIN FINGERPRINT
            if (idfinger != null && idfinger.UserId == model.Email && idfinger.DeviceId == model.Password)
            {
                var userActiveFinger = await _context.UserActives
                    .FirstOrDefaultAsync(u => u.UserActiveId.ToString() == model.Email && u.IsActive);

                if (userActiveFinger == null)
                    return Unauthorized(new { message = "User fingerprint tidak ditemukan atau tidak aktif" });

                var roleNameFinger = await _context.TipeUsers
                    .Where(t => t.TipeUserId == userActiveFinger.TipeUserId)
                    .Select(t => t.NamaTipeUser)
                    .FirstOrDefaultAsync() ?? "Guest";

                var fingerprint = await _context.Fingerprints
                    .FirstOrDefaultAsync(f => f.UserId == idfinger.UserId);

                if (fingerprint != null)
                {
                    fingerprint.DeviceId = Guid.NewGuid().ToString();
                    _context.Fingerprints.Update(fingerprint);
                    await _context.SaveChangesAsync();
                }

                var jwt = BuildJwtToken(
                    userActiveFinger.Email ?? model.Email,
                    roleNameFinger,
                    userActiveFinger.UserActiveId.ToString(),
                    userActiveFinger.FullName
                );

                if (setCookie)
                {
                    await SignInIdentityCookieAsync(
                        userActiveFinger.Email ?? model.Email,
                        userActiveFinger.FullName ?? string.Empty,
                        userActiveFinger.UserActiveId.ToString(),
                        roleNameFinger
                    );
                }

                SetSession(
                    userActiveFinger.Email ?? model.Email,
                    userActiveFinger.FullName ?? string.Empty,
                    userActiveFinger.UserActiveId.ToString(),
                    roleNameFinger
                );

                var identityUserFinger = await _userManager.FindByEmailAsync(userActiveFinger.Email ?? "");
                if (identityUserFinger != null)
                {
                    identityUserFinger.IsOnline = true;
                    await _userManager.UpdateAsync(identityUserFinger);
                }

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    token = jwt.Token,
                    expiration = jwt.ExpirationUtc,
                    sessionDurationMinutes = GetSessionTimeoutMinutes(),
                    cookieCreated = setCookie
                });
            }

            // 3. LOGIN EMAIL + PASSWORD
            var user = await _signInManager.UserManager.FindByNameAsync(model.Email)
                       ?? await _userManager.FindByEmailAsync(model.Email);

            var userActive = await _context.UserActives
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null)
                return NotFound(new { message = "User belum terdaftar" });

            if (user.IsActive == false)
                return BadRequest(new { message = "Maaf, akun anda belum aktif... || 400 Bad Request" });

            if (userActive == null)
                return BadRequest(new { message = "Data UserActive tidak ditemukan atau belum aktif" });

            var roleName = await _context.TipeUsers
                .Where(t => t.TipeUserId == userActive.TipeUserId)
                .Select(t => t.NamaTipeUser)
                .FirstOrDefaultAsync() ?? "Guest";

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);

            if (result.Succeeded)
            {
                var jwt = BuildJwtToken(
                    model.Email,
                    roleName,
                    userActive.UserActiveId.ToString(),
                    userActive.FullName
                );

                if (setCookie)
                {
                    await SignInIdentityCookieAsync(
                        model.Email,
                        userActive.FullName ?? string.Empty,
                        userActive.UserActiveId.ToString(),
                        roleName
                    );
                }

                SetSession(
                    model.Email,
                    userActive.FullName ?? string.Empty,
                    userActive.UserActiveId.ToString(),
                    roleName
                );

                user.IsOnline = true;
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    token = jwt.Token,
                    expiration = jwt.ExpirationUtc,
                    sessionDurationMinutes = GetSessionTimeoutMinutes(),
                    cookieCreated = setCookie
                });
            }

            if (result.IsLockedOut)
            {
                var lockTime = await _userManager.GetLockoutEndDateAsync(user);
                var timeRemaining = lockTime.HasValue
                    ? lockTime.Value - DateTimeOffset.UtcNow
                    : TimeSpan.Zero;

                return BadRequest(new
                {
                    message = "Maaf, akun anda di blokir sementara... || 400 Bad Request",
                    remainingMinutes = Math.Max(0, (int)Math.Ceiling(timeRemaining.TotalMinutes))
                });
            }

            return Unauthorized(new { message = "Password salah || 401 Unauthorized" });
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        //[Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst(ClaimTypes.Email)?.Value
                     ?? HttpContext.Session.GetString("Email")
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                user.IsOnline = false;
                await _userManager.UpdateAsync(user);
            }

            var userActive = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (userActive != null)
            {
                var fingerprint = await _context.Fingerprints
                    .FirstOrDefaultAsync(f => f.UserId == userActive.UserActiveId.ToString());

                if (fingerprint != null)
                {
                    fingerprint.DeviceId = Guid.NewGuid().ToString();
                    _context.Fingerprints.Update(fingerprint);
                    await _context.SaveChangesAsync();
                }
            }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("Identity.Application");

            Response.Cookies.Delete(".Quilvian.Auth");
            Response.Cookies.Delete(".Quilvian.Session");

            return Ok(new { message = "Logout berhasil." });
        }

   
        // swagger/service: /api/auth?token=xxx&redirect=false&setCookie=false
        [AllowAnonymous]
        [HttpGet("AutoLogin")]
        public async Task<IActionResult> AutoLogin(
            [FromQuery] string token,
            [FromQuery] bool redirect = true,
            [FromQuery] bool setCookie = true)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Token kosong." });

            var result = AutoLoginHelper.ValidateTokenDebug(token, _optAutoLogin.SecretKey);

            if (!result.IsValid || string.IsNullOrWhiteSpace(result.UserId))
            {
                return Unauthorized(new
                {
                    message = "Token tidak valid atau sudah kadaluarsa.",
                    debug = result.Error,
                    serverUtcNow = DateTime.UtcNow,
                    secretLength = _optAutoLogin.SecretKey?.Length
                });
            }

            if (!Guid.TryParse(result.UserId, out Guid userId))
                return Unauthorized(new { message = "UserId pada token tidak valid." });

            var userActive = await _context.UserActives
                .FirstOrDefaultAsync(x => x.UserActiveId == userId && x.IsActive);

            if (userActive == null)
                return Unauthorized(new { message = "User tidak ditemukan atau tidak aktif." });

            var appUser = await _userManager.FindByEmailAsync(userActive.Email ?? "");
            if (appUser == null)
                return Unauthorized(new { message = "Identity user tidak ditemukan." });

            var roleName = await _context.TipeUsers
                .Where(t => t.TipeUserId == userActive.TipeUserId)
                .Select(t => t.NamaTipeUser)
                .FirstOrDefaultAsync() ?? "Guest";

            if (setCookie)
            {
                await SignInIdentityCookieAsync(
                    userActive.Email ?? userActive.UserActiveId.ToString(),
                    userActive.FullName ?? string.Empty,
                    userActive.UserActiveId.ToString(),
                    roleName
                );
            }

            SetSession(
                userActive.Email ?? userActive.UserActiveId.ToString(),
                userActive.FullName ?? string.Empty,
                userActive.UserActiveId.ToString(),
                roleName
            );

            appUser.IsOnline = true;
            await _userManager.UpdateAsync(appUser);

            var jwt = BuildJwtToken(
                userActive.Email ?? userActive.UserActiveId.ToString(),
                roleName,
                userActive.UserActiveId.ToString(),
                userActive.FullName
            );

            if (redirect)
            {
                if (string.IsNullOrWhiteSpace(result.TargetUrl))
                    return BadRequest(new { message = "Target URL kosong." });

                if (!IsAllowedRedirect(result.TargetUrl))
                    return BadRequest(new { message = "Target URL tidak valid." });

                return AutoLoginRedirectWithJwt(
                    jwt.Token,
                    jwt.ExpirationUtc,
                    result.TargetUrl
                );
            }

            return Ok(new
            {
                message = "Autologin berhasil",
                token = jwt.Token,
                expiration = jwt.ExpirationUtc,
                targetUrl = result.TargetUrl,
                sessionDurationMinutes = GetSessionTimeoutMinutes(),
                cookieCreated = setCookie,
                tokenType = "Bearer"
            });
        }

        [HttpGet("session-info")]
        [Authorize]
        //[Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
        public IActionResult SessionInfo()
        {
            return Ok(new
            {
                email = HttpContext.Session.GetString("Email"),
                fullName = HttpContext.Session.GetString("FullName"),
                role = HttpContext.Session.GetString("Role"),
                userActiveId = HttpContext.Session.GetString("UserActiveId"),
                sessionExpiresAtUtc = HttpContext.Session.GetString("SessionExpiresAtUtc"),
                sessionDurationMinutes = GetSessionTimeoutMinutes()
            });
        }

        [HttpGet("me")]
        [Authorize]
        //[Authorize(AuthenticationSchemes = "Identity.Application")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var email =
                User.FindFirst(ClaimTypes.Email)?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Claim email tidak ditemukan."
                });
            }

            var user = await _context.UserActives
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    x.IsActive &&
                    (x.IsDelete == false || x.IsDelete == null),
                    ct);

            if (user == null)
            {
                await HttpContext.SignOutAsync(
                    IdentityConstants.ApplicationScheme);

                return Unauthorized(new
                {
                    success = false,
                    message = "User tidak ditemukan atau tidak aktif."
                });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    user.UserActiveId,
                    user.FullName,
                    user.Email,
                    user.IsActive
                }
            });
        }

        //[HttpGet("me")]
        //[Authorize]
        ////[Authorize(AuthenticationSchemes = "Identity.Application")]
        //public async Task<IActionResult> Me(CancellationToken ct)
        //{
        //    try
        //    {
        //        if (User?.Identity?.IsAuthenticated != true)
        //        {
        //            return Unauthorized(new
        //            {
        //                success = false,
        //                statusCode = 401,
        //                message = "User belum login."
        //            });
        //        }

        //        var email =
        //            User.FindFirst(ClaimTypes.Email)?.Value ??
        //            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //        var fullName =
        //            User.FindFirst(ClaimTypes.Name)?.Value;

        //        var role =
        //            User.FindFirst("role")?.Value ??
        //            User.FindFirst(ClaimTypes.Role)?.Value;

        //        var userActiveId =
        //            User.FindFirst("UserActiveId")?.Value;

        //        // Superadmin hardcode
        //        if (email?.Equals(
        //                "superadmin@admin.com",
        //                StringComparison.OrdinalIgnoreCase) == true)
        //        {
        //            return Ok(new
        //            {
        //                success = true,
        //                statusCode = 200,
        //                data = new
        //                {
        //                    Email = email,
        //                    FullName = fullName,
        //                    Role = role,
        //                    UserActiveId = userActiveId,
        //                    IsSuperAdmin = true
        //                }
        //            });
        //        }

        //        if (string.IsNullOrWhiteSpace(email))
        //        {
        //            return Unauthorized(new
        //            {
        //                success = false,
        //                statusCode = 401,
        //                message = "Claim email tidak ditemukan."
        //            });
        //        }

        //        var user = await _context.UserActives
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(x =>
        //                x.Email == email &&
        //                x.IsActive &&
        //                (x.IsDelete == false || x.IsDelete == null),
        //                ct);

        //        if (user == null)
        //        {
        //            await HttpContext.SignOutAsync("Identity.Application");

        //            return Unauthorized(new
        //            {
        //                success = false,
        //                statusCode = 401,
        //                message = "User tidak ditemukan atau sudah tidak aktif."
        //            });
        //        }

        //        var tipeUser = await _context.TipeUsers
        //            .AsNoTracking()
        //            .Where(x => x.TipeUserId == user.TipeUserId)
        //            .Select(x => x.NamaTipeUser)
        //            .FirstOrDefaultAsync(ct);

        //        return Ok(new
        //        {
        //            success = true,
        //            statusCode = 200,
        //            data = new
        //            {
        //                user.UserActiveId,
        //                user.UserActiveCode,
        //                user.FullName,
        //                user.Email,
        //                user.Handphone,
        //                user.IsActive,
        //                user.DepartemenId,
        //                user.PositionId,
        //                user.TipeUserId,
        //                Role = tipeUser ?? role,
        //                IsSuperAdmin = false
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);

        //        return StatusCode(500, new
        //        {
        //            success = false,
        //            statusCode = 500,
        //            message = ex.Message
        //        });
        //    }
        //}

        [HttpGet("debug-auth")]
        [Authorize]
        public IActionResult DebugAuth()
        {
            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                AuthenticationType = User.Identity?.AuthenticationType,
                Claims = User.Claims.Select(x => new
                {
                    x.Type,
                    x.Value
                })
            });
        }

        [HttpGet("debug-cookie")]
        [AllowAnonymous]
        public IActionResult DebugCookie()
        {
            return Ok(new
            {
                Cookies = Request.Cookies.Keys.ToList(),
                HasAuthCookie = Request.Cookies.ContainsKey(".Quilvian.Auth"),
                HasSessionCookie = Request.Cookies.ContainsKey(".Quilvian.Session"),
                AuthCookie = Request.Cookies.ContainsKey(".Quilvian.Auth")
                    ? "FOUND"
                    : "NOT_FOUND"
            });
        }

        [HttpPost("keep-alive")]
        [Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
        public async Task<IActionResult> KeepAlive([FromQuery] bool setCookie = true)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? HttpContext.Session.GetString("Email");

            var userActiveIdClaim = User.FindFirst("UserActiveId")?.Value
                                 ?? HttpContext.Session.GetString("UserActiveId");

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(userActiveIdClaim))
            {
                return Unauthorized(new { message = "User tidak terautentikasi." });
            }

            UserActive? userActive = null;

            if (Guid.TryParse(userActiveIdClaim, out var userActiveId))
            {
                userActive = await _context.UserActives
                    .FirstOrDefaultAsync(x => x.UserActiveId == userActiveId && x.IsActive);
            }

            if (userActive == null && !string.IsNullOrWhiteSpace(email))
            {
                userActive = await _context.UserActives
                    .FirstOrDefaultAsync(x => x.Email == email && x.IsActive);
            }

            if (userActive == null)
            {
                return Unauthorized(new { message = "User tidak ditemukan atau tidak aktif." });
            }

            var roleName = await _context.TipeUsers
                .Where(t => t.TipeUserId == userActive.TipeUserId)
                .Select(t => t.NamaTipeUser)
                .FirstOrDefaultAsync() ?? "Guest";

            if (setCookie)
            {
                await SignInIdentityCookieAsync(
                    userActive.Email ?? userActive.UserActiveId.ToString(),
                    userActive.FullName ?? string.Empty,
                    userActive.UserActiveId.ToString(),
                    roleName
                );
            }

            SetSession(
                userActive.Email ?? userActive.UserActiveId.ToString(),
                userActive.FullName ?? string.Empty,
                userActive.UserActiveId.ToString(),
                roleName
            );

            var jwt = BuildJwtToken(
                userActive.Email ?? userActive.UserActiveId.ToString(),
                roleName,
                userActive.UserActiveId.ToString(),
                userActive.FullName
            );

            return Ok(new
            {
                message = "Session diperpanjang.",
                token = jwt.Token,
                tokenType = "Bearer",
                expiration = jwt.ExpirationUtc,
                sessionDurationMinutes = GetSessionTimeoutMinutes(),
                sessionExpiresAtUtc = DateTime.UtcNow.AddMinutes(GetSessionTimeoutMinutes()),
                cookieUpdated = setCookie
            });
        }

        private JwtResult BuildJwtToken(string email, string role, string? userActiveId, string? fullName)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", role ?? "Guest"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName ?? email)
            };

            if (!string.IsNullOrWhiteSpace(userActiveId))
                claims.Add(new Claim("UserActiveId", userActiveId));

            var expiresUtc = DateTime.UtcNow.AddMinutes(GetJwtExpireMinutes());
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresUtc,
                signingCredentials: credentials
            );

            return new JwtResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpirationUtc = token.ValidTo
            };
        }

        private async Task SignInIdentityCookieAsync(string email, string fullName, string? userActiveId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, fullName ?? string.Empty),
                new Claim("role", role ?? "Guest")
            };

            if (!string.IsNullOrWhiteSpace(userActiveId))
                claims.Add(new Claim("UserActiveId", userActiveId));

            var identity = new ClaimsIdentity(claims, IdentityScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                IdentityScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(GetSessionTimeoutMinutes())
                    //ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(GetSessionTimeoutSeconds())
                });
        }

        private void SetSession(string email, string fullName, string? userActiveId, string role)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(GetSessionTimeoutMinutes());
            //var expiresAt = DateTime.UtcNow.AddSeconds(GetSessionTimeoutSeconds());
            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("FullName", fullName ?? string.Empty);
            HttpContext.Session.SetString("Role", role ?? "Guest");
            HttpContext.Session.SetString("SessionId", Guid.NewGuid().ToString());
            HttpContext.Session.SetString("SessionExpiresAtUtc", expiresAt.ToString("O"));

            if (!string.IsNullOrWhiteSpace(userActiveId))
                HttpContext.Session.SetString("UserActiveId", userActiveId);
        }

        //private int GetJwtExpireSeconds()
        //{
        //    var seconds = _configuration.GetValue<int?>("Jwt:ExpirationInSeconds");

        //    if (seconds.HasValue && seconds.Value > 0)
        //        return seconds.Value;

        //    var minutes = _configuration.GetValue<int?>("Jwt:ExpirationInMinutes");

        //    return (minutes.HasValue && minutes.Value > 0 ? minutes.Value : 180) * 60;
        //}

        private int GetJwtExpireMinutes()
        {
            var value = _configuration.GetValue<int?>("Jwt:ExpirationInMinutes");
            return value.HasValue && value.Value > 0 ? value.Value : 180;
        }

        private int GetSessionTimeoutMinutes()
        {
            var value = _configuration.GetValue<int?>("AuthSession:CookieExpireMinutes");
            return value.HasValue && value.Value > 0 ? value.Value : 180;
        }

        //private int GetSessionTimeoutSeconds()
        //{
        //    var seconds = _configuration.GetValue<int?>("AuthSession:CookieExpireSeconds");

        //    if (seconds.HasValue && seconds.Value > 0)
        //        return seconds.Value;

        //    var minutes = _configuration.GetValue<int?>("AuthSession:CookieExpireMinutes");

        //    return (minutes.HasValue && minutes.Value > 0 ? minutes.Value : 180) * 60;
        //}
        private bool IsAllowedRedirect(string targetUrl)
        {
            if (Url.IsLocalUrl(targetUrl))
                return true;

            if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
                return false;

            var allowedHosts = _configuration
                .GetSection("AutoLogin:AllowedRedirectHosts")
                .Get<string[]>() ?? Array.Empty<string>();

            return allowedHosts.Any(h => h.Equals(uri.Host, StringComparison.OrdinalIgnoreCase));
        }

        private ContentResult AutoLoginRedirectWithJwt(
                string token,
                DateTime expirationUtc,
                string targetUrl)
                    {
                        var safeToken = JavaScriptEncoder.Default.Encode(token);
                        var safeExpiration = JavaScriptEncoder.Default.Encode(expirationUtc.ToString("O"));
                        var safeTarget = JavaScriptEncoder.Default.Encode(targetUrl);

                        var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8' />
                <title>Auto Login</title>
            </head>
            <body>
                <p>Sedang login otomatis...</p>

                <script>
                    localStorage.setItem('token', '{safeToken}');
                    localStorage.setItem('expiration', '{safeExpiration}');
                    localStorage.setItem('tokenType', 'Bearer');

                    window.location.replace('{safeTarget}');
                </script>
            </body>
            </html>";

            return Content(html, "text/html; charset=utf-8");
        }

        private sealed class JwtResult
        {
            public string Token { get; set; } = string.Empty;
            public DateTime ExpirationUtc { get; set; }
        }
    }

    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // false = hanya JWT
        // true  = JWT + cookie Identity
        //public bool SetCookie { get; set; } = false;
    }
}