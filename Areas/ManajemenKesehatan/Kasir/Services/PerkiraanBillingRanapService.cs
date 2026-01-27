using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;

public sealed class PerkiraanBillingRanapService : IPerkiraanBillingRanapService
{
    private readonly IBillingKunjunganReadService _billingRead;

    public PerkiraanBillingRanapService(IBillingKunjunganReadService billingRead)
    {
        _billingRead = billingRead;
    }

    public async Task<BillingKunjunganDto?> GetPerkiraanBillingIpAsync(Guid kunjunganId, CancellationToken ct = default)
    {
        var snap = DateTime.Now;

        // reuse: billing keseluruhan sudah include kamar sampai snap
        var dto = await _billingRead.GetBillingKeseluruhanAsync(kunjunganId, snap, ct);
        if (dto == null) return null;

        if (!IsRawatInapIP(dto.JenisKunjungan))
            throw new InvalidOperationException(BuildJenisKunjunganMessage(dto.JenisKunjungan));

        // dto.AsOf sudah ter-set snap
        return dto;
    }

    private static bool IsRawatInapIP(string? jenisKunjungan)
    {
        var j = (jenisKunjungan ?? "").Trim().ToUpperInvariant();
        return j == "IP" || j == "RAWAT INAP" || j == "INAP";
    }

    private static string BuildJenisKunjunganMessage(string? jenisKunjungan)
    {
        var j = (jenisKunjungan ?? "").Trim().ToUpperInvariant();
        var readable = j switch
        {
            "OP" => "Rawat Jalan (OP)",
            "RAWAT JALAN" => "Rawat Jalan",
            "" => "bukan Rawat Inap (IP)",
            _ => jenisKunjungan ?? "bukan Rawat Inap (IP)"
        };

        return $"Maaf, prakiraan billing ini hanya untuk Rawat Inap (IP). Kunjungan yang dipilih adalah {readable}.";
    }
}
