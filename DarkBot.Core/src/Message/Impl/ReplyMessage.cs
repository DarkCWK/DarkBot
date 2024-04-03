namespace DarkBot.Core.Message.Impl;

public class ReplyMessage(int messageId) : IMessage {
    public int MessageId { get; } = messageId;
}