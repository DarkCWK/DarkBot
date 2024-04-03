namespace DarkBot.Core.Message.Impl;

public class FaceMessage(int faceId) : IMessage {
    public int FaceId { get; } = faceId;
}