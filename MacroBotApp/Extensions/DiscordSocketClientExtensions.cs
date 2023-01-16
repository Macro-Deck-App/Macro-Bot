﻿using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MacroBot.Extensions;

public static class DiscordSocketClientExtensions
{
    public static void UseSerilog(this DiscordSocketClient client)
    {
        var logger = Log.ForContext<DiscordSocketClient>();
        client.Log += delegate(LogMessage message)
        {
            if (message.Message is null || 
                message.Exception?.Message is null)
            {
                return Task.CompletedTask;
            }

            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    logger.Fatal(
                        message.Message,
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Error:
                    logger.Error(
                        message.Message,
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Warning:
                    logger.Warning(
                        message.Message, 
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Info:
                    logger.Information(
                        message.Message, 
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Verbose:
                    logger.Verbose(
                        message.Message, 
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Debug:
                default:
                    logger.Debug(
                        message.Message, 
                        message.Exception.InnerException);
                    break;
            }

            return Task.CompletedTask;
        };
    }

    public static async Task MapModulesAsync(this DiscordSocketClient discordSocketClient,
        InteractionService interactionService,
        IServiceScopeFactory serviceScopeFactory, ILogger logger)
    {
        var scope = serviceScopeFactory.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
        logger.Information(
            "{NoOfModules} modules mapped - {Modules}", 
            interactionService.Modules.Count,
            string.Join(",", interactionService.Modules.Select(x => x.Name)));
    }
}