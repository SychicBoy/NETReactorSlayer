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

namespace NETReactorSlayer.De4dot.Renamer {
    public class PropertyNameCreator : TypeNames {
        public PropertyNameCreator() {
            FullNameToShortName = OurFullNameToShortName;
            FullNameToShortNamePrefix = OurFullNameToShortNamePrefix;
        }

        protected override string FixName(string prefix, string name) => prefix.ToUpperInvariant() + UpperFirst(name);

        private static readonly Dictionary<string, string> OurFullNameToShortName = new(StringComparer.Ordinal);

        private static readonly Dictionary<string, string> OurFullNameToShortNamePrefix = new(StringComparer.Ordinal);
    }
}