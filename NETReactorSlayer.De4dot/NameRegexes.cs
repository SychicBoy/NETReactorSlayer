/*
    Copyright (C) 2011-2015 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;

namespace NETReactorSlayer.De4dot;

public class NameRegexes
{
    public NameRegexes() : this("")
    {
    }

    public NameRegexes(string regex)
    {
        Set(regex);
    }

    public void Set(string regexesString)
    {
        Regexes = new List<NameRegex>();
        if (regexesString != "")
            foreach (var regex in regexesString.Split(RegexSeparatorChar))
                Regexes.Add(new NameRegex(regex));
    }

    public bool IsMatch(string s)
    {
        foreach (var regex in Regexes)
            if (regex.IsMatch(s))
                return regex.MatchValue;

        return DefaultValue;
    }

    public override string ToString()
    {
        var s = "";
        for (var i = 0; i < Regexes.Count; i++)
        {
            if (i > 0)
                s += RegexSeparatorChar;
            s += Regexes[i].ToString();
        }

        return s;
    }

    public const char RegexSeparatorChar = '&';

    public bool DefaultValue { get; set; }
    public IList<NameRegex> Regexes { get; private set; }
}