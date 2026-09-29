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

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MethodNameGroups
    {
        public void Same(MMethodDef a, MMethodDef b) => Merge(Get(a), Get(b));

        public void Add(MMethodDef methodDef) => Get(methodDef);

        public MethodNameGroup Get(MMethodDef method)
        {
            if (!method.IsVirtual())
                throw new ApplicationException("Not a virtual method");
            if (_methodGroups.TryGetValue(method, out var group))
                return group;
            _methodGroups[method] = group = new MethodNameGroup();
            group.Add(method);

            return group;
        }

        private void Merge(MethodNameGroup a, MethodNameGroup b)
        {
            if (a == b)
                return;

            if (a.Count < b.Count)
                (a, b) = (b, a);

            a.Merge(b);
            foreach (var methodDef in b.Methods)
                _methodGroups[methodDef] = a;
        }

        public IEnumerable<MethodNameGroup> GetAllGroups() => Utils.Unique(_methodGroups.Values);

        private readonly Dictionary<MMethodDef, MethodNameGroup> _methodGroups = new();
    }
}