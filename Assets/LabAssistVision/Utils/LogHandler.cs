using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LogHandler : ILogHandler
{
    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame stackFrame = stackTrace.GetFrame(2);
        Type reflectedType = stackFrame.GetMethod().ReflectedType;
        if (reflectedType?.Name == "LoggerExtensions") // TODO: Check comparison
        {
            stackFrame = stackTrace.GetFrame(3);
            reflectedType = stackFrame.GetMethod().ReflectedType;
        }
        string className = reflectedType != null ? reflectedType.Name : "n/a";
        int threadId = Thread.CurrentThread.ManagedThreadId;
        format = $"[{threadId}] ({className}) {format}";

        Debug.unityLogger.logHandler.LogFormat(logType, context, format, args);
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        Debug.unityLogger.LogException(exception, context);
    }
}

public static class LoggerExtensions
{
    public static void LogWarning(this Logger logger, string message)
    {
        logger.LogFormat(LogType.Warning, null, "{0}", message);
    }
    public static void LogError(this Logger logger, string message)
    {
        logger.LogFormat(LogType.Error, null, "{0}", message);
    }
}