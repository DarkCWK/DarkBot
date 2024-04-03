namespace DarkBot.Core.Message.Impl;

public class ImageMessage(string url) : IMessage {
    public string Url { get; } = url;
}