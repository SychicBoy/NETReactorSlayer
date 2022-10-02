using System;
using System.Collections.Generic;
using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class DerivedFrom
    {
        public DerivedFrom(string[] classNames)
        {
            foreach (var className in classNames)
                AddName(className);
        }

        private void AddName(string className) => _classNames[className] = true;

        public bool Check(MTypeDef type) => Check(type, 0);

        public bool Check(MTypeDef type, int recurseCount)
        {
            if (recurseCount >= 100)
                return false;
            if (_results.ContainsKey(type))
                return _results[type];

            bool val;
            if (_classNames.ContainsKey(type.TypeDef.FullName))
                val = true;
            else if (type.BaseType == null)
                val = type.TypeDef.BaseType != null && _classNames.ContainsKey(type.TypeDef.BaseType.FullName);
            else
                val = Check(type.BaseType.TypeDef, recurseCount + 1);

            _results[type] = val;
            return val;
        }

        private readonly Dictionary<string, bool> _classNames = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly Dictionary<MTypeDef, bool> _results = new Dictionary<MTypeDef, bool>();
    }
}