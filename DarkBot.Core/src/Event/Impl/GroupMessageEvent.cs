using System;
using System.Collections.Generic;
using DarkBot.Core.Message;

namespace DarkBot.Core.Event.Impl;

public class GroupMessageEvent(DateTime time, uint selfId, int messageId, uint groupId, uint senderId, IEnumerable<IMessage> messages) : IEvent {
    public DateTime Time { get; } = time;
    public uint SelfId { get; } = selfId;
    public int MessageId { get; } = messageId;
    public uint GroupId { get; } = groupId;
    public uint SenderId { get; } = senderId;
    public IEnumerable<IMessage> Messages { get; } = messages;
}