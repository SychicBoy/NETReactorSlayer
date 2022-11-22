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
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules {
    public class InterfaceMethodInfos {
        public void InitializeFrom(InterfaceMethodInfos other, GenericInstSig git) {
            foreach (var pair in other._interfaceMethods) {
                var oldTypeInfo = pair.Value.Face;
                var newTypeInfo = new TypeInfo(oldTypeInfo, git);
                var oldKey = oldTypeInfo.TypeRef;
                var newKey = newTypeInfo.TypeRef;

                var newMethodsInfo = new InterfaceMethodInfo(newTypeInfo, other._interfaceMethods[oldKey]);
                if (_interfaceMethods.ContainsKey(newKey))
                    newMethodsInfo.Merge(_interfaceMethods[newKey]);
                _interfaceMethods[newKey] = newMethodsInfo;
            }
        }

        public void AddInterface(TypeInfo iface) {
            var key = iface.TypeRef;
            if (!_interfaceMethods.ContainsKey(key))
                _interfaceMethods[key] = new InterfaceMethodInfo(iface);
        }

        public MMethodDef AddMethod(TypeInfo iface, MMethodDef ifaceMethod, MMethodDef classMethod) =>
            AddMethod(iface.TypeRef, ifaceMethod, classMethod);

        public MMethodDef AddMethod(ITypeDefOrRef iface, MMethodDef ifaceMethod, MMethodDef classMethod) {
            if (!_interfaceMethods.TryGetValue(iface, out var info))
                throw new ApplicationException("Could not find interface");
            return info.AddMethod(ifaceMethod, classMethod);
        }

        public void AddMethodIfEmpty(TypeInfo iface, MMethodDef ifaceMethod, MMethodDef classMethod) {
            if (!_interfaceMethods.TryGetValue(iface.TypeRef, out var info))
                throw new ApplicationException("Could not find interface");
            info.AddMethodIfEmpty(ifaceMethod, classMethod);
        }

        private readonly Dictionary<ITypeDefOrRef, InterfaceMethodInfo> _interfaceMethods =
            new(TypeEqualityComparer.Instance);

        public IEnumerable<InterfaceMethodInfo> AllInfos => _interfaceMethods.Values;
    }
}