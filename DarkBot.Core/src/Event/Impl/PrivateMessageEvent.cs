using System;
using System.Collections.Generic;
using DarkBot.Core.Message;

namespace DarkBot.Core.Event.Impl;

public class PrivateMessageEvent(DateTime time, uint selfId, int messageId, uint senderId, IEnumerable<IMessage> messages) : IEvent {
    public DateTime Time { get; } = time;
    public uint SelfId { get; } = selfId;
    public int MessageId { get; } = messageId;
    public uint SenderId { get; } = senderId;
    public IEnumerable<IMessage> Messages { get; } = messages;
}