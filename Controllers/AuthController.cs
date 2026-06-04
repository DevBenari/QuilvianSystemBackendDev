using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
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
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IOptions<AutoLoginDTO> optAutoLogin,
            ILogger<AuthController> logger)
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
                    SetJwtCookie(jwt.Token, jwt.ExpirationUtc);
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

            // 2. LOGIN EMAIL + PASSWORD
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
                    SetJwtCookie(jwt.Token, jwt.ExpirationUtc);
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
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst(ClaimTypes.Email)?.Value
                     ?? HttpContext.Session.GetString("Email")
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(email))
            {
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
            }

            HttpContext.Session.Clear();

            DeleteJwtCookie();
            DeleteSessionCookie();

            // Hapus cookie lama kalau masih tersisa dari versi Identity Cookie sebelumnya.
            Response.Cookies.Delete(".Quilvian.Auth", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return Ok(new { message = "Logout berhasil." });
        }

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

            var jwt = BuildJwtToken(
                userActive.Email ?? userActive.UserActiveId.ToString(),
                roleName,
                userActive.UserActiveId.ToString(),
                userActive.FullName
            );

            if (setCookie)
            {
                SetJwtCookie(jwt.Token, jwt.ExpirationUtc);
            }

            SetSession(
                userActive.Email ?? userActive.UserActiveId.ToString(),
                userActive.FullName ?? string.Empty,
                userActive.UserActiveId.ToString(),
                roleName
            );

            appUser.IsOnline = true;
            await _userManager.UpdateAsync(appUser);

            if (redirect)
            {
                if (string.IsNullOrWhiteSpace(result.TargetUrl))
                    return BadRequest(new { message = "Target URL kosong." });

                if (!IsAllowedRedirect(result.TargetUrl))
                    return BadRequest(new { message = "Target URL tidak valid." });

                return AutoLoginRedirect(result.TargetUrl);
            }

            return Ok(new
            {
                message = "Autologin berhasil",
                expiration = jwt.ExpirationUtc,
                targetUrl = result.TargetUrl,
                sessionDurationMinutes = GetSessionTimeoutMinutes(),
                sessionExpiresAtUtc = DateTime.UtcNow.AddMinutes(GetSessionTimeoutMinutes()),
                jwtCookieCreated = setCookie,
                tokenType = "Bearer",
                authMode = "JwtHttpOnlyCookie"
            });
        }

        [HttpGet("session-info")]
        [Authorize]
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
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var email =
                User.FindFirst(ClaimTypes.Email)?.Value ??
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Claim email tidak ditemukan."
                });
            }

            if (email.Equals("superadmin@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        UserActiveId = (string?)null,
                        FullName = "Superadmin",
                        Email = email,
                        IsActive = true,
                        IsSuperAdmin = true
                    }
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
                DeleteJwtCookie();
                DeleteSessionCookie();

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

        [HttpGet("debug-auth")]
        [Authorize]
        public IActionResult DebugAuth()
        {
            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                AuthenticationType = User.Identity?.AuthenticationType,

                JwtCookieName = GetJwtCookieName(),
                HasJwtCookie = Request.Cookies.ContainsKey(GetJwtCookieName()),
                HasSessionCookie = Request.Cookies.ContainsKey(".Quilvian.Session"),
                HasOldIdentityAuthCookie = Request.Cookies.ContainsKey(".Quilvian.Auth"),

                HasAuthorizationHeader = Request.Headers.ContainsKey("Authorization"),

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
            var jwtCookieName = GetJwtCookieName();

            return Ok(new
            {
                Cookies = Request.Cookies.Keys.ToList(),

                JwtCookieName = jwtCookieName,
                HasJwtCookie = Request.Cookies.ContainsKey(jwtCookieName),

                HasSessionCookie = Request.Cookies.ContainsKey(".Quilvian.Session"),

                // Cookie lama dari Identity Cookie. Seharusnya nanti tidak dipakai lagi.
                HasOldIdentityAuthCookie = Request.Cookies.ContainsKey(".Quilvian.Auth")
            });
        }

        [HttpPost("keep-alive")]
        [Authorize]
        public async Task<IActionResult> KeepAlive()
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
                if (email?.Equals("superadmin@admin.com", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var superJwt = BuildJwtToken(
                        email,
                        "Superadmin",
                        null,
                        "Superadmin"
                    );

                    SetJwtCookie(superJwt.Token, superJwt.ExpirationUtc);

                    SetSession(
                        email,
                        "Superadmin",
                        null,
                        "Superadmin"
                    );

                    return Ok(new
                    {
                        message = "Session diperpanjang.",
                        tokenType = "Bearer",
                        authMode = "JwtHttpOnlyCookie",
                        expiration = superJwt.ExpirationUtc,
                        sessionDurationMinutes = GetSessionTimeoutMinutes(),
                        sessionExpiresAtUtc = DateTime.UtcNow.AddMinutes(GetSessionTimeoutMinutes()),
                        jwtCookieUpdated = true
                    });
                }

                return Unauthorized(new { message = "User tidak ditemukan atau tidak aktif." });
            }

            var roleName = await _context.TipeUsers
                .Where(t => t.TipeUserId == userActive.TipeUserId)
                .Select(t => t.NamaTipeUser)
                .FirstOrDefaultAsync() ?? "Guest";

            var jwt = BuildJwtToken(
                userActive.Email ?? userActive.UserActiveId.ToString(),
                roleName,
                userActive.UserActiveId.ToString(),
                userActive.FullName
            );

            SetJwtCookie(jwt.Token, jwt.ExpirationUtc);

            SetSession(
                userActive.Email ?? userActive.UserActiveId.ToString(),
                userActive.FullName ?? string.Empty,
                userActive.UserActiveId.ToString(),
                roleName
            );

            return Ok(new
            {
                message = "Session diperpanjang.",
                tokenType = "Bearer",
                authMode = "JwtHttpOnlyCookie",
                expiration = jwt.ExpirationUtc,
                sessionDurationMinutes = GetSessionTimeoutMinutes(),
                sessionExpiresAtUtc = DateTime.UtcNow.AddMinutes(GetSessionTimeoutMinutes()),
                jwtCookieUpdated = true
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
                new Claim(ClaimTypes.Role, role ?? "Guest"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, email),
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

        private void SetSession(string email, string fullName, string? userActiveId, string role)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(GetSessionTimeoutMinutes());

            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("FullName", fullName ?? string.Empty);
            HttpContext.Session.SetString("Role", role ?? "Guest");
            HttpContext.Session.SetString("SessionId", Guid.NewGuid().ToString());
            HttpContext.Session.SetString("SessionExpiresAtUtc", expiresAt.ToString("O"));

            if (!string.IsNullOrWhiteSpace(userActiveId))
                HttpContext.Session.SetString("UserActiveId", userActiveId);
        }

        private string GetJwtCookieName()
        {
            return _configuration["Jwt:CookieName"] ?? ".Quilvian.AccessToken";
        }

        private void SetJwtCookie(string token, DateTime expirationUtc)
        {
            Response.Cookies.Append(
                GetJwtCookieName(),
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = new DateTimeOffset(expirationUtc),
                    IsEssential = true,
                    Path = "/"
                });
        }

        private void DeleteJwtCookie()
        {
            Response.Cookies.Delete(
                GetJwtCookieName(),
                new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
        }

        private void DeleteSessionCookie()
        {
            Response.Cookies.Delete(
                ".Quilvian.Session",
                new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
        }

        private int GetJwtExpireMinutes()
        {
            var expireMinutes =
                _configuration.GetValue<int?>("Jwt:ExpirationInMinutes") ??
                _configuration.GetValue<int?>("Jwt:ExpireMinutes");

            return expireMinutes.HasValue && expireMinutes.Value > 0
                ? expireMinutes.Value
                : 180;
        }

        private int GetSessionTimeoutMinutes()
        {
            var value = _configuration.GetValue<int?>("AuthSession:CookieExpireMinutes");
            return value.HasValue && value.Value > 0 ? value.Value : 180;
        }

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

        private ContentResult AutoLoginRedirect(string targetUrl)
        {
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
    }
}