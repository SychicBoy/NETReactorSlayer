/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NETReactorSlayer.
    NETReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NETReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NETReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer {
    public class DerivedFrom {
        public DerivedFrom(IEnumerable<string> classNames) {
            foreach (var className in classNames)
                AddName(className);
        }

        private void AddName(string className) => _classNames[className] = true;

        public bool Check(MTypeDef type) => Check(type, 0);

        public bool Check(MTypeDef type, int recurseCount) {
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

        private readonly Dictionary<string, bool> _classNames = new(StringComparer.Ordinal);
        private readonly Dictionary<MTypeDef, bool> _results = new();
    }
}