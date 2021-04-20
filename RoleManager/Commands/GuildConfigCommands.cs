using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using RoleManager.Model;
using RoleManager.Repository;
using RoleManager.Service;
using RoleManager.Utils;
using static RoleManager.Utils.CommandUtils;

namespace RoleManager.Commands
{
    // Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
    public class GuildConfigCommands : ModuleBase<SocketCommandContext>
    {
        private readonly InteractivityService _interactivity;
        private readonly IGuildConfigRepository _repo;
        private readonly SourcedLoggingService _logging;
        private readonly DiscordSocketRestClient _client;

        public GuildConfigCommands(InteractivityService interactivity, IGuildConfigRepository repo, ILoggingService logging, DiscordSocketClient client)
        {
            logging.Info("Initialising Guild Config Command Service");
            _interactivity = interactivity;
            _repo = repo;
            _logging = new SourcedLoggingService(logging, "ConfigCmds");
            _client = client.Rest;
        }

        [Command("setup", RunMode = RunMode.Async)]
        public async Task SetupGuildConfig()
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return;
            var model = modelResult.Get();
            
            _logging.Info($"{Context.User.Username}#{Context.User.Discriminator} in Guild {Context.Guild.Name}({Context.Guild.Id}) calling Setup...");
            await SendChannelMessage(
                $"**Setting up bot...** (Called by {MentionUtils.MentionUser(Context.User.Id)})");
            await SendChannelMessage(
                "**What is the logging channel?**");
            ulong channel = 0;
            var result = await _interactivity.NextMessageAsync(message =>
                MentionUtils.TryParseChannel(message.Content, out channel));
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return;
            }

            await SendChannelMessage(
                $"**Set logging channel as {MentionUtils.MentionChannel(channel)}...**");
            await SendChannelMessage(
                $"**What are the staff roles? (Type `skip` to skip)**");
            IEnumerable<ulong> roles = new List<ulong>();
            result = await _interactivity.NextMessageAsync(message =>
            {
                try
                {
                    if (message.Content == "skip")
                    {
                        return true;
                    }

                    _logging.Verbose($"Roles: {message.Content}");
                    roles = message.MentionedRoles.Select(x => x.Id);
                }
                catch (Exception)
                {
                    _logging.Verbose("Role parsing error!");
                    return false;
                }

                return true;
            });
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return;
            }

            var guildConfig = new GuildConfigModel(Context.Guild.Id, channel, roles.ToImmutableHashSet());
            await _repo.StoreGuildConfig(guildConfig);
            _logging.Info("Successfully completed setup!");
            await SendChannelMessage(
                $"**Sucessfully completed setup!**");
            await SendChannelMessage(embed: CreateConfigInfoEmbedLog(guildConfig));

        }

        [Command("loggingChannel", RunMode = RunMode.Async)]
        public async Task LoggingChannel(IChannel channel)
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return;
            var model = modelResult.Get();
            
            model = model with {LogChannel = channel.Id};
            await _repo.StoreGuildConfig(model);
            _logging.Info("Successfully updated Log Channel!");
            await SendChannelMessage(
                $"Successfully update Log Channel to {MentionUtils.MentionChannel(model.LogChannel)}!");
        }
        
        [Command("setStaff", RunMode = RunMode.Async)]
        public async Task SetStaff(IRole role)
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return;
            var model = modelResult.Get();

            model = model.WithAddedStaff(role.Id);
            await _repo.StoreGuildConfig(model);
            _logging.Info("Successfully updated Staff!");
            await SendChannelMessage(
                $"Successfully updated Staff to {model.StaffRoles.GetRoleMentions()}!");
        }
        
        [Command("unsetStaff", RunMode = RunMode.Async)]
        public async Task UnsetStaff(IRole role)
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return;
            var model = modelResult.Get();

            model = model.WithRemovedStaff(role.Id);
            await _repo.StoreGuildConfig(model);
            _logging.Info("Successfully updated Staff!");
            await SendChannelMessage(
                $"Successfully updated Staff to {model.StaffRoles.GetRoleMentions()}!");
        }
        
        
        [Command("configInfo", RunMode = RunMode.Async)]
        public async Task GetInfo()
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return;
            var model = modelResult.Get();
            await SendChannelMessage(embed: CreateConfigInfoEmbedLog(model));
        }

        private async Task<Result<GuildConfigModel>> CheckStaffAndRetrieveModel()
        {
            var userRes = await _client.GetGuildUser(Context.Guild.Id, Context.User.Id);
            if (userRes.IsFailure()) return new KeyNotFoundException();
            var user = userRes.Get();
            var result = await _repo.GetGuildConfig(Context.Guild.Id);
            var model = result.GetModelOrDefault(Context.Guild);
            if (!user.IsStaff(model.StaffRoles)) return new UnauthorizedAccessException();
            return model;
        }
        
        private async Task SendChannelMessage(string msg = null, Embed embed = null)
        {
            await Context.Channel.SendMessageAsync(text:msg, embed:embed);
        }
        
    }
}