using Microsoft.Extensions.Logging;
using System;
using United.Utility.Serilog;

namespace United.Utility.Helper
{
    public interface ICacheLog
    {
        void LogInformation(string messageTemplate, params object[] Values);
        void LogError(string messageTemplate, params object[] Values);
        void LogWarning(string messageTemplate, params object[] Values);
        IDisposable BeginTimedOperation(
             string description,
             string transationId = null,
             LogLevel level = LogLevel.Information,
             TimeSpan? warnIfExceeds = null,
             LogLevel levelExceeds = LogLevel.Warning,
             string beginningMessage = TimedOperation.BeginningOperationTemplate, string completedMessage = TimedOperation.CompletedOperationTemplate, string exceededOperationMessage = TimedOperation.OperationExceededTemplate,
             params object[] propertyValues);
    }


    public interface ICacheLog<out T> : ICacheLog
    {

    }
}