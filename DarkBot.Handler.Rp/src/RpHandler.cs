using DarkBot.Core.Event;
using DarkBot.Core.Event.Impl;
using DarkBot.Core.Handler;
using DarkBot.Core.Message;
using DarkBot.Core.Message.Impl;
using DarkBot.Core.Operation;
using DarkBot.Handler.Rp.Configuration;
using DarkBot.Handler.Rp.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DarkBot.Handler.Rp;

public partial class RpHandler(ILogger<RpHandler> logger, IEventRegister register, IConfiguration configuration) {
    public ILogger<RpHandler> Logger { get; } = logger;
    public IEventRegister Register { get; } = register;
    public RootConfiguration Configuration { get; } = configuration.GetRequired<RootConfiguration>();
}

public partial class RpHandler : IHandler {
    public Task StartAsync(CancellationToken token) {
        Register.OnPrivateMessage += HandlePrivate;
        Register.OnGroupMessage += HandleGroup;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken token) {
        Register.OnPrivateMessage -= HandlePrivate;
        Register.OnGroupMessage -= HandleGroup;

        return Task.CompletedTask;
    }
}

public partial class RpHandler {
    private Task HandlePrivate(IOperation operation, PrivateMessageEvent @event) {
        PermissionUser permission = Configuration.Permission.User;
        bool hasGroup = permission.Users.Contains(@event.SenderId);
        if (permission.Type switch {
            "whitelist" => !hasGroup,
            "blacklist" => hasGroup,
            _ => throw new NotSupportedException($"permission type '{permission.Type}' not supported")
        }) return Task.CompletedTask;

        if (!CommandCheck(@event.Messages)) return Task.CompletedTask;

        CancellationTokenSource cts = new(Configuration.OperationTimeout);
        return operation.SendPrivateMessage(@event.SenderId, GetRpDescriptionMessages(@event.SenderId, false), cts.Token);
    }

    private Task HandleGroup(IOperation operation, GroupMessageEvent @event) {
        PermissionGroup permission = Configuration.Permission.Group;
        bool hasGroup = permission.Groups.Contains(@event.GroupId);
        if (permission.Type switch {
            "whitelist" => !hasGroup,
            "blacklist" => hasGroup,
            _ => throw new NotSupportedException($"permission type '{permission.Type}' not supported")
        }) return Task.CompletedTask;

        if (!CommandCheck(@event.Messages)) return Task.CompletedTask;

        CancellationTokenSource cts = new(Configuration.OperationTimeout);
        return operation.SendGroupMessage(@event.GroupId, GetRpDescriptionMessages(@event.SenderId, false), cts.Token);
    }

    private static bool CommandCheck(IEnumerable<IMessage> messages) {
        if (!messages.Any()) return false;

        if (messages.First() is AtMessage) messages = messages.Take(1);

        if (messages.First() is not TextMessage) return false;

        if (((TextMessage)messages.First()).Text != ".jrrp") return false;

        return true;
    }
}

public partial class RpHandler {
    private static IMessage YouTextMessage { get; } = new TextMessage("你");

    private IMessage[] GetRpDescriptionMessages(uint userId, bool isPrivate) {
        uint todayRp = CalculateTodayRp(userId);

        return [
            new TextMessage(GetRpDescriptionPrefix(todayRp)),
            isPrivate ? YouTextMessage : new AtMessage(userId),
            new TextMessage(GetRpDescriptionSuffix(todayRp))
        ];
    }

    private static uint CalculateTodayRp(uint userId) {
        return (uint)(RandomUtil.GetRandom(DateTime.UtcNow.Date.ToUnixTimestamp() % userId) % 101);
    }

    private string GetRpDescriptionPrefix(uint rp) {
        foreach (IConfigurationSection description in Configuration.Descriptions) {
            if (byte.Parse(description.Key) >= rp) {
                return string.Format(description.GetRequiredSection("prefix").GetRequired<string>(), rp);
            }
        }
        throw new KeyNotFoundException();
    }

    private string GetRpDescriptionSuffix(uint rp) {
        foreach (IConfigurationSection description in Configuration.Descriptions) {
            if (byte.Parse(description.Key) >= rp) {
                return string.Format(description.GetRequiredSection("suffix").GetRequired<string>(), rp);
            }
        }
        throw new KeyNotFoundException();
    }
}

public static partial class RpHandlerLogger { }
