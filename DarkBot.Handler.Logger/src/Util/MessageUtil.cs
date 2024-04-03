using System.Collections.Generic;
using System.Linq;
using DarkBot.Core.Message;
using DarkBot.Core.Message.Impl;

namespace DarkBot.Handler.Logger.Util;

public static class MessageUtil {
    public static string ToLoggerString(this IEnumerable<IMessage> messages) {
        return string.Join("", messages.Select(m => m.ToLoggerString()));
    }

    public static string ToLoggerString(this IMessage message) {
        return message switch {
            TextMessage text => $"[Text:{text.Text.Replace("\n", "\\n")}]",
            FaceMessage face => $"[Face:{face.FaceId}]",
            ImageMessage image => $"[Image:{image.Url}]",
            RecordMessage record => $"[Record:{record.Url}]",
            AtMessage at => $"[At:{at.UserId}]",
            ReplyMessage reply => $"[Reply:{reply.MessageId}]",
            JsonMessage json => $"[Json:{json.Json.GetRawText()}]",
            _ => "[Unknown]"
        };
    }
}