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
using System.Linq;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules {
    public class MethodNameGroup {
        public void Add(MMethodDef method) => Methods.Add(method);

        public void Merge(MethodNameGroup other) {
            if (this == other)
                return;
            Methods.AddRange(other.Methods);
        }

        public bool HasNonRenamableMethod() => Methods.Any(method => !method.Owner.HasModule);

        public bool HasInterfaceMethod() => Methods.Any(method => method.Owner.TypeDef.IsInterface);

        public bool HasGetterOrSetterPropertyMethod() => (from method in Methods
            where method.Property != null
            let prop = method.Property
            where method == prop.GetMethod || method == prop.SetMethod
            select method).Any();

        public bool HasAddRemoveOrRaiseEventMethod() => (from method in Methods
            where method.Event != null
            let evt = method.Event
            where method == evt.AddMethod || method == evt.RemoveMethod || method == evt.RaiseMethod
            select method).Any();

        public bool HasProperty() => Methods.Any(method => method.Property != null);

        public bool HasEvent() => Methods.Any(method => method.Event != null);

        public override string ToString() => $"{Methods.Count} -- {(Methods.Count > 0 ? Methods[0].ToString() : "")}";

        public int Count => Methods.Count;

        public List<MMethodDef> Methods { get; } = new();
    }
}