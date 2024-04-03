using System;
using DarkBot.Core.Event;

namespace DarkBot.Protocol.OneBotv11.Event.Impl;

public class HeartbeatEvent(DateTime time, uint selfId) : IEvent {
    public DateTime Time { get; } = time;
    public uint SelfId { get; } = selfId;
}