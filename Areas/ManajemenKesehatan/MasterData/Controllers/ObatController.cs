using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ObatController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ObatController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ObatController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ObatController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllObat(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.Obats
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            ProductId = a.ProductId,
                            ProductCode = a.ProductCode,
                            ProductName = a.ProductName,
                            Barcode = a.Barcode,
                            SupplierId = a.SupplierId,
                            SupplierName = a.SupplierName,
                            KategoryObatId = a.KategoryObatId,
                            NamaKategoriObat = a.NamaKategoriObat,
                            MeasurementId = a.MeasurementId,
                            MeasurementName = a.MeasurementName,
                            WarehouseLocationId = a.WarehouseLocationId,
                            WarehouseLocationName = a.WarehouseLocationName,
                            DiscountId = a.DiscountId,
                            DiscountValue = a.DiscountValue,
                            ExpiredDate = a.ExpiredDate,
                            DosageStrength = a.DosageStrength,
                            DosageVolume = a.DosageVolume,
                            DosageForm = a.DosageForm,
                            Stock = a.Stock,
                            Cogs = a.Cogs,
                            BuyPrice = a.BuyPrice,
                            RetailPrice = a.RetailPrice,
                            StorageLocation = a.StorageLocation,
                            RackNumber = a.RackNumber,
                            IsSupplierUtama = a.IsSupplierUtama,
                            IsActive = a.IsActive,
                            Note = a.Note,
                        };

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetObatById(Guid id)
        {
            var listdata = _applicationDbContext.Obats.Find(id);
            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateObat([FromBody] ObatViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var dateNow = DateTime.UtcNow; ;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Obats
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.ProductCode)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"MDC{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.ProductCode.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"MDC{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"MDC{setDateNow}" + (Convert.ToInt32(lastCode.ProductCode.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.Obats
                    .Any(c => c.ProductCode == kode && c.ProductName == vm.ProductName);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var data = new Obat
                    {
                        ProductId = Guid.NewGuid(),
                        ProductCode = kode,
                        ProductName = vm.ProductName,
                        ProductExtCode = vm.ProductExtCode,
                        Barcode = vm.Barcode,
                        SupplierId = vm.SupplierId,
                        SupplierName = vm.SupplierName,
                        KategoryObatId = vm.KategoryObatId,
                        NamaKategoriObat = vm.NamaKategoriObat,
                        MeasurementId = vm.MeasurementId,
                        MeasurementName = vm.MeasurementName,
                        WarehouseLocationId = vm.WarehouseLocationId,
                        WarehouseLocationName = vm.WarehouseLocationName,
                        DiscountId = vm.DiscountId,
                        DiscountValue = vm.DiscountValue,
                        ExpiredDate = vm.ExpiredDate,
                        DosageStrength = vm.DosageStrength,
                        DosageVolume = vm.DosageVolume,
                        DosageForm = vm.DosageForm,
                        Stock = vm.Stock,
                        Cogs = vm.Cogs,
                        BuyPrice = vm.BuyPrice,
                        RetailPrice = vm.RetailPrice,
                        StorageLocation = vm.StorageLocation,
                        RackNumber = vm.RackNumber,
                        IsSupplierUtama = vm.IsSupplierUtama,
                        IsActive = true,
                        Note = vm.Note,
                    };

                    _applicationDbContext.Obats.Add(data);
                    _applicationDbContext.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateObat(Guid id, [FromBody] ObatViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }
            var data = _applicationDbContext.Obats.Find(id);
            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;
                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }
                // Validate ModelState
                if (ModelState.IsValid)
                {
                    // Update Data
                    data.ProductExtCode = vm.ProductExtCode;
                    data.ProductName = vm.ProductName;
                    data.Barcode = vm.Barcode;
                    data.SupplierId = vm.SupplierId;
                    data.SupplierName = vm.SupplierName;
                    data.KategoryObatId = vm.KategoryObatId;
                    data.NamaKategoriObat = vm.NamaKategoriObat;
                    data.MeasurementId = vm.MeasurementId;
                    data.MeasurementName = vm.MeasurementName;
                    data.WarehouseLocationId = vm.WarehouseLocationId;
                    data.WarehouseLocationName = vm.WarehouseLocationName;
                    data.DiscountId = vm.DiscountId;
                    data.DiscountValue = vm.DiscountValue;
                    data.ExpiredDate = vm.ExpiredDate;
                    data.DosageStrength = vm.DosageStrength;
                    data.DosageVolume = vm.DosageVolume;
                    data.DosageForm = vm.DosageForm;
                    data.Stock = vm.Stock;
                    data.Cogs = vm.Cogs;
                    data.BuyPrice = vm.BuyPrice;
                    data.RetailPrice = vm.RetailPrice;
                    data.StorageLocation = vm.StorageLocation;
                    data.RackNumber = vm.RackNumber;
                    data.IsSupplierUtama = vm.IsSupplierUtama;

                    data.UpdateDateTime = DateTimeOffset.UtcNow;
                    data.UpdateBy = UserActiveId;

                    _applicationDbContext.Obats.Update(data);
                    _applicationDbContext.SaveChanges();
                    return Ok(new
                    {
                        message = "Data berhasil diubah. || 200 OK",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
            catch
            (Exception ex)
            {
                return BadRequest(new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObat(Guid id)
        {
            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Pasien**
                var data = _applicationDbContext.Obats.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsActive = false;
                data.IsDelete = true;

                _applicationDbContext.Obats.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedObat(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            // Query data
            var query = from a in _applicationDbContext.Obats
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            ProductId = a.ProductId,
                            ProductCode = a.ProductCode,
                            ProductName = a.ProductName,
                            Barcode = a.Barcode,
                            SupplierId = a.SupplierId,
                            SupplierName = a.SupplierName,
                            KategoryObatId = a.KategoryObatId,
                            NamaKategoriObat = a.NamaKategoriObat,
                            MeasurementId = a.MeasurementId,
                            MeasurementName = a.MeasurementName,
                            WarehouseLocationId = a.WarehouseLocationId,
                            WarehouseLocationName = a.WarehouseLocationName,
                            DiscountId = a.DiscountId,
                            DiscountValue = a.DiscountValue,
                            ExpiredDate = a.ExpiredDate,
                            DosageStrength = a.DosageStrength,
                            DosageVolume = a.DosageVolume,
                            DosageForm = a.DosageForm,
                            Stock = a.Stock,
                            Cogs = a.Cogs,
                            BuyPrice = a.BuyPrice,
                            RetailPrice = a.RetailPrice,
                            StorageLocation = a.StorageLocation,
                            RackNumber = a.RackNumber,
                            IsSupplierUtama = a.IsSupplierUtama,
                            IsActive = a.IsActive,
                            Note = a.Note,
                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.ProductName.Contains(search) || u.SupplierName.Contains(search)
                );
            }

            // Filter berdasarkan daterange jika keduanya memiliki nilai
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }


            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll)
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "ProductName" => query.OrderByDescending(u => u.ProductName),
                    "SupplierName" => query.OrderByDescending(u => u.SupplierName),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "ProductName" => query.OrderBy(u => u.ProductName),
                    "SupplierName" => query.OrderBy(u => u.SupplierName),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = rows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }



    }
}
