namespace DarkBot.Core.Message.Impl;

public class AtMessage(uint userId) : IMessage {
    public uint UserId { get; } = userId;
}