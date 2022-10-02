using System;
using System.Collections.Generic;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class PropertyNameCreator : TypeNames
    {
        public PropertyNameCreator()
        {
            FullNameToShortName = OurFullNameToShortName;
            FullNameToShortNamePrefix = OurFullNameToShortNamePrefix;
        }

        protected override string FixName(string prefix, string name) => prefix.ToUpperInvariant() + UpperFirst(name);

        private static readonly Dictionary<string, string> OurFullNameToShortName =
            new Dictionary<string, string>(StringComparer.Ordinal);

        private static readonly Dictionary<string, string> OurFullNameToShortNamePrefix =
            new Dictionary<string, string>(StringComparer.Ordinal);
    }
}