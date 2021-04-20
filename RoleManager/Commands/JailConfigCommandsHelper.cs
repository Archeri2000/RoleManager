using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using static RoleManager.Utils.CommandUtils;
using Discord.WebSocket;
using RoleManager.Model;

namespace RoleManager.Commands
{
    public partial class JailConfigCommands
    {
                private async Task<Result<RoleManageModel>> GetJailRoles()
        {
            await SendChannelMessage(
                $"**What are the roles to add when jailing? (Type `skip` to skip)**");
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
                $"**What are the roles to remove on jailing? (Type `skip` to skip)**");
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
            
            return new RoleManageModel(rolesToAdd.ToImmutableHashSet(), rolesToRemove.ToImmutableHashSet());
        } 
                
                private async Task<Result<bool>> GetShouldJailBeLogged()
                {
                    await SendChannelMessage("**Should the jail command be logged? (y/n)**");
                    return await GetBool()
                        .DoAwait(async x =>
                        {
                            await SendChannelMessage(x?"> **Jail command will be logged.**":"> **Jail command will not be logged.**");
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

        private Predicate<SocketMessage> CheckUserAndChannelForMessage() => CheckUserAndChannelForMessage(_ => true);

        private Predicate<SocketMessage> CheckUserAndChannelForMessage(Predicate<SocketMessage> filter)
        {
            return x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id && filter(x);
        }
    }
}