using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DarkBot.Core.Message;
using DarkBot.Core.Misc;

namespace DarkBot.Core.Operation;

public interface IOperation {
    public Task<int> SendPrivateMessage(uint userId, IEnumerable<IMessage> messages, CancellationToken token);

    public Task<int> SendGroupMessage(uint groupId, IEnumerable<IMessage> messages, CancellationToken token);

    public Task<UserInfo> GetSelfInfo(CancellationToken token);

    public Task<UserInfo> GetFriendInfo(uint userId, CancellationToken token);

    public Task<IEnumerable<UserInfo>> GetFriendInfos(CancellationToken token);

    public Task<GroupMemberInfo> GetGroupMemberInfo(uint groupId, uint userId, CancellationToken token);

    public Task<GroupInfo> GetGroupInfo(uint groupId, CancellationToken token);
}