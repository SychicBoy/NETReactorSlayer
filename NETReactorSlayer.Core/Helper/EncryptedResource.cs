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
using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Helper
{
    internal class EncryptedResource : IDisposable
    {
        public EncryptedResource(MethodDef method, IList<string> additionalTypes)
        {
            SimpleDeobfuscator.Deobfuscate(method);
            DecrypterMethod = method;
            AdditionalTypes = additionalTypes;
            Decrypter = GetDecrypter();
            EmbeddedResource = GetEncryptedResource();
        }

        public EncryptedResource(MethodDef method)
        {
            SimpleDeobfuscator.Deobfuscate(method);
            DecrypterMethod = method;
            AdditionalTypes = Array.Empty<string>();
            Decrypter = GetDecrypter();
            EmbeddedResource = GetEncryptedResource();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            EmbeddedResource = null;
            Decrypter = null;
        }

        public static bool IsKnownDecrypter(MethodDef method, IList<string> additionalTypes, bool checkResource)
        {
            SimpleDeobfuscator.Deobfuscate(method);
            if (checkResource)
            {
                if (!method.HasBody || !method.Body.HasInstructions)
                    return false;

                if (!DotNetUtils.GetCodeStrings(method)
                        .Any(x => DotNetUtils.GetResource(Context.Module, x) is EmbeddedResource))
                    return false;
            }

            if (!method.IsStatic || !method.HasBody)
                return false;

            var localTypes = new LocalTypes(method);
            if (DecrypterV1.CouldBeResourceDecrypter(method, localTypes, additionalTypes))
                return true;

            if (DecrypterV3.CouldBeResourceDecrypter(localTypes, additionalTypes))
                return true;

            if (DecrypterV4.CouldBeResourceDecrypter(method, localTypes, additionalTypes))
                return true;

            if (DecrypterV2.CouldBeResourceDecrypter(localTypes, additionalTypes))
                return true;

            return false;
        }

        public byte[] Decrypt() => Decrypter.Decrypt(EmbeddedResource);

        #region Enums

        public enum DecrypterVersion
        {
            V69,
            V6X
        }

        #endregion

        #region Interfaces

        private interface IDecrypter
        {
            byte[] Decrypt(EmbeddedResource resource);
        }

        #endregion

        #region Private Methods

        private EmbeddedResource GetEncryptedResource()
        {
            if (!DecrypterMethod.HasBody || !DecrypterMethod.Body.HasInstructions)
                return null;

            foreach (var s in DotNetUtils.GetCodeStrings(DecrypterMethod))
                if (DotNetUtils.GetResource(Context.Module, s) is EmbeddedResource resource)
                    return resource;

            return null;
        }

        private IDecrypter GetDecrypter()
        {
            if (!DecrypterMethod.IsStatic || !DecrypterMethod.HasBody)
                return null;

            var localTypes = new LocalTypes(DecrypterMethod);

            if (DecrypterV1.CouldBeResourceDecrypter(DecrypterMethod, localTypes, AdditionalTypes))
                return new DecrypterV1(DecrypterMethod);

            if (DecrypterV3.CouldBeResourceDecrypter(localTypes, AdditionalTypes))
                return new DecrypterV3(DecrypterMethod);

            if (DecrypterV4.CouldBeResourceDecrypter(DecrypterMethod, localTypes, AdditionalTypes))
                return new DecrypterV4(DecrypterMethod);

            return DecrypterV2.CouldBeResourceDecrypter(localTypes, AdditionalTypes)
                ? new DecrypterV2(DecrypterMethod)
                : null;
        }

        private static byte[] GetDecryptionKey(MethodDef method) => ArrayFinder.GetInitializedByteArray(method, 32);

        private static byte[] GetDecryptionIV(MethodDef method)
        {
            var bytes = ArrayFinder.GetInitializedByteArray(method, 16);

            if (CallsMethodContains(method, "System.Array::Reverse"))
                Array.Reverse(bytes);

            if (!UsesPublicKeyToken(method))
                return bytes;

            if (!(Context.Module.Assembly.PublicKeyToken is PublicKeyToken publicKeyToken) ||
                publicKeyToken.Data.Length == 0) return bytes;

            for (var i = 0; i < 8; i++)
                bytes[i * 2 + 1] = publicKeyToken.Data[i];

            return bytes;
        }

        private static bool UsesPublicKeyToken(MethodDef method)
        {
            int[] indexes = { 1, 0, 3, 1, 5, 2, 7, 3, 9, 4, 11, 5, 13, 6, 15, 7 };
            var index = 0;
            foreach (var instr in method.Body.Instructions)
                if (instr.OpCode.FlowControl != FlowControl.Next)
                    index = 0;
                else if (instr.IsLdcI4())
                {
                    if (instr.GetLdcI4Value() != indexes[index++]) index = 0;
                    else if (index >= indexes.Length) return true;
                }

            return false;
        }

        private static bool CallsMethodContains(MethodDef method, string fullName)
        {
            if (method?.Body == null)
                return false;

            return (from instr in method.Body.Instructions
                    where instr.OpCode.Code is Code.Call || instr.OpCode.Code is Code.Callvirt ||
                          instr.OpCode.Code is Code.Newobj
                    select instr.Operand).OfType<IMethod>()
                .Any(calledMethod => calledMethod.FullName.Contains(fullName));
        }

        #endregion

        #region Properties

        public MethodDef DecrypterMethod { get; }

        public EmbeddedResource EmbeddedResource { get; private set; }

        private IList<string> AdditionalTypes { get; }

        private IDecrypter Decrypter { get; set; }

        #endregion

        #region Nested Types

        private class DecrypterV1 : IDecrypter
        {
            public DecrypterV1(MethodDef method)
            {
                _key = GetDecryptionKey(method);
                _iv = GetDecryptionIV(method);
            }

            public static bool CouldBeResourceDecrypter(MethodDef method, LocalTypes localTypes,
                IList<string> additionalTypes)
            {
                var requiredTypes = new[]
                {
                    new List<string>
                    {
                        "System.Byte[]",
                        "System.Security.Cryptography.CryptoStream",
                        "System.Security.Cryptography.ICryptoTransform",
                        "System.String",
                        "System.Boolean"
                    },
                    new List<string>
                    {
                        "System.Security.Cryptography.ICryptoTransform",
                        "System.IO.Stream",
                        "System.Int32",
                        "System.Byte[]",
                        "System.Boolean"
                    }
                };
                requiredTypes[0].AddRange(additionalTypes);
                return (localTypes.All(requiredTypes[0]) || localTypes.All(requiredTypes[1])) &&
                       (DotNetUtils.GetMethod(method.DeclaringType, "System.Security.Cryptography.SymmetricAlgorithm",
                           "()") == null || (!localTypes.Exists("System.UInt64") &&
                                             (!localTypes.Exists("System.UInt32") ||
                                              localTypes.Exists("System.Reflection.Assembly"))));
            }

            public byte[] Decrypt(EmbeddedResource resource) =>
                DeobUtils.AesDecrypt(resource.CreateReader().ToArray(), _key, _iv);

            #region Fields

            private readonly byte[] _key, _iv;

            #endregion
        }

        private class DecrypterV2 : IDecrypter
        {
            public DecrypterV2(MethodDef method)
            {
                _key = GetDecryptionKey(method);
                _iv = GetDecryptionIV(method);
                _method = method;
                _locals = new List<Local>(_method.Body.Variables);
                if (!Initialize()) throw new ApplicationException("Could not initialize decrypter");
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

            #region Private Methods

            private bool Initialize()
            {
                var origInstrs = _method.Body.Instructions;
                if (!Find(origInstrs, out var emuStartIndex, out var emuEndIndex, out _emuLocal) &&
                    !FindStartEnd(origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal) &&
                    !FindStartEnd2(origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal))
                {
                    if (!FindStartEnd3(ref origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal, out _emuArg,
                            ref _emuMethod, ref _locals)) return false;
                    _isNewDecrypter = true;
                }

                if (!_isNewDecrypter)
                    for (var i = 0; i < _iv.Length; i++)
                    {
                        var array = _key;
                        array[i] ^= _iv[i];
                    }

                var count = emuEndIndex - emuStartIndex + 1;
                _instructions = new List<Instruction>(count);
                for (var j = 0; j < count; j++) _instructions.Add(origInstrs[emuStartIndex + j].Clone());
                return true;
            }

            private Local CheckLocal(Instruction instr, bool isLdloc)
            {
                switch (isLdloc)
                {
                    case true when !instr.IsLdloc():
                    case false when !instr.IsStloc():
                        return null;
                    default:
                        return instr.GetLocal(_locals);
                }
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
                    if (!instr.IsStloc() || instr.GetLocal(_locals) != _emuLocal) continue;
                    endIndex = i - 1;
                    return true;
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

            private bool FindStartEnd(IList<Instruction> instrs, out int startIndex, out int endIndex,
                out Local tmpLocal)
            {
                var i = 0;
                while (i + 8 < instrs.Count)
                {
                    if (instrs[i].OpCode.Code.Equals(Code.Conv_R_Un) &&
                        instrs[i + 1].OpCode.Code.Equals(Code.Conv_R8) &&
                        instrs[i + 2].OpCode.Code.Equals(Code.Conv_U4) &&
                        instrs[i + 3].OpCode.Code.Equals(Code.Add))
                    {
                        var newEndIndex = i + 3;
                        var newStartIndex = -1;
                        for (var x = newEndIndex; x > 0; x--)
                            if (instrs[x].OpCode.FlowControl != FlowControl.Next)
                            {
                                if (instrs[x].OpCode.Equals(OpCodes.Bne_Un) ||
                                    instrs[x].OpCode.Equals(OpCodes.Bne_Un_S))
                                {
                                    _decrypterVersion = DecrypterVersion.V69;
                                    continue;
                                }

                                newStartIndex = x + 1;
                                break;
                            }

                        if (!_decrypterVersion.Equals(DecrypterVersion.V69))
                        {
                            startIndex = 0;
                            endIndex = 0;
                            tmpLocal = null;
                            return false;
                        }

                        if (newStartIndex >= 0)
                        {
                            var checkLocs = new List<Local>();
                            var ckStartIndex = -1;
                            for (var y = newEndIndex; y >= newStartIndex; y--)
                                if (CheckLocal(instrs[y], true) is Local loc)
                                {
                                    if (!checkLocs.Contains(loc)) checkLocs.Add(loc);
                                    if (checkLocs.Count == 10) break;

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

            private bool FindStartEnd2(IList<Instruction> instrs, out int startIndex, out int endIndex,
                out Local tmpLocal)
            {
                var i = 0;
                while (i + 8 < instrs.Count)
                {
                    if (instrs[i].OpCode.Code.Equals(Code.Conv_R_Un) &&
                        instrs[i + 1].OpCode.Code.Equals(Code.Conv_R8) &&
                        instrs[i + 2].OpCode.Code.Equals(Code.Conv_U4) &&
                        instrs[i + 3].OpCode.Code.Equals(Code.Add))
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
                                if (CheckLocal(instrs[y], true) is Local loc)
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

            private bool FindStartEnd3(ref IList<Instruction> instrs, out int startIndex, out int endIndex,
                out Local tmpLocal, out Parameter tmpArg, ref MethodDef methodDef, ref List<Local> locals)
            {
                foreach (var instr in instrs)
                {
                    MethodDef method;
                    if (instr.OpCode.Equals(OpCodes.Call) && (method = instr.Operand as MethodDef) != null &&
                        method.ReturnType.FullName == "System.Byte[]")
                        using (var enumerator2 = DotNetUtils.GetMethodCalls(method).GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                MethodDef calledMethod;
                                if ((calledMethod = enumerator2.Current as MethodDef) == null ||
                                    calledMethod.Parameters.Count != 2) continue;
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

                endIndex = 0;
                startIndex = 0;
                tmpLocal = null;
                tmpArg = null;
                return false;
            }

            private uint ReadUInt32(byte[] ary, int index)
            {
                var sizeLeft = ary.Length - index;
                if (sizeLeft >= 4)
                    return BitConverter.ToUInt32(ary, index);
                switch (sizeLeft)
                {
                    case 1: return ary[index];
                    case 2: return (uint)(ary[index] | (ary[index + 1] << 8));
                    case 3: return (uint)(ary[index] | (ary[index + 1] << 8) | (ary[index + 2] << 16));
                    default: throw new ApplicationException("Can't read data");
                }
            }

            private void WriteUInt32(IList<byte> ary, int index, uint value)
            {
                var num = ary.Count - index;
                if (num >= 1) ary[index] = (byte)value;
                if (num >= 2) ary[index + 1] = (byte)(value >> 8);
                if (num >= 3) ary[index + 2] = (byte)(value >> 16);
                if (num >= 4) ary[index + 3] = (byte)(value >> 24);
            }

            private uint CalculateMagic(uint input)
            {
                if (_emuArg == null)
                {
                    _instrEmulator.Initialize(_method, _method.Parameters, _locals, _method.Body.InitLocals, false);
                    _instrEmulator.SetLocal(_emuLocal, new Int32Value((int)input));
                }
                else
                {
                    _instrEmulator.Initialize(_emuMethod, _emuMethod.Parameters, _locals, _emuMethod.Body.InitLocals,
                        false);
                    _instrEmulator.SetArg(_emuArg, new Int32Value((int)input));
                }

                var index = 0;
                while (index < _instructions.Count)
                {
                    try
                    {
                        if (_decrypterVersion != DecrypterVersion.V69)
                            goto Emulate;
                        if (!_instructions[index].IsLdloc()) goto Emulate;
                        if (!_instructions[index + 1].OpCode.Equals(OpCodes.Ldc_I4_0) &&
                            (!_instructions[index + 1].IsLdcI4() || _instructions[index + 1].GetLdcI4Value() != 0))
                            goto Emulate;
                        if (!_instructions[index + 2].OpCode.Equals(OpCodes.Bne_Un) &&
                            !_instructions[index + 2].OpCode.Equals(OpCodes.Bne_Un_S)) goto Emulate;
                        if (!_instructions[index + 3].IsLdloc()) goto Emulate;
                        if (!_instructions[index + 4].OpCode.Equals(OpCodes.Ldc_I4_1) &&
                            (!_instructions[index + 4].IsLdcI4() || _instructions[index + 4].GetLdcI4Value() != 1))
                            goto Emulate;
                        if (!_instructions[index + 5].OpCode.Equals(OpCodes.Sub)) goto Emulate;
                        if (!_instructions[index + 6].IsStloc()) goto Emulate;
                        if (_instrEmulator.GetLocal(CheckLocal(_instructions[index + 6], false)
                                .Index) is Int32Value local && local.Value != Int32Value.Zero.Value)
                            index += 7;
                    }
                    catch
                    {
                    }

                    Emulate:
                    _instrEmulator.Emulate(_instructions[index]);
                    index++;
                }

                if (!(_instrEmulator.Pop() is Int32Value tos) || !tos.AllBitsValid())
                    throw new ApplicationException("Couldn't calculate magic value");
                return (uint)tos.Value;
            }

            #endregion

            #region Fields

            private readonly InstructionEmulator _instrEmulator = new InstructionEmulator();
            private readonly byte[] _key, _iv;
            private readonly MethodDef _method;
            private Parameter _emuArg;
            private Local _emuLocal;
            private MethodDef _emuMethod;
            private List<Instruction> _instructions;
            private bool _isNewDecrypter;
            private List<Local> _locals;
            private DecrypterVersion _decrypterVersion = DecrypterVersion.V6X;

            #endregion
        }

        private class DecrypterV3 : IDecrypter
        {
            public DecrypterV3(MethodDef method)
            {
                _method = method;
                _locals = new List<Local>(_method.Body.Variables);
                if (!Initialize()) throw new ApplicationException("Could not initialize decrypter");
            }

            public static bool CouldBeResourceDecrypter(LocalTypes localTypes, IEnumerable<string> additionalTypes)
            {
                var requiredTypes = new List<string>
                {
                    "System.Reflection.Emit.DynamicMethod",
                    "System.Reflection.Emit.ILGenerator"
                };
                requiredTypes.AddRange(additionalTypes);
                return localTypes.All(requiredTypes);
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

            #region Private Methods

            private bool Initialize()
            {
                var origInstrs = _method.Body.Instructions;
                if (!Find(origInstrs, out var emuStartIndex, out var emuEndIndex, out _emuLocal) &&
                    !FindStartEnd(origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal) &&
                    !FindStartEnd2(origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal)) return false;
                var count = emuEndIndex - emuStartIndex + 1;
                _instructions = new List<Instruction>(count);
                for (var i = 0; i < count; i++) _instructions.Add(origInstrs[emuStartIndex + i].Clone());
                return true;
            }

            private uint CalculateMagic(uint input)
            {
                _instrEmulator.Initialize(_method, _method.Parameters, _locals, _method.Body.InitLocals, false);
                _instrEmulator.SetLocal(_emuLocal, new Int32Value((int)input));

                var index = 0;
                while (index < _instructions.Count)
                {
                    try
                    {
                        if (_decrypterVersion != DecrypterVersion.V69)
                            goto Emulate;
                        if (!_instructions[index].IsLdloc()) goto Emulate;
                        if (!_instructions[index + 1].OpCode.Equals(OpCodes.Ldc_I4_0) &&
                            (!_instructions[index + 1].IsLdcI4() || _instructions[index + 1].GetLdcI4Value() != 0))
                            goto Emulate;
                        if (!_instructions[index + 2].OpCode.Equals(OpCodes.Bne_Un) &&
                            !_instructions[index + 2].OpCode.Equals(OpCodes.Bne_Un_S)) goto Emulate;
                        if (!_instructions[index + 3].IsLdloc()) goto Emulate;
                        if (!_instructions[index + 4].OpCode.Equals(OpCodes.Ldc_I4_1) &&
                            (!_instructions[index + 4].IsLdcI4() || _instructions[index + 4].GetLdcI4Value() != 1))
                            goto Emulate;
                        if (!_instructions[index + 5].OpCode.Equals(OpCodes.Sub)) goto Emulate;
                        if (!_instructions[index + 6].IsStloc()) goto Emulate;
                        if (_instrEmulator.GetLocal(CheckLocal(_instructions[index + 6], false)
                                .Index) is Int32Value local && local.Value != Int32Value.Zero.Value)
                            index += 7;
                    }
                    catch
                    {
                    }

                    Emulate:
                    _instrEmulator.Emulate(_instructions[index]);
                    index++;
                }

                if (!(_instrEmulator.Pop() is Int32Value tos) || !tos.AllBitsValid())
                    throw new ApplicationException("Couldn't calculate magic value");
                return (uint)tos.Value;
            }

            private uint ReadUInt32(byte[] ary, int index)
            {
                var sizeLeft = ary.Length - index;
                if (sizeLeft >= 4)
                    return BitConverter.ToUInt32(ary, index);
                switch (sizeLeft)
                {
                    case 1: return ary[index];
                    case 2: return (uint)(ary[index] | (ary[index + 1] << 8));
                    case 3: return (uint)(ary[index] | (ary[index + 1] << 8) | (ary[index + 2] << 16));
                    default: throw new ApplicationException("Can't read data");
                }
            }

            private void WriteUInt32(IList<byte> ary, int index, uint value)
            {
                var num = ary.Count - index;
                if (num >= 1) ary[index] = (byte)value;
                if (num >= 2) ary[index + 1] = (byte)(value >> 8);
                if (num >= 3) ary[index + 2] = (byte)(value >> 16);
                if (num >= 4) ary[index + 3] = (byte)(value >> 24);
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

            private bool FindStartEnd(IList<Instruction> instrs, out int startIndex, out int endIndex,
                out Local tmpLocal)
            {
                var i = 0;
                while (i + 8 < instrs.Count)
                {
                    if (instrs[i].OpCode.Code.Equals(Code.Conv_R_Un) &&
                        instrs[i + 1].OpCode.Code.Equals(Code.Conv_R8) &&
                        instrs[i + 2].OpCode.Code.Equals(Code.Conv_U4) &&
                        instrs[i + 3].OpCode.Code.Equals(Code.Add))
                    {
                        var newEndIndex = i + 3;
                        var newStartIndex = -1;
                        for (var x = newEndIndex; x > 0; x--)
                            if (instrs[x].OpCode.FlowControl != FlowControl.Next)
                            {
                                if (instrs[x].OpCode.Equals(OpCodes.Bne_Un) ||
                                    instrs[x].OpCode.Equals(OpCodes.Bne_Un_S))
                                {
                                    _decrypterVersion = DecrypterVersion.V69;
                                    continue;
                                }

                                newStartIndex = x + 1;
                                break;
                            }

                        if (!_decrypterVersion.Equals(DecrypterVersion.V69))
                        {
                            startIndex = 0;
                            endIndex = 0;
                            tmpLocal = null;
                            return false;
                        }

                        if (newStartIndex >= 0)
                        {
                            var checkLocs = new List<Local>();
                            var ckStartIndex = -1;
                            for (var y = newEndIndex; y >= newStartIndex; y--)
                                if (CheckLocal(instrs[y], true) is Local loc)
                                {
                                    if (!checkLocs.Contains(loc)) checkLocs.Add(loc);
                                    if (checkLocs.Count == 10) break;

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

            private bool FindStartEnd2(IList<Instruction> instrs, out int startIndex, out int endIndex,
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
                                if (CheckLocal(instrs[y], true) is Local loc)
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
                switch (isLdloc)
                {
                    case true when !instr.IsLdloc():
                    case false when !instr.IsStloc():
                        return null;
                    default:
                        return instr.GetLocal(_locals);
                }
            }

            #endregion

            #region Fields

            private readonly InstructionEmulator _instrEmulator = new InstructionEmulator();
            private readonly List<Local> _locals;
            private readonly MethodDef _method;
            private Local _emuLocal;
            private List<Instruction> _instructions;
            private DecrypterVersion _decrypterVersion = DecrypterVersion.V6X;

            #endregion
        }

        private class DecrypterV4 : IDecrypter
        {
            public DecrypterV4(MethodDef method)
            {
                if (!FindDecrypterMethod(method))
                    throw new ApplicationException("Could not find decrypter method");

                if (!FindEmulateMethod(_decryptMethod))
                    throw new ApplicationException("Could not find emulate method");

                _key = GetDecryptionKey(_decryptMethod);
                _iv = GetDecryptionIV(_decryptMethod);
                _locals = new List<Local>(_emuMethod.Body.Variables);
                if (!Initialize())
                    throw new ApplicationException("Could not initialize decrypter");
            }

            public static bool CouldBeResourceDecrypter(MethodDef method, LocalTypes localTypes,
                IEnumerable<string> additionalTypes)
            {
                var requiredTypes = new List<string>
                {
                    "System.Int32",
                    "System.Byte[]"
                };
                requiredTypes.AddRange(additionalTypes);
                if (!localTypes.All(requiredTypes))
                    return false;

                var instrs = method.Body.Instructions;

                foreach (var instr in instrs)
                {
                    if (instr.OpCode != OpCodes.Newobj)
                        continue;

                    if (instr.Operand is IMethod newObj
                        && newObj.FullName == "System.Void System.Diagnostics.StackFrame::.ctor(System.Int32)")
                        return true;
                }

                return false;
            }

            public byte[] Decrypt(EmbeddedResource resource)
            {
                var encrypted = resource.CreateReader().ToArray();
                var decrypted = new byte[encrypted.Length];

                uint sum = 0;
                for (var i = 0; i < encrypted.Length; i += 4)
                {
                    sum = CalculateMagic(sum + ReadUInt32(_key, i % _key.Length));
                    WriteUInt32(decrypted, i, sum ^ ReadUInt32(encrypted, i));
                }

                return decrypted;
            }

            #region Private Methods

            private bool FindDecrypterMethod(MethodDef method)
            {
                var instrs = method.Body.Instructions;
                for (var i = 0; i < instrs.Count; i++)
                {
                    if (instrs[i].OpCode != OpCodes.Ldsfld)
                        continue;
                    if (instrs[i + 1].OpCode != OpCodes.Ldstr)
                        continue;
                    if (instrs[i + 2].OpCode != OpCodes.Callvirt)
                        continue;
                    if (instrs[i + 3].OpCode != OpCodes.Ldarg_0)
                        continue;
                    var call = instrs[i + 4];
                    if (call.OpCode != OpCodes.Call)
                        continue;

                    _decryptMethod = call.Operand as MethodDef;
                    return true;
                }

                return false;
            }

            private bool FindEmulateMethod(MethodDef method)
            {
                var instrs = method.Body.Instructions;
                for (var i = 0; i < instrs.Count; i++)
                {
                    if (instrs[i].OpCode != OpCodes.Newobj)
                        continue;
                    if (!instrs[i + 1].IsLdloc())
                        continue;
                    if (!instrs[i + 2].IsLdloc())
                        continue;
                    if (!instrs[i + 3].IsLdloc())
                        continue;
                    var call = instrs[i + 4];
                    if (call.OpCode != OpCodes.Call)
                        continue;

                    _emuMethod = call.Operand as MethodDef;
                    return true;
                }

                return false;
            }

            private bool Initialize()
            {
                var origInstrs = _emuMethod.Body.Instructions;

                if (!Find(origInstrs, out var emuStartIndex, out var emuEndIndex, out _emuLocal))
                    if (!FindStartEnd(origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal) &&
                        !FindStartEnd2(origInstrs, out emuStartIndex, out emuEndIndex, out _emuLocal))
                        return false;

                for (var i = 0; i < _iv.Length; i++)
                    _key[i] ^= _iv[i];

                var count = emuEndIndex - emuStartIndex + 1;
                _instructions = new List<Instruction>(count);
                for (var i = 0; i < count; i++)
                    _instructions.Add(origInstrs[emuStartIndex + i].Clone());

                return true;
            }

            private bool Find(IList<Instruction> instrs, out int startIndex, out int endIndex, out Local tmpLocal)
            {
                startIndex = 0;
                endIndex = 0;
                tmpLocal = null;

                if (!FindStart(instrs, out var emuStartIndex, out _emuLocal))
                    return false;
                if (!FindEnd(instrs, emuStartIndex, out var emuEndIndex))
                    return false;
                startIndex = emuStartIndex;
                endIndex = emuEndIndex;
                tmpLocal = _emuLocal;
                return true;
            }

            private bool FindStartEnd(IList<Instruction> instrs, out int startIndex, out int endIndex,
                out Local tmpLocal)
            {
                var i = 0;
                while (i + 8 < instrs.Count)
                {
                    if (instrs[i].OpCode.Code.Equals(Code.Conv_R_Un) &&
                        instrs[i + 1].OpCode.Code.Equals(Code.Conv_R8) &&
                        instrs[i + 2].OpCode.Code.Equals(Code.Conv_U4) &&
                        instrs[i + 3].OpCode.Code.Equals(Code.Add))
                    {
                        var newEndIndex = i + 3;
                        var newStartIndex = -1;
                        for (var x = newEndIndex; x > 0; x--)
                            if (instrs[x].OpCode.FlowControl != FlowControl.Next)
                            {
                                if (instrs[x].OpCode.Equals(OpCodes.Bne_Un) ||
                                    instrs[x].OpCode.Equals(OpCodes.Bne_Un_S))
                                {
                                    _decrypterVersion = DecrypterVersion.V69;
                                    continue;
                                }

                                newStartIndex = x + 1;
                                break;
                            }

                        if (!_decrypterVersion.Equals(DecrypterVersion.V69))
                        {
                            startIndex = 0;
                            endIndex = 0;
                            tmpLocal = null;
                            return false;
                        }

                        if (newStartIndex >= 0)
                        {
                            var checkLocs = new List<Local>();
                            var ckStartIndex = -1;
                            for (var y = newEndIndex; y >= newStartIndex; y--)
                                if (CheckLocal(instrs[y], true) is Local loc)
                                {
                                    if (!checkLocs.Contains(loc)) checkLocs.Add(loc);
                                    if (checkLocs.Count == 10) break;

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

            private bool FindStartEnd2(IList<Instruction> instrs, out int startIndex, out int endIndex,
                out Local tmpLocal)
            {
                for (var i = 0; i + 8 < instrs.Count; i++)
                {
                    if (instrs[i].OpCode.Code != Code.Conv_R_Un)
                        continue;
                    if (instrs[i + 1].OpCode.Code != Code.Conv_R8)
                        continue;
                    if (instrs[i + 2].OpCode.Code != Code.Conv_U4)
                        continue;
                    if (instrs[i + 3].OpCode.Code != Code.Add)
                        continue;
                    var newEndIndex = i + 3;
                    var newStartIndex = -1;
                    for (var x = newEndIndex; x > 0; x--)
                        if (instrs[x].OpCode.FlowControl != FlowControl.Next)
                        {
                            newStartIndex = x + 1;
                            break;
                        }

                    if (newStartIndex < 0)
                        continue;

                    var checkLocs = new List<Local>();
                    var ckStartIndex = -1;
                    for (var y = newEndIndex; y >= newStartIndex; y--)
                    {
                        var loc = CheckLocal(instrs[y], true);
                        if (loc == null)
                            continue;
                        if (!checkLocs.Contains(loc))
                            checkLocs.Add(loc);
                        if (checkLocs.Count == 3) break;

                        ckStartIndex = y;
                    }

                    endIndex = newEndIndex;
                    startIndex = Math.Max(ckStartIndex, newStartIndex);
                    tmpLocal = CheckLocal(instrs[startIndex], true);
                    return true;
                }

                endIndex = 0;
                startIndex = 0;
                tmpLocal = null;
                return false;
            }

            private bool FindStart(IList<Instruction> instrs, out int startIndex, out Local tmpLocal)
            {
                for (var i = 0; i + 8 < instrs.Count; i++)
                {
                    if (instrs[i].OpCode.Code != Code.Conv_U)
                        continue;
                    if (instrs[i + 1].OpCode.Code != Code.Ldelem_U1)
                        continue;
                    if (instrs[i + 2].OpCode.Code != Code.Or)
                        continue;
                    if (CheckLocal(instrs[i + 3], false) == null)
                        continue;
                    Local local;
                    if ((local = CheckLocal(instrs[i + 4], true)) == null)
                        continue;
                    if (CheckLocal(instrs[i + 5], true) == null)
                        continue;
                    if (instrs[i + 6].OpCode.Code != Code.Add)
                        continue;
                    if (CheckLocal(instrs[i + 7], false) != local)
                        continue;
                    var instr = instrs[i + 8];
                    var newStartIndex = i + 8;
                    if (instr.IsBr())
                    {
                        instr = instr.Operand as Instruction;
                        newStartIndex = instrs.IndexOf(instr);
                    }

                    if (newStartIndex < 0 || instr == null)
                        continue;
                    if (CheckLocal(instr, true) != local)
                        continue;

                    startIndex = newStartIndex;
                    tmpLocal = local;
                    return true;
                }

                startIndex = 0;
                tmpLocal = null;
                return false;
            }

            private bool FindEnd(IList<Instruction> instrs, int startIndex, out int endIndex)
            {
                for (var i = startIndex; i < instrs.Count; i++)
                {
                    var instr = instrs[i];
                    if (instr.OpCode.FlowControl != FlowControl.Next)
                        break;
                    if (instr.IsStloc() && instr.GetLocal(_locals) == _emuLocal)
                    {
                        endIndex = i - 1;
                        return true;
                    }
                }

                endIndex = 0;
                return false;
            }

            private Local CheckLocal(Instruction instr, bool isLdloc)
            {
                if (isLdloc && !instr.IsLdloc())
                    return null;
                if (!isLdloc && !instr.IsStloc())
                    return null;

                return instr.GetLocal(_locals);
            }

            private uint CalculateMagic(uint input)
            {
                _instrEmulator.Initialize(_emuMethod, _emuMethod.Parameters, _locals, _emuMethod.Body.InitLocals,
                    false);
                _instrEmulator.SetLocal(_emuLocal, new Int32Value((int)input));

                var index = 0;
                while (index < _instructions.Count)
                {
                    try
                    {
                        if (_decrypterVersion != DecrypterVersion.V69)
                            goto Emulate;
                        if (!_instructions[index].IsLdloc()) goto Emulate;
                        if (!_instructions[index + 1].OpCode.Equals(OpCodes.Ldc_I4_0) &&
                            (!_instructions[index + 1].IsLdcI4() || _instructions[index + 1].GetLdcI4Value() != 0))
                            goto Emulate;
                        if (!_instructions[index + 2].OpCode.Equals(OpCodes.Bne_Un) &&
                            !_instructions[index + 2].OpCode.Equals(OpCodes.Bne_Un_S)) goto Emulate;
                        if (!_instructions[index + 3].IsLdloc()) goto Emulate;
                        if (!_instructions[index + 4].OpCode.Equals(OpCodes.Ldc_I4_1) &&
                            (!_instructions[index + 4].IsLdcI4() || _instructions[index + 4].GetLdcI4Value() != 1))
                            goto Emulate;
                        if (!_instructions[index + 5].OpCode.Equals(OpCodes.Sub)) goto Emulate;
                        if (!_instructions[index + 6].IsStloc()) goto Emulate;
                        if (_instrEmulator.GetLocal(CheckLocal(_instructions[index + 6], false)
                                .Index) is Int32Value local && local.Value != Int32Value.Zero.Value)
                            index += 7;
                    }
                    catch
                    {
                    }

                    Emulate:
                    _instrEmulator.Emulate(_instructions[index]);
                    index++;
                }

                var tos = _instrEmulator.Pop() as Int32Value;
                if (tos == null || !tos.AllBitsValid())
                    throw new ApplicationException("Couldn't calculate magic value");
                return (uint)tos.Value;
            }

            private uint ReadUInt32(byte[] ary, int index)
            {
                var sizeLeft = ary.Length - index;
                if (sizeLeft >= 4)
                    return BitConverter.ToUInt32(ary, index);
                switch (sizeLeft)
                {
                    case 1: return ary[index];
                    case 2: return (uint)(ary[index] | (ary[index + 1] << 8));
                    case 3: return (uint)(ary[index] | (ary[index + 1] << 8) | (ary[index + 2] << 16));
                    default: throw new ApplicationException("Can't read data");
                }
            }

            private void WriteUInt32(byte[] ary, int index, uint value)
            {
                var sizeLeft = ary.Length - index;
                if (sizeLeft >= 1)
                    ary[index] = (byte)value;
                if (sizeLeft >= 2)
                    ary[index + 1] = (byte)(value >> 8);
                if (sizeLeft >= 3)
                    ary[index + 2] = (byte)(value >> 16);
                if (sizeLeft >= 4)
                    ary[index + 3] = (byte)(value >> 24);
            }

            #endregion

            #region Fields

            private readonly byte[] _key, _iv;
            private MethodDef _decryptMethod;
            private MethodDef _emuMethod;
            private List<Instruction> _instructions;
            private readonly List<Local> _locals;
            private readonly InstructionEmulator _instrEmulator = new InstructionEmulator();
            private Local _emuLocal;
            private DecrypterVersion _decrypterVersion = DecrypterVersion.V6X;

            #endregion
        }

        #endregion
    }
}