using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Observasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ObservasiCairanWsdController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ObservasiCairanWsdController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager
        )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from o in _db.ObservasiCairanWsds
                        where o.IsDelete == false
                        select new
                        {
                            o.ObservasiCairanWSDId,
                            o.KunjunganId,
                            o.PasienId,
                            o.UserActiveId,
                            o.TglAwalObservasiWSD,
                            o.TglAkhirObservasiWSD,
                            o.CairanSisaWSDSebelumnya,
                            o.CairanWSDBertambah,
                            o.CairanSisaWSDTabung,
                            o.TtdId,
                            o.PathTtd,
                            o.Keterangan,
                            o.CreateDateTime
                        };

            var totalRows = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreateDateTime)
                                  .Skip((page - 1) * perPage)
                                  .Take(perPage)
                                  .ToListAsync();

            return Ok(new
            {
                message = "Data retrieved successfully",
                data,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _db.ObservasiCairanWsds.FindAsync(id);
            if (item == null || item.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan", data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ObservasiCairanWsdViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _db.UserActives.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            var entity = new ObservasiCairanWsd
            {
                ObservasiCairanWSDId = Guid.NewGuid(),
                KunjunganId = vm.KunjunganId,
                PasienId = vm.PasienId,
                UserActiveId = user.UserActiveId,
                TglAwalObservasiWSD = vm.TglAwalObservasiWSD,
                TglAkhirObservasiWSD = vm.TglAkhirObservasiWSD,
                CairanSisaWSDSebelumnya = vm.CairanSisaWSDSebelumnya,
                CairanWSDBertambah = vm.CairanWSDBertambah,
                CairanSisaWSDTabung = vm.CairanSisaWSDTabung,
                TtdId = vm.TtdId,
                PathTtd = vm.PathTtd,
                Keterangan = vm.Keterangan,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = user.UserActiveId
            };

            _db.ObservasiCairanWsds.Add(entity);
            await _db.SaveChangesAsync();

            return Created("", new { message = "Data berhasil ditambahkan." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ObservasiCairanWsdViewModel vm)
        {
            var item = await _db.ObservasiCairanWsds.FindAsync(id);
            if (item == null || item.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _db.UserActives.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            item.KunjunganId = vm.KunjunganId;
            item.PasienId = vm.PasienId;
            item.TglAwalObservasiWSD = vm.TglAwalObservasiWSD;
            item.TglAkhirObservasiWSD = vm.TglAkhirObservasiWSD;
            item.CairanSisaWSDSebelumnya = vm.CairanSisaWSDSebelumnya;
            item.CairanWSDBertambah = vm.CairanWSDBertambah;
            item.CairanSisaWSDTabung = vm.CairanSisaWSDTabung;
            item.TtdId = vm.TtdId;
            item.PathTtd = vm.PathTtd;
            item.Keterangan = vm.Keterangan;
            item.UpdateBy = user.UserActiveId;
            item.UpdateDateTime = DateTime.UtcNow;

            _db.ObservasiCairanWsds.Update(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Data berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var item = await _db.ObservasiCairanWsds.FindAsync(id);
            if (item == null || item.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _db.UserActives.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            item.IsDelete = true;
            item.DeleteBy = user.UserActiveId;
            item.DeleteDateTime = DateTime.UtcNow;

            _db.ObservasiCairanWsds.Update(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus (soft delete)." });
        }
    }
}
