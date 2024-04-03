using DarkBot.Core.Event.Impl;
using DarkBot.Core.Operation;

namespace DarkBot.Core.Event;

public interface IEventEmiter {
    public OnlineEvent EmitOnline(IOperation operation, OnlineEvent @event);
    public PrivateMessageEvent EmitPrivateMessage(IOperation operation, PrivateMessageEvent @event);
    public GroupMessageEvent EmitGroupMessage(IOperation operation, GroupMessageEvent @event);
}