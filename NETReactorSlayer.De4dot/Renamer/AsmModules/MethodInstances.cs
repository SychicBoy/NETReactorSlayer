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

using System.Collections.Generic;
using de4dot.blocks;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules {
    public class MethodInstances {
        public void InitializeFrom(MethodInstances other, GenericInstSig git) {
            foreach (var list in other._methodInstances.Values)
            foreach (var methodInst in list) {
                var newMethod = GenericArgsSubstitutor.Create(methodInst.MethodRef, git);
                Add(new MethodInst(methodInst.OrigMethodDef, newMethod));
            }
        }

        public void Add(MethodInst methodInst) {
            var key = methodInst.MethodRef;
            if (methodInst.OrigMethodDef.IsNewSlot() || !_methodInstances.TryGetValue(key, out var list))
                _methodInstances[key] = list = new List<MethodInst>();
            list.Add(methodInst);
        }

        public List<MethodInst> Lookup(IMethodDefOrRef methodRef) {
            _methodInstances.TryGetValue(methodRef, out var list);
            return list;
        }

        public IEnumerable<List<MethodInst>> GetMethods() => _methodInstances.Values;

        private readonly Dictionary<IMethodDefOrRef, List<MethodInst>> _methodInstances =
            new(MethodEqualityComparer.DontCompareDeclaringTypes);
    }
}