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

using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core.Deobfuscators
{
    internal class StrongNamePatcher : IStage
    {
        public void Execute()
        {
            long count = 0;
            var methodDef = Find();
            if (methodDef == null)
            {
                Logger.Warn("Couldn't find any strong name removal protection.");
                return;
            }

            foreach (var type in Context.Module.GetTypes())
            foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                try
                {
                    var blocks = new Blocks(method);
                    var block = GetBlock(blocks, methodDef);
                    if (block?.FallThrough == null || block.Targets.Count != 1)
                        continue;
                    count++;
                    block.ReplaceLastInstrsWithBranch(11, block.Targets[0]);
                    if (block.FallThrough.FallThrough == block.FallThrough && block.FallThrough.Sources.Count == 1 &&
                        block.FallThrough.Targets == null)
                        block.FallThrough.Parent.RemoveGuaranteedDeadBlock(block.FallThrough);
                    if (block.FallThrough.Instructions.Count <= 1 &&
                        block.FallThrough.LastInstr.OpCode.Code == Code.Nop &&
                        block.FallThrough.FallThrough != null && block.FallThrough.Targets == null &&
                        block.FallThrough.Sources.Count == 0)
                        if (block.FallThrough.FallThrough.FallThrough == block.FallThrough.FallThrough &&
                            block.FallThrough.FallThrough.Sources.Count == 2 &&
                            block.FallThrough.FallThrough.Targets == null)
                        {
                            block.FallThrough.Parent.RemoveGuaranteedDeadBlock(block.FallThrough);
                            block.FallThrough.FallThrough.Parent.RemoveGuaranteedDeadBlock(
                                block.FallThrough.FallThrough);
                        }

                    blocks.GetCode(out var allInstructions, out var allExceptionHandlers);
                    DotNetUtils.RestoreBody(method, allInstructions, allExceptionHandlers);
                }
                catch
                {
                }

            if (count > 0) Logger.Done($"Strong name removal protection removed from {(int)count} methods.");
            else Logger.Warn("Couldn't find strong name removal protection.");
        }

        #region Private Methods

        private static MethodDef Find() =>
            (from type in Context.Module.GetTypes()
                from method in type.Methods
                where method.IsStatic && method.Body != null
                let sig = method.MethodSig
                where sig != null && sig.Params.Count == 2
                where sig.RetType.ElementType == ElementType.Object || sig.RetType.ElementType == ElementType.String
                where sig.Params[0]?.ElementType == ElementType.Object ||
                      sig.Params[0]?.ElementType == ElementType.String
                where sig.Params[1]?.ElementType == ElementType.Object ||
                      sig.Params[1]?.ElementType == ElementType.String
                select method).FirstOrDefault(method => new LocalTypes(method).All(new[]
            {
                "System.Byte[]",
                "System.IO.MemoryStream",
                "System.Security.Cryptography.CryptoStream",
                "System.Security.Cryptography.MD5",
                "System.Security.Cryptography.Rijndael"
            }) || new LocalTypes(method).All(new[]
            {
                "System.Byte[]",
                "System.IO.MemoryStream",
                "System.Security.Cryptography.SymmetricAlgorithm",
                "System.Security.Cryptography.CryptoStream"
            }));

        private static Block GetBlock(Blocks blocks, IMethod methodDef) =>
            (from block in blocks.MethodBlocks.GetAllBlocks()
                where block.LastInstr.IsBrfalse()
                let instructions = block.Instructions
                where instructions.Count >= 11
                let i = instructions.Count - 11
                where instructions[i].OpCode.Code == Code.Ldtoken
                where instructions[i].Operand is ITypeDefOrRef
                where instructions[i + 1].OpCode.Code == Code.Call ||
                      (instructions[i + 1].OpCode.Code == Code.Callvirt &&
                       instructions[i + 1].Operand is IMethod iMethod1 && iMethod1.FullName ==
                       "System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)")
                where instructions[i + 2].OpCode.Code == Code.Call ||
                      (instructions[i + 2].OpCode.Code == Code.Callvirt &&
                       instructions[i + 2].Operand is IMethod iMethod2 && iMethod2.FullName ==
                       "System.Reflection.Assembly System.Type::get_Assembly()")
                where instructions[i + 3].OpCode.Code == Code.Call ||
                      (instructions[i + 3].OpCode.Code == Code.Callvirt &&
                       instructions[i + 3].Operand is IMethod iMethod3 && iMethod3.FullName ==
                       "System.Reflection.AssemblyName System.Reflection.Assembly::GetName()")
                where instructions[i + 4].OpCode.Code == Code.Call ||
                      (instructions[i + 4].OpCode.Code == Code.Callvirt &&
                       instructions[i + 4].Operand is IMethod iMethod4 && iMethod4.FullName ==
                       "System.Byte[] System.Reflection.AssemblyName::GetPublicKeyToken()")
                where instructions[i + 5].OpCode.Code == Code.Call ||
                      (instructions[i + 5].OpCode.Code == Code.Callvirt &&
                       instructions[i + 5].Operand is IMethod iMethod5 && iMethod5.FullName ==
                       "System.String System.Convert::ToBase64String(System.Byte[])")
                where instructions[i + 6].OpCode.Code == Code.Ldstr
                where instructions[i + 7].OpCode.Code == Code.Call ||
                      (instructions[i + 7].OpCode.Code == Code.Callvirt &&
                       instructions[i + 7].Operand is IMethod calledMethod &&
                       MethodEqualityComparer.CompareDeclaringTypes.Equals(calledMethod, methodDef))
                where instructions[i + 8].OpCode.Code == Code.Ldstr
                where instructions[i + 9].OpCode.Code == Code.Call ||
                      (instructions[i + 9].OpCode.Code == Code.Callvirt &&
                       instructions[i + 9].Operand is IMethod iMethod6 && iMethod6.FullName ==
                       "System.Boolean System.String::op_Inequality(System.String,System.String)")
                select block).FirstOrDefault();

        #endregion
    }
}