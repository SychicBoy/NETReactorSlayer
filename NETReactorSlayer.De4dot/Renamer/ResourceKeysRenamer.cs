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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Resources;

namespace NETReactorSlayer.De4dot.Renamer;

public class ResourceKeysRenamer
{
    public ResourceKeysRenamer(ModuleDefMD module, INameChecker nameChecker)
    {
        _module = module;
        _nameChecker = nameChecker;
    }

    public void Rename()
    {
        foreach (var type in _module.GetTypes())
        {
            var resourceName = GetResourceName(type);
            if (resourceName == null)
                continue;
            var resource = GetResource(resourceName);
            if (resource == null) continue;
            Rename(type, resource);
        }
    }

    private EmbeddedResource GetResource(string resourceName)
    {
        if (DotNetUtils.GetResource(_module, resourceName + ".resources") is EmbeddedResource resource)
            return resource;

        var name = "";
        var pieces = resourceName.Split('.');
        Array.Reverse(pieces);
        foreach (var piece in pieces)
        {
            name = piece + name;
            resource = DotNetUtils.GetResource(_module, name + ".resources") as EmbeddedResource;
            if (resource != null)
                return resource;
        }

        return null;
    }

    private static string GetResourceName(TypeDef type)
    {
        foreach (var method in type.Methods)
        {
            if (method.Body == null)
                continue;
            var instrs = method.Body.Instructions;
            string resourceName = null;
            for (var i = 0; i < instrs.Count; i++)
            {
                var instr = instrs[i];
                if (instr.OpCode.Code == Code.Ldstr)
                {
                    resourceName = instr.Operand as string;
                    continue;
                }

                if (instr.OpCode.Code != Code.Newobj) continue;
                var ctor = instr.Operand as IMethod;
                if (ctor?.FullName !=
                    "System.Void System.Resources.ResourceManager::.ctor(System.String,System.Reflection.Assembly)")
                    continue;
                if (resourceName == null) continue;

                return resourceName;
            }
        }

        return null;
    }

    private void Rename(TypeDef type, EmbeddedResource resource)
    {
        _newNames.Clear();
        var resourceSet = ResourceReader.Read(_module, resource.CreateReader());
        var renamed = new List<RenameInfo>();
        foreach (var elem in resourceSet.ResourceElements)
        {
            if (_nameChecker.IsValidResourceKeyName(elem.Name))
            {
                _newNames.Add(elem.Name, true);
                continue;
            }

            renamed.Add(new RenameInfo(elem, GetNewName(elem)));
        }

        if (renamed.Count == 0)
            return;

        Rename(type, renamed);

        var outStream = new MemoryStream();
        ResourceWriter.Write(_module, outStream, resourceSet);
        var newResource = new EmbeddedResource(resource.Name, outStream.ToArray(), resource.Attributes);
        var resourceIndex = _module.Resources.IndexOf(resource);
        if (resourceIndex < 0)
            throw new ApplicationException("Could not find index of resource");
        _module.Resources[resourceIndex] = newResource;
    }

    private void Rename(TypeDef type, List<RenameInfo> renamed)
    {
        var nameToInfo = new Dictionary<string, RenameInfo>(StringComparer.Ordinal);
        foreach (var info in renamed)
            nameToInfo[info.Element.Name] = info;

        foreach (var method in type.Methods)
        {
            if (method.Body == null)
                continue;

            var instrs = method.Body.Instructions;
            for (var i = 0; i < instrs.Count; i++)
            {
                var call = instrs[i];
                if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Callvirt)
                    continue;
                var calledMethod = call.Operand as IMethod;
                if (calledMethod == null)
                    continue;

                int ldstrIndex;
                switch (calledMethod.FullName)
                {
                    case
                        "System.String System.Resources.ResourceManager::GetString(System.String,System.Globalization.CultureInfo)"
                        :
                    case
                        "System.IO.UnmanagedMemoryStream System.Resources.ResourceManager::GetStream(System.String,System.Globalization.CultureInfo)"
                        :
                    case
                        "System.Object System.Resources.ResourceManager::GetObject(System.String,System.Globalization.CultureInfo)"
                        :
                        ldstrIndex = i - 2;
                        break;

                    case "System.String System.Resources.ResourceManager::GetString(System.String)":
                    case "System.IO.UnmanagedMemoryStream System.Resources.ResourceManager::GetStream(System.String)":
                    case "System.Object System.Resources.ResourceManager::GetObject(System.String)":
                        ldstrIndex = i - 1;
                        break;

                    default:
                        continue;
                }

                Instruction ldstr = null;
                string name;
                if (ldstrIndex >= 0)
                    ldstr = instrs[ldstrIndex];
                if (ldstr == null || (name = ldstr.Operand as string) == null) continue;

                if (!nameToInfo.TryGetValue(name, out var info))
                    continue; // should not be renamed

                ldstr.Operand = info.NewName;
                info.Element.Name = info.NewName;
            }
        }
    }

    private string GetNewName(ResourceElement elem)
    {
        if (elem.ResourceData.Code != ResourceTypeCode.String)
            return CreateDefaultName();
        var stringData = (BuiltInResourceData)elem.ResourceData;
        var name = CreatePrefixFromStringData((string)stringData.Data);
        return CreateName(counter => counter == 0 ? name : $"{name}_{counter}");
    }

    private string CreatePrefixFromStringData(string data)
    {
        var sb = new StringBuilder();
        data = data.Substring(0, Math.Min(data.Length, 100));
        data = Regex.Replace(data, "[`'\"]", "");
        data = Regex.Replace(data, @"[^\w]+", " ");
        foreach (var piece in data.Split(' '))
        {
            if (piece.Length == 0)
                continue;
            var piece2 = piece.Substring(0, 1).ToUpperInvariant() + piece.Substring(1).ToLowerInvariant();
            var maxLen = ResourceKeyMaxLen - sb.Length;
            if (maxLen <= 0)
                break;
            if (piece2.Length > maxLen)
                piece2 = piece2.Substring(0, maxLen);
            sb.Append(piece2);
        }

        if (sb.Length <= 3)
            return CreateDefaultName();
        return sb.ToString();
    }

    private string CreateDefaultName()
    {
        return CreateName(counter => $"{DefaultKeyName}{counter}");
    }

    private string CreateName(Func<int, string> create)
    {
        for (var counter = 0;; counter++)
        {
            var newName = create(counter);
            if (!_newNames.ContainsKey(newName))
            {
                _newNames[newName] = true;
                return newName;
            }
        }
    }

    private readonly ModuleDefMD _module;
    private readonly INameChecker _nameChecker;
    private readonly Dictionary<string, bool> _newNames = new();

    private const string DefaultKeyName = "Key";

    private const int ResourceKeyMaxLen = 50;

    private class RenameInfo
    {
        public RenameInfo(ResourceElement element, string newName)
        {
            Element = element;
            NewName = newName;
        }

        public override string ToString()
        {
            return $"{Element} => {NewName}";
        }

        public readonly ResourceElement Element;
        public readonly string NewName;
    }
}