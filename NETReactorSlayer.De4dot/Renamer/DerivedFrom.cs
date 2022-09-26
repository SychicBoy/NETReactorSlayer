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

using System;
using System.Collections.Generic;
using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer;

public class DerivedFrom
{
    public DerivedFrom(string className)
    {
        AddName(className);
    }

    public DerivedFrom(string[] classNames)
    {
        foreach (var className in classNames)
            AddName(className);
    }

    private void AddName(string className)
    {
        _classNames[className] = true;
    }

    public bool Check(MTypeDef type)
    {
        return Check(type, 0);
    }

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
        {
            if (type.TypeDef.BaseType != null)
                val = _classNames.ContainsKey(type.TypeDef.BaseType.FullName);
            else
                val = false;
        }
        else
            val = Check(type.BaseType.TypeDef, recurseCount + 1);

        _results[type] = val;
        return val;
    }

    private readonly Dictionary<string, bool> _classNames = new(StringComparer.Ordinal);
    private readonly Dictionary<MTypeDef, bool> _results = new();
}