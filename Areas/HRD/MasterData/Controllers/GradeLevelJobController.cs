using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class GradeLevelJobController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GradeLevelJobController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/GradeLevelJob?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetPagedGradeLevelJobs(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from j in _context.GradeLevelJobs
                        join p in _context.Positions on j.PositionId equals p.PositionId into positionGradeJoin
                        from p in positionGradeJoin.DefaultIfEmpty()
                        join g in _context.GradePays on j.GradeId equals g.GradePayId into gradeGradeLevelJoin
                        from g in gradeGradeLevelJoin.DefaultIfEmpty()
                        join l in _context.Levels on j.LevelId equals l.LevelId into levelGradeLevelJoin
                        from l in levelGradeLevelJoin.DefaultIfEmpty()
                        join uc in _context.UserActives on j.CreateBy equals uc.UserActiveId into createdByJoin
                        from uc in createdByJoin.DefaultIfEmpty()
                        orderby j.CreateDateTime descending
                        select new
                        {
                            j.GradeLevelJobId,
                            j.PositionId,
                            p.PositionName,
                            j.GradeId,
                            g.KodeGrade,
                            g.MinSalary,
                            g.MaxSalary,
                            j.LevelId,
                            l.KodeLevel,
                            LevelMinSal = l.MinSalary,
                            LevelMaxSal = l.MaxSalary,
                        };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)perPage);

            var data = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                currentPage = page,
                perPage,
                totalItems,
                totalPages,
                data
            });
        }


        // GET: api/GradeLevelJob/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGradeLevelJob(Guid id)
        {
            var item = await _context.GradeLevelJobs.FindAsync(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        // POST: api/GradeLevelJob
        [HttpPost]
        public async Task<IActionResult> CreateGradeLevelJob(GradeLevelJob gradeLevelJob)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            gradeLevelJob.GradeLevelJobId = Guid.NewGuid();
            _context.GradeLevelJobs.Add(gradeLevelJob);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGradeLevelJob), new { id = gradeLevelJob.GradeLevelJobId }, gradeLevelJob);
        }

        // PUT: api/GradeLevelJob/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGradeLevelJob(Guid id, GradeLevelJob gradeLevelJob)
        {
            if (id != gradeLevelJob.GradeLevelJobId)
                return BadRequest();

            _context.Entry(gradeLevelJob).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.GradeLevelJobs.Any(e => e.GradeLevelJobId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/GradeLevelJob/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGradeLevelJob(Guid id)
        {
            var item = await _context.GradeLevelJobs.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.GradeLevelJobs.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
