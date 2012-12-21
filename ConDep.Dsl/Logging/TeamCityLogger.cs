using System;
using System.Text;
using log4net;
using log4net.Core;

namespace ConDep.Dsl.Logging
{
    public enum TeamCityMessageStatus
    {
        NORMAL,
        WARNING,
        FAILURE,
        ERROR
    }

    public class TeamCityLogger : LoggerBase
    {
        private readonly ILog _serviceMessageLogger;

        public TeamCityLogger(ILog log) : base(log)
        {
            _serviceMessageLogger = LogManager.GetLogger("condep-teamcity-servicemessage");
        }

        public override void Warn(string message, params object[] formatArgs)
        {
            TeamCityMessage(message, null, TeamCityMessageStatus.WARNING, formatArgs);
        }

        public override void Warn(string message, Exception ex, params object[] formatArgs)
        {
            TeamCityMessage(message, null, TeamCityMessageStatus.WARNING, formatArgs);
        }

        public override void Error(string message, params object[] formatArgs)
        {
            TeamCityMessage(message, null, TeamCityMessageStatus.ERROR, formatArgs);
        }

        public override void Error(string message, Exception ex, params object[] formatArgs)
        {
            TeamCityMessage(message, ex, TeamCityMessageStatus.ERROR, formatArgs);
        }

        public override void LogSectionStart(string name)
        {
            TeamCityBlockStart(name);
            TeamCityProgressMessage(name);
        }

        public override void LogSectionEnd(string name)
        {
            TeamCityBlockEnd(name);
        }

        private void TeamCityMessage(string message, Exception ex, TeamCityMessageStatus status, params object[] formatArgs)
        {
            var formattedMessage = (formatArgs != null && formatArgs.Length > 0) ? string.Format(message, formatArgs) : message;
            var sb = new StringBuilder(formattedMessage);
            sb.Replace("|", "||")
                .Replace("'", "|'")
                .Replace("\n", "|n")
                .Replace("\r", "|r")
                .Replace("\u0085", "|x")
                .Replace("\u2028", "|l")
                .Replace("\u2029", "|p")
                .Replace("[", "|[")
                .Replace("]", "|]");

            var errorDetails = "";
            if(ex != null)
            {
                errorDetails = string.Format("Message: {0}\n\nStack Trace:\n{1}", ex.Message, ex.StackTrace);
            }

            var tcMessage = string.Format("##teamcity[message text='{0}' errorDetails='{1}' status='{2}']", sb, errorDetails, status);
            _serviceMessageLogger.Logger.Log(typeof(Logger), Level.All, tcMessage, null);
        }

        private void TeamCityBlockStart(string name)
        {
            _serviceMessageLogger.Logger.Log(typeof(Logger), Level.All, string.Format("##teamcity[blockOpened name='{0}']", name), null);
        }

        private void TeamCityBlockEnd(string name)
        {
            _serviceMessageLogger.Logger.Log(typeof(Logger), Level.All, string.Format("##teamcity[blockClosed name='{0}']", name), null);
        }

        private void TeamCityProgressMessage(string message)
        {
            _serviceMessageLogger.Logger.Log(typeof(Logger), Level.All, string.Format("##teamcity[progressMessage '{0}']", message), null);
        }

    }
}