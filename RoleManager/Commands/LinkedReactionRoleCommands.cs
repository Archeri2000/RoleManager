using System;
using System.Collections.Generic;
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
    public partial class LinkedReactionRoleCommands : ModuleBase<SocketCommandContext>
    {
        private readonly InteractivityService _interactivity;
        private readonly IGuildConfigRepository _repo;
        private readonly ReactionRoleService _rrService;
        private readonly IReactionRoleRuleRepository _rrRepo;
        private readonly SourcedLoggingService _logging;
        private readonly DiscordSocketRestClient _client;

        public LinkedReactionRoleCommands(InteractivityService interactivity, IGuildConfigRepository repo, ILoggingService logging, DiscordSocketClient client, ReactionRoleService rrService, IReactionRoleRuleRepository rrRepo)
        {
            logging.Info("Initialising Reaction Role Command Service");
            _interactivity = interactivity;
            _repo = repo;
            _rrService = rrService;
            _rrRepo = rrRepo;
            _logging = new SourcedLoggingService(logging, "RRCmds");
            _client = client.Rest;
        }

        [Command("linkedRR", RunMode = RunMode.Async)]
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
            
            _logging.Info($"{Context.User.Username}#{Context.User.Discriminator} in Guild {Context.Guild.Name}({Context.Guild.Id}) calling Setup Linked RR...");
            await SendChannelMessage(
                $"**Setting up linked reaction role...** (Called by {MentionUtils.MentionUser(Context.User.Id)})");
            var result =
                from name in GetRRName()
                from channel in GetRRChannel()
                from shouldRemove in GetShouldRRBeRemoved()
                from shouldLog in GetShouldRRBeLogged()
                from rrId in GetRRStorageKey()
                select new ReactionRoleModel
                {
                    Name = name,
                    ChannelId = channel,
                    GuildId = Context.Guild.Id,
                    Rule = new ReverseRuleModel()
                    {
                        Config = new ReactionRoleConfig(shouldRemove, shouldLog, false, rrId, name)
                    }
                };

            var rr = await result;
            if (rr.IsFailure()) return false;
            var rrModel = rr.Get();

            var emoteTask = GetEmote();
            var emoteRes = await emoteTask;
            if (emoteRes.IsFailure()) return false;
            var emote = emoteRes.Get();
            rrModel = rrModel.WithEmoteLinked(emote.Name);
            
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
            
            await msg.AddReactionAsync(emote);
            _rrService.UpsertReactionMessage(rrModel);
            await _rrRepo.AddOrUpdateReactionRole(rrModel);

            _logging.Info("Successfully completed Linked RR setup!");
            await SendChannelMessage(
                $"> **Sucessfully completed Linked Reaction Role setup!**");
            return true;
        }

    }
}