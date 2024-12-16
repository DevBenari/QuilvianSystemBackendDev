using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystem.Areas.MasterData.Models;
using QuilvianSystem.Areas.MasterData.ViewModels;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;
using System.Data;

namespace QuilvianSystem.Areas.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class UserActiveController : Controller
    {
            private readonly ApplicationDbContext _applicationDbContext;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SignInManager<ApplicationUser> _signInManager;


        public UserActiveController
            (
                ApplicationDbContext applicationDbContext,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager
            )
            {
                _userManager = userManager;
            _signInManager = signInManager;
            _applicationDbContext = applicationDbContext;
            }

        [HttpGet]
            public IActionResult GetUserActives()
            {
                var useractive = _applicationDbContext.UserActives.ToList();
                if (useractive == null || !useractive.Any())
                {
                    return NotFound(new { message = "Belum ada data useractive." });
                }
                return Ok(useractive);
            }
        [HttpPost]
        public async Task<IActionResult> AddUserActive([FromBody] UserActiveViewModel useractive)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var lastCode = _applicationDbContext.UserActives
                                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                                .OrderByDescending(k => k.UserActiveCode)
                                .FirstOrDefault();

            if (lastCode == null)
            {
                useractive.UserActiveCode = "USR" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.UserActiveCode.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    useractive.UserActiveCode = "USR" + setDateNow + "0001";
                }
                else
                {
                    useractive.UserActiveCode = "USR" + setDateNow + (Convert.ToInt32(lastCode.UserActiveCode.Substring(9, lastCode.UserActiveCode.Length - 9)) + 1).ToString("D4");
                }
            }

            if (ModelState.IsValid)
            {
                var userLogin = new ApplicationUser
                {
                    UserName = useractive.Email, // Biasanya gunakan Email sebagai Username
                    Email = useractive.Email,
                    NamaBelakang = useractive.UserActiveCode,
                    NamaDepan = useractive.FullName
                };

                var daftar = new UserActive
                {
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UserActiveId = Guid.NewGuid(),
                    UserActiveCode = useractive.UserActiveCode,
                    FullName = useractive.FullName,
                    IdentityNumber = useractive.IdentityNumber,
                    PlaceOfBirth = useractive.PlaceOfBirth,
                    DateOfBirth = useractive.DateOfBirth,
                    Gender = useractive.Gender,
                    Address = useractive.Address,
                    Handphone = useractive.Handphone,
                    Email = useractive.Email,
                    Foto = "uniqueFileName", // Anda bisa mengatur foto sesuai dengan logic Anda
                    IsActive = true
                };

                // Cek duplikasi UserActiveCode
                var checkDuplicate = _applicationDbContext.UserActives
                                      .Where(c => c.UserActiveCode == useractive.UserActiveCode)
                                      .ToList();

                if (checkDuplicate.Count == 0)
                {
                    var passTglLahir = useractive.DateOfBirth.ToString("ddMMMyyyy") + "@"; // Menambahkan simbol sebagai password
                    // Buat User menggunakan UserManager
                    var resultLogin = await _userManager.CreateAsync(userLogin, passTglLahir);

                    if (resultLogin.Succeeded)
                    {
                        // Jika pembuatan user berhasil, simpan data ke tabel UserActives
                        _applicationDbContext.UserActives.Add(daftar);
                        await _applicationDbContext.SaveChangesAsync();

                        // Return response yang sesuai
                        return CreatedAtAction(nameof(GetUserActives), new { message = "Tambah Data Sukses" }, useractive);
                    }
                    else
                    {
                        // Kembalikan error jika pembuatan user gagal
                        return BadRequest(new { message = "Pembuatan user gagal", errors = resultLogin.Errors });
                    }
                }
                else
                {
                    return NotFound(new { message = "Terdapat duplikasi data !!!" });
                }
            }
            else
            {
                return BadRequest(new { message = "Data tidak valid !!!" });
            }

            // Server
        }

            [HttpPut("{id}")]
            public IActionResult UpdateUserActive(Guid id, [FromBody] UserActive updateUserActive)
            {
                var useractive = _applicationDbContext.UserActives.Find(id);
                if (useractive == null) return NotFound();

                useractive.FullName = updateUserActive.FullName;
                useractive.IdentityNumber = updateUserActive.IdentityNumber;
                useractive.PlaceOfBirth = updateUserActive.PlaceOfBirth;
                useractive.DateOfBirth = updateUserActive.DateOfBirth;
                useractive.Gender = updateUserActive.Gender;
                useractive.Email = updateUserActive.Email;
                useractive.Address = updateUserActive.Address;
                useractive.Handphone = updateUserActive.Handphone;
                useractive.Email = updateUserActive.Email;
                useractive.Foto = updateUserActive.Foto;
                useractive.IsActive = updateUserActive.IsActive;

                _applicationDbContext.SaveChanges();
                return NoContent();
            }

            [HttpDelete("{id}")]
            public IActionResult DeleteUserActive(Guid id)
            {
                var useractive = _applicationDbContext.UserActives.Find(id);
                if (useractive == null) return NotFound();

                _applicationDbContext.UserActives.Remove(useractive);
                _applicationDbContext.SaveChanges();
                return NoContent();
            }
        
    }
}
