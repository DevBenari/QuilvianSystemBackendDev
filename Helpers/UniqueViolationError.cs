using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace QuilvianSystemBackendDev.Helpers
{
    public static class UniqueViolationError
    {
        public static bool IsUniqueViolation(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg &&
                pg.SqlState == PostgresErrorCodes.UniqueViolation)
                return true;

            if (ex.InnerException?.InnerException is PostgresException pg2 &&
                pg2.SqlState == PostgresErrorCodes.UniqueViolation)
                return true;

            return false;
        }
    }
}
