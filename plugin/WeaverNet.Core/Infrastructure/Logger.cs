using System;
using System.IO;
using System.Runtime.CompilerServices;
using WeaverNet.Core.Infrastructure.Interfaces;

public static class PluginLog
{
    private static IPluginLogger _logger;

    public static void Bind(IPluginLogger logger)
    {
        if (_logger != null)
            throw new InvalidOperationException("Logger already bound");

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static void Info(
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        Write(
            LogLevel.Info,
            message,
            member,
            file);
    }

    public static void Warning(
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        Write(
            LogLevel.Warning,
            message,
            member,
            file);
    }

    public static void Error(
        string message,
        Exception exception = null,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        string source =
            $"{Path.GetFileNameWithoutExtension(file)}.{member}:{line}";

        _logger.Error($"[{source}] {message}", exception);
    }

    private static void Write(
        LogLevel level,
        string message,
        string member,
        string file)
    {
        if (_logger == null)
            return;

        string source =
            $"{System.IO.Path.GetFileNameWithoutExtension(file)}.{member}";

        string formatted =
            $"[{source}] {message}";

        switch (level)
        {
            case LogLevel.Info:
                _logger.Info(formatted);
                break;

            case LogLevel.Warning:
                _logger.Warning(formatted);
                break;

            case LogLevel.Error:
                _logger.Error(formatted);
                break;
        }
    }

    private enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}