using System.Text.RegularExpressions;
using SamWinOptimize.Models;

namespace SamWinOptimize.Services;

public static partial class CommandOutputRedactor
{
    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var redacted = string.IsNullOrWhiteSpace(userProfile)
            ? value
            : value.Replace(userProfile, "<USER_PROFILE>", StringComparison.OrdinalIgnoreCase);
        redacted = EmailRegex().Replace(redacted, "<EMAIL>");
        redacted = BearerTokenRegex().Replace(redacted, "$1<REDACTED>");
        redacted = NamedSecretRegex().Replace(redacted, "$1<REDACTED>");
        redacted = ProductKeyRegex().Replace(redacted, "*****-*****-*****-*****-$2");
        return redacted;
    }

    public static CommandResult Redact(CommandResult result) => result with
    {
        Output = Redact(result.Output),
        Error = Redact(result.Error)
    };

    [GeneratedRegex(@"(?i)\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?i)(\bBearer\s+)[A-Z0-9._~+/=-]+", RegexOptions.CultureInvariant)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"(?i)(\b(?:password|passwd|pwd|access[_-]?token|refresh[_-]?token|client[_-]?secret|api[_-]?key|authorization)\s*[:=]\s*)[^\s,;]+", RegexOptions.CultureInvariant)]
    private static partial Regex NamedSecretRegex();

    [GeneratedRegex(@"(?i)\b(?:([A-Z0-9]{5})-){4}([A-Z0-9]{5})\b", RegexOptions.CultureInvariant)]
    private static partial Regex ProductKeyRegex();
}
