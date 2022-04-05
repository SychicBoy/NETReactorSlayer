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
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Helper.De4dot;
using System;
using System.Collections.Generic;

namespace NETReactorSlayer.Core.Deobfuscators
{
    class ResourceDecryptor : IDeobfuscator
    {
        public void Execute()
        {
            TypeDef typeDef = null;
            byte[] decryptedBytes = null;
            EmbeddedResource encryptedResource = null;
            HashSet<MethodDef> methodsToPatch = new HashSet<MethodDef>();
            foreach (TypeDef type in DeobfuscatorContext.Module.GetTypes())
            {
                MethodDef method1 = type.FindMethod(".ctor");
                if (method1 != null && method1.HasBody && method1.Body.HasInstructions)
                {
                    for (int i = 0; i < method1.Body.Instructions.Count; i++)
                    {
                        if (method1.Body.Instructions[i].OpCode.Equals(OpCodes.Newobj) && method1.Body.Instructions[i].Operand.ToString().Contains("System.ResolveEventHandler") && method1.Body.Instructions[i - 1].OpCode.Equals(OpCodes.Ldftn))
                        {
                            if (method1.Body.Instructions[i - 1].Operand is MethodDef method2 && method2.DeclaringType.Equals(type) && method2.HasReturnType && method2.ReturnType.FullName.Equals("System.Reflection.Assembly"))
                            {
                                foreach (Instruction instruction in method2.Body.Instructions)
                                {
                                    try
                                    {
                                        if (instruction.OpCode.Equals(OpCodes.Call))
                                        {
                                            if (instruction.Operand is MethodDef decryptorMethod && decryptorMethod.DeclaringType.Equals(type) && !decryptorMethod.HasReturnType)
                                            {
                                                foreach (string s in DotNetUtils.GetCodeStrings(decryptorMethod))
                                                {
                                                    if ((encryptedResource = (DotNetUtils.GetResource(DeobfuscatorContext.Module, s) as EmbeddedResource)) != null)
                                                    {
                                                        foreach (var methodToRemove in type.Methods)
                                                            methodsToPatch.Add(methodToRemove);
                                                        typeDef = type;
                                                        DnrDecrypterType decrypterType = GetDecrypterType(decryptorMethod, new string[0]);
                                                        byte[] key = ArrayFinder.GetInitializedByteArray(decryptorMethod, 32);
                                                        if (decrypterType == DnrDecrypterType.V3)
                                                        {
                                                            V3 V3 = new V3(decryptorMethod);
                                                            decryptedBytes = V3.Decrypt(encryptedResource);
                                                            return;
                                                        }
                                                        byte[] iv = ArrayFinder.GetInitializedByteArray(decryptorMethod, 16);
                                                        if (DotNetUtils.CallsMethod(decryptorMethod, "System.Array::Reverse"))
                                                            Array.Reverse(iv);
                                                        if (UsesPublicKeyToken(decryptorMethod))
                                                        {
                                                            PublicKeyToken publicKeyToken = DeobfuscatorContext.Module.Assembly.PublicKeyToken;
                                                            if (publicKeyToken != null && publicKeyToken.Data.Length != 0)
                                                            {
                                                                for (int z = 0; z < 8; z++)
                                                                {
                                                                    iv[z * 2 + 1] = publicKeyToken.Data[z];
                                                                }
                                                            }
                                                        }
                                                        if (decrypterType == DnrDecrypterType.V1)
                                                        {
                                                            V1 V1 = new V1(iv, key);
                                                            decryptedBytes = V1.Decrypt(encryptedResource);
                                                            return;
                                                        }
                                                        else if (decrypterType == DnrDecrypterType.V2)
                                                        {
                                                            V2 V2 = new V2(iv, key, decryptorMethod);
                                                            decryptedBytes = V2.Decrypt(encryptedResource);
                                                            goto Decompress;
                                                        }
                                                        else
                                                        {
                                                            Logger.Warn("Couldn't find resource decrypter method.");
                                                            return;
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
            Logger.Warn("Couldn't find any encrypted resource.");
            return;
        Decompress:
            if (encryptedResource == null)
            {
                Logger.Warn("Couldn't find any encrypted resource.");
                return;
            }
            try
            {
                DeobUtils.DecryptAndAddResources(DeobfuscatorContext.Module, delegate
                {
                    byte[] result;
                    try
                    {
                        result = QuickLZ.Decompress(decryptedBytes);
                    }
                    catch
                    {
                        try
                        {
                            result = DeobUtils.Inflate(decryptedBytes, true);
                        }
                        catch
                        {
                            result = null;
                        }
                    }
                    return result;
                });
                foreach (var m in methodsToPatch)
                {
                    Cleaner.MethodsToPatch.Add(m);
                }
                Cleaner.TypesToRemove.Add(typeDef);
                Cleaner.ResourceToRemove.Add(encryptedResource);
                Logger.Done("Assembly resources decrypted");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to decrypt resources. " + ex.Message);
            }
        }

        public static byte[] GetBytes(MethodDef method, int size)
        {
            byte[] result = new byte[size];
            Local local = null;
            if (method == null || !method.HasBody || !method.Body.HasInstructions) return null;
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                if (local == null && method.Body.Instructions[i].OpCode.Equals(OpCodes.Newarr) && method.Body.Instructions[i].Operand.ToString().Equals("System.Byte") && method.Body.Instructions[i - 1].OpCode.Equals(OpCodes.Ldc_I4) && method.Body.Instructions[i - 1].GetLdcI4Value() == size && method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Stloc))
                {
                    local = method.Body.Instructions[i + 1].Operand as Local;
                    i = 0;
                }
                else if (method.Body.Instructions[i].IsLdloc() &&
                    (method.Body.Instructions[i].Operand as Local) == local &&
                    method.Body.Instructions[i + 1].IsLdcI4() &&
                    method.Body.Instructions[i + 2].IsLdcI4() &&
                    method.Body.Instructions[i + 3].OpCode.Equals(OpCodes.Stelem_I1))
                {
                    try
                    {
                        result[method.Body.Instructions[i + 1].GetLdcI4Value()] = (byte)(int)(method.Body.Instructions[i + 2].GetLdcI4Value());
                    }
                    catch { return result; }
                }
            }
            return result;
        }

        public static bool IsNeedReverse(MethodDef method)
        {
            if (method != null && method.HasBody && method.Body.HasInstructions)
            {
                foreach (var instr in method.Body.Instructions)
                {
                    try
                    {
                        IMethod calledMethod = instr.Operand as IMethod;
                        if (calledMethod.FullName.Contains("System.Array::Reverse"))
                            return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        public static bool UsesPublicKeyToken(MethodDef resourceDecrypterMethod)
        {
            int pktIndex = 0;
            foreach (Instruction instr in resourceDecrypterMethod.Body.Instructions)
            {
                if (instr.OpCode.FlowControl != FlowControl.Next) pktIndex = 0;
                else if (instr.IsLdcI4())
                {
                    if (instr.GetLdcI4Value() != pktIndexes[pktIndex++]) pktIndex = 0;
                    else if (pktIndex >= pktIndexes.Length) return true;
                }
            }
            return false;
        }

        public static DnrDecrypterType GetDecrypterType(MethodDef method, IList<string> additionalTypes)
        {
            if (method == null || !method.IsStatic || method.Body == null)
            {
                return DnrDecrypterType.Unknown;
            }
            if (additionalTypes == null)
            {
                additionalTypes = new string[0];
            }
            LocalTypes localTypes = new LocalTypes(method);
            if (V1.CouldBeResourceDecrypter(method, localTypes, additionalTypes))
            {
                return DnrDecrypterType.V1;
            }
            if (V2.CouldBeResourceDecrypter(localTypes, additionalTypes))
            {
                return DnrDecrypterType.V2;
            }
            if (V3.CouldBeResourceDecrypter(localTypes, additionalTypes))
            {
                return DnrDecrypterType.V3;
            }
            return DnrDecrypterType.Unknown;
        }

        public class V1
        {
            readonly byte[] key, iv;
            public V1(byte[] IV, byte[] KEY)
            {
                iv = IV;
                key = KEY;
            }
            public static bool CouldBeResourceDecrypter(MethodDef method, LocalTypes localTypes, IList<string> additionalTypes)
            {
                List<string> requiredTypes = new List<string>
                {
                    "System.Byte[]",
                    "System.Security.Cryptography.CryptoStream",
                    "System.Security.Cryptography.ICryptoTransform",
                    "System.String",
                    "System.Boolean"
                };
                List<string> requiredTypes2 = new List<string>
                {
                    "System.Security.Cryptography.ICryptoTransform",
                    "System.IO.Stream",
                    "System.Int32",
                    "System.Byte[]",
                    "System.Boolean"
                };
                requiredTypes.AddRange(additionalTypes);
                return (localTypes.All(requiredTypes) || localTypes.All(requiredTypes2)) && (DotNetUtils.GetMethod(method.DeclaringType, "System.Security.Cryptography.SymmetricAlgorithm", "()") == null || (!localTypes.Exists("System.UInt64") && (!localTypes.Exists("System.UInt32") || localTypes.Exists("System.Reflection.Assembly"))));
            }
            public byte[] Decrypt(EmbeddedResource resource)
            {
                return DeobUtils.AesDecrypt(resource.CreateReader().ToArray(), key, iv);
            }
        }

        public class V2
        {
            public V2(byte[] IV, byte[] KEY, MethodDef Method)
            {
                iv = IV;
                key = KEY;
                method = Method;
                locals = new List<Local>(method.Body.Variables);
                if (!Initialize())
                {
                    throw new ApplicationException("Could not initialize decrypter");
                }
            }
            bool Initialize()
            {
                IList<Instruction> origInstrs = method.Body.Instructions;
                if (!Find(origInstrs, out int emuStartIndex, out int emuEndIndex, out emuLocal) && !FindStartEnd(origInstrs, out emuStartIndex, out emuEndIndex, out emuLocal))
                {
                    if (!FindStartEnd2(ref origInstrs, out emuStartIndex, out emuEndIndex, out emuLocal, out emuArg, ref emuMethod, ref locals))
                    {
                        return false;
                    }
                    isNewDecrypter = true;
                }
                if (!isNewDecrypter)
                {
                    for (int i = 0; i < iv.Length; i++)
                    {
                        byte[] array = key;
                        int num = i;
                        array[num] ^= iv[i];
                    }
                }
                int count = emuEndIndex - emuStartIndex + 1;
                instructions = new List<Instruction>(count);
                for (int j = 0; j < count; j++)
                {
                    instructions.Add(origInstrs[emuStartIndex + j].Clone());
                }
                return true;
            }
            Local CheckLocal(Instruction instr, bool isLdloc)
            {
                if (isLdloc && !instr.IsLdloc())
                {
                    return null;
                }
                if (!isLdloc && !instr.IsStloc())
                {
                    return null;
                }
                return instr.GetLocal(locals);
            }
            bool Find(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
            {
                startIndex = 0;
                endIndex = 0;
                tmpLocal = null;
                if (!FindStart(instrs, out int emuStartIndex, out emuLocal))
                {
                    return false;
                }
                if (!FindEnd(instrs, emuStartIndex, out int emuEndIndex))
                {
                    return false;
                }
                startIndex = emuStartIndex;
                endIndex = emuEndIndex;
                tmpLocal = emuLocal;
                return true;
            }
            bool FindEnd(IList<Instruction> instrs, int startIndex, out int endIndex)
            {
                for (int i = startIndex; i < instrs.Count; i++)
                {
                    Instruction instr = instrs[i];
                    if (instr.OpCode.FlowControl != FlowControl.Next)
                    {
                        break;
                    }
                    if (instr.IsStloc() && instr.GetLocal(locals) == emuLocal)
                    {
                        endIndex = i - 1;
                        return true;
                    }
                }
                endIndex = 0;
                return false;
            }
            bool FindStart(IList<Instruction> instrs, out int startIndex, out Local tmpLocal)
            {
                int i = 0;
                while (i + 8 < instrs.Count)
                {
                    Local local;
                    if (instrs[i].OpCode.Code.Equals(Code.Conv_U) && instrs[i + 1].OpCode.Code.Equals(Code.Ldelem_U1) && instrs[i + 2].OpCode.Code.Equals(Code.Or) && CheckLocal(instrs[i + 3], false) != null && (local = CheckLocal(instrs[i + 4], true)) != null && CheckLocal(instrs[i + 5], true) != null && instrs[i + 6].OpCode.Code.Equals(Code.Add) && CheckLocal(instrs[i + 7], false) == local)
                    {
                        Instruction instr = instrs[i + 8];
                        int newStartIndex = i + 8;
                        if (instr.IsBr())
                        {
                            instr = (instr.Operand as Instruction);
                            newStartIndex = instrs.IndexOf(instr);
                        }
                        if (newStartIndex >= 0 && instr != null && CheckLocal(instr, true) == local)
                        {
                            startIndex = newStartIndex;
                            tmpLocal = local;
                            return true;
                        }
                    }
                    i++;
                }
                startIndex = 0;
                tmpLocal = null;
                return false;
            }
            bool FindStartEnd(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
            {
                int i = 0;
                while (i + 8 < instrs.Count)
                {
                    if (instrs[i].OpCode.Code.Equals(Code.Conv_R_Un) && instrs[i + 1].OpCode.Code.Equals(Code.Conv_R8) && instrs[i + 2].OpCode.Code.Equals(Code.Conv_U4) && instrs[i + 3].OpCode.Code.Equals(Code.Add))
                    {
                        int newEndIndex = i + 3;
                        int newStartIndex = -1;
                        for (int x = newEndIndex; x > 0; x--)
                        {
                            if (instrs[x].OpCode.FlowControl != FlowControl.Next)
                            {
                                newStartIndex = x + 1;
                                break;
                            }
                        }
                        if (newStartIndex >= 0)
                        {
                            List<Local> checkLocs = new List<Local>();
                            int ckStartIndex = -1;
                            for (int y = newEndIndex; y >= newStartIndex; y--)
                            {
                                Local loc = CheckLocal(instrs[y], true);
                                if (loc != null)
                                {
                                    if (!checkLocs.Contains(loc))
                                    {
                                        checkLocs.Add(loc);
                                    }
                                    if (checkLocs.Count == 3)
                                    {
                                        break;
                                    }
                                    ckStartIndex = y;
                                }
                            }
                            endIndex = newEndIndex;
                            startIndex = Math.Max(ckStartIndex, newStartIndex);
                            tmpLocal = CheckLocal(instrs[startIndex], true);
                            return true;
                        }
                    }
                    i++;
                }
                endIndex = 0;
                startIndex = 0;
                tmpLocal = null;
                return false;
            }
            bool FindStartEnd2(ref IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal, out Parameter tmpArg, ref MethodDef methodDef, ref List<Local> locals)
            {
                foreach (Instruction instr in instrs)
                {
                    MethodDef method;
                    if (instr.OpCode.Equals(OpCodes.Call) && (method = (instr.Operand as MethodDef)) != null && method.ReturnType.FullName == "System.Byte[]")
                    {
                        using IEnumerator<IMethod> enumerator2 = DotNetUtils.GetMethodCalls(method).GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            MethodDef calledMethod;
                            if ((calledMethod = (enumerator2.Current as MethodDef)) != null && calledMethod.Parameters.Count == 2)
                            {
                                instrs = calledMethod.Body.Instructions;
                                methodDef = calledMethod;
                                locals = new List<Local>(calledMethod.Body.Variables);
                                startIndex = 0;
                                endIndex = instrs.Count - 1;
                                tmpLocal = null;
                                tmpArg = calledMethod.Parameters[1];
                                return true;
                            }
                        }
                    }
                }
                endIndex = 0;
                startIndex = 0;
                tmpLocal = null;
                tmpArg = null;
                return false;
            }
            public static bool CouldBeResourceDecrypter(LocalTypes localTypes, IList<string> additionalTypes)
            {
                List<string> requiredTypes = new List<string>
                {
                    "System.Int32",
                    "System.Byte[]"
                };
                requiredTypes.AddRange(additionalTypes);
                return localTypes.All(requiredTypes);
            }
            public byte[] Decrypt(EmbeddedResource resource)
            {
                byte[] encrypted = resource.CreateReader().ToArray();
                byte[] decrypted = new byte[encrypted.Length];
                uint sum = 0U;
                if (isNewDecrypter)
                {
                    for (int i = 0; i < encrypted.Length; i += 4)
                    {
                        uint value = ReadUInt32(key, i % key.Length);
                        sum += value + CalculateMagic(sum + value);
                        WriteUInt32(decrypted, i, sum ^ ReadUInt32(encrypted, i));
                    }
                }
                else
                {
                    for (int j = 0; j < encrypted.Length; j += 4)
                    {
                        sum = CalculateMagic(sum + ReadUInt32(key, j % key.Length));
                        WriteUInt32(decrypted, j, sum ^ ReadUInt32(encrypted, j));
                    }
                }
                return decrypted;
            }
            uint ReadUInt32(byte[] ary, int index)
            {
                int sizeLeft = ary.Length - index;
                if (sizeLeft >= 4)
                {
                    return BitConverter.ToUInt32(ary, index);
                }
                return sizeLeft switch
                {
                    1 => (uint)ary[index],
                    2 => (uint)((int)ary[index] | (int)ary[index + 1] << 8),
                    3 => (uint)((int)ary[index] | (int)ary[index + 1] << 8 | (int)ary[index + 2] << 16),
                    _ => throw new ApplicationException("Can't read data"),
                };
            }
            void WriteUInt32(byte[] ary, int index, uint value)
            {
                int num = ary.Length - index;
                if (num >= 1)
                {
                    ary[index] = (byte)value;
                }
                if (num >= 2)
                {
                    ary[index + 1] = (byte)(value >> 8);
                }
                if (num >= 3)
                {
                    ary[index + 2] = (byte)(value >> 16);
                }
                if (num >= 4)
                {
                    ary[index + 3] = (byte)(value >> 24);
                }
            }
            public uint CalculateMagic(uint input)
            {
                if (emuArg == null)
                {
                    instrEmulator.Initialize(method, method.Parameters, locals, method.Body.InitLocals, false);
                    instrEmulator.SetLocal(emuLocal, new Int32Value((int)input));
                }
                else
                {
                    instrEmulator.Initialize(emuMethod, emuMethod.Parameters, locals, emuMethod.Body.InitLocals, false);
                    instrEmulator.SetArg(emuArg, new Int32Value((int)input));
                }
                foreach (Instruction instr in instructions)
                {
                    instrEmulator.Emulate(instr);
                }
                if (!(instrEmulator.Pop() is Int32Value tos) || !tos.AllBitsValid())
                {
                    throw new ApplicationException("Couldn't calculate magic value");
                }
                return (uint)tos.Value;
            }

            bool isNewDecrypter;
            readonly InstructionEmulator instrEmulator = new InstructionEmulator();
            Parameter emuArg;
            readonly byte[] key, iv;
            readonly MethodDef method;
            List<Local> locals;
            Local emuLocal;
            MethodDef emuMethod;
            List<Instruction> instructions;
        }

        public class V3
        {
            public V3(MethodDef Method)
            {
                method = Method;
                locals = new List<Local>(method.Body.Variables);
                if (!Initialize())
                {
                    throw new ApplicationException("Could not initialize decrypter");
                }
            }
            bool Initialize()
            {
                IList<Instruction> origInstrs = method.Body.Instructions;
                if (!Find(origInstrs, out int emuStartIndex, out int emuEndIndex, out emuLocal) && !FindStartEnd(origInstrs, out emuStartIndex, out emuEndIndex, out emuLocal))
                {
                    return false;
                }
                int count = emuEndIndex - emuStartIndex + 1;
                instructions = new List<Instruction>(count);
                for (int i = 0; i < count; i++)
                {
                    instructions.Add(origInstrs[emuStartIndex + i].Clone());
                }
                return true;
            }
            public byte[] Decrypt(EmbeddedResource resource)
            {
                byte[] encrypted = resource.CreateReader().ToArray();
                byte[] decrypted = new byte[encrypted.Length];
                uint sum = 0U;
                for (int i = 0; i < encrypted.Length; i += 4)
                {
                    sum = CalculateMagic(sum);
                    WriteUInt32(decrypted, i, sum ^ ReadUInt32(encrypted, i));
                }
                return decrypted;
            }
            uint CalculateMagic(uint input)
            {
                instrEmulator.Initialize(method, method.Parameters, locals, method.Body.InitLocals, false);
                instrEmulator.SetLocal(emuLocal, new Int32Value((int)input));
                foreach (Instruction instr in instructions)
                {
                    instrEmulator.Emulate(instr);
                }
                if (!(instrEmulator.Pop() is Int32Value tos) || !tos.AllBitsValid())
                {
                    throw new ApplicationException("Couldn't calculate magic value");
                }
                return (uint)tos.Value;
            }
            uint ReadUInt32(byte[] ary, int index)
            {
                int sizeLeft = ary.Length - index;
                if (sizeLeft >= 4)
                {
                    return BitConverter.ToUInt32(ary, index);
                }
                return sizeLeft switch
                {
                    1 => (uint)ary[index],
                    2 => (uint)((int)ary[index] | (int)ary[index + 1] << 8),
                    3 => (uint)((int)ary[index] | (int)ary[index + 1] << 8 | (int)ary[index + 2] << 16),
                    _ => throw new ApplicationException("Can't read data"),
                };
            }
            void WriteUInt32(byte[] ary, int index, uint value)
            {
                int num = ary.Length - index;
                if (num >= 1) ary[index] = (byte)value;
                if (num >= 2) ary[index + 1] = (byte)(value >> 8);
                if (num >= 3) ary[index + 2] = (byte)(value >> 16);
                if (num >= 4) ary[index + 3] = (byte)(value >> 24);
            }
            bool Find(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
            {
                startIndex = 0;
                endIndex = 0;
                tmpLocal = null;
                if (!FindStart(instrs, out int emuStartIndex, out emuLocal)) return false;
                if (!FindEnd(instrs, emuStartIndex, out int emuEndIndex)) return false;
                startIndex = emuStartIndex;
                endIndex = emuEndIndex;
                tmpLocal = emuLocal;
                return true;
            }
            bool FindEnd(IList<Instruction> instrs, int startIndex, out int endIndex)
            {
                for (int i = startIndex; i < instrs.Count; i++)
                {
                    Instruction instr = instrs[i];
                    if (instr.OpCode.FlowControl != FlowControl.Next) break;
                    if (instr.IsStloc() && instr.GetLocal(locals) == emuLocal)
                    {
                        endIndex = i - 1;
                        return true;
                    }
                }
                endIndex = 0;
                return false;
            }
            bool FindStart(IList<Instruction> instrs, out int startIndex, out Local tmpLocal)
            {
                int i = 0;
                while (i + 8 < instrs.Count)
                {
                    Local local;
                    if (instrs[i].OpCode.Code.Equals(Code.Conv_U) && instrs[i + 1].OpCode.Code.Equals(Code.Ldelem_U1) && instrs[i + 2].OpCode.Code.Equals(Code.Or) && CheckLocal(instrs[i + 3], false) != null && (local = CheckLocal(instrs[i + 4], true)) != null && CheckLocal(instrs[i + 5], true) != null && instrs[i + 6].OpCode.Code.Equals(Code.Add) && CheckLocal(instrs[i + 7], false) == local)
                    {
                        Instruction instr = instrs[i + 8];
                        int newStartIndex = i + 8;
                        if (instr.IsBr())
                        {
                            instr = (instr.Operand as Instruction);
                            newStartIndex = instrs.IndexOf(instr);
                        }
                        if (newStartIndex >= 0 && instr != null && CheckLocal(instr, true) == local)
                        {
                            startIndex = newStartIndex;
                            tmpLocal = local;
                            return true;
                        }
                    }
                    i++;
                }
                startIndex = 0;
                tmpLocal = null;
                return false;
            }
            bool FindStartEnd(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
            {
                int i = 0;
                while (i + 8 < instrs.Count)
                {
                    if (instrs[i].OpCode.Code.Equals(Code.Conv_R_Un) && instrs[i + 1].OpCode.Code.Equals(Code.Conv_R8) && instrs[i + 2].OpCode.Code.Equals(Code.Conv_U4) && instrs[i + 3].OpCode.Code.Equals(Code.Add))
                    {
                        int newEndIndex = i + 3;
                        int newStartIndex = -1;
                        for (int x = newEndIndex; x > 0; x--)
                        {
                            if (instrs[x].OpCode.FlowControl != FlowControl.Next)
                            {
                                newStartIndex = x + 1;
                                break;
                            }
                        }
                        if (newStartIndex >= 0)
                        {
                            List<Local> checkLocs = new List<Local>();
                            int ckStartIndex = -1;
                            for (int y = newEndIndex; y >= newStartIndex; y--)
                            {
                                Local loc = CheckLocal(instrs[y], true);
                                if (loc != null)
                                {
                                    if (!checkLocs.Contains(loc))
                                    {
                                        checkLocs.Add(loc);
                                    }
                                    if (checkLocs.Count == 3)
                                    {
                                        break;
                                    }
                                    ckStartIndex = y;
                                }
                            }
                            endIndex = newEndIndex;
                            startIndex = Math.Max(ckStartIndex, newStartIndex);
                            tmpLocal = CheckLocal(instrs[startIndex], true);
                            return true;
                        }
                    }
                    i++;
                }
                endIndex = 0;
                startIndex = 0;
                tmpLocal = null;
                return false;
            }
            Local CheckLocal(Instruction instr, bool isLdloc)
            {
                if (isLdloc && !instr.IsLdloc()) return null;
                if (!isLdloc && !instr.IsStloc()) return null;
                return instr.GetLocal(locals);
            }
            public static bool CouldBeResourceDecrypter(LocalTypes localTypes, IList<string> additionalTypes)
            {
                List<string> requiredTypes = new List<string>
                {
                    "System.Reflection.Emit.DynamicMethod",
                    "System.Reflection.Emit.ILGenerator"
                };
                requiredTypes.AddRange(additionalTypes);
                return localTypes.All(requiredTypes);
            }

            readonly MethodDef method;
            readonly List<Local> locals;
            Local emuLocal;
            List<Instruction> instructions;
            readonly InstructionEmulator instrEmulator = new InstructionEmulator();
        }

        readonly static int[] pktIndexes = new int[]
  {
            1,
            0,
            3,
            1,
            5,
            2,
            7,
            3,
            9,
            4,
            11,
            5,
            13,
            6,
            15,
            7
  };
        internal enum DnrDecrypterType
        {
            Unknown,
            V1,
            V2,
            V3
        }
    }
}
