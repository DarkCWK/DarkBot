using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace DarkBot.Protocol.OneBotv11.Util;

public static class ConfigurationUtil {
    public static T GetRequired<T>(this IConfiguration configuration) {
        return configuration.Get<T>() ?? throw new KeyNotFoundException();
    }
}