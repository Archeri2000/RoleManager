using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RoleManager;
using RoleManager.Database;
using RoleManager.Model;
using RoleManager.Repository;
using RoleManager.Service;

// Top Level program
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var databaseUri = new Uri(databaseUrl);
var userInfo = databaseUri.UserInfo.Split(':');

var builder = new NpgsqlConnectionStringBuilder
{
    Host = databaseUri.Host,
    Port = databaseUri.Port,
    Username = userInfo[0],
    Password = userInfo[1],
    Database = databaseUri.LocalPath.TrimStart('/'),
    SslMode = SslMode.Require,
    TrustServerCertificate = true
};

var services = new Setup(builder.ToString()).BuildServiceProvider();
new Program(services).MainAsync().GetAwaiter().GetResult();

namespace RoleManager
{
    
    internal class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly SourcedLoggingService _logger;

        public Program(IServiceProvider serviceProvider)
        {
            _client = serviceProvider.GetService<DiscordSocketClient>();
            _logger = new SourcedLoggingService(serviceProvider.GetService<ILoggingService>(),"Main");
            var dbhosted = new DbMigratorHostedService(serviceProvider, serviceProvider.GetService<ILoggingService>());
            dbhosted.StartAsync(new CancellationToken()).RunSynchronously();
            var reactionRole = serviceProvider.GetService<ReactionRoleService>();
            serviceProvider.GetService<CommandHandler>().InstallCommandsAsync(serviceProvider);
            reactionRole.InitialiseReactionRoles(serviceProvider.GetService<IReactionRoleRuleRepository>());
            // //Testing only
            // var testModel = new ReactionRoleModel
            // {
            //     Name = "test",
            //     GuildId = 826478889181511731,
            //     ChannelId = 826478889181511734,
            //     MessageId = 831544273324212235,
            //     Rule = new ReactionRuleModel
            //     {
            //         Config = new ReactionRoleConfig(true, false, true, new Guid(), "test"),
            //         Reactions = new Dictionary<string, RoleManageModel>{{"\uD83D\uDE14", 
            //             new RoleManageModel(
            //                 new List<ulong>{831543746335997971, 831543788936757248}.ToImmutableHashSet(),  
            //                 new List<ulong>{831543823372910692, 831543852300632124}.ToImmutableHashSet())}}.ToImmutableDictionary()
            //     }
            // };
            // var respModel = new ReactionRoleModel
            // {
            //     Name = "response",
            //     GuildId = 826478889181511731,
            //     ChannelId = 831558317812875374,
            //     MessageId = 831558420443168769,
            //     Rule = new ReverseRuleModel
            //     {
            //         Config = new ReactionRoleConfig(true, false, false, testModel.Rule.Config.StorageKey, "test2"),
            //         Emote = "\uD83D\uDE14"
            //     }
            // };
            // reactionRole.UpsertReactionMessage(testModel);
            // reactionRole.UpsertReactionMessage(respModel);
            _client.Log += _logger.Logger.Log;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.ReactionAdded += reactionRole.OnReaction;
        }

        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token"));
            await _client.StartAsync();
            Console.WriteLine("Starting server");
            // Block the program until it is closed.
            await Task.Delay(Timeout.Infinite);
        }

        // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            ulong stella = 566062763189993503;
            ulong arch = 318686043639644160;
            // The user to check
            ulong uni = 416436297314598912;
            if (message.Author.Id != arch)
                return;
            // The phrase to match against (oh)
            var match = new Regex(@"(\b|_)[oO0]+[hH]+(\b|_)");
            ulong cookie = 719083021071941663;
            if (match.IsMatch(message.Content))
                // The user to ping
                await message.Channel.SendMessageAsync(MentionUtils.MentionUser(arch) + " Uni said oh again!");
        }

        private async Task ReactionReceivedAsnyc(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            Console.WriteLine("Reaction Received");
            if (channel is not SocketTextChannel guildChannel) return;
            Console.WriteLine("Is Guild");
            var guild = guildChannel.Guild;
            if (guild.Id != 738391374021394493) return;
            Console.WriteLine("Is Correct Guild");
            if (guildChannel.Id != 738391374021394496) return;
            Console.WriteLine("Is Correct Channel");
            if (message.Id != 828873869548191766) return;
            Console.WriteLine("Is Correct Id");
            if (reaction.Emote.Name != "\uD83D\uDE14") return;
            Console.WriteLine("Is Correct Emote");
            Console.WriteLine("User Id: " + reaction.UserId);
            var user = (reaction.User.Value as SocketGuildUser);
            var role = guild.GetRole(830361820052193280);
            Console.WriteLine(role.Name);
            Console.WriteLine(user.Id);
            Console.WriteLine("user"+user.Id);
            await user.AddRoleAsync(role);
            Console.WriteLine("Role Added");
        }
    }
}