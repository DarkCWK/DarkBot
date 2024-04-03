namespace DarkBot.Core.Misc;

public class UserInfo(uint userId, string nickname, string? remark) {
    public uint UserId { get; } = userId;
    public string Nickname { get; } = nickname;
    public string? Remark { get; } = remark;
}