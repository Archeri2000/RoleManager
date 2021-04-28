using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RoleManager.Model;
using RoleManager.Service;

namespace RoleManager.Utils
{
    public static class RoleUtils
    {
        public static Func<RoleManageModel, RoleManageDomain> 
            MapRoles(IGuild guild, IGuildUser user)
        {
            return x => new RoleManageDomain(x.ToAdd.MapToGuildRoles(guild), user.GetMatchingRoles(x.ToRemove).MapToGuildRoles(guild));
        }

        public static ImmutableList<ulong> GetMatchingRoles(this IGuildUser user,
            IEnumerable<ulong> roles)
        {
            return user.RoleIds.Where(x => roles.Contains(x)).ToImmutableList();
        }

        public static ImmutableList<IRole> MapToGuildRoles(this IEnumerable<ulong> roles,
            IGuild guild)
        {
            return roles.Select(guild.GetRole).ToImmutableList();
        }

        public static async Task<RoleUpdateEvent> EditRoles(this IGuildUser user, RoleManageDomain rolesToChange)
        {
            while (true)
            {
                try
                {
                    await user.RemoveRolesAsync(rolesToChange.ToRemove);
                    await user.AddRolesAsync(rolesToChange.ToAdd);
                    break;
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
            }

            return new RoleUpdateEvent(user, rolesToChange);
        }

        public static string MapToString(this List<string> strings)
        {
            return strings.Aggregate((x, y) => x + ", " + y);
        }

        public static string Stringify(this IEnumerable<IRole> roles)
        {
            var socketRoles = roles.ToList();
            if (!socketRoles.Any()) return "";
            return socketRoles.Select(x => x.Name).Aggregate((acc, role) => acc + ", " + role);
        }
    }

    public static class ResultUtils
    {
        public static Result<TOut> BranchingIf<TIn, TOut>(this Result<TIn> result, Func<TIn, bool> predicate, Func<TIn, Result<TOut>> Then,
            Func<TIn, Result<TOut>> Else)
        {
            return result.Match(
                Success: x => predicate(x) ? Then(x) : Else(x),
                Failure: x => new Failure<TOut>(x));
        }
        
        public static Task<Result<TOut>> BranchingIf<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, bool> predicate, Func<TIn, Result<TOut>> Then,
            Func<TIn, Result<TOut>> Else)
        {
            return result.Match(
                Success: x => predicate(x) ? Then(x) : Else(x),
                Failure: x => new Failure<TOut>(x));
        }
        
        public static Task<Result<TOut>> BranchingIfAwait<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, bool> predicate, Func<TIn, Task<Result<TOut>>> Then,
            Func<TIn, Task<Result<TOut>>> Else)
        {
            return result.MatchAwait(
                Success: x => predicate(x) ? Then(x) : Else(x),
                Failure: x => Task.FromResult((Result<TOut>)x));
        }
    }

}