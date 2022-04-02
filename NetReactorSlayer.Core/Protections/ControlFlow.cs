/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NetReactorSlayer.
    NetReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NetReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NetReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/
using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace NETReactorSlayer.Core.Protections
{
    class ControlFlow
    {
        private static Dictionary<int, int> Fields = new Dictionary<int, int>();
        private static BlocksCflowDeobfuscator BlocksCflowDeob = new BlocksCflowDeobfuscator();

        static ControlFlow()
        {
            if (Fields.Count == 0)
                Initialize();
        }

        private static void Initialize()
        {
            foreach (TypeDef type in Context.Module.GetTypes().Where(x => x.Fields.Count > 100))
            {
                foreach (MethodDef method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions && x.Body.Instructions.Any(z => z.OpCode.Equals(OpCodes.Stsfld))))
                {
                    DeobfuscateEquations(method);
                    DeobfuscateBlocks(method);
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i].IsLdcI4() && (method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Stsfld) || method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Stfld)))
                        {
                            var key = (method.Body.Instructions[i + 1].Operand as IField).MDToken.ToInt32();
                            var value = method.Body.Instructions[i].GetLdcI4Value();
                            if (!Fields.ContainsKey(key))
                                Fields.Add(key, value);
                            else
                                Fields[key] = value;
                        }
                    }
                }
            }
        }

        public static void Execute()
        {
            long count = 0L;
            foreach (TypeDef type in Context.Module.GetTypes())
            {
                foreach (MethodDef method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x).ToArray<MethodDef>())
                {
                    if (Variables.options["cflow"])
                    {
                        count += DeobfuscateEquations(method);
                        DeobfuscateBlocks(method);
                    }
                }
            }
            if (count > 0L) Logger.Info((int)count + " Arithmetic equations resolved.");
            else Logger.Warn("Couldn't found any arithmetic equation in methods.");
        }

        public static bool IsContainsSwitch(MethodDef method)
        {
            return method.Body.Instructions.Any((Instruction instr) => instr.OpCode.Equals(OpCodes.Switch));
        }

        private static void DeobfuscateBlocks(MethodDef method)
        {
            try
            {
                BlocksCflowDeob = new BlocksCflowDeobfuscator();
                Blocks blocks = new Blocks(method);
                List<Block> allBlocks = blocks.MethodBlocks.GetAllBlocks();
                blocks.RemoveDeadBlocks();
                blocks.RepartitionBlocks();
                blocks.UpdateBlocks();
                blocks.Method.Body.SimplifyBranches();
                blocks.Method.Body.OptimizeBranches();
                BlocksCflowDeob.Initialize(blocks);
                BlocksCflowDeob.Deobfuscate();
                blocks.RepartitionBlocks();
                blocks.GetCode(out IList<Instruction> instructions, out IList<ExceptionHandler> exceptionHandlers);
                DotNetUtils.RestoreBody(method, instructions, exceptionHandlers);
            }
            catch
            {
            }
        }

        private static long DeobfuscateEquations(MethodDef method)
        {
            long count = 0L;
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                try
                {
                    if (method.Body.Instructions[i].OpCode.Equals(OpCodes.Call) && method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Brtrue) && method.Body.Instructions[i + 2].OpCode.Equals(OpCodes.Pop))
                    {
                        if (method.Body.Instructions[i].Operand.ToString().Contains("System.Boolean"))
                        {
                            count += 1L;
                            method.Body.Instructions[i].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i + 1].OpCode = OpCodes.Br_S;
                        }
                        else
                        {
                            count += 1L;
                            method.Body.Instructions[i].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                        }
                    }
                    else if (method.Body.Instructions[i].OpCode.Equals(OpCodes.Call) && method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Brfalse) && method.Body.Instructions[i + 2].OpCode.Equals(OpCodes.Pop))
                    {
                        if (method.Body.Instructions[i].Operand.ToString().Contains("System.Boolean"))
                        {
                            count += 1L;
                            method.Body.Instructions[i].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                        }
                        else
                        {
                            count += 1L;
                            method.Body.Instructions[i].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i + 1].OpCode = OpCodes.Br_S;
                        }
                    }
                    else if (method.Body.Instructions[i].Operand is IField field && field.ResolveFieldDef().FieldType.FullName.Equals("System.Int32"))
                    {
                        if (!Fields.TryGetValue(field.MDToken.ToInt32(), out int value) || field.DeclaringType.Equals(method.DeclaringType)) continue;
                        if (method.Body.Instructions[i - 1].Operand is IField parent && field.DeclaringType.Equals(parent.DeclaringType))
                            method.Body.Instructions[i - 1] = Instruction.Create(OpCodes.Nop);
                        method.Body.Instructions[i] = Instruction.CreateLdcI4(value);
                        count += 1L;
                    }
                }
                catch { }
            }
            return count;
        }
    }
}
