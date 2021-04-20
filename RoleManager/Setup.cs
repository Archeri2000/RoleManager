using System;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoleManager.Database;
using RoleManager.Repository;
using RoleManager.Service;

namespace RoleManager
{
 public class Setup
 {
 	private readonly CommandService _commands;
 	private readonly DiscordSocketClient _client;
    private readonly string _connectionString;
 
 	// Ask if there are existing CommandService and DiscordSocketClient
 	// instance. If there are, we retrieve them and add them to the
 	// DI container; if not, we create our own.
 	public Setup(string connectionString, CommandService commands = null, DiscordSocketClient client = null)
 	{
 		_commands = commands ?? new CommandService();
 		_client = client ?? new DiscordSocketClient();
        _connectionString = connectionString;
    }
 
 	public IServiceProvider BuildServiceProvider() => new ServiceCollection()
 		.AddSingleton(_client)
 		.AddSingleton(_commands)
 		// You can pass in an instance of the desired type
        .AddDbContext<CoreDbContext>(options => options.UseNpgsql(_connectionString))
        .AddHostedService<DbMigratorHostedService>()
        .AddSingleton<ILoggingService, LoggingService>()
        .AddSingleton<IGuildConfigRepository, GuildConfigRepository>()
 		.AddSingleton<IReactionRoleRuleRepository, ReactionRoleRuleRepository>()
        .AddSingleton<IRoleEventStorageRepository, RoleEventStorageRepository>()
        .AddSingleton<IDiscordLogMessageService, DiscordLogMessageService>()
        .AddSingleton<ReactionRoleService>()
        .AddSingleton<GuildConfigRepository>()
        .AddSingleton<CommandHandler>()
        .AddSingleton(new InteractivityService(_client, TimeSpan.FromMinutes(1)))
        .BuildServiceProvider();
 }
}