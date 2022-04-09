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
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using HarmonyLib;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NETReactorSlayer.Core.Deobfuscators
{
    class StringDecryptor : IDeobfuscator
    {
        public void Execute()
        {
            Cleaner.RemoveNOPs();
            StacktracePatcher.Patch();
            long count = 0L;
            foreach (TypeDef type in DeobfuscatorContext.Module.GetTypes())
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
                                MethodDef strMethod = ((IMethod)method.Body.Instructions[i + 1].Operand).ResolveMethodDef();
                                if (!strMethod.HasReturnType)
                                    continue;
                                if (strMethod.ReturnType.FullName != "System.String" && !(strMethod.DeclaringType != null && strMethod.DeclaringType == type && strMethod.ReturnType.FullName == "System.Object"))
                                    continue;
                                if (!strMethod.HasParams() || strMethod.Parameters.Count != 1 || strMethod.Parameters[0].Type.FullName != "System.Int32")
                                    continue;

                                result = (StacktracePatcher.PatchStackTraceGetMethod.MethodToReplace = (DeobfuscatorContext.Assembly.ManifestModule.ResolveMethod((int)strMethod.ResolveMethodDef().MDToken.Raw) as MethodInfo)).Invoke(null, new object[]
                                {
                                method.Body.Instructions[i].GetLdcI4Value()
                                });
                                if (result != null && result.GetType() == typeof(string))
                                {
                                    try
                                    {
                                        foreach (var str in DotNetUtils.GetCodeStrings(strMethod.ResolveMethodDef()))
                                        {
                                            foreach (var name in DeobfuscatorContext.Assembly.GetManifestResourceNames())
                                            {
                                                if (str == name)
                                                {
                                                    if (strMethod.DeclaringType != type)
                                                    {
                                                        Cleaner.ResourceToRemove.Add(DeobfuscatorContext.Module.Resources.Find(name));
                                                        Cleaner.MethodsToPatch.Add(strMethod.ResolveMethodDef());
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
                                    if ((string)result == "This assembly is protected by an unregistered version of Eziriz's \".NET Reactor\"!")
                                    {
                                        foreach (var methodToRemove in method.DeclaringType.Methods)
                                            Cleaner.MethodsToPatch.Add(method);
                                        Cleaner.TypesToRemove.Add(method.DeclaringType);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            if (count > 0L) Logger.Done((int)count + " Strings decrypted.");
            else Logger.Warn("Couldn't find any encrypted string.");
        }

        public class StacktracePatcher
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
