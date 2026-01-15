namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Helpers
{
    public enum ShiftPengambilan
    {
        Pagi,
        Siang,
        Malam
    }

    public static class ShiftHelper
    {
        private static TimeZoneInfo GetJakartaTimeZone()
        {
            // Linux/Mac biasanya "Asia/Jakarta", Windows biasanya "SE Asia Standard Time"
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        }

        public static string? GetShift(DateTimeOffset? tglPengambilanObat)
        {
            if (!tglPengambilanObat.HasValue) return null;

            var tz = GetJakartaTimeZone();
            var wib = TimeZoneInfo.ConvertTime(tglPengambilanObat.Value, tz);

            var t = wib.TimeOfDay;

            var pagi = new TimeSpan(8, 0, 0);
            var siang = new TimeSpan(14, 0, 0);
            var malam = new TimeSpan(20, 0, 0);

            if (t >= pagi && t < siang) return "Pagi";
            if (t >= siang && t < malam) return "Siang";
            return "Malam";
        }

        public static string GetShiftName(DateTime tglPengambilanObat)
            => GetShift(tglPengambilanObat).ToString();
    }
}
