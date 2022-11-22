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
using System.Linq;

namespace NETReactorSlayer.Core.Helper {
    public class StringCounts {
        public void Add(string s) {
            _strings.TryGetValue(s, out var count);
            _strings[s] = count + 1;
        }

        public bool Exists(string s) => s != null && _strings.ContainsKey(s);

        public bool All(IList<string> list) => list.All(Exists);

        public int Count(string s) {
            _strings.TryGetValue(s, out var count);
            return count;
        }

        private readonly Dictionary<string, int> _strings = new(StringComparer.Ordinal);
    }
}