using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using DarkBot.Core.Event;
using DarkBot.Core.Message;
using DarkBot.Core.Misc;
using DarkBot.Core.Operation;
using DarkBot.Core.Protocol;
using DarkBot.Protocol.OneBotv11.Client;
using DarkBot.Protocol.OneBotv11.Configuration;
using DarkBot.Protocol.OneBotv11.Event.Impl;
using DarkBot.Protocol.OneBotv11.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DarkBot.Protocol.OneBotv11;

public partial class ForwardWebSocket(ILogger<ForwardWebSocket> logger, IConfiguration configuration, IEventEmiter emiter) {
    private ILogger<ForwardWebSocket> Logger { get; } = logger;
    private ForwardWebSocketConfiguration Configuration { get; } = configuration.GetRequired<ForwardWebSocketConfiguration>();
    private IEventEmiter Emiter { get; } = emiter;
}

public partial class ForwardWebSocket : IProtocol {
    private CancellationTokenSource? MainCts { get; set; }
    private Task? MainTask { get; set; }

    public Task StartAsync(CancellationToken token) {
        Logger.DebugStarting(Configuration.Url);

        MainCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        MainTask = MainLoop(MainCts.Token)
            .ContinueWith(t => { if (t.IsFaulted) Logger.ErrorUnprocessedException(Configuration.Url, t.Exception); }, MainCts.Token);

        return (MainTask.IsCompleted ? MainTask : Task.CompletedTask)
            .ContinueWith(t => { if (t.IsCompletedSuccessfully) Logger.DebugStarted(Configuration.Url); }, token);
    }

    public async Task StopAsync(CancellationToken token) {
        Logger.DebugStoping(Configuration.Url);

        if (MainTask == null) return;

        MainCts?.Cancel();
        await MainTask.WaitAsync(token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        Logger.DebugStopped(Configuration.Url);
    }
}

public partial class ForwardWebSocket {
    private WebSocketClient Client { get; set; } = new();

    private TaskCompletionSource<HeartbeatEvent> HeartbeatTcs { get; set; } = new();

    public async Task MainLoop(CancellationToken token) {
        while (true) {
            try {
                await Client.ConnectAsync(Configuration.Url, token);
            } catch (Exception e) when (e is not OperationCanceledException) {
                Logger.ErrorConnectionException(Configuration.Url, e);

                await Task.Delay(Configuration.ReconnectInterval, token); continue;
            }

            try {
                CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                Task[] tasks = [MessageLoop(cts.Token), HeartbeatLoop(cts.Token)];

                await Task.WhenAny(tasks)
                    .ContinueWith(_ => cts.Cancel(), token);
                await Task.WhenAll(tasks);
            } catch (OperationCanceledException) {
                if (token.IsCancellationRequested) throw;
            } catch (Exception e) {
                Logger.ErrorWebSocketException(Configuration.Url, e);
            } finally {
                try {
                    await Client.CloseAsync(5000, default);
                } catch (Exception e) { Logger.ErrorCloseException(Configuration.Url, e); }
            }

            token.ThrowIfCancellationRequested();
        }
    }

    public async Task MessageLoop(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            (WebSocketMessageType type, byte[] bytes) = await Client.ReceiveAsync(token);
            if (type == WebSocketMessageType.Close) return;

            if (Logger.IsEnabled(LogLevel.Trace)) Logger.TraceReceived(Configuration.Url, bytes);

            JsonElement json = JsonDocument.Parse(bytes).RootElement;
            try {
                if (
                    json.TryGetProperty("echo", out JsonElement echoJson) &&
                    EchoTcss.TryGetValue(echoJson.GetUInt64(), out TaskCompletionSource<JsonElement>? tcs)
                ) {
                    tcs.SetResult(json);
                } else if (Emiter.EmiteEventFromJson(this, json) is HeartbeatEvent @event) {
                    HeartbeatTcs.SetResult(@event);
                }
            } catch (NotSupportedException) { }
        }

        token.ThrowIfCancellationRequested();
    }

    public async Task HeartbeatLoop(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            try {
                await HeartbeatTcs.Task.WaitAsync(TimeSpan.FromMilliseconds(Configuration.HeartbeatInterval), token);
                HeartbeatTcs = new();
            } catch (TimeoutException) {
                break;
            }
        }

        token.ThrowIfCancellationRequested();
    }
}

public partial class ForwardWebSocket : IOperation {
    private ConcurrentDictionary<ulong, TaskCompletionSource<JsonElement>> EchoTcss { get; } = [];

    private ulong EchoSerialNumber = 0;

    public async Task<int> SendPrivateMessage(uint userId, IEnumerable<IMessage> messages, CancellationToken toke) {
        JsonElement result = await CallApiAsync("send_private_msg", new JsonObject([
            new("user_id", userId),
            new("message", messages.ToJsonArray())
        ]), toke);

        return result.GetProperty("message_id").GetInt32();
    }

    public async Task<int> SendGroupMessage(uint groupId, IEnumerable<IMessage> messages, CancellationToken toke) {
        JsonElement result = await CallApiAsync("send_group_msg", new JsonObject([
            new("group_id", groupId),
            new("message", messages.ToJsonArray())
        ]), toke);

        return result.GetProperty("message_id").GetInt32();
    }

    public async Task<UserInfo> GetSelfInfo(CancellationToken token) {
        JsonElement result = await CallApiAsync("get_login_info", new JsonObject([]), token);
        return new(result.GetProperty("user_id").GetUInt32(), result.GetProperty("nickname").GetRequiredString(), null);
    }

    public async Task<UserInfo> GetFriendInfo(uint userId, CancellationToken token) {
        return (await GetFriendInfos(token)).First(u => u.UserId == userId);
    }

    public async Task<IEnumerable<UserInfo>> GetFriendInfos(CancellationToken token) {
        return (await CallApiAsync("get_friend_list", new JsonObject([]), token)).EnumerateArray()
            .Select(j => new UserInfo(
                j.GetProperty("user_id").GetUInt32(),
                j.GetProperty("nickname").GetRequiredString(),
                j.GetProperty("remark").GetString() switch {
                    null or "" => null,
                    string remark => remark,
                }
            ));
    }

    public async Task<GroupMemberInfo> GetGroupMemberInfo(uint groupId, uint userId, CancellationToken token) {
        JsonElement result = await CallApiAsync("get_group_member_info", new JsonObject([
            new("group_id", groupId),
            new("user_id", userId)
        ]), token);

        return new(
            result.GetProperty("group_id").GetUInt32(),
            result.GetProperty("user_id").GetUInt32(),
            result.GetProperty("nickname").GetRequiredString(),
            result.GetProperty("card").GetString() switch {
                null or "" => null,
                string card => card,
            }
        );
    }

    public async Task<GroupInfo> GetGroupInfo(uint groupId, CancellationToken token) {
        JsonElement result = await CallApiAsync("get_group_info", new JsonObject([
            new("group_id", groupId),
        ]), token);

        return new(
            result.GetProperty("group_id").GetUInt32(),
            result.GetProperty("group_name").GetRequiredString()
        );
    }

    public async Task<JsonElement> CallApiAsync(string action, JsonObject @params, CancellationToken token) {
        ulong echo = Interlocked.Increment(ref EchoSerialNumber);
        TaskCompletionSource<JsonElement> tsc = new();
        EchoTcss.TryAdd(echo, tsc);

        try {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new JsonObject([
                new("action", action),
                new("params", @params),
                new("echo", echo)
            ]));

            Logger.TraceSend(Configuration.Url, bytes);

            await Client.SendAsync(bytes, WebSocketMessageType.Text, token);

            JsonElement resultJson = await tsc.Task;

            int retcode = resultJson.GetProperty("retcode").GetInt32();
            if (retcode != 0) throw new ResultException(retcode);

            return resultJson.GetProperty("data");
        } finally {
            EchoTcss.TryRemove(echo, out _);
        }
    }
}

public partial class ForwardWebSocket : IDisposable {
    public void Dispose() {
        Client.Dispose();

        GC.SuppressFinalize(this);
    }
}

public static partial class ForwardWebSocketLogger {
    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "{tag} Connected")]
    public static partial void InformationConnected(this ILogger<ForwardWebSocket> logger, string tag);

    public static void TraceSend(this ILogger<ForwardWebSocket> logger, string tag, byte[] payload) {
        if (logger.IsEnabled(LogLevel.Trace)) {
            string text = Encoding.UTF8.GetString(payload);

            if (text.Length > 1024) {
                text = string.Concat(text.AsSpan(0, 1024), "...", (text.Length - 1024).ToString(), "bytes");
            }
            InternalTraceSend(logger, tag, text);
        }
    }

    [LoggerMessage(EventId = 11, Level = LogLevel.Trace, Message = "{tag} Send: {payload}", SkipEnabledCheck = true)]
    private static partial void InternalTraceSend(this ILogger<ForwardWebSocket> logger, string tag, string payload);

    public static void TraceReceived(this ILogger<ForwardWebSocket> logger, string tag, byte[] payload) {
        if (logger.IsEnabled(LogLevel.Trace)) {
            string text = Encoding.UTF8.GetString(payload);

            if (text.Length > 1024) {
                text = string.Concat(text.AsSpan(0, 1024), "...", (text.Length - 1024).ToString(), "bytes");
            }
            InternalTraceReceived(logger, tag, text);
        }
    }

    [LoggerMessage(EventId = 12, Level = LogLevel.Trace, Message = "{tag} Receive: {payload}", SkipEnabledCheck = true)]
    private static partial void InternalTraceReceived(this ILogger<ForwardWebSocket> logger, string tag, string payload);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "{tag} Disconnected")]
    public static partial void InformationDisconnected(this ILogger<ForwardWebSocket> logger, string tag);


    [LoggerMessage(EventId = 997, Level = LogLevel.Error, Message = "{Tag} CloseException")]
    public static partial void ErrorCloseException(this ILogger<ForwardWebSocket> logger, string tag, Exception e);

    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{Tag} WebSocketException")]
    public static partial void ErrorWebSocketException(this ILogger<ForwardWebSocket> logger, string tag, Exception e);

    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{Tag} ConnectionException")]
    public static partial void ErrorConnectionException(this ILogger<ForwardWebSocket> logger, string tag, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "{Tag} UnprocessedException")]
    public static partial void ErrorUnprocessedException(this ILogger<ForwardWebSocket> logger, string tag, Exception e);
}