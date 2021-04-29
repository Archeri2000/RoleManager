using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using RoleManager.Model;
using RoleManager.Utils;
using static RoleManager.Utils.CommandUtils;

namespace RoleManager.Commands
{
    public partial class ReactionRoleCommands
    {
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

        private async Task<Result<bool>> GetShouldRRBeRemoved()
        {
            await SendChannelMessage("**Should the reaction be removed after? (y/n)**");
            return await GetBool()
                .DoAwait(async x =>
                {
                    await SendChannelMessage(x?"> **Reaction will be removed after.**":"> **Reaction will not be removed.**");
                }, Errors.MapAll);
        }
        
        private async Task<Result<bool>> GetShouldRRBeLogged()
        {
            await SendChannelMessage("**Should the reaction role be logged? (y/n)**");
            return await GetBool()
                .DoAwait(async x =>
                {
                    await SendChannelMessage(x?"> **Reaction will be logged.**":"> **Reaction will not be logged.**");
                }, Errors.MapAll);
        }
        
        private async Task<Result<bool>> GetShouldRRBeSaved()
        {
            await SendChannelMessage("**Should the roles changed (Used for linked Reaction Roles) be saved? (y/n)**");
            return await GetBool()
                .DoAwait(async x =>
                {
                    await SendChannelMessage(x?"> **Roles changed will be saved.**":"> **Roles changed will not be saved.**");
                }, Errors.MapAll);
        }

        private async Task<Result<bool>> GetBool()
        {
            var isTrue = false;
            var result = await _interactivity.NextMessageAsync(CheckUserAndChannelForMessage( message => message.Content.ToLower() is "y" or "n"),
                async (message, b) =>
                {
                    if (message.Content.ToLower() is "y")
                    {
                        isTrue = true;
                    }
                });
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }
            return isTrue;
        }
        
        private async Task<Result<bool>> GetIsThereAnotherRule()
        {
            _logging.Verbose("Checking if there is another rule...");
            await SendChannelMessage("**Is there another rule? (y/n)**");
            return await GetBool()
                .DoAwait(async x =>
                {
                    await SendChannelMessage(x?"> **Preparing next rule...**":"> **Adding all rules...**");
                }, Errors.MapAll);
        } 

        private async Task<Result<string>> GetRRName()
        {
            var existingRRNames = _rrService.GetNames(Context.Guild.Id);
            await SendChannelMessage($"> **Current Reaction Role identifiers: {existingRRNames.MapToString()}**");
            // Get Name
            await SendChannelMessage(
                "**What is the identifier of this reaction role (No spaces, this is used for configs later)?**");
            var name = "";
            var result = await _interactivity.NextMessageAsync(CheckUserAndChannelForMessage(x => x.Content.Split(' ').Length == 1 && CheckNotInList(existingRRNames)(x)), async (message, b) =>
            {
                name = message.Content;
            });
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }
            await SendChannelMessage(
                $"> **Creating Reaction Role with identifier: {name}...**");
            return name;
        }
        
        private async Task<Result<string>> GetRRTitle()
        {
            await SendChannelMessage(
                "**What is the title of this RR?**");
            var title = "";
            var result = await _interactivity.NextMessageAsync(CheckUserAndChannelForMessage(), actions: async (message, b) =>
            {
                title = message.Content;
            });
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }
            await SendChannelMessage(
                $"> **Set title as: {title}...**");
            return title;
        }
        
        private async Task<Result<string>> GetRRContent()
        {
            // Get Name
            await SendChannelMessage(
                "**What is the text of this RR?**");
            var content = "";
            var result = await _interactivity.NextMessageAsync(CheckUserAndChannelForMessage(), actions: async (message, b) =>
            {
                content = message.Content;
            });
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }
            await SendChannelMessage(
                $"> **Set text as: {content}...**");
            return content;
        }

        private async Task<Result<ulong>> GetRRChannel()
        {
            //Get Channel
            await SendChannelMessage(
                $"**What channel do you want it in?**");
            
            ulong channel = 0;
            var result = await _interactivity.NextMessageAsync(CheckUserAndChannelForMessage(message =>
                MentionUtils.TryParseChannel(message.Content, out channel)));
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }

            await SendChannelMessage(
                $"> **Creating Reaction Role in channel: {MentionUtils.MentionChannel(channel)}...**");
            return channel;
        }

        private async Task<Result<(IEmote, RoleManageModel)>> GetReactionRole()
        {
            while (true)
            {
                IEmote iEmote;
                //Get Channel
                var msg = await SendChannelMessage($"**What reaction do you want to use? (React to this message)**");
                string emoteString = null;
                var result = await _interactivity.NextReactionAsync(CheckUserAndMessageForReaction(msg.Id), actions: async (x, b) =>
                {
                    if (b) await SendChannelMessage($"> Invalid Reaction! Reaction must be from this server!");
                    emoteString = x.Emote.Name;
                });

                if (!result.IsSuccess)
                {
                    _logging.Error("Setup command timed out...");
                    return new TimeoutException();
                }

                await SendChannelMessage($"> **Reaction: {emoteString}**");

                if (Emote.TryParse(emoteString, out var emote))
                {
                    iEmote = emote;
                }
                else
                {
                    iEmote = new Emoji(emoteString);
                }

                await SendChannelMessage($"**What are the roles to add on reaction? (Mention all roles in one line, or type `skip` to skip)**");
                var rolesToAdd = new List<ulong>();
                var result2 = await _interactivity.NextMessageAsync(CheckUserAndChannelForMessage(message =>
                {
                    try
                    {
                        if (message.Content == "skip")
                        {
                            return true;
                        }

                        _logging.Verbose($"Roles: {message.Content}");
                        rolesToAdd = message.MentionedRoles.Select(x => x.Id).ToList();
                        if (rolesToAdd.Count == 0)
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        _logging.Verbose("Role parsing error!");
                        return false;
                    }

                    return true;
                }), async (_, b) =>
                {
                    if (b) await SendChannelMessage("> Invalid roles list");
                });
                if (!result2.IsSuccess)
                {
                    _logging.Error("Setup command timed out...");
                    return new TimeoutException();
                }

                await SendChannelMessage($"**What are the roles to remove on reaction? (Mention all roles in one line, or type `skip` to skip)**");
                var rolesToRemove = new List<ulong>();
                result2 = await _interactivity.NextMessageAsync(CheckUserAndChannelForMessage(message =>
                {
                    try
                    {
                        if (message.Content == "skip")
                        {
                            return true;
                        }

                        _logging.Verbose($"Roles: {message.Content}");
                        rolesToRemove = message.MentionedRoles.Select(x => x.Id).ToList();
                        if (rolesToRemove.Count == 0)
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        _logging.Verbose("Role parsing error!");
                        return false;
                    }

                    return true;
                }), async (_, b) =>
                {
                    if (b) await SendChannelMessage("> Invalid roles list");
                });
                if (!result2.IsSuccess)
                {
                    _logging.Error("Setup command timed out...");
                    return new TimeoutException();
                }

                var embed = CreateReactionRoleRuleEmbed(iEmote.Name, rolesToAdd, rolesToRemove);
                await SendChannelMessage(embed: embed);
                await SendChannelMessage("> **Is the above rule correct? (y/n)**");
                var isOk = await GetBool();
                if (isOk.IsFailure()) return new TimeoutException();
                if (isOk.Get())
                {
                    return (iEmote,
                        new RoleManageModel(rolesToAdd.ToImmutableHashSet(), rolesToRemove.ToImmutableHashSet()));
                }
            }
        }

        private async Task<RestUserMessage> SendChannelMessage(string msg = null, Embed embed = null)
        {
            return await Context.Channel.SendMessageAsync(text:msg, embed:embed);
        }

        private Predicate<SocketMessage> CheckUserAndChannelForMessage() => CheckUserAndChannelForMessage(_ => true);

        private Predicate<SocketMessage> CheckUserAndChannelForMessage(Predicate<SocketMessage> filter)
        {
            return x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id && filter(x);
        }

        private Predicate<SocketReaction> CheckUserAndMessageForReaction(ulong msgId) => CheckUserAndMessageForReaction(msgId, _ => true);
        private Predicate<SocketReaction> CheckUserAndMessageForReaction(ulong msgId, Predicate<SocketReaction> filter)
        {
            return x => x.UserId == Context.User.Id && x.MessageId == msgId && (x.Emote is Emoji || Context.Guild.Emotes.Contains(x.Emote)) && filter(x);
        }

        private Predicate<SocketMessage> CheckNotInList(List<string> identifiers)
        {
            return x => !identifiers.Contains(x.Content);
        }
        
    }
}