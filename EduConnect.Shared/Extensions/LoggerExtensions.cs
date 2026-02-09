using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Runtime.CompilerServices;

namespace EduConnect.Shared.Extensions;

/// <summary>
/// Standard logging extensions that push Method and LineNumber into Serilog LogContext.
/// All message parameters are passed through <see cref="RedactCredentials"/> — never log credentials.
/// </summary>
public static class LoggerExtensions
{
    private static readonly string[] CredentialKeys = ["password", "token", "secret", "authorization", "credential", "apikey", "api_key"];

    /// <summary>
    /// Redacts common credential-like substrings from a message. Use for any user-provided or request-derived log message.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RedactCredentials(string? message)
    {
        if (string.IsNullOrEmpty(message)) return message ?? string.Empty;
        foreach (var key in CredentialKeys)
        {
            int i;
            while ((i = message.IndexOf(key, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                var start = i;
                var end = i + key.Length;
                while (end < message.Length && (char.IsLetterOrDigit(message[end]) || message[end] == '_' || message[end] == '=' || message[end] == ':')) end++;
                message = message.Remove(start, end - start).Insert(start, "[REDACTED]");
            }
        }
        return message;
    }

    public static void ErrorLog(
        this ILogger logger,
        Exception ex,
        string message = "Exception at",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        using (LogContext.PushProperty("Method", memberName))
        using (LogContext.PushProperty("LineNumber", sourceLineNumber))
        {
            logger.LogError(ex, RedactCredentials(message));
        }
    }

    public static void ErrorLog(
        this ILogger logger,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        using (LogContext.PushProperty("Method", memberName))
        using (LogContext.PushProperty("LineNumber", sourceLineNumber))
        {
            logger.LogError(RedactCredentials(message));
        }
    }

    public static void InformationLog(
        this ILogger logger,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        using (LogContext.PushProperty("Method", memberName))
        using (LogContext.PushProperty("LineNumber", sourceLineNumber))
        {
            logger.LogInformation(RedactCredentials(message));
        }
    }

    public static void WarningLog(
        this ILogger logger,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        using (LogContext.PushProperty("Method", memberName))
        using (LogContext.PushProperty("LineNumber", sourceLineNumber))
        {
            logger.LogWarning(RedactCredentials(message));
        }
    }

    public static void DebugLog(
        this ILogger logger,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        using (LogContext.PushProperty("Method", memberName))
        using (LogContext.PushProperty("LineNumber", sourceLineNumber))
        {
            logger.LogDebug(RedactCredentials(message));
        }
    }
}
