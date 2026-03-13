using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services
{
    public class DepositRanapNumberService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public DepositRanapNumberService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<string> GenerateNoKwitansiAsync(CancellationToken cancellationToken = default)
        {
            var yearNow = DateTime.Now.Year.ToString();
            var prefix = $"DP{yearNow}";

            var lastNoKwitansi = await _applicationDbContext.DepositRanaps
                .AsNoTracking()
                .Where(x => x.NoKwitansi != null && x.NoKwitansi.StartsWith(prefix))
                .OrderByDescending(x => x.NoKwitansi)
                .Select(x => x.NoKwitansi)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(lastNoKwitansi))
            {
                return $"{prefix}0001";
            }

            var numberPart = lastNoKwitansi.Substring(prefix.Length);

            if (!int.TryParse(numberPart, out int lastNumber))
            {
                return $"{prefix}0001";
            }

            return $"{prefix}{(lastNumber + 1):D4}";
        }
    }
}
