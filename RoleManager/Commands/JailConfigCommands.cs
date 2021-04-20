using System;
using System.Collections.Generic;
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

namespace RoleManager.Commands
{
    public partial class JailConfigCommands : ModuleBase<SocketCommandContext>
    {
        private readonly InteractivityService _interactivity;
        private readonly IGuildConfigRepository _repo;
        private readonly IJailSettingsRepository _jailSettings;
        private readonly SourcedLoggingService _logging;
        private readonly DiscordSocketRestClient _client;

        public JailConfigCommands(DiscordSocketRestClient client, ILoggingService logging, IJailSettingsRepository jailSettings, IGuildConfigRepository repo, InteractivityService interactivity)
        {
            _client = client;
            _jailSettings = jailSettings;
            _repo = repo;
            _interactivity = interactivity;
            _logging = new SourcedLoggingService(logging, "jailconfig");
        }
        
        [Command("configJail", RunMode = RunMode.Async)]
        public async Task SetupJailCommand()
        {
            if (!await SetupJail())
            {
                await SendChannelMessage("> **Timeout or an Error occured and the jail command was unable to be configured.**");
            }
        }

        private async Task<bool> SetupJail()
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return false;

            _logging.Info(
                $"{Context.User.Username}#{Context.User.Discriminator} in Guild {Context.Guild.Name}({Context.Guild.Id}) calling Setup Jail...");
            await SendChannelMessage(
                $"**Setting up jail...** (Called by {MentionUtils.MentionUser(Context.User.Id)})");
            var task =
                from shouldLog in GetShouldJailBeLogged()
                from rolesChanged in GetJailRoles()
                select new JailConfigModel(Context.Guild.Id, shouldLog, rolesChanged);
            var res = await task;
            if (res.IsFailure()) return false;
            var model = res.Get();
            if ((await _jailSettings.StoreJailConfig(model)).IsFailure()) return false;
            await SendChannelMessage(embed:model.CreateJailCommandEmbed());
            _logging.Info("Successfully completed jail setup");
            return true;
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