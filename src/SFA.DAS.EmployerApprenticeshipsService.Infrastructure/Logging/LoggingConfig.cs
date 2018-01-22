﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure;
using NLog;
using NLog.Config;

namespace SFA.DAS.EAS.Infrastructure.Logging
{
    public static class LoggingConfig
    {
        public static void ConfigureLogging()
        {
            var loggingLevels = LogLevel.AllLoggingLevels.ToList();
            var minLevel = GetLogLevelFromConfigurationManager();
            var levelIndex = loggingLevels.IndexOf(minLevel);

            foreach (var rule in LogManager.Configuration?.LoggingRules ?? new List<LoggingRule>())
            {
                for (var i = 0; i < loggingLevels.Count; i++)
                {
                    var level = loggingLevels[i];
                    var hasLevel = rule.IsLoggingEnabledForLevel(level);
                    if (i < levelIndex && hasLevel)
                    {
                        rule.DisableLoggingForLevel(level);
                    }
                    else if (i >= levelIndex && !hasLevel)
                    {
                        rule.EnableLoggingForLevel(level);
                    }
                }
            }

            LogManager.ReconfigExistingLoggers();
        }

        private static LogLevel GetLogLevelFromConfigurationManager()
        {
            var settingValue = CloudConfigurationManager.GetSetting("LogLevel");
            return LogLevel.FromString(settingValue ?? "DEBUG");
        }
    }
}
