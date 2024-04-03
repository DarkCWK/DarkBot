namespace DarkBot.Core.Misc;

public class GroupMemberInfo(uint groupId, uint userId, string nickname, string? card) {
    public uint GroupId { get; } = groupId;
    public uint UserId { get; } = userId;
    public string Nickname { get; } = nickname;
    public string? Card { get; } = card;
}