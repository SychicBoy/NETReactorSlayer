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

using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core.Deobfuscators {
    internal class MethodInliner : IStage {
        public void Execute() {
            long count = 0;
            var proxies = new HashSet<MethodDef>();
            foreach (var method in Context.Module.GetTypes().SelectMany(type =>
                         from x in type.Methods.ToList() where x.HasBody && x.Body.HasInstructions select x))
                try {
                    var length = method.Body.Instructions.Count;
                    var i = 0;
                    for (; i < length; i++) {
                        MethodDef methodDef;
                        if (!method.Body.Instructions[i].OpCode.Equals(OpCodes.Call) ||
                            (methodDef = method.Body.Instructions[i].Operand as MethodDef) == null ||
                            !IsInlineMethod(methodDef, out var instructions) ||
                            !IsCompatibleType(method.DeclaringType, methodDef.DeclaringType))
                            continue;
                        count++;
                        method.Body.Instructions[i].OpCode = OpCodes.Nop;
                        method.Body.Instructions[i].Operand = null;
                        length += instructions.Count;
                        foreach (var instr in instructions)
                            method.Body.Instructions.Insert(i++, instr);
                        method.Body.UpdateInstructionOffsets();
                        proxies.Add(methodDef);
                    }

                    SimpleDeobfuscator.DeobfuscateBlocks(method);
                } catch { }

            foreach (var instruction in from type in Context.Module.GetTypes()
                     from method in from x in type.Methods.ToArray() where x.HasBody && x.Body.HasInstructions select x
                     from instruction in method.Body.Instructions
                     select instruction)
                try {
                    MethodDef item;
                    if (instruction.OpCode.OperandType == OperandType.InlineMethod &&
                        (item = instruction.Operand as MethodDef) != null && proxies.Contains(item))
                        proxies.Remove(item);
                } catch { }

            foreach (var method in proxies)
                method.DeclaringType.Remove(method);
            InlinedMethods += count;
        }

        #region Private Methods

        private static bool IsInlineMethod(MethodDef method, out List<Instruction> instructions) {
            instructions = new List<Instruction>();
            if (!method.HasBody || !method.IsStatic)
                return false;
            var list = method.Body.Instructions;
            var index = list.Count - 1;
            if (index < 1 || list[index].OpCode != OpCodes.Ret)
                return false;
            var code = list[index - 1].OpCode.Code;
            int length;
            if (code != Code.Call && code != Code.Callvirt && code != Code.Newobj) {
                if (code != Code.Ldfld)
                    return false;
                instructions.Add(new Instruction(list[index - 1].OpCode, list[index - 1].Operand));
                length = (from i in list
                    where i.OpCode != OpCodes.Nop
                    select i).Count() - 2;
                return length == 1 && length == method.Parameters.Count - 1;
            }

            instructions.Add(new Instruction(list[index - 1].OpCode, list[index - 1].Operand));
            length = list.Count(i => i.OpCode != OpCodes.Nop) - 2;
            var count = list.Count - 2;
            if (length != method.Parameters.Count) {
                if (list[index - 2].IsLdcI4() && --length == method.Parameters.Count) {
                    count = list.Count - 3;
                    instructions.Insert(0, new Instruction(list[index - 2].OpCode, list[index - 2].Operand));
                } else
                    return false;
            }

            var num = 0;
            for (var j = 0; j < count; j++)
                if (list[j].OpCode != OpCodes.Nop) {
                    if (!list[j].IsLdarg())
                        return false;
                    if (list[j].GetParameterIndex() != num)
                        return false;
                    num++;
                }

            return length == num;
        }

        private static bool IsCompatibleType(IType origType, IType newType) =>
            new SigComparer(SigComparerOptions.IgnoreModifiers).Equals(origType, newType);

        #endregion

        #region Fields

        public static long InlinedMethods;

        #endregion
    }
}