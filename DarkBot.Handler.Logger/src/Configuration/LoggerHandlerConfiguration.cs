namespace DarkBot.Handler.Logger.Configuration;

public class LoggerHandlerConfiguration(int operationTimeout) {
    public int OperationTimeout { get; } = operationTimeout;
}