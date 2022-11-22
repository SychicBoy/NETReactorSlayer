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
using System.Text;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer {
    public class ResourceRenamer {
        public ResourceRenamer(Module module) => _module = module;

        public void Rename(List<TypeInfo> renamedTypes) {
            renamedTypes.Sort((a, b) =>
            {
                var aesc = EscapeTypeName(a.OldFullName);
                var besc = EscapeTypeName(b.OldFullName);
                return besc.Length != aesc.Length
                    ? besc.Length.CompareTo(aesc.Length)
                    : string.Compare(besc, aesc, StringComparison.Ordinal);
            });

            _nameToResource =
                new Dictionary<string, Resource>(_module.ModuleDefMd.Resources.Count * 3, StringComparer.Ordinal);
            foreach (var resource in _module.ModuleDefMd.Resources) {
                var name = resource.Name.String;
                _nameToResource[name] = resource;
                if (name.EndsWith(".g.resources"))
                    _nameToResource[name.Substring(0, name.Length - 12)] = resource;
                var index = name.LastIndexOf('.');
                if (index > 0)
                    _nameToResource[name.Substring(0, index)] = resource;
            }

            RenameResourceNamesInCode(renamedTypes);
            RenameResources(renamedTypes);
        }

        private void RenameResourceNamesInCode(List<TypeInfo> renamedTypes) {
            var oldNameToTypeInfo = new Dictionary<string, TypeInfo>(StringComparer.Ordinal);
            foreach (var info in renamedTypes)
                oldNameToTypeInfo[EscapeTypeName(info.OldFullName)] = info;

            foreach (var instrs in from method in _module.GetAllMethods()
                     where method.HasBody
                     select method.Body.Instructions)
                for (var i = 0; i < instrs.Count; i++) {
                    var instr = instrs[i];
                    if (instr.OpCode != OpCodes.Ldstr)
                        continue;
                    var codeString = (string)instr.Operand;
                    if (string.IsNullOrEmpty(codeString))
                        continue;

                    if (!_nameToResource.TryGetValue(codeString, out _))
                        continue;

                    if (!oldNameToTypeInfo.TryGetValue(codeString, out var typeInfo))
                        continue;
                    var newName = EscapeTypeName(typeInfo.Type.TypeDef.FullName);

                    var renameCodeString = _module.ObfuscatedFile.RenameResourcesInCode ||
                                           IsCallingResourceManagerCtor(instrs, i, typeInfo);
                    if (renameCodeString)
                        instr.Operand = newName;
                }
        }

        private static bool IsCallingResourceManagerCtor(IList<Instruction> instrs, int ldstrIndex, TypeInfo typeInfo) {
            try {
                var index = ldstrIndex + 1;

                var ldtoken = instrs[index++];
                if (ldtoken.OpCode.Code != Code.Ldtoken)
                    return false;
                if (!new SigComparer().Equals(typeInfo.Type.TypeDef, ldtoken.Operand as ITypeDefOrRef))
                    return false;

                if (!CheckCalledMethod(instrs[index++], "System.Type", "(System.RuntimeTypeHandle)"))
                    return false;
                if (!CheckCalledMethod(instrs[index++], "System.Reflection.Assembly", "()"))
                    return false;

                var newobj = instrs[index];
                if (newobj.OpCode.Code != Code.Newobj)
                    return false;
                return newobj.Operand.ToString() ==
                       "System.Void System.Resources.ResourceManager::.ctor(System.String,System.Reflection.Assembly)";
            } catch (ArgumentOutOfRangeException) {
                return false;
            } catch (IndexOutOfRangeException) {
                return false;
            }
        }

        private static bool CheckCalledMethod(Instruction instr, string returnType, string parameters) {
            if (instr.OpCode.Code != Code.Call && instr.OpCode.Code != Code.Callvirt)
                return false;
            return DotNetUtils.IsMethod(instr.Operand as IMethod, returnType, parameters);
        }

        private void RenameResources(List<TypeInfo> renamedTypes) {
            var newNames = new Dictionary<Resource, RenameInfo>();
            foreach (var info in renamedTypes) {
                var oldFullName = EscapeTypeName(info.OldFullName);
                if (!_nameToResource.TryGetValue(oldFullName, out var resource))
                    continue;
                if (newNames.ContainsKey(resource))
                    continue;
                var newTypeName = EscapeTypeName(info.Type.TypeDef.FullName);
                var newName = newTypeName + resource.Name.String.Substring(oldFullName.Length);
                newNames[resource] = new RenameInfo(resource, newName);
                resource.Name = newName;
            }
        }

        private static bool IsReservedTypeNameChar(char c) {
            switch (c) {
                case ',':
                case '[':
                case ']':
                case '&':
                case '*':
                case '+':
                case '\\':
                    return true;
                default:
                    return false;
            }
        }

        private static bool HasReservedTypeNameChar(string s) => s.Any(IsReservedTypeNameChar);

        private static string EscapeTypeName(string name) {
            if (!HasReservedTypeNameChar(name))
                return name;
            var sb = new StringBuilder();
            foreach (var c in name) {
                if (IsReservedTypeNameChar(c))
                    sb.Append('\\');
                sb.Append(c);
            }

            return sb.ToString();
        }

        private readonly Module _module;
        private Dictionary<string, Resource> _nameToResource;

        private class RenameInfo {
            public RenameInfo(Resource resource, string newResourceName) {
                _resource = resource;
                _newResourceName = newResourceName;
            }

            public override string ToString() => $"{_resource.Name} => {_newResourceName}";

            private readonly string _newResourceName;
            private readonly Resource _resource;
        }
    }
}