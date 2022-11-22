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
using System.Linq;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules {
    public class InterfaceMethodInfo {
        public InterfaceMethodInfo(TypeInfo iface) {
            Face = iface;
            foreach (var methodDef in iface.TypeDef.AllMethods)
                IfaceMethodToClassMethod[new MethodDefKey(methodDef)] = null;
        }

        public InterfaceMethodInfo(TypeInfo iface, InterfaceMethodInfo other) {
            Face = iface;
            foreach (var key in other.IfaceMethodToClassMethod.Keys)
                IfaceMethodToClassMethod[key] = other.IfaceMethodToClassMethod[key];
        }

        public void Merge(InterfaceMethodInfo other) {
            foreach (var key in other.IfaceMethodToClassMethod.Keys.Where(
                         key => other.IfaceMethodToClassMethod[key] != null)) {
                if (IfaceMethodToClassMethod[key] != null)
                    throw new ApplicationException("Interface method already initialized");
                IfaceMethodToClassMethod[key] = other.IfaceMethodToClassMethod[key];
            }
        }

        public MMethodDef AddMethod(MMethodDef ifaceMethod, MMethodDef classMethod) {
            var ifaceKey = new MethodDefKey(ifaceMethod);
            if (!IfaceMethodToClassMethod.ContainsKey(ifaceKey))
                throw new ApplicationException("Could not find interface method");

            IfaceMethodToClassMethod.TryGetValue(ifaceKey, out var oldMethod);
            IfaceMethodToClassMethod[ifaceKey] = classMethod;
            return oldMethod;
        }

        public void AddMethodIfEmpty(MMethodDef ifaceMethod, MMethodDef classMethod) {
            if (IfaceMethodToClassMethod[new MethodDefKey(ifaceMethod)] == null)
                AddMethod(ifaceMethod, classMethod);
        }

        public override string ToString() => Face.ToString();

        public TypeInfo Face { get; }

        public Dictionary<MethodDefKey, MMethodDef> IfaceMethodToClassMethod { get; } = new();
    }
}