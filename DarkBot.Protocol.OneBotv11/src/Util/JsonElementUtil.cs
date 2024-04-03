using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace DarkBot.Protocol.OneBotv11.Util;

public static class JsonElementUtil {
    public static string GetRequiredString(this JsonElement json) {
        return json.GetString() ?? throw new JsonException();
    }
}