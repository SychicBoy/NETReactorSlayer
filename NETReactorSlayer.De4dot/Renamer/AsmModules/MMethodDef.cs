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

using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class MMethodDef : Ref
{
    public MMethodDef(MethodDef methodDef, MTypeDef owner, int index)
        : base(methodDef, owner, index)
    {
        GenericParams = MGenericParamDef.CreateGenericParamDefList(MethodDef.GenericParameters);
        VisibleParameterBaseIndex = methodDef.MethodSig != null && methodDef.MethodSig.HasThis ? 1 : 0;
        for (var i = 0; i < methodDef.Parameters.Count; i++)
        {
            var param = methodDef.Parameters[i];
            if (param.IsNormalMethodParameter)
                VisibleParameterCount++;
            ParamDefs.Add(new MParamDef(param, i));
        }

        ReturnParamDef = new MParamDef(methodDef.Parameters.ReturnParameter, -1);
    }

    public bool IsPublic() => MethodDef.IsPublic;

    public bool IsVirtual() => MethodDef.IsVirtual;

    public bool IsNewSlot() => MethodDef.IsNewSlot;

    public bool IsStatic() => MethodDef.IsStatic;

    public IEnumerable<MParamDef> AllParamDefs
    {
        get
        {
            yield return ReturnParamDef;
            foreach (var paramDef in ParamDefs)
                yield return paramDef;
        }
    }

    public MEventDef Event { get; set; }

    public IList<MGenericParamDef> GenericParams { get; }

    public MethodDef MethodDef => (MethodDef)MemberRef;

    public IList<MParamDef> ParamDefs { get; } = new List<MParamDef>();

    public MPropertyDef Property { get; set; }

    public MParamDef ReturnParamDef { get; }

    public int VisibleParameterBaseIndex { get; }
    public int VisibleParameterCount { get; }
}