namespace DarkBot.Protocol.OneBotv11.Configuration;

public class ForwardWebSocketConfiguration(string url, int heartbeatInterval, int reconnectInterval) {
    public string Url { get; } = url;
    public int HeartbeatInterval { get; } = heartbeatInterval;
    public int ReconnectInterval { get; } = reconnectInterval;
}