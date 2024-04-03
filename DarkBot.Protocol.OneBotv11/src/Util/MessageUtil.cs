using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using DarkBot.Core.Message;
using DarkBot.Core.Message.Impl;

namespace DarkBot.Protocol.OneBotv11.Util;

public static class MessageUtil {
    public static IEnumerable<IMessage> ToMessages(this JsonElement json) {
        return json.EnumerateArray().Select(j => j.ToMessage());
    }

    public static IMessage ToMessage(this JsonElement json) {
        JsonElement dataJson = json.GetProperty("data");

        return json.GetProperty("type").GetString() switch {
            "text" => new TextMessage(dataJson.GetProperty("text").GetRequiredString()),
            "face" => new FaceMessage(int.Parse(dataJson.GetProperty("id").GetRequiredString())),
            "image" => new ImageMessage(dataJson.GetProperty("file").GetRequiredString()),
            "record" => new RecordMessage(dataJson.GetProperty("file").GetRequiredString()),
            "at" => new AtMessage(uint.Parse(dataJson.GetProperty("qq").GetRequiredString())),
            "reply" => new ReplyMessage(int.Parse(dataJson.GetProperty("id").GetRequiredString())),
            "json" => new JsonMessage(JsonDocument.Parse(dataJson.GetProperty("data").GetRequiredString()).RootElement),
            _ => throw new NotSupportedException()
        };
    }

    public static JsonArray ToJsonArray(this IEnumerable<IMessage> messages) {
        return new JsonArray(messages.Select(m => m.ToJsonObject()).ToArray());
    }

    public static JsonObject ToJsonObject(this IMessage message) {
        return message switch {
            TextMessage text => new JsonObject([
                new("type", "text"),
                new("data", new JsonObject([
                    new("text", text.Text)
                ]))
            ]),
            FaceMessage face => new JsonObject([
                new("type", "face"),
                new("data", new JsonObject([
                    new("id", $"{face.FaceId}")
                ]))
            ]),
            ImageMessage image => new JsonObject([
                new("type", "image"),
                new("data", new JsonObject([
                    new("file", image.Url)
                ]))
            ]),
            RecordMessage record => new JsonObject([
                new("type", "record"),
                new("data", new JsonObject([
                    new("file", record.Url)
                ]))
            ]),
            AtMessage at => new JsonObject([
                new("type", "at"),
                new("data", new JsonObject([
                    new("qq", $"{at.UserId}")
                ]))
            ]),
            ReplyMessage reply => new JsonObject([
                new("type", "reply"),
                new("data", new JsonObject([
                    new("id", $"{reply.MessageId}")
                ]))
            ]),
            JsonMessage json => new JsonObject([
                new("type", "json"),
                new("data", new JsonObject([
                    new("data", json.Json.GetRawText())
                ]))
            ]),
            _ => throw new NotSupportedException()
        };
    }
}