using System;
using DarkBot.Core.Event;
using DarkBot.Core.Handler;
using DarkBot.Core.Protocol;
using DarkBot.Launcher.Manager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DarkBot.Launcher;

internal class Program {
    private static void Main(string[] args) {
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => {
                services.AddSingleton<EventAdapter>();
                services.AddSingleton<IEventEmiter>(GetEventAdapter);
                services.AddSingleton<IEventRegister>(GetEventAdapter);

                services.AddHostedService<DllManager<IHandler>>(provide => new(
                    provide,
                    context.Configuration.GetRequiredSection("Handler").GetChildren(),
                    context.Configuration["HandlerDirectory"] ?? "Handler",
                    (p, token) => p.StartAsync(token),
                    (p, token) => p.StopAsync(token)
                ));

                services.AddHostedService<DllManager<IProtocol>>(provide => new(
                    provide,
                    context.Configuration.GetRequiredSection("Protocol").GetChildren(),
                    context.Configuration["ProtocolDirectory"] ?? "Protocol",
                    (p, token) => p.StartAsync(token),
                    (p, token) => p.StopAsync(token)
                ));
            })
            .Build()
            .Run();
    }

    private static EventAdapter GetEventAdapter(IServiceProvider provider) {
        return provider.GetRequiredService<EventAdapter>();
    }
}