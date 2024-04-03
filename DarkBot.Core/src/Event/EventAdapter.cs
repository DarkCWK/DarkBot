using DarkBot.Core.Event.Impl;
using DarkBot.Core.Operation;

namespace DarkBot.Core.Event;

public class EventAdapter : IEventEmiter, IEventRegister {
    public event DarkBotEventHandler<OnlineEvent>? OnOnline;
    public event DarkBotEventHandler<PrivateMessageEvent>? OnPrivateMessage;
    public event DarkBotEventHandler<GroupMessageEvent>? OnGroupMessage;

    public OnlineEvent EmitOnline(IOperation operation, OnlineEvent @event) {
        OnOnline?.Invoke(operation, @event); return @event;
    }
    public PrivateMessageEvent EmitPrivateMessage(IOperation operation, PrivateMessageEvent @event) {
        OnPrivateMessage?.Invoke(operation, @event); return @event;
    }
    public GroupMessageEvent EmitGroupMessage(IOperation operation, GroupMessageEvent @event) {
        OnGroupMessage?.Invoke(operation, @event); return @event;
    }
}
