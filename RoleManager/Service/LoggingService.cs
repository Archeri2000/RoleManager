using System;
using System.Threading.Tasks;
using Discord;

namespace RoleManager.Service
{
    public class LoggingService : ILoggingService
    {
        public Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}