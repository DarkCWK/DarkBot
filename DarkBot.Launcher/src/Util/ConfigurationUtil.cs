using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace DarkBot.Launcher.Util;

public static class ConfigurationUtil {
    public static string GetRequiredString(this IConfiguration configuration, string key) {
        return configuration[key] ?? throw new KeyNotFoundException();
    }
}