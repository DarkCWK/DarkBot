namespace DarkBot.Core.Message.Impl;

public class RecordMessage(string url) : IMessage {
    public string Url { get; } = url;
}