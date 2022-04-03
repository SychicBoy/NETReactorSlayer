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
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;

namespace NETReactorSlayer.Core.Deobfuscators
{
    public class Cleaner : IDeobfuscator
    {
        public void Execute()
        {
            #region Patch Methods
            Instruction Ldnull = Instruction.Create(OpCodes.Ldnull);
            Instruction Ret = Instruction.Create(OpCodes.Ret);
            CilBody cliBody;
            foreach (var method in MethodsToPatch)
            {
                try
                {
                    MethodDef methodDef = DeobfuscatorContext.Module.Find(method.DeclaringType.FullName, false).FindMethod(method.Name);
                    if (!methodDef.HasReturnType)
                    {
                        cliBody = new CilBody();
                        cliBody.Instructions.Add(Ret);
                        methodDef.Body = cliBody;
                    }
                    else
                    {
                        cliBody = new CilBody();
                        cliBody.Instructions.Add(Ldnull);
                        cliBody.Instructions.Add(Ret);
                        methodDef.Body = cliBody;
                    }
                    if (methodDef.DeclaringType.FindStaticConstructor() != null)
                    {
                        cliBody = new CilBody();
                        cliBody.Instructions.Add(Ret);
                        methodDef.DeclaringType.FindStaticConstructor().Body = cliBody;
                    }
                }
                catch { }
            }
            #endregion
            #region Remove Calls To Obfuscator Methods
            foreach (var type in DeobfuscatorContext.Module.GetTypes())
                foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                {
                    int length = method.Body.Instructions.Count;
                    int index = 0;
                    for (; index < length; index++)
                    {
                        if (method.Body.Instructions[index].OpCode.Equals(OpCodes.Call))
                        {
                            if (method.Body.Instructions[index].Operand is IMethod iMethod && MethodsToPatch.Any(x => x.MDToken.ToInt32() == iMethod.MDToken.ToInt32()))
                            {
                                method.Body.Instructions.RemoveAt(index);
                                index--;
                                length = method.Body.Instructions.Count;
                            }
                        }
                        else if (method.Body.Instructions[index].OpCode.Equals(OpCodes.Newobj) && method.Body.Instructions[index + 1].OpCode.Equals(OpCodes.Pop))
                        {
                            if (method.Body.Instructions[index].Operand is IMethod iMethod && MethodsToPatch.Any(x => x.MDToken.ToInt32() == iMethod.MDToken.ToInt32()))
                            {
                                method.Body.Instructions.RemoveAt(index);
                                method.Body.Instructions.RemoveAt(index);
                                index -= 2;
                                length = method.Body.Instructions.Count;
                            }
                        }
                    }
                }
            #endregion
            #region Delete Obfuscator Types, Methods, etc...
            foreach (MethodDef method in MethodsToPatch)
            {
                try { method.DeclaringType.Remove(method); } catch { }
            }
            foreach (ITypeDefOrRef type in TypesToRemove)
            {
                var typeDef = type.ResolveTypeDef();
                if (typeDef.DeclaringType != null)
                    typeDef.DeclaringType.NestedTypes.Remove(typeDef);
                else
                    DeobfuscatorContext.Module.Types.Remove(typeDef);
            }
            #endregion
            #region Remove NoInline Attributes
            foreach (var type in DeobfuscatorContext.Module.GetTypes())
            {
                foreach (var method in type.Methods.ToArray<MethodDef>())
                {
                    method.IsNoInlining = false;
                    for (int i = 0; i < method.CustomAttributes.Count; i++)
                    {
                        try
                        {
                            var cattr = method.CustomAttributes[i];
                            if (cattr.TypeFullName != "System.Runtime.CompilerServices.MethodImplAttribute")
                                continue;
                            int options = 0;
                            if (!GetMethodImplOptions(cattr, ref options))
                                continue;
                            if (options != 0 && options != (int)MethodImplAttributes.NoInlining)
                                continue;
                            method.CustomAttributes.RemoveAt(i);
                            i--;
                        }
                        catch { }
                    }
                }
            }
            #endregion
            #region Remove Unused Resources
            foreach (var rrsource in ResourceToRemove)
                DeobfuscatorContext.Module.Resources.Remove(DeobfuscatorContext.Module.Resources.Find(rrsource.Name));
            #endregion
            #region Unhide Entrypoint
            if (DeobfuscatorContext.Module.EntryPoint.DeclaringType.Name.Contains("<PrivateImplementationDetails>"))
            {
                var entryPoint = (DeobfuscatorContext.Module.EntryPoint.Body.Instructions.Where(x => x.OpCode == OpCodes.Call && x.Operand is IMethod iMethod && iMethod.ResolveMethodDef().IsStatic).LastOrDefault().Operand as IMethod).ResolveMethodDef();
                if (entryPoint != null)
                {
                    foreach (var attribute in DeobfuscatorContext.Module.EntryPoint.CustomAttributes)
                    {
                        entryPoint.CustomAttributes.Add(attribute);
                    }
                    if (DeobfuscatorContext.Module.EntryPoint.DeclaringType.DeclaringType != null)
                        DeobfuscatorContext.Module.EntryPoint.DeclaringType.DeclaringType.NestedTypes.Remove(DeobfuscatorContext.Module.EntryPoint.DeclaringType);
                    else
                        DeobfuscatorContext.Module.Types.Remove(DeobfuscatorContext.Module.EntryPoint.DeclaringType);
                    DeobfuscatorContext.Module.EntryPoint = entryPoint;
                }
            }
            #endregion
            #region Remove Junk Methods & Fields
            foreach (var type in DeobfuscatorContext.Module.GetTypes())
            {
                foreach (var method in type.Methods.Where(x => x.IsAssembly && x.IsStatic && x.HasBody && x.Body.HasInstructions && x.HasReturnType).ToList())
                {
                    if (!DeobfuscatorContext.Module.GetTypes()
                        .SelectMany(x => x.Methods)
                        .OfType<MethodDef>()
                        .Where(x => x.HasBody && x.Body.HasInstructions)
                        .SelectMany(x => x.Body.Instructions)
                        .OfType<Instruction>()
                        .Any(x => x.Operand is IMethod iMethod && iMethod.MDToken.ToInt32() == method.MDToken.ToInt32()))
                    {
                        foreach (var instruction in method.Body.Instructions)
                        {
                            if (instruction.Operand is IField iField && iField.ResolveFieldDef() != null && (iField.ResolveFieldDef().IsAssembly || iField.ResolveFieldDef().IsPrivate) && iField.ResolveFieldDef().IsStatic && iField.DeclaringType == method.DeclaringType)
                            {
                                if (!type.Methods
                                    .Where(x=> x.MDToken.ToInt32() != method.MDToken.ToInt32())
                                    .OfType<MethodDef>()
                                    .Where(x => x.HasBody && x.Body.HasInstructions)
                                    .SelectMany(x => x.Body.Instructions)
                                    .OfType<Instruction>()
                                    .Any(x => x.Operand is IField field && field.MDToken.ToInt32() == iField.MDToken.ToInt32()))
                                {
                                    type.Fields.Remove(iField.ResolveFieldDef());
                                    break;
                                }
                            }
                        }
                        type.Remove(method);
                    }
                }
            }
            #endregion
            RemoveNOPs();
        }

        public static void RemoveNOPs()
        {
            foreach (var type in DeobfuscatorContext.Module.GetTypes())
                foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                {
                    int length = method.Body.Instructions.Count;
                    int index = 0;
                    for (; index < length; index++)
                    {
                        if (method.Body.Instructions[index].OpCode.Equals(OpCodes.Nop))
                        {
                            method.Body.Instructions.RemoveAt(index);
                            index--;
                            length = method.Body.Instructions.Count;
                        }
                    }
                }
        }

        bool GetMethodImplOptions(CustomAttribute cA, ref int value)
        {
            if (cA.IsRawBlob)
                return false;
            if (cA.ConstructorArguments.Count != 1)
                return false;
            if (cA.ConstructorArguments[0].Type.ElementType != ElementType.I2 &&
                cA.ConstructorArguments[0].Type.FullName != "System.Runtime.CompilerServices.MethodImplOptions")
                return false;
            var arg = cA.ConstructorArguments[0].Value;
            if (arg is short @int)
            {
                value = @int;
                return true;
            }
            if (arg is int int1)
            {
                value = int1;
                return true;
            }
            return false;
        }

        public static HashSet<MethodDef> MethodsToPatch = new HashSet<MethodDef>();
        public static HashSet<ITypeDefOrRef> TypesToRemove = new HashSet<ITypeDefOrRef>();
        public static HashSet<Resource> ResourceToRemove = new HashSet<Resource>();
    }
}
