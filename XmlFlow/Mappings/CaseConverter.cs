using System.Text.RegularExpressions;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Mappings;

public class CaseConverter : ICaseConverter
{
    private readonly LetterCase _letterCase;
    private readonly CaseType _caseType;

    public CaseConverter(LetterCase letterCase, CaseType caseType)
    {
        _letterCase = letterCase;
        _caseType = caseType;
    }

    public string Convert(string? input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        
        return _caseType switch
        {
            CaseType.Snake => ToSeparatedCase(input, "_"),
            CaseType.Kebab => ToSeparatedCase(input, "-"),
            CaseType.Camel => ToCamelPascalCase(input, firstWordLower: true),
            CaseType.Pascal => ToCamelPascalCase(input, firstWordLower: false),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private string ToSeparatedCase(string input, string separator)
    {
        var words = GetWords(input);
        var joined = string.Join(separator, words);
        return ApplyLetterCase(joined);
    }

    private string ToCamelPascalCase(string input, bool firstWordLower)
    {
        var words = GetWords(input);
        
        if (words.Length is 0)
            return string.Empty;

        var firstWord = firstWordLower ? ApplyLetterCase(words[0], LetterCase.Lower)
                                       : ApplyLetterCase(words[0], LetterCase.Upper);

        var restWords = words.Skip(1)
                             .Select(word => ApplyLetterCase(word, LetterCase.Upper));

        return firstWord + string.Join("", restWords);
    }

    private string[] GetWords(string input)
    {
        return Regex.Matches(input, @"[A-Z]?[a-z]+|\d+|[A-Z]+(?=[A-Z][a-z]|$)")
                    .Select(m => m.Value.ToLower())
                    .ToArray();
    }

    private string ApplyLetterCase(string value, LetterCase? letterCase = null)
    {
        letterCase ??= _letterCase;
        
        return letterCase switch
        {
            LetterCase.Lower => value.ToLowerInvariant(),
            LetterCase.Upper => value.ToUpperInvariant(),
            _ => value,
        };
    }
}