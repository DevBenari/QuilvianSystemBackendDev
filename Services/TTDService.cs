using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Services
{
    public class TTDService : ITTDService
    {
        private readonly ApplicationDbContext _db;

        public TTDService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<TTDResult> CheckTTDAsync(Guid userActiveId)
        {
            var ttd = await _db.MasterTTDs
                .Where(x => x.UserActiveId == userActiveId)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync();

            if (ttd == null)
            {
                return new TTDResult
                {
                    HasTTD = false,
                    Message = "Anda belum memiliki tanda tangan. Harap upload tanda tangan terlebih dahulu."
                };
            }

            return new TTDResult
            {
                HasTTD = true,
                Path = ttd.TTDPath,
                TTDId = ttd.TTDId,
                Message = "Tanda tangan ditemukan."
            };
        }
    }
}
