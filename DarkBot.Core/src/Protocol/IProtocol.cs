using System.Threading;
using System.Threading.Tasks;

namespace DarkBot.Core.Protocol;

public interface IProtocol {
    public Task StartAsync(CancellationToken token);
    public Task StopAsync(CancellationToken token);
}