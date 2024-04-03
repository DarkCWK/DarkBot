using System;
using System.Text.Json;
using DarkBot.Core.Event;
using DarkBot.Core.Operation;
using DarkBot.Protocol.OneBotv11.Event.Impl;

namespace DarkBot.Protocol.OneBotv11.Util;

public static class EventUtil {
    public static IEvent EmiteEventFromJson(this IEventEmiter emiter, IOperation operation, JsonElement json) {
        return json.GetProperty("post_type").GetString() switch {
            "message" => emiter.EmiteEventFromMessageJson(operation, json),
            // "notice" => // NOTNOW
            // "request" => // NOTNOW
            "meta_event" => emiter.EmiteEventFromMetaJson(operation, json),
            _ => throw new NotSupportedException()
        };
    }

    private static IEvent EmiteEventFromMessageJson(this IEventEmiter emiter, IOperation operation, JsonElement json) {
        return json.GetProperty("message_type").GetString() switch {
            "private" => emiter.EmitPrivateMessage(operation, new(
                DateTimeUtil.UnixTimestampToDateTime(json.GetProperty("time").GetUInt64()),
                json.GetProperty("self_id").GetUInt32(),
                json.GetProperty("message_id").GetInt32(),
                json.GetProperty("user_id").GetUInt32(),
                json.GetProperty("message").ToMessages()
            )),
            "group" => emiter.EmitGroupMessage(operation, new(
                DateTimeUtil.UnixTimestampToDateTime(json.GetProperty("time").GetUInt64()),
                json.GetProperty("self_id").GetUInt32(),
                json.GetProperty("message_id").GetInt32(),
                json.GetProperty("group_id").GetUInt32(),
                json.GetProperty("user_id").GetUInt32(),
                json.GetProperty("message").ToMessages()
            )),
            _ => throw new NotSupportedException()
        };
    }

    private static IEvent EmiteEventFromMetaJson(this IEventEmiter emiter, IOperation operation, JsonElement json) {
        return json.GetProperty("meta_event_type").GetString() switch {
            "lifecycle" => emiter.EmiteEventFromLifecycleJson(operation, json),
            "heartbeat" => new HeartbeatEvent(
                DateTimeUtil.UnixTimestampToDateTime(json.GetProperty("time").GetUInt64()),
                json.GetProperty("self_id").GetUInt32()
            ),
            _ => throw new NotSupportedException()
        };
    }

    private static IEvent EmiteEventFromLifecycleJson(this IEventEmiter emiter, IOperation operation, JsonElement json) {
        return json.GetProperty("sub_type").GetString() switch {
            "connect" => emiter.EmitOnline(operation, new(
                DateTimeUtil.UnixTimestampToDateTime(json.GetProperty("time").GetUInt64()),
                json.GetProperty("self_id").GetUInt32()
            )),
            // "disable" => // NOTNOW
            // "connect" => // NOTNOW
            _ => throw new NotSupportedException()
        };
    }
}