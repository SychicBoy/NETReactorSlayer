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
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class Cleaner : IDeobfuscator
{
    public static HashSet<MethodDef> MethodsToRemove = new();
    public static HashSet<ITypeDefOrRef> TypesToRemove = new();
    public static HashSet<Resource> ResourceToRemove = new();
    public static bool KeepTypes = false;
    private CallCounter _methodCallCounter = new();

    public void Execute()
    {
        MethodCallsRemover.RemoveCalls(MethodsToRemove.ToList());

        DeobfuscateEntrypoint();

        if (!KeepTypes)
        {
            foreach (var method in MethodsToRemove)
                try
                {
                    method.DeclaringType.Remove(method);
                }
                catch { }

            foreach (var typeDef in TypesToRemove.Select(type => type.ResolveTypeDef()))
                if (typeDef.DeclaringType != null)
                    typeDef.DeclaringType.NestedTypes.Remove(typeDef);
                else
                    DeobfuscatorContext.Module.Types.Remove(typeDef);

            foreach (var rsrc in ResourceToRemove)
                DeobfuscatorContext.Module.Resources.Remove(DeobfuscatorContext.Module.Resources.Find(rsrc.Name));
        }

        foreach (var type in DeobfuscatorContext.Module.GetTypes().ToList())
        foreach (var method in type.Methods.ToList())
            try
            {
                RemoveAttributes(method);
                RemoveJunks(method);
            } catch { }
    }

    private void DeobfuscateEntrypoint()
    {
        if (DeobfuscatorContext.Module.IsEntryPointValid &&
            DeobfuscatorContext.Module.EntryPoint.DeclaringType.Name.Contains("<PrivateImplementationDetails>"))
            if ((DeobfuscatorContext.Module.EntryPoint.Body.Instructions
                    .Last(x => x.OpCode == OpCodes.Call && x.Operand is IMethod iMethod &&
                               iMethod.ResolveMethodDef().IsStatic).Operand as IMethod).ResolveMethodDef() is
                { } entryPoint)
            {
                foreach (var attribute in DeobfuscatorContext.Module.EntryPoint.CustomAttributes)
                    entryPoint.CustomAttributes.Add(attribute);
                if (DeobfuscatorContext.Module.EntryPoint.DeclaringType.DeclaringType != null)
                    DeobfuscatorContext.Module.EntryPoint.DeclaringType.DeclaringType.NestedTypes.Remove(
                        DeobfuscatorContext.Module.EntryPoint.DeclaringType);
                else
                    DeobfuscatorContext.Module.Types.Remove(DeobfuscatorContext.Module.EntryPoint.DeclaringType);
                DeobfuscatorContext.Module.EntryPoint = entryPoint;
            }
    }

    private void RemoveAttributes(MethodDef method)
    {
        method.IsNoInlining = false;
        method.IsSynchronized = false;
        method.IsNoOptimization = false;
        for (var i = 0; i < method.CustomAttributes.Count; i++)
            try
            {
                var cattr = method.CustomAttributes[i];
                if (cattr.Constructor is
                    {FullName: "System.Void System.Diagnostics.DebuggerHiddenAttribute::.ctor()"})
                {
                    method.CustomAttributes.RemoveAt(i);
                    i--;
                    continue;
                }

                if (cattr.TypeFullName ==
                    "System.Diagnostics.DebuggerStepThroughAttribute")
                {
                    method.CustomAttributes.RemoveAt(i);
                    i--;
                    continue;
                }

                if (cattr.TypeFullName ==
                    "System.Diagnostics.DebuggerNonUserCodeAttribute")
                {
                    method.CustomAttributes.RemoveAt(i);
                    i--;
                    continue;
                }

                if (cattr.TypeFullName != "System.Runtime.CompilerServices.MethodImplAttribute")
                    continue;
                var options = 0;
                if (!GetMethodImplOptions(cattr, ref options))
                    continue;
                if (options != 0 && options != (int) MethodImplAttributes.NoInlining)
                    continue;
                method.CustomAttributes.RemoveAt(i);
                i--;
            } catch { }
    }

    private void RemoveJunks(MethodDef method)
    {
        if (!method.HasBody || !method.Body.HasInstructions)
            return;
        if (_methodCallCounter != null && method.Name == ".ctor" || method.Name == ".cctor" ||
            DeobfuscatorContext.Module.EntryPoint == method)
        {
            #region IsEmpty

            static bool IsEmpty(MethodDef methodDef)
            {
                if (!DotNetUtils.IsEmptyObfuscated(methodDef))
                    return false;

                var type = methodDef.DeclaringType;
                if (type.HasEvents || type.HasProperties)
                    return false;
                if (type.Fields.Count != 1)
                    return false;
                if (type.Fields[0].FieldType.FullName != "System.Boolean")
                    return false;
                if (type.IsPublic)
                    return false;

                var otherMethods = 0;
                foreach (var method in type.Methods)
                {
                    if (method.Name == ".ctor" || method.Name == ".cctor")
                        continue;
                    if (method == methodDef)
                        continue;
                    otherMethods++;
                    if (method.Body == null)
                        return false;
                    if (method.Body.Instructions.Count > 20)
                        return false;
                }

                if (otherMethods > 8)
                    return false;

                return true;
            }

            #endregion

            foreach (var calledMethod in DotNetUtils.GetCalledMethods(DeobfuscatorContext.Module, method))
            {
                if (!calledMethod.IsStatic || calledMethod.Body == null)
                    continue;
                if (!DotNetUtils.IsMethod(calledMethod, "System.Void", "()"))
                    continue;
                if (IsEmpty(calledMethod))
                    _methodCallCounter?.Add(calledMethod);
            }

            var numCalls = 0;
            var methodDef = (MethodDef) _methodCallCounter?.Most(out numCalls);
            if (numCalls >= 10)
            {
                MethodCallsRemover.RemoveCalls(methodDef);
                try
                {
                    if (methodDef != null && methodDef.DeclaringType.DeclaringType != null)
                        methodDef.DeclaringType.DeclaringType.NestedTypes.Remove(methodDef.DeclaringType);
                    else
                        DeobfuscatorContext.Module.Types.Remove(methodDef?.DeclaringType);
                } catch { }

                _methodCallCounter = null;
            }
        }
        else if (method.Body.Instructions.Count == 4 &&
                 method.Body.Instructions[0].OpCode.Equals(OpCodes.Ldsfld) &&
                 method.Body.Instructions[1].OpCode.Equals(OpCodes.Ldnull) &&
                 method.Body.Instructions[2].OpCode.Equals(OpCodes.Ceq) &&
                 method.Body.Instructions[3].OpCode.Equals(OpCodes.Ret))
        {
            if (method.Body.Instructions[0].Operand is not FieldDef {IsPublic: false} field ||
                field.FieldType.FullName != "System.Object" && (field.DeclaringType == null ||
                                                                field.FieldType.FullName !=
                                                                field.DeclaringType.FullName)) return;
            foreach (var method2 in method.DeclaringType.Methods
                         .Where(x => x.HasBody && x.Body.HasInstructions && x.Body.Instructions.Count == 2)
                         .ToList().Where(method2 => !method2.IsPublic &&
                                                    (method2.ReturnType.FullName == "System.Object" ||
                                                     method2.DeclaringType != null &&
                                                     method2.ReturnType.FullName == field.DeclaringType.FullName))
                         .Where(method2 => method2.Body.Instructions[0].OpCode.Equals(OpCodes.Ldsfld) &&
                                           method2.Body.Instructions[1].OpCode.Equals(OpCodes.Ret)))
            {
                if (method2.Body.Instructions[0].Operand is not FieldDef field2 ||
                    field2.MDToken.ToInt32() != field.MDToken.ToInt32()) continue;
                try
                {
                    method.DeclaringType.Methods.Remove(method);
                } catch { }

                try
                {
                    method2.DeclaringType.Methods.Remove(method2);
                } catch { }

                try
                {
                    field.DeclaringType.Fields.Remove(field);
                } catch { }
            }
        }
        else if (IsInlineMethod(method))
        {
            try
            {
                method.DeclaringType.Remove(method);
            } catch { }
        }
        else if (DotNetUtils.IsMethod(method, "System.Void", "()") &&
                 method.IsStatic &&
                 method.IsAssembly &&
                 DotNetUtils.IsEmptyObfuscated(method) &&
                 method.DeclaringType.Methods.Any(x => DotNetUtils.GetMethodCalls(x).Contains(method)))
        {
            try
            {
                method.DeclaringType.Remove(method);
            } catch { }
        }
    }

    private bool IsInlineMethod(MethodDef method)
    {
        if (!method.IsStatic ||
            !method.IsAssembly &&
            !method.IsPrivateScope &&
            !method.IsPrivate ||
            method.GenericParameters.Count > 0 ||
            method.Name == ".cctor" ||
            !method.HasBody ||
            !method.Body.HasInstructions ||
            method.Body.Instructions.Count < 2)
            return false;

        switch (method.Body.Instructions[0].OpCode.Code)
        {
            case Code.Ldc_I4:
            case Code.Ldc_I4_0:
            case Code.Ldc_I4_1:
            case Code.Ldc_I4_2:
            case Code.Ldc_I4_3:
            case Code.Ldc_I4_4:
            case Code.Ldc_I4_5:
            case Code.Ldc_I4_6:
            case Code.Ldc_I4_7:
            case Code.Ldc_I4_8:
            case Code.Ldc_I4_M1:
            case Code.Ldc_I4_S:
            case Code.Ldc_I8:
            case Code.Ldc_R4:
            case Code.Ldc_R8:
            case Code.Ldftn:
            case Code.Ldnull:
            case Code.Ldstr:
            case Code.Ldtoken:
            case Code.Ldsfld:
            case Code.Ldsflda:
                if (method.Body.Instructions[1].OpCode.Code != Code.Ret)
                    return false;
                break;

            case Code.Ldarg:
            case Code.Ldarg_S:
            case Code.Ldarg_0:
            case Code.Ldarg_1:
            case Code.Ldarg_2:
            case Code.Ldarg_3:
            case Code.Ldarga:
            case Code.Ldarga_S:
            case Code.Call:
            case Code.Newobj:
                if (!IsCallMethod(method))
                    return false;
                break;

            default:
                return false;
        }

        return true;
    }

    private bool IsCallMethod(MethodDef method)
    {
        var loadIndex = 0;
        var methodArgsCount = DotNetUtils.GetArgsCount(method);
        var instrs = method.Body.Instructions;
        var i = 0;
        for (; i < instrs.Count && i < methodArgsCount; i++)
        {
            var instr = instrs[i];
            switch (instr.OpCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarg_S:
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                case Code.Ldarga:
                case Code.Ldarga_S:
                    if (instr.GetParameterIndex() != loadIndex)
                        return false;
                    loadIndex++;
                    continue;
            }

            break;
        }

        if (loadIndex != methodArgsCount)
            return false;
        if (i + 1 >= instrs.Count)
            return false;

        switch (instrs[i].OpCode.Code)
        {
            case Code.Call:
            case Code.Callvirt:
            case Code.Newobj:
            case Code.Ldfld:
            case Code.Ldflda:
            case Code.Ldftn:
            case Code.Ldvirtftn:
                break;
            default:
                return false;
        }

        if (instrs[i + 1].OpCode.Code != Code.Ret)
            return false;

        return true;
    }

    private bool GetMethodImplOptions(CustomAttribute cA, ref int value)
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
}