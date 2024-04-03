namespace DarkBot.Handler.Rp.Util;

public static class DateTimeUtil {
    public static ulong ToUnixTimestamp(this DateTime time) {
        return (ulong)((time.Ticks - /* DateTime.UnixEpoch.Ticks */ 621355968000000000) / TimeSpan.TicksPerMillisecond);
    }
}
