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
using System.IO;
using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Abstractions;
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core.Stages
{
    internal class BooleanDecrypter : IStage
    {
        public void Run(IContext context)
        {
            Context = context;
            try
            {
                if (!Find())
                    return;

                var bytes = _encryptedResource.Decrypt();

                var count = InlineAllBooleans(bytes, out var failures);

                if (count > 0)
                {
                    Context.Logger.Info(count + " Booleans decrypted.");
                    if (failures == 0)
                    {
                        Cleaner.AddResourceToBeRemoved(_encryptedResource.EmbeddedResource);
                        Cleaner.AddMethodToBeRemoved(_encryptedResource.DecrypterMethod);
                    }
                    else
                        Context.Logger.Warn("Boolean decryption was partial; the decrypter and resource were retained.");
                }
            }
            catch (Exception ex)
            {
                Context.Logger.Error($"Boolean decryption failed ({ex.GetType().Name}): {ex.Message}");
            }

            _encryptedResource?.Dispose();
        }

        private bool Find()
        {
            foreach (var type in Context.Module.Types.Where(x =>
                         x.BaseType is { FullName: "System.Object" }))
                if (DotNetUtils.GetMethod(type, "System.Boolean", "(System.Int32)") is { } methodDef &&
                    EncryptedResource.IsKnownDecrypter(methodDef, Array.Empty<string>(), true))
                {
                    _encryptedResource = new EncryptedResource(Context, methodDef);
                    if (_encryptedResource.EmbeddedResource != null)
                        return true;
                    _encryptedResource.Dispose();
                }

            return false;
        }

        private long InlineAllBooleans(byte[] bytes, out int failureCount)
        {
            long count = 0;
            failureCount = 0;
            foreach (var method in Context.Module.GetTypes().Where(x => x.HasMethods)
                         .SelectMany(type => type.Methods.Where(x => x.HasBody && x.Body.HasInstructions)))
            {
                for (var i = 0; i < method.Body.Instructions.Count; i++)
                {
                    if (i == 0 || i + 1 >= method.Body.Instructions.Count ||
                        !method.Body.Instructions[i].OpCode.Equals(OpCodes.Call) ||
                        !method.Body.Instructions[i - 1].IsLdcI4() ||
                        !method.Body.Instructions[i + 1].IsConditionalBranch() ||
                        method.Body.Instructions[i].Operand is not IMethod iMethod ||
                        iMethod != _encryptedResource.DecrypterMethod)
                        continue;

                    var offset = method.Body.Instructions[i - 1].GetLdcI4Value();
                    try
                    {
                        var value = Decrypt(offset, bytes);
                        if (value)
                            method.Body.Instructions[i] = Instruction.CreateLdcI4(1);
                        else
                            method.Body.Instructions[i] = Instruction.CreateLdcI4(0);
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        count++;
                    }
                    catch (Exception)
                    {
                        failureCount++;
                    }
                }

                SimpleDeobfuscator.DeobfuscateBlocks(method);
            }

            if (failureCount > 0)
                Context.Logger.Warn($"{failureCount} boolean call(s) could not be decrypted because their data was invalid.");
            return count;
        }

        private bool Decrypt(int offset, byte[] bytes)
        {
            if (bytes == null || offset < 0 || offset > bytes.Length - sizeof(uint))
                throw new InvalidDataException("The boolean reference is outside the decrypted resource.");

            var moduleOffset = BitConverter.ToUInt32(bytes, offset);
            if (moduleOffset >= Context.ModuleBytes.Length)
                throw new InvalidDataException("The boolean reference is outside the input assembly.");

            return Context.ModuleBytes[moduleOffset] == 0x80;
        }

        private IContext Context { get; set; }
        private EncryptedResource _encryptedResource;
    }
}