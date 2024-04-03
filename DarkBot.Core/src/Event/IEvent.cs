using System;

namespace DarkBot.Core.Event;

public interface IEvent {
    public DateTime Time { get; }

    public uint SelfId { get; }
}