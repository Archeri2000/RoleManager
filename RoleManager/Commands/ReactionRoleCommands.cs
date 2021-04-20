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
using RoleManager.Utils;
using static RoleManager.Utils.CommandUtils;

namespace RoleManager.Commands
{
    // Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
    public partial class ReactionRoleCommands : ModuleBase<SocketCommandContext>
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
            return await ManageRR(rrModel, "created Reaction Role");
        }


        [Command("updateRR", RunMode = RunMode.Async)]
        public async Task UpdateRRCommand(string name)
        {
            if (!await UpdateRR(name))
            {
                await SendChannelMessage("> **Timeout or an Error occured and the reaction role was unable to be created.**");
            }
        }
        private async Task<bool> UpdateRR(string name)
        {
            var modelResult = await CheckStaffAndRetrieveModel();
            if (modelResult.IsFailure()) return false;
            
            _logging.Info($"{Context.User.Username}#{Context.User.Discriminator} in Guild {Context.Guild.Name}({Context.Guild.Id}) calling Update RR...");
            await SendChannelMessage(
                $"**Setting up reaction role...** (Called by {MentionUtils.MentionUser(Context.User.Id)})");
            var rr = await _rrRepo.GetReactionRole(Context.Guild.Id, name);
            if (rr.IsFailure())
            {
                await SendChannelMessage($"> Unable to find Reaction Role with name {name}!");
                return false;
            }
            var rrModel = rr.Get();
            await SendChannelMessage($"> Loading current config for reference...");
            await SendChannelMessage(embed: CreateReactionRoleRuleEmbed(rrModel));
            return await ManageRR(rrModel, "updated Reaction Role");
        }

        private async Task<bool> ManageRR(ReactionRoleModel rrModel, string eventType)
        {
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
            //await msg.RemoveAllReactionsAsync();
            await msg.AddReactionsAsync(emotes.ToArray());
            _rrService.UpsertReactionMessage(rrModel);
            await _rrRepo.AddOrUpdateReactionRole(rrModel);

            _logging.Info($"Successfully {eventType}!");
            await SendChannelMessage(
                $"> **Successfully {eventType}!**");
            await SendChannelMessage(embed: CreateReactionRoleRuleEmbed(rrModel));
            return true;
        }

    }
}