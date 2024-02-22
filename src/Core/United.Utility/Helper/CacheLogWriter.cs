using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Utility.Serilog;

namespace United.Utility.Helper
{
    public class CacheLogWriter
    {
        private ILogger<CacheLogWriter> _logger;
        private readonly List<LogModel> _SavedObject;

        public CacheLogWriter(ILogger<CacheLogWriter> logger)
        {
            _logger = logger;
            _SavedObject = new List<LogModel>();
        }

        public void AddLog(LogModel log)
        {
            _SavedObject.Add(log);
        }

        public void LogInformation(string messageTemplate, params object[] values)
        {
            _logger.LogInformation(messageTemplate, values ?? null);
        }

        public void LogError(string messageTemplate, params object[] values)
        {
            _logger.LogError(messageTemplate, values ?? null);
        }

        public void LogWarning(string messageTemplate, params object[] values)
        {
            _logger.LogWarning(messageTemplate, values ?? null);
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
            return null;
            //return _logger.BeginTimedOperation(description, transationId, level, warnIfExceeds, levelExceeds, beginningMessage, completedMessage, exceededOperationMessage, propertyValues);
        }

        public void SaveToLogs()
        {
            if (_SavedObject != null && _SavedObject.Any(_ => _.LogType == LogType.ERROR || _.LogType == LogType.WARNING))
            {
                foreach (var _tempObj in _SavedObject)
                {
                    try
                    {
                        switch (_tempObj.LogType)
                        {
                            case LogType.INFORMATION:
                                _logger.LogInformation( _tempObj.MessageTemplate, _tempObj.Values ?? null);
                                break;
                            case LogType.WARNING:
                                _logger.LogWarning(_tempObj.MessageTemplate, _tempObj.Values ?? null);
                                break;
                            case LogType.ERROR:
                                _logger.LogError(_tempObj.MessageTemplate, _tempObj.Values ?? null);
                                break;
                            default:
                                _logger.LogInformation(_tempObj.MessageTemplate, _tempObj.Values ?? null);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error in adding the list item" + _tempObj.MessageTemplate, ex);
                    }
                }
            }
            _SavedObject.Clear();
        }
    }
}