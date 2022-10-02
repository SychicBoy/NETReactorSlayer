using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class ExistingNames
    {
        public void Add(string name) => _allNames[name] = true;

        public bool Exists(string name) => _allNames.ContainsKey(name);

        public string GetName(UTF8String oldName, INameCreator nameCreator) =>
            GetName(UTF8String.ToSystemStringOrEmpty(oldName), nameCreator);

        public string GetName(string oldName, INameCreator nameCreator) => GetName(oldName, nameCreator.Create);

        public string GetName(UTF8String oldName, Func<string> createNewName) =>
            GetName(UTF8String.ToSystemStringOrEmpty(oldName), createNewName);

        public string GetName(string oldName, Func<string> createNewName)
        {
            string prevName = null;
            while (true)
            {
                var name = createNewName();
                if (name == prevName)
                    throw new ApplicationException($"Could not rename symbol to {Utils.ToCsharpString(name)}");

                if (!Exists(name) || name == oldName)
                {
                    _allNames[name] = true;
                    return name;
                }

                prevName = name;
            }
        }

        public void Merge(ExistingNames other)
        {
            if (this == other)
                return;
            foreach (var key in other._allNames.Keys)
                _allNames[key] = true;
        }

        private readonly Dictionary<string, bool> _allNames = new Dictionary<string, bool>(StringComparer.Ordinal);
    }
}