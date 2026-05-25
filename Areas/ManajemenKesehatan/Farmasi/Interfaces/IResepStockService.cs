namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Interfaces
{
    public interface IResepStockService
    {
        // RAJAL:
        // Dipanggil saat pembayaran kasir kunjungan OP lunas.
        Task FinalizeRajalByKunjunganAsync(
            Guid kunjunganId,
            Guid userActiveId,
            CancellationToken ct);

        // RAJAL:
        // Opsional, kalau ingin finalize satu resep tertentu saja.
        Task FinalizeRajalByResepAsync(
            Guid resepId,
            Guid userActiveId,
            CancellationToken ct);

        // RANAP:
        // Dipanggil saat obat diberikan ke pasien.
        // Bisa pilih beberapa waktu sekaligus: PAGI, SIANG, MALAM.
        // Qty tidak dikirim, dihitung otomatis dari Signa.
        Task FinalizeRanapPemberianAsync(
            Guid detailResepId,
            List<string> waktuPengambilan,
            Guid userActiveId,
            CancellationToken ct);

        // RESEP TEBUS:
        // Dipanggil saat pembayaran resep tebus lunas.
        Task FinalizeResepTebusAsync(
            Guid resepTebusId,
            Guid userActiveId,
            CancellationToken ct);
    }
}
