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

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class MethodNameGroups
{
    public void Same(MMethodDef a, MMethodDef b) => Merge(Get(a), Get(b));

    public void Add(MMethodDef methodDef) => Get(methodDef);

    public MethodNameGroup Get(MMethodDef method)
    {
        if (!method.IsVirtual())
            throw new ApplicationException("Not a virtual method");
        if (!_methodGroups.TryGetValue(method, out var group))
        {
            _methodGroups[method] = group = new MethodNameGroup();
            group.Add(method);
        }

        return group;
    }

    private void Merge(MethodNameGroup a, MethodNameGroup b)
    {
        if (a == b)
            return;

        if (a.Count < b.Count)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        a.Merge(b);
        foreach (var methodDef in b.Methods)
            _methodGroups[methodDef] = a;
    }

    public IEnumerable<MethodNameGroup> GetAllGroups() => Utils.Unique(_methodGroups.Values);

    private readonly Dictionary<MMethodDef, MethodNameGroup> _methodGroups =
        new Dictionary<MMethodDef, MethodNameGroup>();
}