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
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Helper.De4dot;

public class StringCounts
{
    private readonly Dictionary<string, int> strings = new(StringComparer.Ordinal);

    public void Add(string s)
    {
        strings.TryGetValue(s, out var count);
        strings[s] = count + 1;
    }

    public bool Exists(string s) => s != null && strings.ContainsKey(s);

    public bool All(IList<string> list) => list.All(Exists);

    public int Count(string s)
    {
        strings.TryGetValue(s, out var count);
        return count;
    }
}

public class FieldTypes : StringCounts
{
    public FieldTypes(IEnumerable<FieldDef> fields) => Initialize(fields);

    private void Initialize(IEnumerable<FieldDef> fields)
    {
        if (fields == null)
            return;
        foreach (var field in fields)
        {
            var type = field.FieldSig.GetFieldType();
            if (type != null)
                Add(type.FullName);
        }
    }
}

public class LocalTypes : StringCounts
{
    public LocalTypes(MethodDef method)
    {
        if (method is {Body: { }})
            Initialize(method.Body.Variables);
    }

    private void Initialize(IEnumerable<Local> locals)
    {
        if (locals == null)
            return;
        foreach (var local in locals)
            Add(local.Type.FullName);
    }
}