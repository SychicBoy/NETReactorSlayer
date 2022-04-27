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
using dnlib.DotNet.MD;
using dnlib.IO;
using NETReactorSlayer.Core.Helper.De4dot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static NETReactorSlayer.Core.Deobfuscators.ResourceDecryptor;

namespace NETReactorSlayer.Core.Deobfuscators
{
    class MethodDecryptor : IDeobfuscator
    {
        public void Execute()
        {
            DumpedMethods dumpedMethods = new DumpedMethods();
            Dictionary<uint, byte[]> tokenToNativeMethod = new Dictionary<uint, byte[]>();
            Dictionary<uint, byte[]> tokenToNativeCode = new Dictionary<uint, byte[]>();
            foreach (var type in DeobfuscatorContext.Module.GetTypes())
            {
                foreach (var method in type.Methods.ToArray<MethodDef>())
                {
                    if (DotNetUtils.IsMethod(method, "System.UInt32", "(System.IntPtr,System.IntPtr,System.IntPtr,System.UInt32,System.IntPtr,System.UInt32&)") || DotNetUtils.IsMethod(method, "System.UInt32", "(System.UInt64&,System.IntPtr,System.IntPtr,System.UInt32,System.IntPtr&,System.UInt32&)"))
                    {
                        foreach (var methodDef in (from x in method.DeclaringType.Methods where x.IsStatic && x.HasBody && x.Body.HasInstructions select x))
                        {
                            foreach (var call in DotNetUtils.GetMethodCalls(methodDef))
                            {
                                if (call.MDToken.ToInt32() == method.MDToken.ToInt32())
                                {
                                    decryptorMethod = methodDef;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            var cctor = DeobfuscatorContext.Module.GlobalType.FindStaticConstructor();
            if (cctor != null && cctor.HasBody && cctor.Body.HasInstructions)
            {
                for (int i = 0; i < cctor.Body.Instructions.Count; i++)
                {
                    if (cctor.Body.Instructions[i].OpCode.Equals(OpCodes.Call))
                    {
                        if (cctor.Body.Instructions[i].Operand is MethodDef methodDef && methodDef.DeclaringType != null && methodDef.HasBody && methodDef.Body.HasInstructions)
                        {
                            if (DotNetUtils.GetMethod(methodDef.DeclaringType, "System.Security.Cryptography.SymmetricAlgorithm", "()") != null && GetEncryptedResource(methodDef) != null && GetDecrypterType(methodDef, new string[0]) != DnrDecrypterType.Unknown)
                            {
                                decryptorMethod = methodDef;
                                break;
                            }
                        }
                    }
                }
            }
            if (decryptorMethod == null)
            {
                Logger.Warn("Couldn't find any encrypted method.");
                return;
            }
            encryptedResource = GetEncryptedResource(decryptorMethod);
            Cleaner.ResourceToRemove.Add(encryptedResource);
            Cleaner.MethodsToRemove.Add(decryptorMethod);
            SimpleDeobfuscator.Deobfuscate(decryptorMethod);
            DnrDecrypterType decrypterType = GetDecrypterType(decryptorMethod, new string[0]);
            byte[] key = GetBytes(decryptorMethod, 32);
            if (decrypterType == DnrDecrypterType.V3)
            {
                V3 V3 = new V3(decryptorMethod);
                methodsData = V3.Decrypt(encryptedResource);
                goto Continue;
            }
            byte[] iv = GetBytes(decryptorMethod, 16);
            if (IsNeedReverse(decryptorMethod))
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
                methodsData = V1.Decrypt(encryptedResource);
                goto Continue;
            }
            else if (decrypterType == DnrDecrypterType.V2)
            {
                V2 V2 = new V2(iv, key, decryptorMethod);
                methodsData = V2.Decrypt(encryptedResource);
                goto Continue;
            }
            Logger.Warn("Couldn't find any encrypted method.");
            return;
        Continue:
            if (encryptedResource == null)
            {
                Logger.Warn("Couldn't find any encrypted method.");
                return;
            }
            if (methodsData == null)
            {
                Logger.Error("Failed to decrypt methods.");
                return;
            }
            bool isFindDnrMethod = false;
            int mode = -1;
            try
            {
                XorEncrypt(methodsData, GetXorKey(decryptorMethod));
                isFindDnrMethod = FindDnrCompileMethod(decryptorMethod.DeclaringType) != null;
                DataReader methodsDataReader = ByteArrayDataReaderFactory.CreateReader(methodsData);
                int tmp = methodsDataReader.ReadInt32();
                if (((long)tmp & unchecked((long)((ulong)-16777216))) == 100663296L)
                {
                    methodsDataReader.ReadInt32();
                }
                else
                {
                    methodsDataReader.Position -= 4U;
                }
                int patchCount = methodsDataReader.ReadInt32();
                mode = methodsDataReader.ReadInt32();
                tmp = methodsDataReader.ReadInt32();
                methodsDataReader.Position -= 4U;
                if (((long)tmp & unchecked((long)((ulong)-16777216))) == 100663296L)
                {
                    methodsDataReader.Position += (uint)(8 * patchCount);
                    patchCount = methodsDataReader.ReadInt32();
                    mode = methodsDataReader.ReadInt32();
                    PatchDwords(DeobfuscatorContext.PEImage, ref methodsDataReader, patchCount);
                    while ((ulong)methodsDataReader.Position < (ulong)((long)(methodsData.Length - 1)))
                    {
                        methodsDataReader.ReadUInt32();
                        int numDwords = methodsDataReader.ReadInt32();
                        PatchDwords(DeobfuscatorContext.PEImage, ref methodsDataReader, numDwords / 2);
                    }
                }
                else
                {
                    if (!isFindDnrMethod || mode == 1)
                    {
                        PatchDwords(DeobfuscatorContext.PEImage, ref methodsDataReader, patchCount);
                        bool isNewer45Decryption = IsNewer45Decryption(decryptorMethod);
                        bool isUsingOffset = !IsUsingRva(decryptorMethod);
                        while ((ulong)methodsDataReader.Position < (ulong)((long)(methodsData.Length - 1)))
                        {
                            uint rva = (uint)methodsDataReader.ReadInt32();
                            int size;
                            if (!isNewer45Decryption)
                            {
                                methodsDataReader.ReadInt32();
                                size = methodsDataReader.ReadInt32();
                            }
                            else
                            {
                                size = methodsDataReader.ReadInt32() * 4;
                            }
                            byte[] newData = methodsDataReader.ReadBytes(size);
                            if (DeobfuscatorContext.IsNative && isUsingOffset)
                            {
                                DeobfuscatorContext.PEImage.DotNetSafeWriteOffset(rva, newData);
                            }
                            else
                            {
                                DeobfuscatorContext.PEImage.DotNetSafeWrite(rva, newData);
                            }
                        }
                    }
                    else
                    {
                        MDTable methodDef = DeobfuscatorContext.PEImage.Metadata.TablesStream.MethodTable;
                        Dictionary<uint, int> rvaToIndex = new Dictionary<uint, int>((int)methodDef.Rows);
                        uint offset = (uint)methodDef.StartOffset;
                        int i = 0;
                        while ((long)i < (long)((ulong)methodDef.Rows))
                        {
                            uint rva2 = DeobfuscatorContext.PEImage.OffsetReadUInt32(offset);
                            offset += methodDef.RowSize;
                            if (rva2 != 0U)
                            {
                                if ((DeobfuscatorContext.PEImage.ReadByte(rva2) & 3) == 2)
                                {
                                    rva2 += 1U;
                                }
                                else
                                {
                                    rva2 += (uint)(4 * (DeobfuscatorContext.PEImage.ReadByte(rva2 + 1U) >> 4));
                                }
                                rvaToIndex[rva2] = i;
                            }
                            i++;
                        }
                        PatchDwords(DeobfuscatorContext.PEImage, ref methodsDataReader, patchCount);
                        methodsDataReader.ReadInt32();
                        dumpedMethods = new DumpedMethods();
                        while ((ulong)methodsDataReader.Position < (ulong)((long)(methodsData.Length - 1)))
                        {
                            uint rva3 = methodsDataReader.ReadUInt32();
                            uint index = methodsDataReader.ReadUInt32();
                            bool isNativeCode = index >= 1879048192U;
                            int size2 = methodsDataReader.ReadInt32();
                            byte[] methodData = methodsDataReader.ReadBytes(size2);
                            if (!rvaToIndex.TryGetValue(rva3, out int methodIndex))
                            {
                                Logger.Warn("Couldn't find method with RVA: " + rva3);
                            }
                            else
                            {
                                uint methodToken = (uint)(100663297 + methodIndex);
                                if (isNativeCode)
                                {
                                    if (tokenToNativeCode != null)
                                    {
                                        tokenToNativeCode[methodToken] = methodData;
                                    }
                                    if (DeobUtils.IsCode(nativeLdci4, methodData))
                                    {
                                        uint val = BitConverter.ToUInt32(methodData, 4);
                                        methodData = new byte[]
                                        {
                                        32,
                                        0,
                                        0,
                                        0,
                                        0,
                                        42
                                        };
                                        methodData[1] = (byte)val;
                                        methodData[2] = (byte)(val >> 8);
                                        methodData[3] = (byte)(val >> 16);
                                        methodData[4] = (byte)(val >> 24);
                                    }
                                    else
                                    {
                                        if (DeobUtils.IsCode(nativeLdci4_0, methodData))
                                        {
                                            methodData = new byte[]
                                            {
                                            22,
                                            42
                                            };
                                        }
                                        else
                                        {
                                            tokenToNativeMethod[methodToken] = methodData;
                                            methodData = new byte[]
                                            {
                                            32,
                                            222,
                                            192,
                                            173,
                                            222,
                                            109,
                                            122
                                            };
                                        }
                                    }
                                }
                                DumpedMethod dumpedMethod = new DumpedMethod();
                                DeobfuscatorContext.PEImage.ReadMethodTableRowTo(dumpedMethod, MDToken.ToRID(methodToken));
                                dumpedMethod.code = methodData;
                                DataReader codeReader = DeobfuscatorContext.PEImage.Reader;
                                codeReader.Position = DeobfuscatorContext.PEImage.RvaToOffset(dumpedMethod.mdRVA);
                                MethodBodyHeader mbHeader = MethodBodyParser.ParseMethodBody(ref codeReader, out byte[] code, out dumpedMethod.extraSections);
                                DeobfuscatorContext.PEImage.UpdateMethodHeaderInfo(dumpedMethod, mbHeader);
                                dumpedMethods.Add(dumpedMethod);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to decrypt methods. " + ex.Message);
            }

            using (DeobfuscatorContext.Module)
            {
                if (!isFindDnrMethod || mode == 1)
                    DeobfuscatorContext.Module = DeobfuscatorContext.AssemblyModule.Reload(DeobfuscatorContext.PEImage.peImageData, CreateDumpedMethodsRestorer(dumpedMethods), null);
                else if (dumpedMethods.Count > 0)
                    DeobfuscatorContext.Module = DeobfuscatorContext.AssemblyModule.Reload(DeobfuscatorContext.ModuleBytes, CreateDumpedMethodsRestorer(dumpedMethods), null);
                else
                {
                    Logger.Error("Failed to decrypt methods.");
                    return;
                }
                Logger.Done("Methods decrypted.");
            }
        }

        MethodDef FindDnrCompileMethod(TypeDef type)
        {
            foreach (MethodDef method in type.Methods)
            {
                if (!method.IsStatic || method.Body == null) continue;
                MethodSig sig = method.MethodSig;
                if (sig != null || sig.Params.Count == 6)
                {
                    if (GetCompileMethodType(method) != CompileMethodType.Unknown)
                    {
                        return method;
                    }
                }
            }
            return null;
        }

        CompileMethodType GetCompileMethodType(MethodDef method)
        {
            if (DotNetUtils.IsMethod(method, "System.UInt32", "(System.UInt64&,System.IntPtr,System.IntPtr,System.UInt32,System.IntPtr&,System.UInt32&)"))
            {
                return CompileMethodType.V1;
            }
            else
            {
                if (DotNetUtils.IsMethod(method, "System.UInt32", "(System.IntPtr,System.IntPtr,System.IntPtr,System.UInt32,System.IntPtr,System.UInt32&)"))
                {
                    return CompileMethodType.V2;
                }
                else
                {
                    return CompileMethodType.Unknown;
                }
            }
        }

        DumpedMethodsRestorer CreateDumpedMethodsRestorer(DumpedMethods dumpedMethods)
        {
            if (dumpedMethods == null || dumpedMethods.Count == 0) return null;
            return new DumpedMethodsRestorer(dumpedMethods);
        }

        void PatchDwords(MyPEImage peImage, ref DataReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                uint rva = reader.ReadUInt32();
                uint data = reader.ReadUInt32();
                peImage.DotNetSafeWrite(rva, BitConverter.GetBytes(data));
            }
        }

        bool IsUsingRva(MethodDef method)
        {
            if (method == null || method.Body == null) return false;
            IList<Instruction> instrs = method.Body.Instructions;
            for (int i = 0; i < instrs.Count; i++)
            {
                if (instrs[i].OpCode.Equals(OpCodes.Ldloca_S))
                {
                    if (instrs[i + 1].OpCode.Equals(OpCodes.Ldsfld))
                    {
                        if (instrs[i + 2].OpCode.Equals(OpCodes.Ldloc_S))
                        {
                            if (instrs[i + 3].OpCode.Equals(OpCodes.Conv_I8))
                            {
                                if (instrs[i + 4].OpCode.Equals(OpCodes.Add))
                                {
                                    if (instrs[i + 5].OpCode.Equals(OpCodes.Ldloc_S))
                                    {
                                        if (instrs[i + 6].OpCode.Equals(OpCodes.Conv_I8))
                                        {
                                            if (instrs[i + 7].OpCode.Equals(OpCodes.Sub))
                                            {
                                                if (instrs[i + 8].OpCode.Code.Equals(Code.Call))
                                                {
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
            return false;
        }

        bool IsNewer45Decryption(MethodDef method)
        {
            if (method == null || method.Body == null) return false;
            for (int i = 0; i < method.Body.Instructions.Count - 4; i++)
            {
                if (method.Body.Instructions[i].IsLdcI4())
                {
                    if (method.Body.Instructions[i + 1].OpCode.Code.Equals(Code.Mul))
                    {
                        if (method.Body.Instructions[i + 2].IsLdcI4())
                        {
                            if (method.Body.Instructions[i + 3].OpCode.Code.Equals(Code.Ldloca_S) || method.Body.Instructions[i + 3].OpCode.Code.Equals(Code.Ldloca))
                            {
                                if (method.Body.Instructions[i + 4].OpCode.Code.Equals(Code.Call))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        long GetXorKey(MethodDef method)
        {
            for (int i = 0; i < method.Body.Instructions.Count - 1; i++)
            {
                if (method.Body.Instructions[i].OpCode.Code.Equals(Code.Ldind_I8))
                {
                    Instruction ldci4 = method.Body.Instructions[i + 1];
                    long result;
                    if (ldci4.IsLdcI4())
                    {
                        result = (long)ldci4.GetLdcI4Value();
                    }
                    else
                    {
                        if (!ldci4.OpCode.Code.Equals(Code.Ldc_I8))
                        {
                            goto Continue;
                        }
                        result = (long)ldci4.Operand;
                    }
                    return result;
                }
            Continue:;
            }
            return 0L;
        }

        EmbeddedResource GetEncryptedResource(MethodDef method)
        {
            if (method == null || !method.HasBody || !method.Body.HasInstructions) return null;
            foreach (string s in DotNetUtils.GetCodeStrings(method))
            {
                if (DotNetUtils.GetResource(DeobfuscatorContext.Module, s) is EmbeddedResource resource) return resource;
            }
            return null;
        }

        void XorEncrypt(byte[] data, long xorKey)
        {
            if (xorKey != 0L)
            {
                MemoryStream stream = new MemoryStream(data);
                BinaryReader reader = new BinaryReader(stream);
                BinaryWriter writer = new BinaryWriter(stream);
                int count = data.Length / 8;
                for (int i = 0; i < count; i++)
                {
                    long val = reader.ReadInt64();
                    val ^= xorKey;
                    stream.Position -= 8L;
                    writer.Write(val);
                }
            }
        }

        MethodDef decryptorMethod = null;
        EmbeddedResource encryptedResource = null;
        byte[] methodsData = null;
        enum CompileMethodType { Unknown, V1, V2 }
        readonly short[] nativeLdci4 = new short[] { 85, 139, 236, 184, -1, -1, -1, -1, 93, 195 };
        readonly short[] nativeLdci4_0 = new short[] { 85, 139, 236, 51, 192, 93, 195 };
    }
}
