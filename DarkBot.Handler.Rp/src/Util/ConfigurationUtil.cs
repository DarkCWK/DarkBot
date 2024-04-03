using Microsoft.Extensions.Configuration;

namespace DarkBot.Handler.Rp.Util;

public static class ConfigurationUtil {
    public static T GetRequired<T>(this IConfiguration configuration) {
        return configuration.Get<T>() ?? throw new KeyNotFoundException();
    }
}