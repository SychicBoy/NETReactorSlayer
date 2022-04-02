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
using dnlib.DotNet.Emit;
using HarmonyLib;
using NETReactorSlayer.Core.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NETReactorSlayer.Core.Protections
{
    class Strings
    {
        public static void Execute()
        {
            Remover.RemoveNOPs();
            StacktracePatcher.Patch();
            long count = 0L;
            foreach (TypeDef type in Context.Module.GetTypes())
            {
                foreach (MethodDef method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x).ToArray<MethodDef>())
                {
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        try
                        {
                            if (method.Body.Instructions[i].IsLdcI4() && method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Call))
                            {
                                object result = null;
                                IMethod strMethod = (IMethod)method.Body.Instructions[i + 1].Operand;
                                try
                                {
                                    if (!DotNetUtils.IsMethod(strMethod, "System.String", "(System.Int32)"))
                                    {
                                        if (type == strMethod.DeclaringType)
                                        {
                                            if (!DotNetUtils.IsMethod(strMethod, "System.Object", "(System.Int32)")) continue;
                                        }
                                        else continue;
                                    }
                                }
                                catch { }
                                result = (StacktracePatcher.PatchStackTraceGetMethod.MethodToReplace = (Context.Assembly.ManifestModule.ResolveMethod((int)strMethod.ResolveMethodDef().MDToken.Raw) as MethodInfo)).Invoke(null, new object[]
                                {
                                method.Body.Instructions[i].GetLdcI4Value()
                                });
                                if (result != null && result.GetType() == typeof(string))
                                {
                                    try
                                    {
                                        foreach (var str in DotNetUtils.GetCodeStrings(strMethod.ResolveMethodDef()))
                                        {
                                            foreach (var name in Context.Assembly.GetManifestResourceNames())
                                            {
                                                if (str == name)
                                                {
                                                    if (strMethod.DeclaringType != type)
                                                    {
                                                        Remover.ResourceToRemove.Add(Context.Module.Resources.Find(name));
                                                        Remover.MethodsToPatch.Add(strMethod.ResolveMethodDef());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch { }
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Ldstr;
                                    method.Body.Instructions[i + 1].Operand = (string)result;
                                    count += 1L;
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            if (count > 0L) Logger.Info((int)count + " Strings decrypted.");
            else Logger.Warn("Couldn't find any encrypted string.");
        }

        public static class StacktracePatcher
        {
            const string HarmonyId = "_";
            static Harmony harmony;

            public static void Patch()
            {
                harmony = new Harmony(HarmonyId);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            [HarmonyPatch(typeof(StackFrame), "GetMethod")]
            public class PatchStackTraceGetMethod
            {
                public static MethodInfo MethodToReplace;

                public static void Postfix(ref MethodBase __result)
                {
                    if (__result.DeclaringType != typeof(RuntimeMethodHandle)) return;
                    __result = MethodToReplace ?? MethodBase.GetCurrentMethod();
                }
            }
        }
    }
}
