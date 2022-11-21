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
using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core.Deobfuscators
{
    internal class BooleanDecrypter : IStage
    {
        public void Execute()
        {
            try
            {
                if (!Find())
                    return;

                var bytes = _encryptedResource.Decrypt();

                var count = InlineAllBooleans(bytes);

                if (count > 0)
                {
                    Logger.Done(count + " Booleans decrypted.");
                    Cleaner.AddResourceToBeRemoved(_encryptedResource.EmbeddedResource);
                    Cleaner.AddMethodToBeRemoved(_encryptedResource.DecrypterMethod);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An unexpected error occurred during decrypting booleans.", ex);
            }

            _encryptedResource?.Dispose();
        }

        #region Private Methods

        private bool Find()
        {
            foreach (var type in Context.Module.Types.Where(x =>
                         x.BaseType != null && x.BaseType.FullName == "System.Object"))
                if (DotNetUtils.GetMethod(type, "System.Boolean", "(System.Int32)") is MethodDef methodDef &&
                    EncryptedResource.IsKnownDecrypter(methodDef, Array.Empty<string>(), true))
                {
                    _encryptedResource = new EncryptedResource(methodDef);
                    if (_encryptedResource.EmbeddedResource == null)
                    {
                        _encryptedResource.Dispose();
                        continue;
                    }

                    return true;
                }

            return false;
        }

        private long InlineAllBooleans(byte[] bytes)
        {
            long count = 0;
            foreach (var type in Context.Module.GetTypes().Where(x => x.HasMethods))
            foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
            {
                for (var i = 0; i < method.Body.Instructions.Count; i++)
                    try
                    {
                        if (!method.Body.Instructions[i].OpCode.Equals(OpCodes.Call) ||
                            !method.Body.Instructions[i - 1].IsLdcI4() ||
                            !method.Body.Instructions[i + 1].IsConditionalBranch())
                            continue;
                        if (!(method.Body.Instructions[i].Operand is IMethod iMethod) ||
                            iMethod != _encryptedResource.DecrypterMethod) continue;
                        var offset = method.Body.Instructions[i - 1].GetLdcI4Value();
                        var value = Decrypt(offset, bytes);
                        if (value)
                            method.Body.Instructions[i] = Instruction.CreateLdcI4(1);
                        else
                            method.Body.Instructions[i] = Instruction.CreateLdcI4(0);
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        count++;
                    }
                    catch
                    {
                    }

                SimpleDeobfuscator.DeobfuscateBlocks(method);
            }

            return count;
        }

        private static bool Decrypt(int offset, byte[] bytes) =>
            Context.ModuleBytes[BitConverter.ToUInt32(bytes, offset)] == 0x80;

        #endregion

        #region Fields

        private EncryptedResource _encryptedResource;

        #endregion
    }
}