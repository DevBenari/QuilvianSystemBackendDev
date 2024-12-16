using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystem.Areas.HealthManagement.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;

namespace QuilvianSystem.Areas.HealthManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class ScheduleTodayController : Controller
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ScheduleTodayController
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


        // Create a new schedule
        [HttpPost("schedule-today")]
        public IActionResult CreateSchedule([FromBody] ScheduleToday scheduleToday)
        {
            if (scheduleToday == null)
                return BadRequest("Invalid schedule data.");

            _applicationDbContext.ScheduleTodays.Add(scheduleToday);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetSchedule), new { id = scheduleToday.ScheduleTodayId }, scheduleToday);
        }

        // Get all schedules for today
        [HttpGet("schedule-today")]
        public IActionResult GetAllSchedules()
        {
            var schedules = _applicationDbContext.ScheduleTodays.ToList();
            return Ok(schedules);
        }

        // Get a specific schedule by ID
        [HttpGet("schedule-today/{id}")]
        public IActionResult GetSchedule(Guid id)
        {
            var scheduleToday = _applicationDbContext.ScheduleTodays.Find(id);
            if (scheduleToday == null)
                return NotFound();

            return Ok(scheduleToday);
        }

        // Update a schedule
        [HttpPut("schedule-today/{id}")]
        public IActionResult UpdateSchedule(Guid id, [FromBody] ScheduleToday updatedSchedule)
        {
            var existingSchedule = _applicationDbContext.ScheduleTodays.Find(id);
            if (existingSchedule == null)
                return NotFound();

            existingSchedule.KodeJadwal = updatedSchedule.KodeJadwal;
            existingSchedule.DoctorId = updatedSchedule.DoctorId;
            existingSchedule.DepartmentId = updatedSchedule.DepartmentId;
            existingSchedule.DayId = updatedSchedule.DayId;
            existingSchedule.TanggalPraktek = updatedSchedule.TanggalPraktek;
            existingSchedule.JamMulai = updatedSchedule.JamMulai;
            existingSchedule.JamSelesai = updatedSchedule.JamSelesai;
            existingSchedule.LamaPeriksaPerPasien = updatedSchedule.LamaPeriksaPerPasien;
            existingSchedule.PagiSore = updatedSchedule.PagiSore;
            existingSchedule.Ruangan = updatedSchedule.Ruangan;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a schedule
        [HttpDelete("schedule-today/{id}")]
        public IActionResult DeleteSchedule(Guid id)
        {
            var scheduleToday = _applicationDbContext.ScheduleTodays.Find(id);
            if (scheduleToday == null)
                return NotFound();

            _applicationDbContext.ScheduleTodays.Remove(scheduleToday);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
