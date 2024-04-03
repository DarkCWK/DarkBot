
using System;

namespace DarkBot.Core.Event.Impl;

public class OnlineEvent(DateTime time, uint selfId) : IEvent {
    public DateTime Time { get; } = time;
    public uint SelfId { get; } = selfId;
}