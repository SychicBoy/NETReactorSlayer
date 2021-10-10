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
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NETReactorSlayer.Core.Protections
{
    class HideCall
    {
        static List<Local> locals;
        static readonly InstructionEmulator instrEmulator = new InstructionEmulator();
        static MethodDef method;
        static Local emuLocal;
        static List<Instruction> instructions;
        static readonly List<MethodDef> delegateCreatorMethods = new List<MethodDef>();
        static EmbeddedResource encryptedResource;
        static Dictionary<int, int> dictionary;

        public static void Execute()
        {
            FindDelegateCreator();
            if (delegateCreatorMethods.Count < 1)
            {
                Logger.Warn("Couldn't find any hidden call.");
                return;
            }
            encryptedResource = FindMethodsDecrypterResource(delegateCreatorMethods.First());
            method = delegateCreatorMethods.First();
            if (method == null || method.Body.Variables == null || method.Body.Variables.Count < 1)
            {
                Logger.Warn("Couldn't find any hidden call.");
                return;
            }
            locals = new List<Local>(method.Body.Variables);
            IList<Instruction> origInstrs = method.Body.Instructions;
            if (!Find(method.Body.Instructions, out int startIndex, out int endIndex, out emuLocal))
            {
                if (!FindStartEnd(origInstrs, out startIndex, out endIndex, out emuLocal))
                {
                    Logger.Warn("Couldn't find any hidden call.");
                    return;
                }
            }
            int num = endIndex - startIndex + 1;
            instructions = new List<Instruction>(num);
            for (int i = 0; i < num; i++) instructions.Add(origInstrs[startIndex + i].Clone());
            GetDictionary();
            long count = 0L;
            foreach (TypeDef type in Context.Module.GetTypes())
            {
                foreach (MethodDef method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x).ToArray<MethodDef>())
                {
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        IField field = null;
                        try
                        {
                            if (method.Body.Instructions[i].OpCode.Equals(OpCodes.Ldsfld) && method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Call))
                            {
                                field = method.Body.Instructions[i].Operand as IField;
                                GetCallInfo(field, out IMethod iMethod, out OpCode opCpde);
                                if (iMethod != null)
                                {
                                    iMethod = Context.Module.Import(iMethod);
                                    if (iMethod != null)
                                    {
                                        method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                        method.Body.Instructions[i + 1] = Instruction.Create(opCpde, iMethod);
                                        method.Body.UpdateInstructionOffsets();
                                        count += 1L;
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (count > 0L) Logger.Info((int)count + " Hidden calls restored.");
            else Logger.Warn("Couldn't find any hidden call.");
        }

        static bool Find(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
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

        static bool FindStartEnd(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
        {
            int i = 0;
            while (i + 8 < instrs.Count)
            {
                if (instrs[i].OpCode.Code == Code.Conv_R_Un)
                {
                    if (instrs[i + 1].OpCode.Code == Code.Conv_R8)
                    {
                        if (instrs[i + 2].OpCode.Code == Code.Conv_U4)
                        {
                            if (instrs[i + 3].OpCode.Code == Code.Add)
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
                                if (newStartIndex > 0)
                                {
                                    List<Local> checkLocs = new List<Local>();
                                    int ckStartIndex = -1;
                                    for (int y = newEndIndex; y >= newStartIndex; y--)
                                    {
                                        Local loc = CheckLocal(instrs[y], true);
                                        if (loc != null)
                                        {
                                            if (!checkLocs.Contains(loc)) checkLocs.Add(loc);
                                            if (checkLocs.Count == 3) break;
                                            ckStartIndex = y;
                                        }
                                    }
                                    endIndex = newEndIndex;
                                    startIndex = Math.Max(ckStartIndex, newStartIndex);
                                    tmpLocal = CheckLocal(instrs[startIndex], true);
                                    return true;
                                }
                            }
                        }
                    }
                }
                i++;
            }
            endIndex = 0; startIndex = 0; tmpLocal = null; return false;
        }

        static bool FindStart(IList<Instruction> instrs, out int startIndex, out Local tmpLocal)
        {
            int i = 0;
            while (i + 8 < instrs.Count)
            {
                if (instrs[i].OpCode.Code == Code.Conv_U)
                {
                    if (instrs[i + 1].OpCode.Code == Code.Ldelem_U1)
                    {
                        if (instrs[i + 2].OpCode.Code == Code.Or)
                        {
                            if (CheckLocal(instrs[i + 3], false) != null)
                            {
                                Local local;
                                if ((local = CheckLocal(instrs[i + 4], true)) != null)
                                {
                                    if (CheckLocal(instrs[i + 5], true) != null)
                                    {
                                        if (instrs[i + 6].OpCode.Code == Code.Add)
                                        {
                                            if (CheckLocal(instrs[i + 7], false) == local)
                                            {
                                                Instruction instr = instrs[i + 8];
                                                int newStartIndex = i + 8;
                                                if (instr.IsBr())
                                                {
                                                    instr = (instr.Operand as Instruction);
                                                    newStartIndex = instrs.IndexOf(instr);
                                                }
                                                if (newStartIndex > 0 && instr != null)
                                                {
                                                    if (CheckLocal(instr, true) == local)
                                                    {
                                                        startIndex = newStartIndex;
                                                        tmpLocal = local;
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                i++;
            }
            startIndex = 0;
            tmpLocal = null;
            return false;
        }

        static Local CheckLocal(Instruction instr, bool isLdloc)
        {
            if (isLdloc && !instr.IsLdloc()) return null;
            if (!isLdloc && !instr.IsStloc()) return null;
            return instr.GetLocal(locals);
        }

        static bool FindEnd(IList<Instruction> instrs, int startIndex, out int endIndex)
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

        static EmbeddedResource FindMethodsDecrypterResource(MethodDef method)
        {
            foreach (string s in DotNetUtils.GetCodeStrings(method))
            {
                if (DotNetUtils.GetResource(Context.Module, s) is EmbeddedResource resource) return resource;
            }
            return null;
        }

        static void GetCallInfo(IField field, out IMethod calledMethod, out OpCode callOpcode)
        {
            callOpcode = OpCodes.Call;
            dictionary.TryGetValue((int)field.MDToken.Raw, out int token);
            if ((token & 1073741824) > 0) callOpcode = OpCodes.Callvirt;
            token &= 1073741823;
            calledMethod = (Context.Module.ResolveToken(token) as IMethod);
        }

        static void GetDictionary()
        {
            byte[] resource = Decrypt();
            int length = resource.Length / 8;
            dictionary = new Dictionary<int, int>();
            BinaryReader reader = new BinaryReader(new MemoryStream(resource));
            for (int i = 0; i < length; i++)
            {
                int key = reader.ReadInt32();
                int value = reader.ReadInt32();
                if (!dictionary.ContainsKey(key)) dictionary.Add(key, value);
            }
            reader.Close();
        }

        static void FindDelegateCreator()
        {
            CallCounter callCounter = new CallCounter();
            foreach (TypeDef type in (from x in Context.Module.GetTypes() where x.Namespace.Equals("") && DotNetUtils.DerivesFromDelegate(x) select x))
            {
                MethodDef cctor = type.FindStaticConstructor();
                if (cctor != null)
                {
                    foreach (IMethod method in DotNetUtils.GetMethodCalls(cctor))
                    {
                        if (method.MethodSig.GetParamCount() == 1 && method.GetParam(0).FullName == "System.RuntimeTypeHandle")
                            callCounter.Add(method);
                    }
                }
            }
            IMethod mostCalls = callCounter.Most();
            if (mostCalls != null) delegateCreatorMethods.Add(DotNetUtils.GetMethod(Context.Module, mostCalls));
        }

        static byte[] Decrypt()
        {
            byte[] encrypted = encryptedResource.CreateReader().ToArray();
            byte[] decrypted = new byte[encrypted.Length];
            uint sum = 0U;
            for (int i = 0; i < encrypted.Length; i += 4)
            {
                sum = CalculateMagic(sum);
                WriteUInt32(decrypted, i, sum ^ ReadUInt32(encrypted, i));
            }
            Remover.ResourceToRemove.Add(encryptedResource);
            return decrypted;
        }

        static uint ReadUInt32(byte[] ary, int index)
        {
            int sizeLeft = ary.Length - index;
            if (sizeLeft >= 4) return BitConverter.ToUInt32(ary, index);
            switch (sizeLeft)
            {
                case 1:
                    return (uint)ary[index];
                case 2:
                    return (uint)((int)ary[index] | (int)ary[index + 1] << 8);
                case 3:
                    return (uint)((int)ary[index] | (int)ary[index + 1] << 8 | (int)ary[index + 2] << 16);
                default:
                    throw new ApplicationException("Can't read data");
            }
        }
        static void WriteUInt32(byte[] ary, int index, uint value)
        {
            int num = ary.Length - index;
            if (num >= 1) ary[index] = (byte)value;
            if (num >= 2) ary[index + 1] = (byte)(value >> 8);
            if (num >= 3) ary[index + 2] = (byte)(value >> 16);
            if (num >= 4) ary[index + 3] = (byte)(value >> 24);
        }
        static uint CalculateMagic(uint input)
        {
            instrEmulator.Initialize(method, method.Parameters, locals, method.Body.InitLocals, false);
            instrEmulator.SetLocal(emuLocal, new Int32Value((int)input));
            foreach (Instruction instr in instructions)
            {
                instrEmulator.Emulate(instr);
            }
            if (!(instrEmulator.Pop() is Int32Value tos) || !tos.AllBitsValid()) throw new Exception("Couldn't calculate magic value");
            return (uint)tos.Value;
        }
    }
}
