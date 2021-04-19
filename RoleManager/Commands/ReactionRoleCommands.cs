using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
using static RoleManager.CommandExtensions;

namespace RoleManager.Commands
{
    // Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
    public class ReactionRoleCommands : ModuleBase<SocketCommandContext>
    {
        private readonly InteractivityService _interactivity;
        private readonly IGuildConfigRepository _repo;
        private readonly ReactionRoleService _rrService;
        private readonly IReactionRoleRuleRepository _rrRepo;
        private readonly SourcedLoggingService _logging;
        private readonly DiscordSocketRestClient _client;

        public ReactionRoleCommands(InteractivityService interactivity, IGuildConfigRepository repo, ILoggingService logging, DiscordSocketClient client, ReactionRoleService rrService, IReactionRoleRuleRepository rrRepo)
        {
            logging.Info("Initialising Reaction Role Command Service");
            _interactivity = interactivity;
            _repo = repo;
            _rrService = rrService;
            _rrRepo = rrRepo;
            _logging = new SourcedLoggingService(logging, "RRCmds");
            _client = client.Rest;
        }

        [Command("newRR", RunMode = RunMode.Async)]
        public async Task SetupRRCommand()
        {
            if (!await SetupRR())
            {
                await SendChannelMessage("> **Timeout or an Error occured and the reaction role was unable to be created.**");
            }
        }
        
        private async Task<bool> SetupRR()
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return false;
            
            _logging.Info($"{Context.User.Username}#{Context.User.Discriminator} in Guild {Context.Guild.Name}({Context.Guild.Id}) calling Setup RR...");
            await SendChannelMessage(
                $"**Setting up reaction role...** (Called by {MentionUtils.MentionUser(Context.User.Id)})");
            var rrId = Guid.NewGuid();
            var result =
                from name in GetRRName()
                from channel in GetRRChannel()
                from shouldRemove in GetShouldRRBeRemoved()
                from shouldLog in GetShouldRRBeLogged()
                from shouldSave in GetShouldRRBeSaved()
                select new ReactionRoleModel
                {
                    Name = name,
                    ChannelId = channel,
                    GuildId = Context.Guild.Id,
                    Rule = new ReactionRuleModel
                    {
                        Config = new ReactionRoleConfig(shouldRemove, shouldLog, shouldSave, rrId, name)
                    }
                };

            var rr = await result;
            if (rr.IsFailure()) return false;
            var rrModel = rr.Get();
            List<IEmote> emotes = new();
            while (true)
            {
                var ruleResult = await GetReactionRole();
                if (ruleResult.IsFailure()) return false;
                var (emote, roleManageModel) = ruleResult.Get();
                _logging.Verbose("Writing new Rule...");
                rrModel = rrModel.UpdateRRModel(emote, roleManageModel);
                emotes.Add(emote);
                var checkLoop = await GetIsThereAnotherRule();
                if (checkLoop.IsFailure()) return false;
                if(checkLoop.Get() == false)
                {
                    break;
                }
            }

            var res =
                from title in GetRRTitle()
                from content in GetRRContent()
                select CreateReactionRoleMessageEmbed(title, content);

            var embedRes = await res;
            if (embedRes.IsFailure()) return false;

            var embed = embedRes.Get();
            _logging.Info("Created embed!");

            var msg = await Context.Guild.GetTextChannel(rrModel.ChannelId).SendMessageAsync(embed:embed);
            rrModel = rrModel with {MessageId = msg.Id};
            
            _logging.Info("Sent Message!");

            await msg.AddReactionsAsync(emotes.ToArray());
            _rrService.UpsertReactionMessage(rrModel);
            await _rrRepo.AddOrUpdateReactionRole(rrModel);

            _logging.Info("Successfully completed RR setup!");
            await SendChannelMessage(
                $"> **Sucessfully completed Reaction Role setup!**");
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
            // Get Name
            await SendChannelMessage(
                "**What is the name of this reaction role (No spaces, this is used for configs later)?**");
            var name = "";
            var result = await _interactivity.NextMessageAsync(CheckUserAndChannelForMessage(x => x.Content.Split(' ').Length == 1), async (message, b) =>
            {
                name = message.Content;
            });
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }
            await SendChannelMessage(
                $"> **Creating Reaction Role with name: {name}...**");
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
            IEmote iEmote;
            //Get Channel
            var msg = await SendChannelMessage($"**What reaction do you want to use? (React to this message)**");
            string emoteString = null;
            var result = await _interactivity.NextReactionAsync(CheckUserAndMessageForReaction(msg.Id), actions: async (x, b) =>
            {
                emoteString = x.Emote.Name;
            });
            
            if (!result.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }
            
            await SendChannelMessage(
                $"> **Reaction: {emoteString}**");

            if (Emote.TryParse(emoteString, out var emote))
            {
                iEmote = emote;
            }
            else
            {
                iEmote = new Emoji(emoteString);
            }
            
            await SendChannelMessage(
                $"**What are the roles to add on reaction? (Type `skip` to skip)**");
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
                }
                catch (Exception)
                {
                    _logging.Verbose("Role parsing error!");
                    return false;
                }

                return true;
            }));
            if (!result2.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }
            
            await SendChannelMessage(
                $"**What are the roles to remove on reaction? (Type `skip` to skip)**");
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
                }
                catch (Exception)
                {
                    _logging.Verbose("Role parsing error!");
                    return false;
                }

                return true;
            }));
            if (!result2.IsSuccess)
            {
                _logging.Error("Setup command timed out...");
                return new TimeoutException();
            }

            var embed = CreateReactionRoleRuleEmbed(iEmote.Name, rolesToAdd, rolesToRemove);
            await SendChannelMessage(embed: embed);
            return (iEmote, new RoleManageModel(rolesToAdd.ToImmutableHashSet(), rolesToRemove.ToImmutableHashSet()));
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
            return x => x.UserId == Context.User.Id && x.MessageId == msgId && filter(x);
        }
        
    }
}