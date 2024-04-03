using Microsoft.Extensions.Logging;

namespace DarkBot.Protocol.OneBotv11.Util;

public static partial class LoggerUtil {
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "{ServiceName} starting")]
    public static partial void DebugStarting(this ILogger logger, string serviceName);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "{ServiceName} started")]
    public static partial void DebugStarted(this ILogger logger, string serviceName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "{ServiceName} stopping")]
    public static partial void DebugStoping(this ILogger logger, string serviceName);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "{ServiceName} stopped")]
    public static partial void DebugStopped(this ILogger logger, string serviceName);
}