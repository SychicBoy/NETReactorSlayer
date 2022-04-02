/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NetReactorSlayer.
    NetReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NetReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NetReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/
using de4dot.blocks;
using dnlib.DotNet;
using NETReactorSlayer.Core.Helper.De4dot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NETReactorSlayer.Core.Deobfuscators
{
    class AssemblyDumper : IDeobfuscator
    {
        public void Execute()
        {
            long count = 0L;
            FindRequirements();
            if (resolverMethod == null || initialMethod == null || resolverType == null)
            {
                Logger.Warn("Couldn't find any embedded assembly (DNR).");
                return;
            }
            List<EmbeddedResource> Assemblies = new List<EmbeddedResource>();
            foreach (string prefix in DotNetUtils.GetCodeStrings(resolverMethod))
            {
                Assemblies.AddRange(GetAssemblies(prefix));
            }
            if (Assemblies.Count < 1)
            {
                Logger.Warn("Couldn't find any embedded assembly (DNR).");
                return;
            }
            Cleaner.MethodsToPatch.Add(initialMethod);
            foreach (var asm in Assemblies)
            {
                int i = 0;
                Cleaner.ResourceToRemove.Add(asm);
                count += 1L;
                string name = GetAssemblyName(asm, false) + ".dll";
                try
                {
                    if (name != null)
                        File.WriteAllBytes(DeobfuscatorContext.SourceDir + "\\" + name, asm.CreateReader().ToArray());
                    else
                        File.WriteAllBytes($"{DeobfuscatorContext.SourceDir}\\DumpedAssembly ({i++}).dll", asm.CreateReader().ToArray());
                }
                catch
                {
                }
            }
            Logger.Done((int)count + " Embedded assemblies dumped (DNR).");
        }

        void FindRequirements()
        {
            foreach (var type in (from x in DeobfuscatorContext.Module.GetTypes() where x.HasFields && !x.HasNestedTypes && !x.HasEvents && !x.HasProperties select x))
            {
                foreach (var method in (from x in type.Methods.ToArray<MethodDef>() where x.HasBody && x.Body.HasInstructions && x.IsStatic && DotNetUtils.IsMethod(x, "System.Void", "()") && x.DeclaringType != null select x))
                {
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        try
                        {
                            if (!method.Body.Instructions[i].Operand.ToString().Contains("add_AssemblyResolve")) continue;
                            if (!CheckFields(method.DeclaringType.Fields)) continue;
                            if (!FindResolverMethod(type, out MethodDef methodDef)) continue;
                            LocalTypes localTypes = new LocalTypes(methodDef);
                            if (!localTypes.All(Locals1) && !localTypes.All(Locals2) && !localTypes.All(Locals3)) continue;
                            resolverMethod = methodDef;
                            resolverType = type;
                            initialMethod = method;
                            return;
                        }
                        catch { }
                    }
                }
            }
        }

        bool FindResolverMethod(TypeDef type, out MethodDef method)
        {
            method = null;
            foreach (var methodDef in type.Methods.ToArray<MethodDef>())
            {
                if (!DotNetUtils.IsMethod(methodDef, "System.Reflection.Assembly", "(System.Object,System.Object)") && !DotNetUtils.IsMethod(methodDef, "System.Reflection.Assembly", "(System.Object,System.ResolveEventArgs)")) continue;
                method = methodDef;
                return true;
            }
            return false;
        }

        bool CheckFields(IList<FieldDef> fields)
        {
            if (fields.Count != 2 && fields.Count != 3 && fields.Count != 4) return false;
            FieldTypes fieldTypes = new FieldTypes(fields);
            if (fieldTypes.Count("System.Boolean") != 1 && fieldTypes.Count("System.Boolean") != 2) return false;
            if (fields.Count > 2)
            {
                return (fieldTypes.Count("System.Collections.Hashtable") == 2 || fieldTypes.Count("System.Object") == 2);
            }
            return (fields.Count == 2 && (fieldTypes.Count("System.Collections.Hashtable") == 1 || fieldTypes.Count("System.Object") == 1));
        }

        List<EmbeddedResource> GetAssemblies(string prefix)
        {
            List<EmbeddedResource> result = new List<EmbeddedResource>();
            if (string.IsNullOrEmpty(prefix)) return null;
            else
            {
                foreach (Resource rsrc in DeobfuscatorContext.Module.Resources)
                {
                    if (!(rsrc is EmbeddedResource resource)) continue;
                    if (StartsWith(resource.Name.String, prefix, StringComparison.Ordinal)) result.Add(resource);
                }
            }
            return result;
        }

        string GetAssemblyName(EmbeddedResource resource, bool FullName)
        {
            try
            {
                using ModuleDefMD module = ModuleDefMD.Load(resource.CreateReader().ToArray());
                if (FullName) return module.Assembly.FullName;
                else return module.Assembly.Name;
            }
            catch
            {
                return null;
            }
        }

        bool StartsWith(string left, string right, StringComparison stringComparison)
        {
            return left.Length > right.Length && left.Substring(0, right.Length).Equals(right, stringComparison);
        }

        readonly string[] Locals1 = new string[] { "System.Byte[]", "System.Reflection.Assembly", "System.String", "System.IO.BinaryReader", "System.IO.Stream" };
        readonly string[] Locals2 = new string[] { "System.Reflection.Assembly", "System.IO.BinaryReader", "System.IO.Stream" };
        readonly string[] Locals3 = new string[] { "System.Reflection.Assembly", "System.Reflection.Assembly[]", "System.IO.Stream" };
        MethodDef resolverMethod = null;
        MethodDef initialMethod = null;
        TypeDef resolverType = null;
    }
}
