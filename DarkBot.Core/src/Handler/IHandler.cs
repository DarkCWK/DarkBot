using System.Threading;
using System.Threading.Tasks;

namespace DarkBot.Core.Handler;

public interface IHandler {
    public Task StartAsync(CancellationToken token);
    public Task StopAsync(CancellationToken token);
}