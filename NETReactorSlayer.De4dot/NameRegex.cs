using System.Text.RegularExpressions;

namespace NETReactorSlayer.De4dot;

public class NameRegex
{
    public NameRegex(string regex)
    {
        if (regex.Length > 0 && regex[0] == InvertChar)
        {
            regex = regex.Substring(1);
            MatchValue = false;
        }
        else
            MatchValue = true;

        _regex = new Regex(regex);
    }

    // Returns true if the regex matches. Use MatchValue to get result.
    public bool IsMatch(string s) => _regex.IsMatch(s);

    public override string ToString()
    {
        if (!MatchValue)
            return InvertChar + _regex.ToString();
        return _regex.ToString();
    }

    private readonly Regex _regex;

    public const char InvertChar = '!';

    public bool MatchValue { get; }
}