using System;
using System.Threading.Tasks;
using Discord;

namespace RoleManager.Service
{
    public interface ILoggingService
    {
        public Task Log(LogMessage msg);
    }

    public static class ILoggingServiceExtensions
    {
        public static async Task Info(this ILoggingService log, string msg, string source="", Exception err=null)
        {
            await log.Log(new LogMessage(LogSeverity.Info, source, msg, err));
        }
        public static async Task Warn(this ILoggingService log, string msg, string source="", Exception err=null)
        {
            await log.Log(new LogMessage(LogSeverity.Warning, source, msg, err));
        }
        public static async Task Error(this ILoggingService log, string msg, string source="", Exception err=null)
        {
            await log.Log(new LogMessage(LogSeverity.Error, source, msg, err));
        }
        public static async Task Fatal(this ILoggingService log, string msg, string source="", Exception err=null)
        {
            await log.Log(new LogMessage(LogSeverity.Critical, source, msg, err));
        }
        public static async Task Debug(this ILoggingService log, string msg, string source="", Exception err=null)
        {
            await log.Log(new LogMessage(LogSeverity.Debug, source, msg, err));
        }

        public static async Task Verbose(this ILoggingService log, string msg, string source = "", Exception err = null)
        {
            await log.Log(new LogMessage(LogSeverity.Verbose, source, msg, err));
        }
    }

    public record SourcedLoggingService(ILoggingService Logger, string Source)
    {
        public async Task Info(string msg, Exception err = null)
        {
            await Logger.Info(msg, Source, err);
        }
        public async Task Warn(string msg,  Exception err = null)
        {
            await Logger.Warn(msg, Source, err);
        }
        public async Task Error(string msg,  Exception err = null)
        {
            await Logger.Error(msg, Source, err);
        }
        public async Task Fatal(string msg,  Exception err = null)
        {
            await Logger.Fatal(msg, Source, err);
        }
        public async Task Debug(string msg, Exception err = null)
        {
            await Logger.Debug(msg, Source, err);
        }

        public async Task Verbose(string msg, Exception err = null)
        {
            await Logger.Verbose(msg, Source, err);
        }
    }
}