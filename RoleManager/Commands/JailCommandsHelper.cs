using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Interactivity;
using RoleManager.Model;
using RoleManager.Repository;
using RoleManager.Service;
using RoleManager.Utils;
using static RoleManager.Utils.CommandUtils;

namespace RoleManager.Commands
{
    public partial class JailCommands
    {
        private const string RegexString = @"^(?:(\d+)(?:d|day|days))?(?:\s*?(\d+)\s*?(?:h|hr|hrs|hour|hours))?(?:\s*?(\d+)\s*?(?:m|min|mins|minute|minutes))?$";
        private async Task<Result<string>> GetJailReason()
        {
            // Get Name
            await SendUserMessage(
                "> **What is the reason for jailing?**");
            
            var content = "";
            var channel = await Context.User.GetOrCreateDMChannelAsync();
            var result = await _interactivity.NextMessageAsync(CheckUserForMessage(channel.Id), actions: async (message, b) =>
            {
                content = message.Content;
            });
            if (!result.IsSuccess)
            {
                _logging.Error("Jail config timed out...");
                return new TimeoutException();
            }
            await SendUserMessage(
                $"> **Configuring jailing reason as: {content}**");
            return content;
        }
        
        private async Task<Result<JailTimeSpan>> GetJailDuration()
        {
            // Get Name
            await SendUserMessage(
                "> **What is the jailing duration (enter based on days, hours, minutes) [e.g. 5d2h30m will jail for 5 days, 2 hours and 30 minutes]?**");
            //TODO
            var content = "";
            var channel = await Context.User.GetOrCreateDMChannelAsync();
            GroupCollection group = null;
            var result = await _interactivity.NextMessageAsync(CheckUserForMessage(channel.Id, msg =>
            {
                var regex = new Regex(RegexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var result = regex.Match(msg.Content);
                group = result.Groups;
                return result.Success;
            }), actions: async (message, b) =>
            {
                content = message.Content;
            });
            if (!result.IsSuccess)
            {
                _logging.Error("Jail config timed out...");
                return new TimeoutException();
            }
            await SendUserMessage(
                $"> **Configuring jailing timing as: {content}**");
            return new JailTimeSpan(int.Parse(group[1].Value == ""?"0":group[1].Value), 
                int.Parse(group[2].Value == ""?"0":group[2].Value), 
                int.Parse(group[3].Value == ""?"0":group[3].Value));
        }

        private async Task<Result<RestGuildUser>> GetTarget(string target)
        {
            if (!MentionUtils.TryParseUser(target, out var userId)) return new ArgumentException("No User was specified!");
            var user = await _client
                .GetGuildUser(Context.Guild.Id, userId)
                .IfAwait(async x =>
                {
                    var jailEntry = await _jailData.Load(x.GuildId, userId);
                    if (jailEntry.IsFailure()) return true;
                    await SendChannelMessage($"> User {x.Username}#{x.Discriminator} has already been jailed!");
                    return false;
                });
            return user;
        }
        
        private async Task<Result<Unit>> SendJailLog(string reason, JailTimeSpan duration, RoleUpdateEvent target, ulong channel)
        {
            var embed = CreateJailLogEmbed(reason, duration, Context.User, target);
            await TrySendLogMessage(logChannel: channel, embed: embed);

            return new Unit();
        }
        
        private Predicate<SocketMessage> CheckUserAndChannelForMessage() => CheckUserAndChannelForMessage(_ => true);

        private Predicate<SocketMessage> CheckUserAndChannelForMessage(Predicate<SocketMessage> filter)
        {
            return x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id && filter(x);
        }
        
        private Predicate<SocketMessage> CheckUserForMessage(ulong channelId) => CheckUserForMessage(channelId, _ => true);

        private Predicate<SocketMessage> CheckUserForMessage(ulong channelId, Predicate<SocketMessage> filter)
        {
            return x => x.Author.Id == Context.User.Id && x.Channel.Id == channelId && filter(x);
        }

        private async Task SendUserMessage(string message=null, Embed embed=null)
        {
            await Context.User.SendMessageAsync(text: message, embed: embed);
        }


    }
}