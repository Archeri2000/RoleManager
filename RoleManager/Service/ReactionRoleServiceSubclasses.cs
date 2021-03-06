using System.Collections.Concurrent;
using System.Collections.Generic;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Service
{
public class GuildReactionRole
    {
        private ConcurrentDictionary<ulong, ChannelReactionRole> _reactionChannels;
        
        public GuildReactionRole(ConcurrentDictionary<ulong, ChannelReactionRole> reactionChannels)
        {
            _reactionChannels = reactionChannels;
        }

        public GuildReactionRole()
        {
            _reactionChannels = new();
        }

        public bool UpsertReactionMessage(ulong channelId, ulong messageId, ReactionRuleModelBase rule)
        {
            if (!_reactionChannels.TryGetValue(channelId, out var channel))
            {
                channel = new ChannelReactionRole();
                _reactionChannels.TryAdd(channelId, channel);
            }
            return channel.UpsertReactionMessage(messageId, rule);
        }
        
        public bool DeleteReactionMessage(ulong channelId, ulong messageId)
        {
            if (!_reactionChannels.TryGetValue(channelId, out var channel))
            {
                return true;
            }
            return channel.DeleteReactionMessage(messageId);
        }

        public Result<ChannelReactionRole> TryGetChannel(ulong channelId)
        {
            if (_reactionChannels.TryGetValue(channelId, out var channel))
            {
                return channel;
            }

            return new KeyNotFoundException($"Channel {channelId} does not have a Reaction Role!");
        }
    }

    public class ChannelReactionRole
    {
        private ConcurrentDictionary<ulong, ReactionRuleModelBase> _reactionMessages;

        public ChannelReactionRole()
        {
            _reactionMessages = new();
        }
        public ChannelReactionRole(ConcurrentDictionary<ulong, ReactionRuleModelBase> reactionMessages)
        {
            _reactionMessages = reactionMessages;
        }

        public bool UpsertReactionMessage(ulong messageId, ReactionRuleModelBase rule)
        {
            _reactionMessages[messageId] = rule;
            return true;        
        }
        
        public bool DeleteReactionMessage(ulong messageId)
        {
            _reactionMessages.Remove(messageId, out _);
            return true;        
        }
        
        public Result<ReactionRuleModelBase> TryGetMessage(ulong messageId)
        {
            if (_reactionMessages.TryGetValue(messageId, out var rule))
            {
                return rule.ToResult();
            }

            return new KeyNotFoundException($"Message {messageId} does not have a Reaction Role!");
        }
    }
}