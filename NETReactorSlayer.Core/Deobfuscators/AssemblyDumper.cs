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
using System.IO;
using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using NETReactorSlayer.Core.Helper.De4dot;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class AssemblyDumper : IDeobfuscator
{
    private readonly string[] _locals1 =
    {
        "System.Byte[]", "System.Reflection.Assembly", "System.String", "System.IO.BinaryReader", "System.IO.Stream"
    };

    private readonly string[] _locals2 =
        {"System.Reflection.Assembly", "System.IO.BinaryReader", "System.IO.Stream"};

    private readonly string[] _locals3 =
        {"System.Reflection.Assembly", "System.Reflection.Assembly[]", "System.IO.Stream"};

    private MethodDef _initialMethod;
    private MethodDef _resolverMethod;
    private TypeDef _resolverType;

    public void Execute()
    {
        var count = 0L;
        FindRequirements();
        if (_resolverMethod == null || _initialMethod == null || _resolverType == null)
        {
            Logger.Warn("Couldn't find any embedded assembly (DNR).");
            return;
        }

        var assemblies = new List<EmbeddedResource>();
        foreach (var prefix in DotNetUtils.GetCodeStrings(_resolverMethod))
            assemblies.AddRange(GetAssemblies(prefix));
        if (assemblies.Count < 1)
        {
            Logger.Warn("Couldn't find any embedded assembly (DNR).");
            return;
        }

        Cleaner.MethodsToRemove.Add(_initialMethod);
        foreach (var asm in assemblies)
        {
            Cleaner.ResourceToRemove.Add(asm);
            count += 1L;
            var name = GetAssemblyName(asm, false) + ".dll";
            try
            {
                File.WriteAllBytes(DeobfuscatorContext.SourceDir + "\\" + name, asm.CreateReader().ToArray());
            } catch { }
        }

        Logger.Done((int) count + " Embedded assemblies dumped (DNR).");
    }

    private void FindRequirements()
    {
        foreach (var type in from x in DeobfuscatorContext.Module.GetTypes()
                 where x.HasFields && !x.HasNestedTypes && !x.HasEvents && !x.HasProperties
                 select x)
        foreach (var method in from x in type.Methods.ToArray()
                 where x.HasBody && x.Body.HasInstructions && x.IsStatic &&
                       DotNetUtils.IsMethod(x, "System.Void", "()") && x.DeclaringType != null
                 select x)
        foreach (var instr in method.Body.Instructions)
            try
            {
                if (!instr.Operand.ToString().Contains("add_AssemblyResolve")) continue;
                if (!CheckFields(method.DeclaringType.Fields)) continue;
                if (!FindResolverMethod(type, out var methodDef)) continue;
                var localTypes = new LocalTypes(methodDef);
                if (!localTypes.All(_locals1) && !localTypes.All(_locals2) && !localTypes.All(_locals3))
                    continue;
                _resolverMethod = methodDef;
                _resolverType = type;
                _initialMethod = method;
                return;
            } catch { }
    }

    private bool FindResolverMethod(TypeDef type, out MethodDef method)
    {
        method = null;
        foreach (var methodDef in type.Methods.ToArray())
        {
            if (!DotNetUtils.IsMethod(methodDef, "System.Reflection.Assembly", "(System.Object,System.Object)") &&
                !DotNetUtils.IsMethod(methodDef, "System.Reflection.Assembly",
                    "(System.Object,System.ResolveEventArgs)")) continue;
            method = methodDef;
            return true;
        }

        return false;
    }

    private bool CheckFields(IList<FieldDef> fields)
    {
        if (fields.Count != 2 && fields.Count != 3 && fields.Count != 4) return false;
        var fieldTypes = new FieldTypes(fields);
        if (fieldTypes.Count("System.Boolean") != 1 && fieldTypes.Count("System.Boolean") != 2) return false;
        if (fields.Count > 2)
            return fieldTypes.Count("System.Collections.Hashtable") == 2 || fieldTypes.Count("System.Object") == 2;
        return fields.Count == 2 && (fieldTypes.Count("System.Collections.Hashtable") == 1 ||
                                     fieldTypes.Count("System.Object") == 1);
    }

    private List<EmbeddedResource> GetAssemblies(string prefix)
    {
        var result = new List<EmbeddedResource>();
        if (string.IsNullOrEmpty(prefix)) return null;
        foreach (var rsrc in DeobfuscatorContext.Module.Resources)
        {
            if (!(rsrc is EmbeddedResource resource)) continue;
            if (StartsWith(resource.Name.String, prefix, StringComparison.Ordinal)) result.Add(resource);
        }

        return result;
    }

    private string GetAssemblyName(EmbeddedResource resource, bool fullName)
    {
        try
        {
            using var module = ModuleDefMD.Load(resource.CreateReader().ToArray());
            if (fullName) return module.Assembly.FullName;
            return module.Assembly.Name;
        } catch
        {
            return null;
        }
    }

    private bool StartsWith(string left, string right, StringComparison stringComparison) =>
        left.Length > right.Length && left.Substring(0, right.Length).Equals(right, stringComparison);
}