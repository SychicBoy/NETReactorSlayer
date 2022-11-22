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

using System.Text.RegularExpressions;

namespace NETReactorSlayer.De4dot {
    public class NameRegex {
        public NameRegex(string regex) {
            if (regex.Length > 0 && regex[0] == InvertChar) {
                regex = regex.Substring(1);
                MatchValue = false;
            } else
                MatchValue = true;

            _regex = new Regex(regex);
        }

        public bool IsMatch(string s) => _regex.IsMatch(s);

        public override string ToString() {
            if (!MatchValue)
                return InvertChar + _regex.ToString();
            return _regex.ToString();
        }

        private readonly Regex _regex;

        public const char InvertChar = '!';

        public bool MatchValue { get; }
    }
}