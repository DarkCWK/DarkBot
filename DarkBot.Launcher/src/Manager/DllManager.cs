using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DarkBot.Launcher.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DarkBot.Launcher.Manager;

public partial class DllManager<T>(IServiceProvider serviceProvider, IEnumerable<IConfiguration> configurations, string dllDirectory, Func<T, CancellationToken, Task> runAction, Func<T, CancellationToken, Task> stopAction) {
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private IEnumerable<IConfiguration> Configurations { get; } = configurations;
    private string DllDirectory { get; } = dllDirectory;
    private Func<T, CancellationToken, Task> RunAction { get; } = runAction;
    private Func<T, CancellationToken, Task> StopAction { get; } = stopAction;
}

public partial class DllManager<T> : IHostedService {
    private List<T> DllServices { get; } = [];

    public Task StartAsync(CancellationToken token) {
        if (!Directory.Exists(DllDirectory)) Directory.CreateDirectory(DllDirectory);

        IEnumerable<(string dll, IEnumerable<(string clazz, IEnumerable<IConfiguration> configurations)> info)> infos = Configurations.Select(c => {
            string[] dllAndClazz = c.GetRequiredString("Type").Split(":");
            return (dll: dllAndClazz[0], clazz: dllAndClazz[1], configuration: c);
        }).GroupBy(i => i.dll, i => (i.clazz, i.configuration))
        .Select(i => (
            dll: i.Key,
            info: i.AsEnumerable()
                .GroupBy(i => i.clazz, i => i.configuration)
                .Select(i => (clazz: i.Key, configurations: i.AsEnumerable()))
        ));

        foreach ((string dll, IEnumerable<(string clazz, IEnumerable<IConfiguration> configurations)> info) in infos) {
            Assembly assembly = Assembly.LoadFrom($"{DllDirectory}/{dll}");
            foreach ((string clazz, IEnumerable<IConfiguration> configurations) in info) {
                Type type = assembly.GetType(clazz) ?? throw new TypeLoadException();
                foreach (IConfiguration configuration in configurations) {
                    try {
                        DllServices.Add((T)ActivatorUtilities.CreateInstance(ServiceProvider, type, configuration));
                    } catch (Exception e) {
                        try {
                            DllServices.Add((T)ActivatorUtilities.CreateInstance(ServiceProvider, type));
                        } catch {
                            throw e;
                        }
                    }
                }
            }
        }

        return Task.WhenAll(DllServices.Select(d => RunAction(d, token)));
    }

    public Task StopAsync(CancellationToken token) {
        return Task.WhenAll(DllServices.Select(d => StopAction(d, token)));
    }
}