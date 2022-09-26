using System;
using System.Collections.Generic;

namespace NETReactorSlayer.De4dot.Renamer;

public class PropertyNameCreator : TypeNames
{
    public PropertyNameCreator()
    {
        FullNameToShortName = OurFullNameToShortName;
        FullNameToShortNamePrefix = OurFullNameToShortNamePrefix;
    }

    protected override string FixName(string prefix, string name)
    {
        return prefix.ToUpperInvariant() + UpperFirst(name);
    }

    private static readonly Dictionary<string, string> OurFullNameToShortName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> OurFullNameToShortNamePrefix = new(StringComparer.Ordinal);
}