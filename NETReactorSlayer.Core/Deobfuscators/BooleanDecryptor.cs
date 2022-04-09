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
using System.Linq;
using static NETReactorSlayer.Core.Deobfuscators.ResourceDecryptor;

namespace NETReactorSlayer.Core.Deobfuscators
{
    class BooleanDecryptor : IDeobfuscator
    {
        private MethodDef decryptorMethod;
        private EmbeddedResource encryptedResource;
        public void Execute()
        {
            long count = 0L;
            Find();
            if (decryptorMethod == null || encryptedResource == null)
            {
                Logger.Warn("Couldn't find any encrypted boolean.");
                return;
            }
            Cleaner.ResourceToRemove.Add(encryptedResource);
            Cleaner.MethodsToPatch.Add(decryptorMethod);
            Deobfuscate(decryptorMethod);
            DnrDecrypterType decrypterType = GetDecrypterType(decryptorMethod, new string[0]);
            byte[] key = GetBytes(decryptorMethod, 32);
            if (decrypterType == DnrDecrypterType.V3)
            {
                V3 V3 = new V3(decryptorMethod);
                decryptedBytes = V3.Decrypt(encryptedResource);
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
                decryptedBytes = V1.Decrypt(encryptedResource);
                goto Continue;
            }
            else if (decrypterType == DnrDecrypterType.V2)
            {
                V2 V2 = new V2(iv, key, decryptorMethod);
                decryptedBytes = V2.Decrypt(encryptedResource);
                goto Continue;
            }
            Logger.Warn("Couldn't find any encrypted boolean.");
            return;
        Continue:
            if (decryptedBytes == null)
            {
                Logger.Error("Failed to decrypt booleans.");
                return;
            }
            foreach (TypeDef type in DeobfuscatorContext.Module.GetTypes().Where(x => x.HasMethods))
            {
                foreach (MethodDef method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                {
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        try
                        {
                            if (!method.Body.Instructions[i].OpCode.Equals(OpCodes.Call) || !method.Body.Instructions[i - 1].IsLdcI4() || !method.Body.Instructions[i + 1].IsConditionalBranch())
                                continue;
                            if (method.Body.Instructions[i].Operand is IMethod iMethod && iMethod.MDToken.ToInt32() == decryptorMethod.MDToken.ToInt32())
                            {
                                int offset = method.Body.Instructions[i - 1].GetLdcI4Value();
                                bool value = Decrypt(offset);
                                if (value)
                                    method.Body.Instructions[i] = Instruction.CreateLdcI4(1);
                                else
                                    method.Body.Instructions[i] = Instruction.CreateLdcI4(0);
                                method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                count += 1L;
                            }
                        }
                        catch { }
                    }
                }
            }
            if (count > 0L) Logger.Done((int)count + " Booleans decrypted.");
            else Logger.Warn("Couldn't find any encrypted boolean.");
        }
        public bool Decrypt(int offset) => DeobfuscatorContext.ModuleBytes[BitConverter.ToUInt32(decryptedBytes, offset)] == 0x80;

        private void Find()
        {
            foreach (var type in DeobfuscatorContext.Module.Types.Where(x => x.BaseType != null && x.BaseType.FullName == "System.Object"))
            {
                var methodDef = DotNetUtils.GetMethod(type, "System.Boolean", "(System.Int32)");
                if (methodDef != null && GetEncryptedResource(methodDef) != null && GetDecrypterType(methodDef, new string[0]) != DnrDecrypterType.Unknown)
                {
                    decryptorMethod = methodDef;
                    encryptedResource = GetEncryptedResource(decryptorMethod);
                    break;
                }
            }
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

        private DnrDecrypterType GetDecrypterType(MethodDef method, IList<string> additionalTypes)
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

        public void Deobfuscate(MethodDef method)
        {
            List<IBlocksDeobfuscator> list = new List<IBlocksDeobfuscator> { new MethodCallInliner(false) };
            SimpleDeobfuscatorFlags flags = 0;
            if (!(method == null || (!((flags & SimpleDeobfuscatorFlags.Force) > 0U) && Check(method, SimpleDeobFlags.HasDeobfuscated))))
            {
                Deobfuscate(method, delegate (Blocks blocks)
                {
                    bool disableNewCFCode = (flags & SimpleDeobfuscatorFlags.DisableConstantsFolderExtraInstrs) > (SimpleDeobfuscatorFlags)0U;
                    BlocksCflowDeobfuscator cflowDeobfuscator = new BlocksCflowDeobfuscator(list, disableNewCFCode);
                    cflowDeobfuscator.Initialize(blocks);
                    cflowDeobfuscator.Deobfuscate();
                });
            }
        }

        void Deobfuscate(MethodDef method, Action<Blocks> handler)
        {
            if (method == null || !method.HasBody || !method.Body.HasInstructions) return;
            try
            {
                if (method.Body.Instructions.Any((Instruction instr) => instr.OpCode.Equals(OpCodes.Switch)))
                    DeobfuscateEquations(method);
                Blocks blocks = new Blocks(method);
                handler(blocks);
                blocks.GetCode(out IList<Instruction> allInstructions, out IList<ExceptionHandler> allExceptionHandlers);
                DotNetUtils.RestoreBody(method, allInstructions, allExceptionHandlers);
            }
            catch
            {
                Logger.Warn("Couldn't deobfuscate " + method.FullName);
            }
        }

        void DeobfuscateEquations(MethodDef method)
        {
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                if (method.Body.Instructions[i].IsBrtrue() && method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Pop) && method.Body.Instructions[i - 1].OpCode.Equals(OpCodes.Call))
                {
                    if (method.Body.Instructions[i - 1].Operand is MethodDef methodDef)
                    {
                        IList<Instruction> methodDefInstr = methodDef.Body.Instructions;
                        if (methodDef.ReturnType.FullName == "System.Boolean")
                        {
                            if (methodDefInstr[methodDefInstr.Count - 2].OpCode.Equals(OpCodes.Ldc_I4_0))
                            {
                                method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i].OpCode = OpCodes.Nop;
                            }
                            else
                            {
                                method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                            }
                        }
                        else
                        {
                            method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i].OpCode = OpCodes.Nop;
                        }
                    }
                }
                else
                {
                    if (method.Body.Instructions[i].IsBrfalse() && method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Pop) && method.Body.Instructions[i - 1].OpCode.Equals(OpCodes.Call))
                    {
                        if (method.Body.Instructions[i - 1].Operand is MethodDef methodDef2)
                        {
                            IList<Instruction> methodDefInstr2 = methodDef2.Body.Instructions;
                            if (methodDef2.ReturnType.FullName == "System.Boolean")
                            {
                                if (methodDefInstr2[methodDefInstr2.Count - 2].OpCode.Equals(OpCodes.Ldc_I4_0))
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                                }
                                else
                                {
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                }
                            }
                            else
                            {
                                method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                            }
                        }
                    }
                }
            }
        }

        bool Check(MethodDef method, SimpleDeobFlags flags)
        {
            if (method == null) return false;
            simpleDeobfuscatorFlags.TryGetValue(method, out SimpleDeobFlags oldFlags);
            simpleDeobfuscatorFlags[method] = (oldFlags | flags);
            return ((oldFlags & flags) == flags);
        }

        [Flags] public enum SimpleDeobfuscatorFlags : uint { Force = 1U, DisableConstantsFolderExtraInstrs = 2U }

        [Flags] enum SimpleDeobFlags { HasDeobfuscated = 1 }

        readonly Dictionary<MethodDef, SimpleDeobFlags> simpleDeobfuscatorFlags = new Dictionary<MethodDef, SimpleDeobFlags>();

        byte[] decryptedBytes = null;
    }
}
