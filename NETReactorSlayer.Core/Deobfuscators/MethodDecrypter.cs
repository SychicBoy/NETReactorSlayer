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
using System.IO;
using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.IO;
using NETReactorSlayer.Core.Helper.De4dot;
using static NETReactorSlayer.Core.Deobfuscators.RsrcDecrypter;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class MethodDecrypter : IStage
{
    private readonly short[] nativeLdci4 = {85, 139, 236, 184, -1, -1, -1, -1, 93, 195};
    private readonly short[] nativeLdci4_0 = {85, 139, 236, 51, 192, 93, 195};

    private MethodDef decryptorMethod;
    private EmbeddedResource encryptedResource;
    private byte[] methodsData;

    public void Execute()
    {
        var dumpedMethods = new DumpedMethods();
        var tokenToNativeMethod = new Dictionary<uint, byte[]>();
        var tokenToNativeCode = new Dictionary<uint, byte[]>();
        foreach (var type in Context.Module.GetTypes())
        foreach (var method in type.Methods.ToList())
            if (DotNetUtils.IsMethod(method, "System.UInt32",
                    "(System.IntPtr,System.IntPtr,System.IntPtr,System.UInt32,System.IntPtr,System.UInt32&)") ||
                DotNetUtils.IsMethod(method, "System.UInt32",
                    "(System.UInt64&,System.IntPtr,System.IntPtr,System.UInt32,System.IntPtr&,System.UInt32&)"))
                foreach (var methodDef in from x in method.DeclaringType.Methods
                         where x.IsStatic && x.HasBody && x.Body.HasInstructions
                         select x)
                foreach (var call in DotNetUtils.GetMethodCalls(methodDef))
                    if (call.MDToken.ToInt32() == method.MDToken.ToInt32())
                    {
                        decryptorMethod = methodDef;
                        break;
                    }

        var cctor = Context.Module.GlobalType.FindStaticConstructor();
        if (cctor is {HasBody: true} && cctor.Body.HasInstructions)
            foreach (var instr in cctor.Body.Instructions)
                if (instr.OpCode.Equals(OpCodes.Call))
                    if (instr.Operand is MethodDef {DeclaringType: { }, HasBody: true} methodDef &&
                        methodDef.Body.HasInstructions)
                        if (DotNetUtils.GetMethod(methodDef.DeclaringType,
                                "System.Security.Cryptography.SymmetricAlgorithm", "()") != null &&
                            GetEncryptedResource(methodDef) != null &&
                            GetDecrypterType(methodDef, Array.Empty<string>()) != DnrDecrypterType.Unknown)
                        {
                            decryptorMethod = methodDef;
                            break;
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
        var decrypterType = GetDecrypterType(decryptorMethod, Array.Empty<string>());
        var key = GetBytes(decryptorMethod, 32);
        if (decrypterType == DnrDecrypterType.V3)
        {
            var V3 = new V3(decryptorMethod);
            methodsData = V3.Decrypt(encryptedResource);
            goto Continue;
        }

        var iv = GetBytes(decryptorMethod, 16);
        if (IsNeedReverse(decryptorMethod))
            Array.Reverse(iv);
        if (UsesPublicKeyToken(decryptorMethod))
        {
            var publicKeyToken = Context.Module.Assembly.PublicKeyToken;
            if (publicKeyToken != null && publicKeyToken.Data.Length != 0)
                for (var z = 0; z < 8; z++)
                    iv[z * 2 + 1] = publicKeyToken.Data[z];
        }

        switch (decrypterType)
        {
            case DnrDecrypterType.V1:
            {
                var V1 = new V1(iv, key);
                methodsData = V1.Decrypt(encryptedResource);
                goto Continue;
            }
            case DnrDecrypterType.V2:
            {
                var V2 = new V2(iv, key, decryptorMethod);
                methodsData = V2.Decrypt(encryptedResource);
                goto Continue;
            }
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

        var isFindDnrMethod = false;
        var mode = -1;
        try
        {
            XorEncrypt(methodsData, GetXorKey(decryptorMethod));
            isFindDnrMethod = FindDnrCompileMethod(decryptorMethod.DeclaringType) != null;
            var methodsDataReader = ByteArrayDataReaderFactory.CreateReader(methodsData);
            var tmp = methodsDataReader.ReadInt32();
            if ((tmp & unchecked((long) (ulong) -16777216)) == 100663296L)
                methodsDataReader.ReadInt32();
            else
                methodsDataReader.Position -= 4U;
            var patchCount = methodsDataReader.ReadInt32();
            mode = methodsDataReader.ReadInt32();
            tmp = methodsDataReader.ReadInt32();
            methodsDataReader.Position -= 4U;
            if ((tmp & unchecked((long) (ulong) -16777216)) == 100663296L)
            {
                methodsDataReader.Position += (uint) (8 * patchCount);
                patchCount = methodsDataReader.ReadInt32();
                mode = methodsDataReader.ReadInt32();
                PatchDwords(Context.PeImage, ref methodsDataReader, patchCount);
                while (methodsDataReader.Position < (ulong) (methodsData.Length - 1))
                {
                    methodsDataReader.ReadUInt32();
                    var numDwords = methodsDataReader.ReadInt32();
                    PatchDwords(Context.PeImage, ref methodsDataReader, numDwords / 2);
                }
            }
            else
            {
                if (!isFindDnrMethod || mode == 1)
                {
                    PatchDwords(Context.PeImage, ref methodsDataReader, patchCount);
                    var isNewer45Decryption = IsNewer45Decryption(decryptorMethod);
                    var isUsingOffset = !IsUsingRva(decryptorMethod);
                    while (methodsDataReader.Position < (ulong) (methodsData.Length - 1))
                    {
                        var rva = (uint) methodsDataReader.ReadInt32();
                        int size;
                        if (!isNewer45Decryption)
                        {
                            methodsDataReader.ReadInt32();
                            size = methodsDataReader.ReadInt32();
                        }
                        else
                            size = methodsDataReader.ReadInt32() * 4;

                        var newData = methodsDataReader.ReadBytes(size);
                        if (Context.IsNative && isUsingOffset)
                            Context.PeImage.DotNetSafeWriteOffset(rva, newData);
                        else
                            Context.PeImage.DotNetSafeWrite(rva, newData);
                    }
                }
                else
                {
                    var methodDef = Context.PeImage.Metadata.TablesStream.MethodTable;
                    var rvaToIndex = new Dictionary<uint, int>((int) methodDef.Rows);
                    var offset = (uint) methodDef.StartOffset;
                    var i = 0;
                    while (i < methodDef.Rows)
                    {
                        var rva2 = Context.PeImage.OffsetReadUInt32(offset);
                        offset += methodDef.RowSize;
                        if (rva2 != 0U)
                        {
                            if ((Context.PeImage.ReadByte(rva2) & 3) == 2)
                                rva2 += 1U;
                            else
                                rva2 += (uint) (4 * (Context.PeImage.ReadByte(rva2 + 1U) >> 4));
                            rvaToIndex[rva2] = i;
                        }

                        i++;
                    }

                    PatchDwords(Context.PeImage, ref methodsDataReader, patchCount);
                    methodsDataReader.ReadInt32();
                    dumpedMethods = new DumpedMethods();
                    while (methodsDataReader.Position < (ulong) (methodsData.Length - 1))
                    {
                        var rva3 = methodsDataReader.ReadUInt32();
                        var index = methodsDataReader.ReadUInt32();
                        var isNativeCode = index >= 1879048192U;
                        var size2 = methodsDataReader.ReadInt32();
                        var methodData = methodsDataReader.ReadBytes(size2);
                        if (rvaToIndex.TryGetValue(rva3, out var methodIndex))
                        {
                            var methodToken = (uint) (100663297 + methodIndex);
                            if (isNativeCode)
                            {
                                if (tokenToNativeCode != null) tokenToNativeCode[methodToken] = methodData;

                                if (DeobUtils.IsCode(nativeLdci4, methodData))
                                {
                                    var val = BitConverter.ToUInt32(methodData, 4);
                                    methodData = new byte[]
                                    {
                                        32,
                                        0,
                                        0,
                                        0,
                                        0,
                                        42
                                    };
                                    methodData[1] = (byte) val;
                                    methodData[2] = (byte) (val >> 8);
                                    methodData[3] = (byte) (val >> 16);
                                    methodData[4] = (byte) (val >> 24);
                                }
                                else
                                {
                                    if (DeobUtils.IsCode(nativeLdci4_0, methodData))
                                        methodData = new byte[]
                                        {
                                            22,
                                            42
                                        };
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

                            var dumpedMethod = new DumpedMethod();
                            Context.PeImage.ReadMethodTableRowTo(dumpedMethod,
                                MDToken.ToRID(methodToken));
                            dumpedMethod.code = methodData;
                            var codeReader = Context.PeImage.Reader;
                            codeReader.Position = Context.PeImage.RvaToOffset(dumpedMethod.mdRVA);
                            var mbHeader = MethodBodyParser.ParseMethodBody(ref codeReader, out _,
                                out dumpedMethod.extraSections);
                            Context.PeImage.UpdateMethodHeaderInfo(dumpedMethod, mbHeader);
                            dumpedMethods.Add(dumpedMethod);
                        }
                    }
                }
            }
        } catch (Exception ex)
        {
            Logger.Error("Failed to decrypt methods. " + ex.Message);
        }

        using (Context.Module)
        {
            if (!isFindDnrMethod || mode == 1)
                Context.Module = Context.AssemblyModule.Reload(
                    Context.PeImage.peImageData, CreateDumpedMethodsRestorer(dumpedMethods), null);
            else if (dumpedMethods.Count > 0)
                Context.Module = Context.AssemblyModule.Reload(
                    Context.ModuleBytes, CreateDumpedMethodsRestorer(dumpedMethods), null);
            else
            {
                Logger.Error("Failed to decrypt methods.");
                return;
            }

            Logger.Done("Methods decrypted.");
        }
    }

    private static MethodDef FindDnrCompileMethod(TypeDef type) =>
        (from method in type.Methods
            where method.IsStatic && method.Body != null
            let sig = method.MethodSig
            where sig != null && sig.Params.Count == 6
            select method).FirstOrDefault(method => GetCompileMethodType(method) != CompileMethodType.Unknown);

    private static CompileMethodType GetCompileMethodType(IMethod method)
    {
        if (DotNetUtils.IsMethod(method, "System.UInt32",
                "(System.UInt64&,System.IntPtr,System.IntPtr,System.UInt32,System.IntPtr&,System.UInt32&)"))
            return CompileMethodType.V1;

        return DotNetUtils.IsMethod(method, "System.UInt32",
            "(System.IntPtr,System.IntPtr,System.IntPtr,System.UInt32,System.IntPtr,System.UInt32&)")
            ? CompileMethodType.V2
            : CompileMethodType.Unknown;
    }

    private static DumpedMethodsRestorer CreateDumpedMethodsRestorer(DumpedMethods dumpedMethods)
    {
        if (dumpedMethods == null || dumpedMethods.Count == 0) return null;
        return new DumpedMethodsRestorer(dumpedMethods);
    }

    private static void PatchDwords(MyPEImage peImage, ref DataReader reader, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var rva = reader.ReadUInt32();
            var data = reader.ReadUInt32();
            peImage.DotNetSafeWrite(rva, BitConverter.GetBytes(data));
        }
    }

    private static bool IsUsingRva(MethodDef method)
    {
        if (method?.Body == null) return false;
        var instrs = method.Body.Instructions;
        return instrs.Where((t, i) => t.OpCode.Equals(OpCodes.Ldloca_S) &&
                                      instrs[i + 1].OpCode.Equals(OpCodes.Ldsfld) &&
                                      instrs[i + 2].OpCode.Equals(OpCodes.Ldloc_S) &&
                                      instrs[i + 3].OpCode.Equals(OpCodes.Conv_I8) &&
                                      instrs[i + 4].OpCode.Equals(OpCodes.Add) &&
                                      instrs[i + 5].OpCode.Equals(OpCodes.Ldloc_S) &&
                                      instrs[i + 6].OpCode.Equals(OpCodes.Conv_I8) &&
                                      instrs[i + 7].OpCode.Equals(OpCodes.Sub) &&
                                      instrs[i + 8].OpCode.Code.Equals(Code.Call)).Any();
    }

    private static bool IsNewer45Decryption(MethodDef method)
    {
        if (method?.Body == null) return false;
        for (var i = 0; i < method.Body.Instructions.Count - 4; i++)
            if (method.Body.Instructions[i].IsLdcI4())
                if (method.Body.Instructions[i + 1].OpCode.Code.Equals(Code.Mul))
                    if (method.Body.Instructions[i + 2].IsLdcI4())
                        if (method.Body.Instructions[i + 3].OpCode.Code.Equals(Code.Ldloca_S) ||
                            method.Body.Instructions[i + 3].OpCode.Code.Equals(Code.Ldloca))
                            if (method.Body.Instructions[i + 4].OpCode.Code.Equals(Code.Call))
                                return true;
        return false;
    }

    private static long GetXorKey(MethodDef method)
    {
        for (var i = 0; i < method.Body.Instructions.Count - 1; i++)
        {
            if (method.Body.Instructions[i].OpCode.Code.Equals(Code.Ldind_I8))
            {
                var ldci4 = method.Body.Instructions[i + 1];
                long result;
                if (ldci4.IsLdcI4())
                    result = ldci4.GetLdcI4Value();
                else
                {
                    if (!ldci4.OpCode.Code.Equals(Code.Ldc_I8)) goto Continue;
                    result = (long) ldci4.Operand;
                }

                return result;
            }

            Continue: ;
        }

        return 0L;
    }

    private static EmbeddedResource GetEncryptedResource(MethodDef method)
    {
        if (method is not {HasBody: true} || !method.Body.HasInstructions) return null;
        foreach (var s in DotNetUtils.GetCodeStrings(method))
            if (DotNetUtils.GetResource(Context.Module, s) is EmbeddedResource resource)
                return resource;
        return null;
    }

    private static void XorEncrypt(byte[] data, long xorKey)
    {
        if (xorKey != 0L)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            var writer = new BinaryWriter(stream);
            var count = data.Length / 8;
            for (var i = 0; i < count; i++)
            {
                var val = reader.ReadInt64();
                val ^= xorKey;
                stream.Position -= 8L;
                writer.Write(val);
            }
        }
    }

    private enum CompileMethodType
    {
        Unknown,
        V1,
        V2
    }
}