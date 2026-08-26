using System.Text;
using System.Text.RegularExpressions;

namespace DotnetAPI.Services;

public static partial class SlugService
{
    public static string Create(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var character in normalized)
        {
            if (char.GetUnicodeCategory(character) == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }
            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        }

        return MultipleDashes().Replace(builder.ToString(), "-").Trim('-');
    }

    [GeneratedRegex("-+")]
    private static partial Regex MultipleDashes();
}
