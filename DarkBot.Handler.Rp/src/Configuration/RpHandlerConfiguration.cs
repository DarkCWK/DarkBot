using Microsoft.Extensions.Configuration;

namespace DarkBot.Handler.Rp.Configuration;

public class RootConfiguration(int operationTimeout, Permission permission, IConfigurationSection[] descriptions) {
    public int OperationTimeout { get; } = operationTimeout;

    public Permission Permission { get; } = permission;

    public IConfigurationSection[] Descriptions { get; } = descriptions;
}

public class Permission(PermissionGroup group, PermissionUser user) {
    public PermissionGroup Group { get; } = group;
    public PermissionUser User { get; } = user;
}

public class PermissionGroup(string type, uint[] groups) {
    public string? Type { get; } = type;
    public uint[] Groups { get; } = groups;
}

public class PermissionUser(string type, uint[] users) {
    public string Type { get; } = type;
    public uint[] Users { get; } = users;
}