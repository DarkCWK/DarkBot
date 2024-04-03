using System;
using System.Threading;
using System.Threading.Tasks;
using DarkBot.Core.Event;
using DarkBot.Core.Event.Impl;
using DarkBot.Core.Handler;
using DarkBot.Core.Misc;
using DarkBot.Core.Operation;
using DarkBot.Handler.Logger.Configuration;
using DarkBot.Handler.Logger.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DarkBot.Handler.Logger;

public partial class LoggerHandler(ILogger<LoggerHandler> logger, IConfiguration configuration, IEventRegister register) {
    public ILogger<LoggerHandler> Logger { get; } = logger;
    private LoggerHandlerConfiguration Configuration { get; } = configuration.GetRequired<LoggerHandlerConfiguration>();
    public IEventRegister Register { get; } = register;
}

public partial class LoggerHandler : IHandler {
    public Task StartAsync(CancellationToken token) {
        Register.OnOnline += LoggerOnline;
        Register.OnPrivateMessage += LoggerPrivateMessage;
        Register.OnGroupMessage += LoggerGroupMessage;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken token) {
        Register.OnOnline -= LoggerOnline;
        Register.OnPrivateMessage -= LoggerPrivateMessage;
        Register.OnGroupMessage -= LoggerGroupMessage;

        return Task.CompletedTask;
    }
}

public partial class LoggerHandler {
    private async Task LoggerOnline(IOperation operation, OnlineEvent @event) {
        try {
            CancellationTokenSource cts = new(Configuration.OperationTimeout);
            UserInfo selfInfo = await operation.GetSelfInfo(cts.Token);

            Logger.InformationOnline(@event.Time, selfInfo.Nickname, @event.SelfId);
        } catch (Exception e) { Logger.ErrorUnprocessedError(e); }
    }

    private async Task LoggerPrivateMessage(IOperation operation, PrivateMessageEvent @event) {
        try {
            CancellationTokenSource cts = new(Configuration.OperationTimeout);
            Task<UserInfo> selfInfoTask = operation.GetSelfInfo(cts.Token);
            Task<UserInfo> senderInfoTask = operation.GetFriendInfo(@event.SenderId, cts.Token);

            UserInfo selfInfo = await selfInfoTask;

            UserInfo senderInfo = await senderInfoTask;
            string senderNickname = senderInfo.Nickname;
            string? senderRemark = senderInfo.Remark;

            Logger.InformationPrivateMessage(
                @event.Time,
                selfInfo.Nickname, @event.SelfId,
                @event.MessageId,
                senderRemark == null ? senderNickname : $"{senderRemark}{{{senderNickname}}}", @event.SenderId,
                @event.Messages.ToLoggerString()
            );
        } catch (Exception e) { Logger.ErrorUnprocessedError(e); }
    }

    private async Task LoggerGroupMessage(IOperation operation, GroupMessageEvent @event) {
        try {
            CancellationTokenSource cts = new(Configuration.OperationTimeout);
            Task<UserInfo> selfInfoTask = operation.GetSelfInfo(cts.Token);
            Task<GroupInfo> groupInfoTask = operation.GetGroupInfo(@event.GroupId, cts.Token);
            Task<GroupMemberInfo> senderInfoTask = operation.GetGroupMemberInfo(@event.GroupId, @event.SenderId, cts.Token);

            UserInfo selfInfo = await selfInfoTask;

            GroupInfo groupInfo = await groupInfoTask;

            GroupMemberInfo senderInfo = await senderInfoTask;
            string senderNickname = senderInfo.Nickname;
            string? senderCard = senderInfo.Card;

            Logger.InformationGroupMessage(
                @event.Time,
                selfInfo.Nickname, @event.SelfId,
                @event.MessageId,
                groupInfo.GroupName, @event.GroupId,
                senderCard == null ? senderNickname : $"{senderCard}{{{senderNickname}}}", @event.SenderId,
                @event.Messages.ToLoggerString()
            );
        } catch (Exception e) { Logger.ErrorUnprocessedError(e); }
    }
}

public static partial class LoggerHandlerLogger {
    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "[Online]\n{s, 6}DateTime: {DateTime}\n{s, 6}Self: {SelfName}({SelfId})\n")]
    public static partial void InformationOnline(this ILogger<LoggerHandler> logger, DateTime dateTime, string selfName, uint selfId, string s = "");

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "[PrivateMessage]\n{s, 6}DateTime: {DateTime}\n{s, 6}Self: {selfName}({SelfId})\n{s, 6}MessageId: {MessageId}\n{s, 6}Sender: {senderName}({SenderId})\n{s, 6}Message: {Message}\n")]
    public static partial void InformationPrivateMessage(this ILogger<LoggerHandler> logger, DateTime dateTime, string selfName, uint selfId, int messageId, string senderName, uint senderId, string message, string s = "");

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "[PrivateMessage]\n{s, 6}DateTime: {DateTime}\n{s, 6}Self: {SelfName}({SelfId})\n{s, 6}MessageId: {MessageId}\n{s, 6}Group: {GroupName}({GroupId})\n{s, 6}Sender: {senderName}({SenderId})\n{s, 6}Message: {Message}\n")]
    public static partial void InformationGroupMessage(this ILogger<LoggerHandler> logger, DateTime dateTime, string selfName, uint selfId, int messageId, string groupName, uint groupId, string senderName, uint senderId, string message, string s = "");

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "")]
    public static partial void ErrorUnprocessedError(this ILogger<LoggerHandler> logger, Exception e);
}