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

namespace NETReactorSlayer.De4dot
{
    public class NameRegexOption : Option
    {
        public NameRegexOption(string shortName, string longName, string description, string val)
            : base(shortName, longName, description) =>
            Default = _val = new NameRegexes(val);

        public override bool Set(string newVal, out string error)
        {
            try
            {
                var regexes = new NameRegexes();
                regexes.Set(newVal);
                _val = regexes;
            }
            catch (ArgumentException)
            {
                error = $"Could not parse regex '{newVal}'";
                return false;
            }

            error = "";
            return true;
        }

        public NameRegexes Get() => _val;

        private NameRegexes _val;
    }
}