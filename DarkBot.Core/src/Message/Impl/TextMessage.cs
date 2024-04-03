namespace DarkBot.Core.Message.Impl;

public class TextMessage(string text) : IMessage {
    public string Text { get; } = text;
}