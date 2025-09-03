using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Helper
{
    public interface ICreateByResolver
    {
        /// <summary>
        /// Tentukan siapa yang dicatat sebagai CreateBy.
        /// Jika ada delegasi aktif → pakai UserDelegasiId,
        /// jika tidak → pakai UserActiveId dari user login.
        /// </summary>
        Task<Guid> ResolveAsync(ClaimsPrincipal user, Guid? delegasiId = null, bool markAsConsumed = false);
    }

    public class CreateByResolver : ICreateByResolver
    {
        private readonly ApplicationDbContext _db;

        public CreateByResolver(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> ResolveAsync(ClaimsPrincipal user, Guid? delegasiId = null, bool markAsConsumed = false)
        {
            // 🔹 Ambil user login
            var email = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("User tidak terautentikasi!");

            var me = await _db.UserActives
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (me == null)
                throw new UnauthorizedAccessException("User aktif tidak ditemukan!");

            // 🔹 Kalau tidak ada delegasi → pakai user login
            if (delegasiId == null)
                return me.UserActiveId;

            // 🔹 Cari delegasi berdasarkan id
            var delegasi = await _db.Delegasis
                .FirstOrDefaultAsync(d =>
                    d.DelegasiId == delegasiId.Value &&
                    d.IsDelegated == true &&
                    (d.IsDelete == false || d.IsDelete == null));

            // Jika delegasi tidak ditemukan / tidak aktif → fallback ke user login
            if (delegasi == null)
                return me.UserActiveId;

            // 🔹 Kalau ingin sekali pakai → tandai delegasi sudah selesai
            if (markAsConsumed)
            {
                delegasi.IsDelegated = false;
                delegasi.UpdateDateTime = DateTimeOffset.UtcNow;
                delegasi.UpdateBy = me.UserActiveId;
                await _db.SaveChangesAsync();
            }

            // 🔹 Return user delegasi
            return delegasi.UserDelegasiId ?? me.UserActiveId;
        }
    }
}
