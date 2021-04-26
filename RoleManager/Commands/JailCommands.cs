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
using static RoleManager.Utils.RoleUtils;

namespace RoleManager.Commands
{
    public partial class JailCommands : ModuleBase<SocketCommandContext>
    {
        private readonly InteractivityService _interactivity;
        private readonly IJailDataRepository _jailData;
        private readonly IGuildConfigRepository _repo;
        private readonly IJailSettingsRepository _jailSettings;
        private readonly SourcedLoggingService _logging;
        private readonly DiscordSocketRestClient _client;

        public JailCommands(InteractivityService interactivity, IGuildConfigRepository repo, IJailSettingsRepository jailSettings, ILoggingService logging, DiscordSocketClient client, IJailDataRepository jailData)
        {
            _interactivity = interactivity;
            _repo = repo;
            _jailSettings = jailSettings;
            _jailData = jailData;
            _logging = new SourcedLoggingService(logging, "Jail");
            _client = client.Rest;
            _logging.Verbose("Set up jail!");
        }

        [Command("jail", RunMode = RunMode.Async)]
        public async Task Jail(IUser u)
        {
            _logging.Verbose("Jail function called");
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return;
            
            _logging.Info($"{Context.User.Username}#{Context.User.Discriminator} in Guild {Context.Guild.Name}({Context.Guild.Id}) calling Jail...");

            var configRes = await _jailSettings.GetJailConfig(Context.Guild.Id);
            if (configRes.IsFailure()) return;
            var config = configRes.Get();
            var resultTask =
                from user in _client.GetGuildUser(Context.Guild.Id, u.Id)
                select user.EditRoles(MapRoles(Context.Guild, user)(config.Roles));
            var result = await resultTask;
            if (result.IsFailure()) return;
            await SendChannelMessage(
                $"> **User {MentionUtils.MentionUser(u.Id)} has been jailed.** (Called by {MentionUtils.MentionUser(Context.User.Id)})");
            //TODO: Write to jail data and create callback
            var model = result.Get();
            await _jailData.Store(Context.Guild.Id, model.ToModel());

            await SetJailDetails(model, config.LogChannel);
        }
        
        [Command("unjail", RunMode = RunMode.Async)]
        public async Task UnJail(IUser u)
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return;
            
            _logging.Info($"{Context.User.Username}#{Context.User.Discriminator} in Guild {Context.Guild.Name}({Context.Guild.Id}) calling UnJail...");
            
            var configRes = await _jailSettings.GetJailConfig(Context.Guild.Id);
            if (configRes.IsFailure()) return;
            var config = configRes.Get();
            
            var model = await _jailData.Load(Context.Guild.Id, u.Id);
            if (model.IsFailure())
            {
                await TrySendLogMessage(config.LogChannel, $"No jail record data for User {MentionUtils.MentionUser(u.Id)}");
                return;
            }
            await UnjailUser(model.Get(), config.LogChannel);
        }

        private async Task SetJailDetails(RoleUpdateEvent updateEvent, ulong logChannel)
        {
            var result =
                from reason in GetJailReason()
                from duration in GetJailDuration()
                from _ in SendJailLog(reason, duration, updateEvent, logChannel)
                select duration;
            var durationResult = await result;
            if (durationResult.IsFailure()) return;
            var dura = durationResult.Get();
            Task.Delay(dura.ToTimeSpan()).ContinueWith(_ => UnjailUser(updateEvent.ToModel(), logChannel));
            await TrySendLogMessage(logChannel,
                    $"User {MentionUtils.MentionUser(updateEvent.User.Id)} will be unjailed automatically in {dura}.");
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

        private async Task TrySendLogMessage(ulong logChannel, string msg = null, Embed embed = null)
        {
            if (logChannel != 0)
            {
                await Context.Guild.GetTextChannel(logChannel).SendMessageAsync(text: msg, embed: embed);
            }
        }
    }
}