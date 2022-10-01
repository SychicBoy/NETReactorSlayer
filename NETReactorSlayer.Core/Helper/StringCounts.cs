using System;
using System.Collections.Generic;
using System.Linq;

namespace NETReactorSlayer.Core.Helper;

public class StringCounts
{
    public void Add(string s)
    {
        _strings.TryGetValue(s, out var count);
        _strings[s] = count + 1;
    }

    public bool Exists(string s) => s != null && _strings.ContainsKey(s);

    public bool All(IList<string> list) => list.All(Exists);

    public int Count(string s)
    {
        _strings.TryGetValue(s, out var count);
        return count;
    }

    private readonly Dictionary<string, int> _strings = new(StringComparer.Ordinal);
}