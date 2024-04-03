using System.Threading.Tasks;
using DarkBot.Core.Event.Impl;
using DarkBot.Core.Operation;

namespace DarkBot.Core.Event;

public delegate Task DarkBotEventHandler<TEvent>(IOperation operation, TEvent @event);

public interface IEventRegister {
    public event DarkBotEventHandler<OnlineEvent>? OnOnline;
    public event DarkBotEventHandler<PrivateMessageEvent>? OnPrivateMessage;
    public event DarkBotEventHandler<GroupMessageEvent>? OnGroupMessage;
}