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

namespace NETReactorSlayer.Core.Deobfuscators {
    internal class TokenDeobfuscator : IStage {
        public void Execute() {
            TypeDef typeDef = null;
            MethodDef fieldMethod = null;
            MethodDef typeMethod = null;
            long count = 0;
            foreach (var type in from type in Context.Module.GetTypes()
                         .Where(x => !x.HasProperties && !x.HasEvents && x.Fields.Count != 0)
                     from _ in type.Fields.Where(x => x.FieldType.FullName.Equals("System.ModuleHandle"))
                     select type) {
                foreach (var method in type.Methods.Where(x => x.MethodSig != null &&
                                                               x.MethodSig.Params.Count.Equals(1) &&
                                                               x.MethodSig.Params[0].GetElementType() == ElementType.I4)
                             .ToList())
                    if (method.MethodSig.RetType.GetFullName().Equals("System.RuntimeTypeHandle"))
                        typeMethod = method;
                    else if (method.MethodSig.RetType.GetFullName().Equals("System.RuntimeFieldHandle"))
                        fieldMethod = method;
                if (typeMethod == null || fieldMethod == null)
                    continue;
                typeDef = type;
                goto Continue;
            }

            Continue:
            if (typeDef != null)
                foreach (var type in Context.Module.GetTypes())
                foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions)) {
                    var gpContext = GenericParamContext.Create(method);
                    var blocks = new Blocks(method);
                    foreach (var block in blocks.MethodBlocks.GetAllBlocks())
                        for (var i = 0; i < block.Instructions.Count; i++)
                            try {
                                if (!block.Instructions[i].OpCode.Code.Equals(Code.Ldc_I4) ||
                                    block.Instructions[i + 1].OpCode.Code != Code.Call)
                                    continue;
                                if (block.Instructions[i + 1].Operand is not IMethod iMethod ||
                                    !default(SigComparer).Equals(typeDef, iMethod.DeclaringType))
                                    continue;
                                var methodDef = DotNetUtils.GetMethod(Context.Module, iMethod);
                                if (methodDef == null)
                                    continue;
                                if (methodDef != typeMethod && methodDef != fieldMethod)
                                    continue;
                                var token = (uint)(int)block.Instructions[i].Operand;
                                block.Instructions[i] = new Instr(OpCodes.Nop.ToInstruction());
                                block.Instructions[i + 1] = new Instr(new Instruction(OpCodes.Ldtoken,
                                    Context.Module.ResolveToken(token, gpContext) as ITokenOperand));
                                count++;
                            } catch { }

                    blocks.GetCode(out var allInstructions, out var allExceptionHandlers);
                    DotNetUtils.RestoreBody(method, allInstructions, allExceptionHandlers);
                }


            if (count == 0)
                Logger.Warn("Couldn't found any obfuscated metadata token.");
            else {
                Cleaner.AddTypeToBeRemoved(typeDef);
                Logger.Done($"{(int)count} Metadata tokens deobfuscated.");
            }
        }
    }
}