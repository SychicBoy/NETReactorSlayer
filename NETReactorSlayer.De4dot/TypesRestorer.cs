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
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot;

public class TypesRestorer : TypesRestorerBase
{
    public TypesRestorer(ModuleDef module)
        : base(module)
    {
    }

    protected override bool IsValidType(IGenericParameterProvider gpp, TypeSig type)
    {
        if (type == null)
            return false;
        if (type.IsValueType)
            return false;
        if (type.ElementType == ElementType.Object)
            return false;
        return base.IsValidType(gpp, type);
    }

    protected override bool IsUnknownType(object o)
    {
        if (o is Parameter arg)
            return arg.Type.GetElementType() == ElementType.Object;

        if (o is FieldDef field)
            return field.FieldSig.GetFieldType().GetElementType() == ElementType.Object;

        if (o is TypeSig sig)
            return sig.ElementType == ElementType.Object;

        throw new ApplicationException($"Unknown type: {o.GetType()}");
    }
}