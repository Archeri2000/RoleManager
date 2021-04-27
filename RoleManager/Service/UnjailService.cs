using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using RoleManager.Model;
using RoleManager.Repository;
using RoleManager.Utils;

namespace RoleManager.Service
{
    public class UnjailService
    {
        private readonly IJailDataRepository _jailData;
        private readonly SourcedLoggingService _logging;
        private readonly DiscordSocketRestClient _client;
        private readonly ConcurrentDictionary<(ulong guildId, ulong userId), CancellationTokenSource> _tokenSources = new();

        
        public async Task UnjailUser(RoleUpdateModel model, ulong logChannel, ulong guildId)
        {
            var guild = await _client.GetGuildAsync(guildId);
            var roles = new RoleManageDomain(model.RolesChanged.ToRemove.Select(x => (IRole)guild.GetRole(x)).ToImmutableList(),
                model.RolesChanged.ToAdd.Select(x => (IRole)guild.GetRole(x)).ToImmutableList());
            var result =
                from user in _client.GetGuildUser(guildId, model.User)
                select user.EditRoles(roles);
            var res = await result;
            if (res.IsFailure())
            {
                await TrySendLogMessage(guild, logChannel, $"Unable to unjail User {MentionUtils.MentionUser(model.User)}!");
            }
            else
            {
                await TrySendLogMessage(guild, logChannel,
                    $"Successfully unjailed User {MentionUtils.MentionUser(model.User)}!");
                await _jailData.Delete(guildId, model.User);
            }
        }

        public async Task ScheduleUnjail(JailTimeSpan dura, RoleUpdateEvent updateEvent, ulong logChannel)
        {
            var token = new CancellationTokenSource();
            var guildId = updateEvent.User.GuildId;
            var userId = updateEvent.User.Id;
            _tokenSources[(guildId, userId)] = token;
            Task.Delay(dura.ToTimeSpan(), token.Token).ContinueWith(async t =>
            {
                if(!t.IsCanceled)
                {
                    await UnjailUser(updateEvent.ToModel(), logChannel, userId);
                    _tokenSources.Remove((guildId, userId), out _);
                }
            });
        }

        public async Task CancelUnjail(ulong guild, ulong user)
        {
            if (_tokenSources.TryGetValue((guild, user), out var tokenSource))
            {
                tokenSource.Cancel();
            }
        }

        private async Task TrySendLogMessage(RestGuild guild, ulong logChannel, string msg = null, Embed embed = null)
        {
            if (logChannel != 0)
            {
                await (await guild.GetTextChannelAsync(logChannel)).SendMessageAsync(text: msg, embed: embed);
            }
        }
    }
}