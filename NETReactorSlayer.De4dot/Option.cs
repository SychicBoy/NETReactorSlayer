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

namespace NETReactorSlayer.De4dot;

public abstract class Option
{
    protected Option(string shortName, string longName, string description)
    {
        if (shortName != null)
            ShortName = ShortnamePrefix + shortName;
        if (longName != null)
            LongName = LongnamePrefix + longName;
        Description = description;
    }

    public abstract bool Set(string val, out string error);
    private const string LongnamePrefix = "--";
    private const string ShortnamePrefix = "-";
    public object Default { get; protected set; }
    public string Description { get; }
    public string LongName { get; }

    public string ShortName { get; }
}