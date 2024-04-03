using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DarkBot.Protocol.OneBotv11.Client;

public class WebSocketClient : IDisposable {
    private ClientWebSocket InnerClient { get; set; } = new();

    public async Task ConnectAsync(string url, CancellationToken token) {
        while (!token.IsCancellationRequested) {
            try {
                await InnerClient.ConnectAsync(new(url), token); return;
            } catch (Exception e) when (e is ObjectDisposedException || e is InvalidOperationException) {
                InnerClient = new();
            }
        }
        token.ThrowIfCancellationRequested();
    }

    public ValueTask SendAsync(byte[] payload, WebSocketMessageType type, CancellationToken token) {
        lock (this) {
            return InnerClient.SendAsync(payload.AsMemory(), type, true, token);
        }
    }

    public async Task<(WebSocketMessageType, byte[])> ReceiveAsync(CancellationToken token) {
        byte[] buffer = new byte[1024];
        int byteCount = 0;
        while (!token.IsCancellationRequested) {
            WebSocketReceiveResult result = await InnerClient.ReceiveAsync(
                new ArraySegment<byte>(buffer, byteCount, buffer.Length - byteCount),
                default
            ).WaitAsync(token);
            if (result.EndOfMessage) return (result.MessageType, buffer[..(byteCount + result.Count)]);
            if ((byteCount += result.Count) == buffer.Length) Array.Resize(ref buffer, buffer.Length << 1);
        }
        throw new OperationCanceledException();
    }

    public Task CloseAsync(ulong timeout, CancellationToken token) {
        return InnerClient.CloseAsync(WebSocketCloseStatus.NormalClosure, null, token)
            .WaitAsync(TimeSpan.FromMilliseconds(timeout), token);
    }

    public void Dispose() {
        InnerClient.Dispose();

        GC.SuppressFinalize(this);
    }
}