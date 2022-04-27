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
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Helper.De4dot;
using static NETReactorSlayer.Core.Deobfuscators.ResourceDecryptor;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class BooleanDecryptor : IDeobfuscator
{
    private byte[] _decryptedBytes;
    private MethodDef _decryptorMethod;
    private EmbeddedResource _encryptedResource;

    public void Execute()
    {
        var count = 0L;
        Find();
        if (_decryptorMethod == null || _encryptedResource == null)
        {
            Logger.Warn("Couldn't find any encrypted boolean.");
            return;
        }

        Cleaner.ResourceToRemove.Add(_encryptedResource);
        Cleaner.MethodsToRemove.Add(_decryptorMethod);
        SimpleDeobfuscator.Deobfuscate(_decryptorMethod);
        var decrypterType = GetDecrypterType(_decryptorMethod, Array.Empty<string>());
        var key = GetBytes(_decryptorMethod, 32);
        if (decrypterType == DnrDecrypterType.V3)
            try
            {
                var v3 = new V3(_decryptorMethod);
                _decryptedBytes = v3.Decrypt(_encryptedResource);
                goto Continue;
            }
            catch { }

        var iv = GetBytes(_decryptorMethod, 16);
        if (IsNeedReverse(_decryptorMethod))
            Array.Reverse(iv);
        if (UsesPublicKeyToken(_decryptorMethod))
            if (DeobfuscatorContext.Module.Assembly.PublicKeyToken is { } publicKeyToken &&
                publicKeyToken.Data.Length != 0)
                for (var z = 0; z < 8; z++)
                    iv[z * 2 + 1] = publicKeyToken.Data[z];

        if (decrypterType == DnrDecrypterType.V1)
            try
            {
                var v1 = new V1(iv, key);
                _decryptedBytes = v1.Decrypt(_encryptedResource);
                goto Continue;
            }
            catch { }

        if (decrypterType == DnrDecrypterType.V2)
            try
            {
                var v2 = new V2(iv, key, _decryptorMethod);
                _decryptedBytes = v2.Decrypt(_encryptedResource);
                goto Continue;
            }
            catch { }

        Logger.Warn("Couldn't find any encrypted boolean.");
        return;
    Continue:
        if (_decryptedBytes == null)
        {
            Logger.Error("Failed to decrypt booleans.");
            return;
        }

        foreach (var type in DeobfuscatorContext.Module.GetTypes().Where(x => x.HasMethods))
            foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
            {
                for (var i = 0; i < method.Body.Instructions.Count; i++)
                    try
                    {
                        if (!method.Body.Instructions[i].OpCode.Equals(OpCodes.Call) ||
                            !method.Body.Instructions[i - 1].IsLdcI4() ||
                            !method.Body.Instructions[i + 1].IsConditionalBranch())
                            continue;
                        if (method.Body.Instructions[i].Operand is not IMethod iMethod ||
                            iMethod != _decryptorMethod) continue;
                        var offset = method.Body.Instructions[i - 1].GetLdcI4Value();
                        var value = Decrypt(offset);
                        if (value)
                            method.Body.Instructions[i] = Instruction.CreateLdcI4(1);
                        else
                            method.Body.Instructions[i] = Instruction.CreateLdcI4(0);
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        count += 1L;
                    }
                    catch { }

                SimpleDeobfuscator.DeobfuscateBlocks(method);
            }

        if (count > 0L) Logger.Done((int)count + " Booleans decrypted.");
        else Logger.Warn("Couldn't find any encrypted boolean.");
    }

    public bool Decrypt(int offset) =>
        DeobfuscatorContext.ModuleBytes[BitConverter.ToUInt32(_decryptedBytes, offset)] == 0x80;

    private void Find()
    {
        foreach (var type in DeobfuscatorContext.Module.Types.Where(x =>
                     x.BaseType is { FullName: "System.Object" }))
            if (DotNetUtils.GetMethod(type, "System.Boolean", "(System.Int32)") is { } methodDef &&
                GetEncryptedResource(methodDef) != null &&
                GetDecrypterType(methodDef, Array.Empty<string>()) != DnrDecrypterType.Unknown)
            {
                _decryptorMethod = methodDef;
                _encryptedResource = GetEncryptedResource(_decryptorMethod);
                break;
            }
    }

    private EmbeddedResource GetEncryptedResource(MethodDef method)
    {
        if (method == null || !method.HasBody || !method.Body.HasInstructions) return null;
        foreach (var s in DotNetUtils.GetCodeStrings(method))
            if (DotNetUtils.GetResource(DeobfuscatorContext.Module, s) is EmbeddedResource resource)
                return resource;
        return null;
    }

    private DnrDecrypterType GetDecrypterType(MethodDef method, IList<string> additionalTypes)
    {
        if (method == null || !method.IsStatic || method.Body == null) return DnrDecrypterType.Unknown;
        additionalTypes ??= Array.Empty<string>();
        var localTypes = new LocalTypes(method);
        if (V1.CouldBeResourceDecrypter(method, localTypes, additionalTypes)) return DnrDecrypterType.V1;
        if (V2.CouldBeResourceDecrypter(localTypes, additionalTypes)) return DnrDecrypterType.V2;
        if (V3.CouldBeResourceDecrypter(localTypes, additionalTypes)) return DnrDecrypterType.V3;
        return DnrDecrypterType.Unknown;
    }
}