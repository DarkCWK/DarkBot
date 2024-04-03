using System.Text.Json;

namespace DarkBot.Core.Message.Impl;

public class JsonMessage(JsonElement json) : IMessage {
    public JsonElement Json { get; } = json;
}