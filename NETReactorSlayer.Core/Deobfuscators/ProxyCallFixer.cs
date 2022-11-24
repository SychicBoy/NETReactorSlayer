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
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core.Deobfuscators
{
    internal class ProxyCallFixer : IStage
    {
        public void Execute()
        {
            try
            {
                if (!Find())
                {
                    Logger.Warn("Couldn't find any proxied call.");
                    return;
                }

                var bytes = _encryptedResource.Decrypt();

                if (!GetDictionary(bytes))
                    throw new InvalidOperationException();

                var count = RestoreCalls();

                if (count > 0)
                {
                    Logger.Done(count + " Proxied calls fixed.");
                    Cleaner.AddMethodToBeRemoved(_encryptedResource.DecrypterMethod);
                    Cleaner.AddResourceToBeRemoved(_encryptedResource.EmbeddedResource);
                } else
                    Logger.Warn("Couldn't find any proxied call.");
            } catch (Exception ex)
            {
                Logger.Error("An unexpected error occurred during fixing proxied calls.", ex);
            }

            _encryptedResource?.Dispose();
        }

        #region Private Methods

        private bool Find()
        {
            var callCounter = new CallCounter();
            foreach (var type in from x in Context.Module.GetTypes()
                     where x.Namespace.Equals("") && DotNetUtils.DerivesFromDelegate(x)
                     select x)
                if (type.FindStaticConstructor() is { } cctor)
                    foreach (var method in DotNetUtils.GetMethodCalls(cctor).Where(method =>
                                 method.MethodSig.GetParamCount() == 1 &&
                                 method.GetParam(0).FullName == "System.RuntimeTypeHandle"))
                        callCounter.Add(method);

            if (callCounter.Most() is not { } mostCalls)
                return false;

            var methodDef = mostCalls.ResolveMethodDef();
            if (methodDef == null || !EncryptedResource.IsKnownDecrypter(methodDef, Array.Empty<string>(), true))
                return false;

            _encryptedResource = new EncryptedResource(methodDef);
            if (_encryptedResource.EmbeddedResource != null)
                return true;
            _encryptedResource.Dispose();
            return false;
        }

        private bool GetDictionary(byte[] bytes)
        {
            var length = bytes.Length / 8;
            _dictionary = new Dictionary<int, int>();
            var reader = new BinaryReader(new MemoryStream(bytes));
            for (var i = 0; i < length; i++)
            {
                var key = reader.ReadInt32();
                var value = reader.ReadInt32();
                if (!_dictionary.ContainsKey(key))
                    _dictionary.Add(key, value);
            }

            reader.Close();
            return true;
        }

        private void GetCallInfo(IMDTokenProvider field, out IMethod calledMethod, out OpCode callOpcode)
        {
            callOpcode = OpCodes.Call;
            _dictionary.TryGetValue((int)field.MDToken.Raw, out var token);
            if ((token & 1073741824) > 0)
                callOpcode = OpCodes.Callvirt;
            token &= 1073741823;
            calledMethod = Context.Module.ResolveToken(token) as IMethod;
        }

        private long RestoreCalls()
        {
            long count = 0;
            foreach (var method in Context.Module.GetTypes().SelectMany(type =>
                         (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x)
                         .ToArray()))
            {
                for (var i = 0; i < method.Body.Instructions.Count; i++)
                    try
                    {
                        if (!method.Body.Instructions[i].OpCode.Equals(OpCodes.Ldsfld) ||
                            !method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Call))
                            continue;
                        var field = method.Body.Instructions[i].Operand as IField;
                        GetCallInfo(field, out var iMethod, out var opCpde);
                        if (iMethod == null)
                            continue;
                        iMethod = Context.Module.Import(iMethod);
                        if (iMethod == null)
                            continue;
                        method.Body.Instructions[i].OpCode = OpCodes.Nop;
                        method.Body.Instructions[i + 1] = Instruction.Create(opCpde, iMethod);
                        method.Body.UpdateInstructionOffsets();
                        count++;
                        Cleaner.AddTypeToBeRemoved(field?.DeclaringType);
                    } catch { }

                SimpleDeobfuscator.DeobfuscateBlocks(method);
            }

            return count;
        }

        #endregion

        #region Fields

        private Dictionary<int, int> _dictionary;
        private EncryptedResource _encryptedResource;

        #endregion
    }
}