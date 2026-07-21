using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.COA.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class COAMappingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<COAMappingController> _logger;

        public COAMappingController(
            ApplicationDbContext context,
            ILogger<COAMappingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================= GET ALL =================
        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int perPage = 10)
        {
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            var query =
                from m in _context.COAMappings

                join coaData in _context.MasterCoas
                        .Where(x => x.IsDelete == false)
                    on m.COAId equals coaData.COAId
                    into coaJoin

                from coa in coaJoin.DefaultIfEmpty()

                join obatData in _context.Obats
                        .Where(x => x.IsDelete == false)
                    on m.TransaksiId equals obatData.ObatId
                    into obatJoin

                from obat in obatJoin.DefaultIfEmpty()

                join tindakanData in _context.Tindakans
                        .Where(x => x.IsDelete == false)
                    on m.TransaksiId equals tindakanData.TindakanId
                    into tindakanJoin

                from tindakan in tindakanJoin.DefaultIfEmpty()

                join pemeriksaanData in _context.LabPemeriksaans
                        .Where(x => x.IsDelete == false)
                    on m.TransaksiId equals pemeriksaanData.PemeriksaanLabId
                    into pemeriksaanJoin

                from pemeriksaan in pemeriksaanJoin.DefaultIfEmpty()

                join userData in _context.UserActives
                    on m.CreateBy equals userData.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where m.IsDelete == false

                select new
                {
                    m.COAMappingId,
                    m.TransaksiId,

                    // OBAT, TINDAKAN, atau PEMERIKSAAN
                    JenisTransaksi = m.NamaTransaksi,

                    // Nama item berdasarkan jenis transaksi
                    NamaItem =
                        m.NamaTransaksi == "OBAT"
                            ? obat != null
                                ? obat.ObatName
                                : null

                        : m.NamaTransaksi == "TINDAKAN"
                            ? tindakan != null
                                ? tindakan.NamaTindakan
                                : null

                        : m.NamaTransaksi == "PEMERIKSAAN"
                            ? pemeriksaan != null
                                ? pemeriksaan.NamaPemeriksaan
                                : null

                        : null,

                    m.COAId,

                    NamaCOA = coa != null
                        ? coa.NamaCOA
                        : m.NamaCOA,

                    m.Keterangan,
                    m.CreateDateTime,

                    CreateByName = user != null
                        ? user.FullName
                        : null
                };

            var totalRows = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                message = "success",
                data,
                pagination = new
                {
                    page,
                    perPage,
                    totalRows,
                    totalPages = (int)Math.Ceiling(
                        totalRows / (double)perPage)
                }
            });
        }

        // ================= GET BY ID =================
        // ================= GET BY ID =================
        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data =
                await (
                    from m in _context.COAMappings

                    join coaData in _context.MasterCoas
                            .Where(x => x.IsDelete == false)
                        on m.COAId equals coaData.COAId
                        into coaJoin

                    from coa in coaJoin.DefaultIfEmpty()

                    join obatData in _context.Obats
                            .Where(x => x.IsDelete == false)
                        on m.TransaksiId equals obatData.ObatId
                        into obatJoin

                    from obat in obatJoin.DefaultIfEmpty()

                    join tindakanData in _context.Tindakans
                            .Where(x => x.IsDelete == false)
                        on m.TransaksiId equals tindakanData.TindakanId
                        into tindakanJoin

                    from tindakan in tindakanJoin.DefaultIfEmpty()

                    join pemeriksaanData in _context.LabPemeriksaans
                            .Where(x => x.IsDelete == false)
                        on m.TransaksiId equals pemeriksaanData.PemeriksaanLabId
                        into pemeriksaanJoin

                    from pemeriksaan in pemeriksaanJoin.DefaultIfEmpty()

                    join userData in _context.UserActives
                        on m.CreateBy equals userData.UserActiveId
                        into userJoin

                    from user in userJoin.DefaultIfEmpty()

                    where m.COAMappingId == id &&
                          m.IsDelete == false

                    select new
                    {
                        m.COAMappingId,
                        m.TransaksiId,

                        // OBAT, TINDAKAN, atau PEMERIKSAAN
                        JenisTransaksi = m.NamaTransaksi,

                        NamaItem =
                            m.NamaTransaksi == "OBAT"
                                ? obat != null
                                    ? obat.ObatName
                                    : null

                            : m.NamaTransaksi == "TINDAKAN"
                                ? tindakan != null
                                    ? tindakan.NamaTindakan
                                    : null

                            : m.NamaTransaksi == "PEMERIKSAAN"
                                ? pemeriksaan != null
                                    ? pemeriksaan.NamaPemeriksaan
                                    : null

                            : null,

                        m.COAId,

                        NamaCOA = coa != null
                            ? coa.NamaCOA
                            : m.NamaCOA,

                        m.Keterangan,
                        m.CreateDateTime,
                        m.UpdateDateTime,

                        CreateByName = user != null
                            ? user.FullName
                            : null
                    }
                )
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    message = "Data tidak ditemukan"
                });
            }

            return Ok(new
            {
                message = "success",
                data
            });
        }

        //// ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] COAMapping model)
        {
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            if (model.TransaksiId == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "TransaksiId wajib diisi"
                });
            }

            if (string.IsNullOrWhiteSpace(model.NamaTransaksi))
            {
                return BadRequest(new
                {
                    message = "NamaTransaksi wajib diisi dengan OBAT, TINDAKAN, atau PEMERIKSAAN"
                });
            }

            var coa = await _context.MasterCoas
                .FirstOrDefaultAsync(x =>
                    x.COAId == model.COAId &&
                    x.IsDelete == false);

            if (coa == null)
            {
                return BadRequest(new
                {
                    message = "COA tidak ditemukan"
                });
            }

            var jenisTransaksi = model.NamaTransaksi
                .Trim()
                .ToUpper();

            string? namaItem = null;

            switch (jenisTransaksi)
            {
                case "OBAT":
                    namaItem = await _context.Obats
                        .Where(x =>
                            x.ObatId == model.TransaksiId &&
                            x.IsDelete == false)
                        .Select(x => x.ObatName)
                        .FirstOrDefaultAsync();
                    break;

                case "TINDAKAN":
                    namaItem = await _context.Tindakans
                        .Where(x =>
                            x.TindakanId == model.TransaksiId &&
                            x.IsDelete == false)
                        .Select(x => x.NamaTindakan)
                        .FirstOrDefaultAsync();
                    break;

                case "PEMERIKSAAN":
                    namaItem = await _context.LabPemeriksaans
                        .Where(x =>
                            x.PemeriksaanLabId == model.TransaksiId &&
                            x.IsDelete == false)
                        .Select(x => x.NamaPemeriksaan)
                        .FirstOrDefaultAsync();
                    break;

                default:
                    return BadRequest(new
                    {
                        message = "NamaTransaksi hanya boleh OBAT, TINDAKAN, atau PEMERIKSAAN"
                    });
            }

            if (string.IsNullOrWhiteSpace(namaItem))
            {
                return BadRequest(new
                {
                    message = $"Data {jenisTransaksi.ToLower()} tidak ditemukan berdasarkan TransaksiId"
                });
            }

            var mappingExists = await _context.COAMappings
                .AnyAsync(x =>
                    x.TransaksiId == model.TransaksiId &&
                    x.NamaTransaksi == jenisTransaksi &&
                    x.COAId == model.COAId &&
                    x.IsDelete == false);

            if (mappingExists)
            {
                return BadRequest(new
                {
                    message = "Mapping transaksi dengan COA tersebut sudah tersedia"
                });
            }

            model.COAMappingId = Guid.NewGuid();

            // NamaTransaksi tetap menyimpan jenis transaksi.
            model.NamaTransaksi = jenisTransaksi;

            model.NamaCOA = coa.NamaCOA;

            model.CreateBy = user.UserActiveId;
            model.CreateDateTime = DateTime.UtcNow;
            model.IsDelete = false;

            _context.COAMappings.Add(model);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "created",
                data = new
                {
                    model.COAMappingId,
                    model.TransaksiId,

                    // Jenis transaksi: OBAT, TINDAKAN, atau PEMERIKSAAN.
                    model.NamaTransaksi,

                    // Nama asli yang ditemukan berdasarkan TransaksiId.
                    NamaItem = namaItem,

                    model.COAId,
                    model.NamaCOA,
                    model.Keterangan
                }
            });
        }
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] COAMapping model)
        //{
        //    var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    var user = await _context.UserActives
        //        .FirstOrDefaultAsync(x => x.Email == email);

        //    if (user == null)
        //        return Unauthorized();

        //    var coa = await _context.MasterCoas
        //        .FirstOrDefaultAsync(x => x.COAId == model.COAId && x.IsDelete == false);

        //    if (coa == null)
        //        return BadRequest(new
        //        {
        //            message = "COA tidak ditemukan"
        //        });

        //    model.COAMappingId = Guid.NewGuid();
        //    model.NamaCOA = coa.NamaCOA;

        //    // Ambil NamaTransaksi dari tabel transaksi sesuai kebutuhan Anda
        //    // model.NamaTransaksi = ...

        //    model.CreateBy = user.UserActiveId;
        //    model.CreateDateTime = DateTime.UtcNow;
        //    model.IsDelete = false;

        //    _context.COAMappings.Add(model);

        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        message = "created"
        //    });
        //}

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] COAMapping model)
        {
            var data = await _context.COAMappings
                .FirstOrDefaultAsync(x => x.COAMappingId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            var coa = await _context.MasterCoas
                .FirstOrDefaultAsync(x => x.COAId == model.COAId && x.IsDelete == false);

            if (coa == null)
                return BadRequest(new
                {
                    message = "COA tidak ditemukan"
                });

            data.TransaksiId = model.TransaksiId;
            data.NamaTransaksi = model.NamaTransaksi;
            data.COAId = model.COAId;
            data.NamaCOA = coa.NamaCOA;
            data.Keterangan = model.Keterangan;

            data.UpdateBy = user?.UserActiveId ?? data.UpdateBy;
            data.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "updated"
            });
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.COAMappings
                .FirstOrDefaultAsync(x => x.COAMappingId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            data.IsDelete = true;
            data.DeleteBy = user.UserActiveId;
            data.DeleteDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "deleted"
            });
        }

        // ================= PAGED =================
        // ================= PAGED =================
        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null)
        {
            if (page < 1)
                page = 1;

            if (perPage < 1)
                perPage = 10;

            var query =
                from m in _context.COAMappings

                join coaData in _context.MasterCoas
                        .Where(x => x.IsDelete == false)
                    on m.COAId equals coaData.COAId
                    into coaJoin

                from coa in coaJoin.DefaultIfEmpty()

                join obatData in _context.Obats
                        .Where(x => x.IsDelete == false)
                    on m.TransaksiId equals obatData.ObatId
                    into obatJoin

                from obat in obatJoin.DefaultIfEmpty()

                join tindakanData in _context.Tindakans
                        .Where(x => x.IsDelete == false)
                    on m.TransaksiId equals tindakanData.TindakanId
                    into tindakanJoin

                from tindakan in tindakanJoin.DefaultIfEmpty()

                join pemeriksaanData in _context.LabPemeriksaans
                        .Where(x => x.IsDelete == false)
                    on m.TransaksiId equals pemeriksaanData.PemeriksaanLabId
                    into pemeriksaanJoin

                from pemeriksaan in pemeriksaanJoin.DefaultIfEmpty()

                join userData in _context.UserActives
                    on m.CreateBy equals userData.UserActiveId
                    into userJoin

                from user in userJoin.DefaultIfEmpty()

                where m.IsDelete == false

                select new
                {
                    m.COAMappingId,
                    m.TransaksiId,

                    // Jenis transaksi:
                    // OBAT, TINDAKAN, atau PEMERIKSAAN
                    JenisTransaksi = m.NamaTransaksi,

                    // Nama asli transaksi berdasarkan TransaksiId
                    NamaItem =
                        m.NamaTransaksi == "OBAT"
                            ? obat != null
                                ? obat.ObatName
                                : null

                        : m.NamaTransaksi == "TINDAKAN"
                            ? tindakan != null
                                ? tindakan.NamaTindakan
                                : null

                        : m.NamaTransaksi == "PEMERIKSAAN"
                            ? pemeriksaan != null
                                ? pemeriksaan.NamaPemeriksaan
                                : null

                        : null,

                    m.COAId,

                    KodeCOA = coa != null
                        ? coa.KodeCOA
                        : null,

                    NamaCOA = coa != null
                        ? coa.NamaCOA
                        : m.NamaCOA,

                    m.Keterangan,
                    m.CreateDateTime,

                    CreateByName = user != null
                        ? user.FullName
                        : null
                };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = $"%{search.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.JenisTransaksi!, keyword) ||
                    EF.Functions.ILike(x.NamaItem!, keyword) ||
                    EF.Functions.ILike(x.NamaCOA!, keyword) ||
                    EF.Functions.ILike(x.KodeCOA!, keyword) ||
                    EF.Functions.ILike(x.Keterangan!, keyword));
            }

            var totalRows = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                message = "success",
                data,
                pagination = new
                {
                    page,
                    perPage,
                    totalRows,
                    totalPages = (int)Math.Ceiling(
                        totalRows / (double)perPage)
                }
            });
        }
        //[HttpGet("paged")]
        //public async Task<IActionResult> Paged(
        //    int page = 1,
        //    int perPage = 10,
        //    string? search = null)
        //{
        //    var query =
        //        from m in _context.COAMappings
        //        join coa in _context.MasterCoas
        //            on m.COAId equals coa.COAId
        //        join u in _context.UserActives
        //            on m.CreateBy equals u.UserActiveId
        //        where m.IsDelete == false
        //        select new
        //        {
        //            m.COAMappingId,
        //            m.TransaksiId,
        //            m.NamaTransaksi,
        //            m.COAId,
        //            NamaCOA = coa.NamaCOA,
        //            m.Keterangan,
        //            m.CreateDateTime,
        //            CreateByName = u.FullName
        //        };

        //    if (!string.IsNullOrWhiteSpace(search))
        //    {
        //        search = $"%{search.ToLower()}%";

        //        query = query.Where(x =>
        //            EF.Functions.ILike(x.NamaTransaksi!, search) ||
        //            EF.Functions.ILike(x.NamaCOA!, search));
        //    }

        //    var totalRows = await query.CountAsync();

        //    var data = await query
        //        .OrderByDescending(x => x.CreateDateTime)
        //        .Skip((page - 1) * perPage)
        //        .Take(perPage)
        //        .ToListAsync();

        //    return Ok(new
        //    {
        //        message = "success",
        //        data,
        //        pagination = new
        //        {
        //            page,
        //            perPage,
        //            totalRows,
        //            totalPages = (int)Math.Ceiling(totalRows / (double)perPage)
        //        }
        //    });
        //}
    }
}
