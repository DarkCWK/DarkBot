using System;
using System.Runtime.CompilerServices;

namespace DarkBot.Protocol.OneBotv11.Util;

public static class DateTimeUtil {
    public static DateTime UnixTimestampToDateTime(ulong unixTimestamp) {
        return UnsafeCreateDateTime(unixTimestamp * TimeSpan.TicksPerSecond + /* DateTime.UnixEpoch.Ticks */ 621355968000000000);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern DateTime UnsafeCreateDateTime(ulong ticks);
}