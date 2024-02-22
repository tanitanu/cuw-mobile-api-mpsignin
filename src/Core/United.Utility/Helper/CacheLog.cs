using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using United.Utility.Serilog;

namespace United.Utility.Helper
{
    public enum LogType { INFORMATION, ERROR, WARNING }
    public class CacheLog<T> : ICacheLog<T> where T : class
    {
        private readonly CacheLogWriter _cacheLogWriter;
        private readonly IConfiguration _configuration;

        public CacheLog(CacheLogWriter cacheLogWriter, IConfiguration configuration)
        {
            _configuration = configuration;
            _cacheLogWriter = cacheLogWriter;
        }
        public void LogInformation(string messageTemplate, params object[] values)
        {
            if (!string.IsNullOrEmpty("messageTemplate"))
            {
                if (!_configuration.GetValue<bool>("SaveLogsLater"))
                {
                    _cacheLogWriter.LogInformation(messageTemplate, values);
                }
                else
                {
                    var logRecord = new LogModel { LogType = LogType.INFORMATION, MessageTemplate = messageTemplate, Values = values };
                    _cacheLogWriter.AddLog(logRecord);
                }
            }
        }

        public void LogError(string messageTemplate, params object[] values)
        {
            if (!string.IsNullOrEmpty("messageTemplate"))
            {
                if (!_configuration.GetValue<bool>("SaveLogsLater"))
                {
                    _cacheLogWriter.LogError(messageTemplate, values);
                }
                else
                {
                    var logRecord = new LogModel { LogType = LogType.ERROR, MessageTemplate = messageTemplate, Values = values };
                    _cacheLogWriter.AddLog(logRecord);
                }
            }
        }

        public void LogWarning(string messageTemplate, params object[] values)
        {
            if (!string.IsNullOrEmpty("messageTemplate"))
            {
                if (!_configuration.GetValue<bool>("SaveLogsLater"))
                {
                    _cacheLogWriter.LogWarning(messageTemplate, values);
                }
                else
                {
                    var logRecord = new LogModel { LogType = LogType.WARNING, MessageTemplate = messageTemplate, Values = values };
                    _cacheLogWriter.AddLog(logRecord);
                }
            }
        }

        public IDisposable BeginTimedOperation(
             string description,
             string transationId = null,
             LogLevel level = LogLevel.Information,
             TimeSpan? warnIfExceeds = null,
             LogLevel levelExceeds = LogLevel.Warning,
             string beginningMessage = TimedOperation.BeginningOperationTemplate, string completedMessage = TimedOperation.CompletedOperationTemplate, string exceededOperationMessage = TimedOperation.OperationExceededTemplate,
             params object[] propertyValues)
        {
            return _cacheLogWriter.BeginTimedOperation(description, transationId, level, warnIfExceeds, levelExceeds, beginningMessage, completedMessage, exceededOperationMessage, propertyValues);
        }
    }
    public class LogModel
    {
        public LogType LogType { get; set; }
        public string MessageTemplate { get; set; }
        public object[] Values { get; set; }
    }
}
