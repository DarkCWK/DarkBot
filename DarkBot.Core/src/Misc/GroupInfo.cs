namespace DarkBot.Core.Misc;

public class GroupInfo(uint groupId, string groupName) {
    public uint GroupId { get; } = groupId;
    public string GroupName { get; } = groupName;
}