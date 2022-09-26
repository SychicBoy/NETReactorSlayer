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
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer;

public abstract class TypeNames
{
    public string Create(TypeSig typeRef)
    {
        typeRef = typeRef.RemovePinnedAndModifiers();
        if (typeRef == null)
            return UnknownNameCreator.Create();
        if (typeRef is GenericInstSig gis)
            if (gis.FullName == "System.Nullable`1" &&
                gis.GenericArguments.Count == 1 && gis.GenericArguments[0] != null)
                typeRef = gis.GenericArguments[0];

        var prefix = GetPrefix(typeRef);

        var elementType = Renamer.GetScopeType(typeRef);
        if (elementType == null && IsFnPtrSig(typeRef))
            return FnPtrNameCreator.Create();
        if (IsGenericParam(elementType))
            return GenericParamNameCreator.Create();

        var typeFullName = typeRef.FullName;
        if (TypeNamesDict.TryGetValue(typeFullName, out var nc))
            return nc.Create();

        var fullName = elementType == null ? typeRef.FullName : elementType.FullName;
        var dict = prefix == "" ? FullNameToShortName : FullNameToShortNamePrefix;
        if (!dict.TryGetValue(fullName, out var shortName))
        {
            fullName = fullName.Replace('/', '.');
            var index = fullName.LastIndexOf('.');
            shortName = index > 0 ? fullName.Substring(index + 1) : fullName;

            index = shortName.LastIndexOf('`');
            if (index > 0)
                shortName = shortName.Substring(0, index);
        }

        return AddTypeName(typeFullName, shortName, prefix).Create();
    }

    private bool IsFnPtrSig(TypeSig sig)
    {
        while (sig != null)
        {
            if (sig is FnPtrSig)
                return true;
            sig = sig.Next;
        }

        return false;
    }

    private bool IsGenericParam(ITypeDefOrRef tdr)
    {
        var ts = tdr as TypeSpec;
        if (ts == null)
            return false;
        var sig = ts.TypeSig.RemovePinnedAndModifiers();
        return sig is GenericSig;
    }

    private static string GetPrefix(TypeSig typeRef)
    {
        var prefix = "";
        while (typeRef != null)
        {
            if (typeRef.IsPointer)
                prefix += "p";
            typeRef = typeRef.Next;
        }

        return prefix;
    }

    protected INameCreator AddTypeName(string fullName, string newName, string prefix)
    {
        newName = FixName(prefix, newName);

        var name2 = " " + newName;
        if (!TypeNamesDict.TryGetValue(name2, out var nc))
            TypeNamesDict[name2] = nc = new NameCreator(newName + "_");

        TypeNamesDict[fullName] = nc;
        return nc;
    }

    protected abstract string FixName(string prefix, string name);

    public virtual TypeNames Merge(TypeNames other)
    {
        if (this == other)
            return this;
        foreach (var pair in other.TypeNamesDict)
            if (TypeNamesDict.TryGetValue(pair.Key, out var nc))
                nc.Merge(pair.Value);
            else
                TypeNamesDict[pair.Key] = pair.Value.Clone();
        GenericParamNameCreator.Merge(other.GenericParamNameCreator);
        FnPtrNameCreator.Merge(other.FnPtrNameCreator);
        UnknownNameCreator.Merge(other.UnknownNameCreator);
        return this;
    }

    protected static string UpperFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;
        return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
    }

    protected NameCreator FnPtrNameCreator = new("fnptr_");
    protected Dictionary<string, string> FullNameToShortName;
    protected Dictionary<string, string> FullNameToShortNamePrefix;
    protected NameCreator GenericParamNameCreator = new("gparam_");
    protected Dictionary<string, NameCreator> TypeNamesDict = new(StringComparer.Ordinal);
    protected NameCreator UnknownNameCreator = new("unknown_");
}