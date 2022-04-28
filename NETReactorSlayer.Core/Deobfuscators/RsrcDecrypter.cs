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
using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Helper.De4dot;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class RsrcDecrypter : IStage
{
    private static readonly int[] PktIndexes =
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

    public void Execute()
    {
        TypeDef typeDef;
        byte[] decryptedBytes;
        EmbeddedResource encryptedResource;
        var methodsToPatch = new HashSet<MethodDef>();
        foreach (var type in Context.Module.GetTypes())
            if (type.FindMethod(".ctor") is {HasBody: true} method1 && method1.Body.HasInstructions)
                for (var i = 0; i < method1.Body.Instructions.Count; i++)
                    if (method1.Body.Instructions[i].OpCode.Equals(OpCodes.Newobj) &&
                        method1.Body.Instructions[i].Operand.ToString().Contains("System.ResolveEventHandler") &&
                        method1.Body.Instructions[i - 1].OpCode.Equals(OpCodes.Ldftn))
                        if (method1.Body.Instructions[i - 1].Operand is MethodDef method2 &&
                            method2.DeclaringType.Equals(type) && method2.HasReturnType &&
                            method2.ReturnType.FullName.Equals("System.Reflection.Assembly"))
                            foreach (var instruction in method2.Body.Instructions)
                                try
                                {
                                    if (instruction.OpCode.Equals(OpCodes.Call))
                                        if (instruction.Operand is MethodDef decryptorMethod &&
                                            decryptorMethod.DeclaringType.Equals(type) &&
                                            !decryptorMethod.HasReturnType)
                                            foreach (var s in DotNetUtils.GetCodeStrings(decryptorMethod))
                                                if ((encryptedResource =
                                                        DotNetUtils.GetResource(Context.Module, s) as
                                                            EmbeddedResource) != null)
                                                {
                                                    foreach (var methodToRemove in type.Methods)
                                                        methodsToPatch.Add(methodToRemove);
                                                    typeDef = type;
                                                    var decrypterType =
                                                        GetDecrypterType(decryptorMethod, Array.Empty<string>());
                                                    var key = ArrayFinder.GetInitializedByteArray(decryptorMethod,
                                                        32);
                                                    if (decrypterType == DnrDecrypterType.V3)
                                                    {
                                                        var v3 = new V3(decryptorMethod);
                                                        decryptedBytes = v3.Decrypt(encryptedResource);
                                                        goto Decompress;
                                                    }

                                                    var iv = ArrayFinder.GetInitializedByteArray(decryptorMethod,
                                                        16);
                                                    if (DotNetUtils.CallsMethod(decryptorMethod,
                                                            "System.Array::Reverse"))
                                                        Array.Reverse(iv);
                                                    if (UsesPublicKeyToken(decryptorMethod))
                                                        if (Context.Module.Assembly
                                                                .PublicKeyToken is { } publicKeyToken &&
                                                            publicKeyToken.Data.Length != 0)
                                                            for (var z = 0; z < 8; z++)
                                                                iv[z * 2 + 1] = publicKeyToken.Data[z];

                                                    if (decrypterType == DnrDecrypterType.V1)
                                                    {
                                                        var v1 = new V1(iv, key);
                                                        decryptedBytes = v1.Decrypt(encryptedResource);
                                                        goto Decompress;
                                                    }

                                                    if (decrypterType == DnrDecrypterType.V2)
                                                    {
                                                        var v2 = new V2(iv, key, decryptorMethod);
                                                        decryptedBytes = v2.Decrypt(encryptedResource);
                                                        goto Decompress;
                                                    }
                                                }
                                } catch { }

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
            DeobUtils.DecryptAndAddResources(Context.Module, delegate
            {
                byte[] result;
                try
                {
                    result = QuickLZ.Decompress(decryptedBytes);
                } catch
                {
                    try
                    {
                        result = DeobUtils.Inflate(decryptedBytes, true);
                    } catch
                    {
                        result = null;
                    }
                }

                return result;
            });
            foreach (var m in methodsToPatch) Cleaner.MethodsToRemove.Add(m);
            Cleaner.TypesToRemove.Add(typeDef);
            Cleaner.ResourceToRemove.Add(encryptedResource);
            Logger.Done("Assembly resources decrypted");
        } catch (Exception ex)
        {
            Logger.Error("Failed to decrypt resources. " + ex.Message);
        }
    }

    public static byte[] GetBytes(MethodDef method, int size)
    {
        var result = new byte[size];
        Local local = null;
        if (method == null || !method.HasBody || !method.Body.HasInstructions) return null;
        for (var i = 0; i < method.Body.Instructions.Count; i++)
            if (local == null && method.Body.Instructions[i].OpCode.Equals(OpCodes.Newarr) &&
                method.Body.Instructions[i].Operand.ToString().Equals("System.Byte") &&
                method.Body.Instructions[i - 1].OpCode.Equals(OpCodes.Ldc_I4) &&
                method.Body.Instructions[i - 1].GetLdcI4Value() == size &&
                method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Stloc))
            {
                local = method.Body.Instructions[i + 1].Operand as Local;
                i = 0;
            }
            else if (method.Body.Instructions[i].IsLdloc() &&
                     method.Body.Instructions[i].Operand as Local == local &&
                     method.Body.Instructions[i + 1].IsLdcI4() &&
                     method.Body.Instructions[i + 2].IsLdcI4() &&
                     method.Body.Instructions[i + 3].OpCode.Equals(OpCodes.Stelem_I1))
            {
                try
                {
                    result[method.Body.Instructions[i + 1].GetLdcI4Value()] =
                        (byte) method.Body.Instructions[i + 2].GetLdcI4Value();
                } catch
                {
                    return result;
                }
            }

        return result;
    }

    public static bool IsNeedReverse(MethodDef method)
    {
        if (method is {HasBody: true} && method.Body.HasInstructions)
            foreach (var instr in method.Body.Instructions)
                try
                {
                    if (instr.Operand is IMethod calledMethod &&
                        calledMethod.FullName.Contains("System.Array::Reverse"))
                        return true;
                } catch { }

        return false;
    }

    public static bool UsesPublicKeyToken(MethodDef resourceDecrypterMethod)
    {
        var pktIndex = 0;
        foreach (var instr in resourceDecrypterMethod.Body.Instructions)
            if (instr.OpCode.FlowControl != FlowControl.Next)
            {
                pktIndex = 0;
            }
            else if (instr.IsLdcI4())
            {
                if (instr.GetLdcI4Value() != PktIndexes[pktIndex++]) pktIndex = 0;
                else if (pktIndex >= PktIndexes.Length) return true;
            }

        return false;
    }

    public static DnrDecrypterType GetDecrypterType(MethodDef method, IList<string> additionalTypes)
    {
        if (method == null || !method.IsStatic || method.Body == null) return DnrDecrypterType.Unknown;
        additionalTypes ??= Array.Empty<string>();
        var localTypes = new LocalTypes(method);
        if (V1.CouldBeResourceDecrypter(method, localTypes, additionalTypes)) return DnrDecrypterType.V1;
        if (V2.CouldBeResourceDecrypter(localTypes, additionalTypes)) return DnrDecrypterType.V2;
        if (V3.CouldBeResourceDecrypter(localTypes, additionalTypes)) return DnrDecrypterType.V3;
        return DnrDecrypterType.Unknown;
    }

    public class V1
    {
        private readonly byte[] _key, _iv;

        public V1(byte[] iv, byte[] key)
        {
            _iv = iv;
            _key = key;
        }

        public static bool CouldBeResourceDecrypter(
            MethodDef method, LocalTypes localTypes,
            IList<string> additionalTypes)
        {
            var requiredTypes = new List<string>
            {
                "System.Byte[]",
                "System.Security.Cryptography.CryptoStream",
                "System.Security.Cryptography.ICryptoTransform",
                "System.String",
                "System.Boolean"
            };
            var requiredTypes2 = new List<string>
            {
                "System.Security.Cryptography.ICryptoTransform",
                "System.IO.Stream",
                "System.Int32",
                "System.Byte[]",
                "System.Boolean"
            };
            requiredTypes.AddRange(additionalTypes);
            return (localTypes.All(requiredTypes) || localTypes.All(requiredTypes2)) &&
                   (DotNetUtils.GetMethod(method.DeclaringType, "System.Security.Cryptography.SymmetricAlgorithm",
                           "()") == null || !localTypes.Exists("System.UInt64") &&
                       (!localTypes.Exists("System.UInt32") || localTypes.Exists("System.Reflection.Assembly")));
        }

        public byte[] Decrypt(EmbeddedResource resource) =>
            DeobUtils.AesDecrypt(resource.CreateReader().ToArray(), _key, _iv);
    }

    public class V2
    {
        private readonly InstructionEmulator _instrEmulator = new();
        private readonly byte[] _key, _iv;
        private readonly MethodDef _method;
        private Parameter _emuArg;
        private Local _emuLocal;
        private MethodDef _emuMethod;
        private List<Instruction> _instructions;

        private bool _isNewDecrypter;
        private List<Local> _locals;

        public V2(byte[] iv, byte[] key, MethodDef method)
        {
            _iv = iv;
            _key = key;
            _method = method;
            _locals = new List<Local>(_method.Body.Variables);
            if (!Initialize()) throw new ApplicationException("Could not initialize decrypter");
        }

        private bool Initialize()
        {
            var origInstrs = _method.Body.Instructions;
            if (!Find(origInstrs, out var emuStartIndex, out var emuEndIndex, out _emuLocal) &&
                !FindStartEnd(origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal))
            {
                if (!FindStartEnd2(ref origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal, out _emuArg,
                        ref _emuMethod, ref _locals)) return false;
                _isNewDecrypter = true;
            }

            if (!_isNewDecrypter)
                for (var i = 0; i < _iv.Length; i++)
                {
                    var array = _key;
                    var num = i;
                    array[num] ^= _iv[i];
                }

            var count = emuEndIndex - emuStartIndex + 1;
            _instructions = new List<Instruction>(count);
            for (var j = 0; j < count; j++) _instructions.Add(origInstrs[emuStartIndex + j].Clone());
            return true;
        }

        private Local CheckLocal(Instruction instr, bool isLdloc)
        {
            if (isLdloc && !instr.IsLdloc()) return null;
            if (!isLdloc && !instr.IsStloc()) return null;
            return instr.GetLocal(_locals);
        }

        private bool Find(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
        {
            startIndex = 0;
            endIndex = 0;
            tmpLocal = null;
            if (!FindStart(instrs, out var emuStartIndex, out _emuLocal)) return false;
            if (!FindEnd(instrs, emuStartIndex, out var emuEndIndex)) return false;
            startIndex = emuStartIndex;
            endIndex = emuEndIndex;
            tmpLocal = _emuLocal;
            return true;
        }

        private bool FindEnd(IList<Instruction> instrs, int startIndex, out int endIndex)
        {
            for (var i = startIndex; i < instrs.Count; i++)
            {
                var instr = instrs[i];
                if (instr.OpCode.FlowControl != FlowControl.Next) break;
                if (instr.IsStloc() && instr.GetLocal(_locals) == _emuLocal)
                {
                    endIndex = i - 1;
                    return true;
                }
            }

            endIndex = 0;
            return false;
        }

        private bool FindStart(IList<Instruction> instrs, out int startIndex, out Local tmpLocal)
        {
            var i = 0;
            while (i + 8 < instrs.Count)
            {
                Local local;
                if (instrs[i].OpCode.Code.Equals(Code.Conv_U) && instrs[i + 1].OpCode.Code.Equals(Code.Ldelem_U1) &&
                    instrs[i + 2].OpCode.Code.Equals(Code.Or) && CheckLocal(instrs[i + 3], false) != null &&
                    (local = CheckLocal(instrs[i + 4], true)) != null && CheckLocal(instrs[i + 5], true) != null &&
                    instrs[i + 6].OpCode.Code.Equals(Code.Add) && CheckLocal(instrs[i + 7], false) == local)
                {
                    var instr = instrs[i + 8];
                    var newStartIndex = i + 8;
                    if (instr.IsBr())
                    {
                        instr = instr.Operand as Instruction;
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

        private bool FindStartEnd(
            IList<Instruction> instrs, out int startIndex, out int endIndex,
            out Local tmpLocal)
        {
            var i = 0;
            while (i + 8 < instrs.Count)
            {
                if (instrs[i].OpCode.Code.Equals(Code.Conv_R_Un) &&
                    instrs[i + 1].OpCode.Code.Equals(Code.Conv_R8) &&
                    instrs[i + 2].OpCode.Code.Equals(Code.Conv_U4) && instrs[i + 3].OpCode.Code.Equals(Code.Add))
                {
                    var newEndIndex = i + 3;
                    var newStartIndex = -1;
                    for (var x = newEndIndex; x > 0; x--)
                        if (instrs[x].OpCode.FlowControl != FlowControl.Next)
                        {
                            newStartIndex = x + 1;
                            break;
                        }

                    if (newStartIndex >= 0)
                    {
                        var checkLocs = new List<Local>();
                        var ckStartIndex = -1;
                        for (var y = newEndIndex; y >= newStartIndex; y--)
                            if (CheckLocal(instrs[y], true) is { } loc)
                            {
                                if (!checkLocs.Contains(loc)) checkLocs.Add(loc);
                                if (checkLocs.Count == 3) break;
                                ckStartIndex = y;
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

        private bool FindStartEnd2(
            ref IList<Instruction> instrs, out int startIndex, out int endIndex,
            out Local tmpLocal, out Parameter tmpArg, ref MethodDef methodDef, ref List<Local> locals)
        {
            foreach (var instr in instrs)
            {
                MethodDef method;
                if (instr.OpCode.Equals(OpCodes.Call) && (method = instr.Operand as MethodDef) != null &&
                    method.ReturnType.FullName == "System.Byte[]")
                {
                    using var enumerator2 = DotNetUtils.GetMethodCalls(method).GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        MethodDef calledMethod;
                        if ((calledMethod = enumerator2.Current as MethodDef) != null &&
                            calledMethod.Parameters.Count == 2)
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
            var requiredTypes = new List<string>
            {
                "System.Int32",
                "System.Byte[]"
            };
            requiredTypes.AddRange(additionalTypes);
            return localTypes.All(requiredTypes);
        }

        public byte[] Decrypt(EmbeddedResource resource)
        {
            var encrypted = resource.CreateReader().ToArray();
            var decrypted = new byte[encrypted.Length];
            var sum = 0U;
            if (_isNewDecrypter)
                for (var i = 0; i < encrypted.Length; i += 4)
                {
                    var value = ReadUInt32(_key, i % _key.Length);
                    sum += value + CalculateMagic(sum + value);
                    WriteUInt32(decrypted, i, sum ^ ReadUInt32(encrypted, i));
                }
            else
                for (var j = 0; j < encrypted.Length; j += 4)
                {
                    sum = CalculateMagic(sum + ReadUInt32(_key, j % _key.Length));
                    WriteUInt32(decrypted, j, sum ^ ReadUInt32(encrypted, j));
                }

            return decrypted;
        }

        private uint ReadUInt32(byte[] ary, int index)
        {
            var sizeLeft = ary.Length - index;
            if (sizeLeft >= 4) return BitConverter.ToUInt32(ary, index);
            return sizeLeft switch
            {
                1 => ary[index],
                2 => (uint) (ary[index] | (ary[index + 1] << 8)),
                3 => (uint) (ary[index] | (ary[index + 1] << 8) | (ary[index + 2] << 16)),
                _ => throw new ApplicationException("Can't read data")
            };
        }

        private void WriteUInt32(byte[] ary, int index, uint value)
        {
            var num = ary.Length - index;
            if (num >= 1) ary[index] = (byte) value;
            if (num >= 2) ary[index + 1] = (byte) (value >> 8);
            if (num >= 3) ary[index + 2] = (byte) (value >> 16);
            if (num >= 4) ary[index + 3] = (byte) (value >> 24);
        }

        public uint CalculateMagic(uint input)
        {
            if (_emuArg == null)
            {
                _instrEmulator.Initialize(_method, _method.Parameters, _locals, _method.Body.InitLocals, false);
                _instrEmulator.SetLocal(_emuLocal, new Int32Value((int) input));
            }
            else
            {
                _instrEmulator.Initialize(_emuMethod, _emuMethod.Parameters, _locals, _emuMethod.Body.InitLocals,
                    false);
                _instrEmulator.SetArg(_emuArg, new Int32Value((int) input));
            }

            foreach (var instr in _instructions) _instrEmulator.Emulate(instr);
            if (!(_instrEmulator.Pop() is Int32Value tos) || !tos.AllBitsValid())
                throw new ApplicationException("Couldn't calculate magic value");
            return (uint) tos.Value;
        }
    }

    public class V3
    {
        private readonly InstructionEmulator _instrEmulator = new();
        private readonly List<Local> _locals;

        private readonly MethodDef _method;
        private Local _emuLocal;
        private List<Instruction> _instructions;

        public V3(MethodDef method)
        {
            _method = method;
            _locals = new List<Local>(_method.Body.Variables);
            if (!Initialize()) throw new ApplicationException("Could not initialize decrypter");
        }

        private bool Initialize()
        {
            var origInstrs = _method.Body.Instructions;
            if (!Find(origInstrs, out var emuStartIndex, out var emuEndIndex, out _emuLocal) &&
                !FindStartEnd(origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal)) return false;
            var count = emuEndIndex - emuStartIndex + 1;
            _instructions = new List<Instruction>(count);
            for (var i = 0; i < count; i++) _instructions.Add(origInstrs[emuStartIndex + i].Clone());
            return true;
        }

        public byte[] Decrypt(EmbeddedResource resource)
        {
            var encrypted = resource.CreateReader().ToArray();
            var decrypted = new byte[encrypted.Length];
            var sum = 0U;
            for (var i = 0; i < encrypted.Length; i += 4)
            {
                sum = CalculateMagic(sum);
                WriteUInt32(decrypted, i, sum ^ ReadUInt32(encrypted, i));
            }

            return decrypted;
        }

        private uint CalculateMagic(uint input)
        {
            _instrEmulator.Initialize(_method, _method.Parameters, _locals, _method.Body.InitLocals, false);
            _instrEmulator.SetLocal(_emuLocal, new Int32Value((int) input));
            foreach (var instr in _instructions) _instrEmulator.Emulate(instr);
            if (!(_instrEmulator.Pop() is Int32Value tos) || !tos.AllBitsValid())
                throw new ApplicationException("Couldn't calculate magic value");
            return (uint) tos.Value;
        }

        private uint ReadUInt32(byte[] ary, int index)
        {
            var sizeLeft = ary.Length - index;
            if (sizeLeft >= 4) return BitConverter.ToUInt32(ary, index);
            return sizeLeft switch
            {
                1 => ary[index],
                2 => (uint) (ary[index] | (ary[index + 1] << 8)),
                3 => (uint) (ary[index] | (ary[index + 1] << 8) | (ary[index + 2] << 16)),
                _ => throw new ApplicationException("Can't read data")
            };
        }

        private void WriteUInt32(byte[] ary, int index, uint value)
        {
            var num = ary.Length - index;
            if (num >= 1) ary[index] = (byte) value;
            if (num >= 2) ary[index + 1] = (byte) (value >> 8);
            if (num >= 3) ary[index + 2] = (byte) (value >> 16);
            if (num >= 4) ary[index + 3] = (byte) (value >> 24);
        }

        private bool Find(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
        {
            startIndex = 0;
            endIndex = 0;
            tmpLocal = null;
            if (!FindStart(instrs, out var emuStartIndex, out _emuLocal)) return false;
            if (!FindEnd(instrs, emuStartIndex, out var emuEndIndex)) return false;
            startIndex = emuStartIndex;
            endIndex = emuEndIndex;
            tmpLocal = _emuLocal;
            return true;
        }

        private bool FindEnd(IList<Instruction> instrs, int startIndex, out int endIndex)
        {
            for (var i = startIndex; i < instrs.Count; i++)
            {
                var instr = instrs[i];
                if (instr.OpCode.FlowControl != FlowControl.Next) break;
                if (instr.IsStloc() && instr.GetLocal(_locals) == _emuLocal)
                {
                    endIndex = i - 1;
                    return true;
                }
            }

            endIndex = 0;
            return false;
        }

        private bool FindStart(IList<Instruction> instrs, out int startIndex, out Local tmpLocal)
        {
            var i = 0;
            while (i + 8 < instrs.Count)
            {
                Local local;
                if (instrs[i].OpCode.Code.Equals(Code.Conv_U) && instrs[i + 1].OpCode.Code.Equals(Code.Ldelem_U1) &&
                    instrs[i + 2].OpCode.Code.Equals(Code.Or) && CheckLocal(instrs[i + 3], false) != null &&
                    (local = CheckLocal(instrs[i + 4], true)) != null && CheckLocal(instrs[i + 5], true) != null &&
                    instrs[i + 6].OpCode.Code.Equals(Code.Add) && CheckLocal(instrs[i + 7], false) == local)
                {
                    var instr = instrs[i + 8];
                    var newStartIndex = i + 8;
                    if (instr.IsBr())
                    {
                        instr = instr.Operand as Instruction;
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

        private bool FindStartEnd(
            IList<Instruction> instrs, out int startIndex, out int endIndex,
            out Local tmpLocal)
        {
            var i = 0;
            while (i + 8 < instrs.Count)
            {
                if (instrs[i].OpCode.Code.Equals(Code.Conv_R_Un) &&
                    instrs[i + 1].OpCode.Code.Equals(Code.Conv_R8) &&
                    instrs[i + 2].OpCode.Code.Equals(Code.Conv_U4) && instrs[i + 3].OpCode.Code.Equals(Code.Add))
                {
                    var newEndIndex = i + 3;
                    var newStartIndex = -1;
                    for (var x = newEndIndex; x > 0; x--)
                        if (instrs[x].OpCode.FlowControl != FlowControl.Next)
                        {
                            newStartIndex = x + 1;
                            break;
                        }

                    if (newStartIndex >= 0)
                    {
                        var checkLocs = new List<Local>();
                        var ckStartIndex = -1;
                        for (var y = newEndIndex; y >= newStartIndex; y--)
                            if (CheckLocal(instrs[y], true) is { } loc)
                            {
                                if (!checkLocs.Contains(loc)) checkLocs.Add(loc);
                                if (checkLocs.Count == 3) break;
                                ckStartIndex = y;
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

        private Local CheckLocal(Instruction instr, bool isLdloc)
        {
            if (isLdloc && !instr.IsLdloc()) return null;
            if (!isLdloc && !instr.IsStloc()) return null;
            return instr.GetLocal(_locals);
        }

        public static bool CouldBeResourceDecrypter(LocalTypes localTypes, IList<string> additionalTypes)
        {
            var requiredTypes = new List<string>
            {
                "System.Reflection.Emit.DynamicMethod",
                "System.Reflection.Emit.ILGenerator"
            };
            requiredTypes.AddRange(additionalTypes);
            return localTypes.All(requiredTypes);
        }
    }

    internal enum DnrDecrypterType
    {
        Unknown,
        V1,
        V2,
        V3
    }
}