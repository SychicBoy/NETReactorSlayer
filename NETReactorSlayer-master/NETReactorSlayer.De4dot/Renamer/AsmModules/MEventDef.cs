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
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MEventDef : Ref
    {
        public MEventDef(IMemberRef memberRef, MTypeDef owner, int index)
            : base(memberRef, owner, index) { }

        public IEnumerable<MethodDef> MethodDefs()
        {
            if (EventDef.AddMethod != null)
                yield return EventDef.AddMethod;
            if (EventDef.RemoveMethod != null)
                yield return EventDef.RemoveMethod;
            if (EventDef.InvokeMethod != null)
                yield return EventDef.InvokeMethod;
            if (EventDef.OtherMethods == null)
                yield break;
            foreach (var m in EventDef.OtherMethods)
                yield return m;
        }

        public bool IsVirtual() => MethodDefs().Any(method => method.IsVirtual);

        public MMethodDef AddMethod { get; set; }
        public EventDef EventDef => (EventDef)MemberRef;
        public MMethodDef RaiseMethod { get; set; }
        public MMethodDef RemoveMethod { get; set; }
    }
}