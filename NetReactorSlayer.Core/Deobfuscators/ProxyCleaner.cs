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
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;

namespace NETReactorSlayer.Core.Deobfuscators
{
    class ProxyCleaner : IDeobfuscator
    {
        public void Execute()
        {
            long count = 0L;
            HashSet<MethodDef> proxies = new HashSet<MethodDef>();
            foreach (TypeDef type in DeobfuscatorContext.Module.GetTypes())
            {
                foreach (MethodDef method in (from x in type.Methods.ToArray<MethodDef>() where x.HasBody && x.Body.HasInstructions select x))
                {
                    try
                    {
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            MethodDef Method;
                            if (method.Body.Instructions[i].OpCode.Equals(OpCodes.Call) && (Method = (method.Body.Instructions[i].Operand as MethodDef)) != null && (IsProxy(Method, out OpCode opCode, out object obj, out int num3) || IsProxy2(Method, out opCode, out obj, out num3)))
                            {
                                count += 1L;
                                if (Method.DeclaringType == method.DeclaringType)
                                {
                                    method.Body.Instructions[i].OpCode = opCode;
                                    method.Body.Instructions[i].Operand = obj;
                                    proxies.Add(Method);

                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            foreach (TypeDef type in DeobfuscatorContext.Module.GetTypes())
            {
                foreach (MethodDef method in (from x in type.Methods.ToArray<MethodDef>() where x.HasBody && x.Body.HasInstructions select x))
                {
                    foreach (Instruction instruction in method.Body.Instructions)
                    {
                        try
                        {
                            MethodDef item;
                            if (instruction.OpCode.OperandType == OperandType.InlineMethod && (item = (instruction.Operand as MethodDef)) != null && proxies.Contains(item))
                            {
                                proxies.Remove(item);
                            }
                        }
                        catch { }
                    }
                }
            }
            foreach (MethodDef Method in proxies) Method.DeclaringType.Remove(Method);
            if (count > 0L) Logger.Done((int)count + " Proxied calls removed.");
            else Logger.Warn("Couldn't find any proxied call.");
        }

        bool IsProxy(MethodDef method, out OpCode code, out object operand, out int len)
        {
            code = null;
            operand = null;
            len = 0;
            if (!method.HasBody || !method.IsStatic) return false;
            IList<Instruction> instructions = method.Body.Instructions;
            int num = instructions.Count - 1;
            if (num < 1 || instructions[num].OpCode != OpCodes.Ret) return false;
            Code code2 = instructions[num - 1].OpCode.Code;
            if (code2 != Code.Call && code2 != Code.Callvirt && code2 != Code.Newobj) return false;
            code = instructions[num - 1].OpCode;
            operand = instructions[num - 1].Operand;
            len = (from i in instructions
                   where i.OpCode != OpCodes.Nop
                   select i).Count<Instruction>() - 2;
            if (len != method.Parameters.Count) return false;
            int num2 = 0;
            for (int j = 0; j < instructions.Count - 2; j++)
            {
                if (instructions[j].OpCode != OpCodes.Nop)
                {
                    if (!instructions[j].IsLdarg()) return false;
                    if (instructions[j].GetParameterIndex() != num2) return false;
                    num2++;
                }
            }
            return len == num2;
        }

        bool IsProxy2(MethodDef method, out OpCode code, out object operand, out int len)
        {
            code = null;
            operand = null;
            len = 0;
            if (!method.HasBody || !method.IsInternalCall) return false;
            IList<Instruction> instructions = method.Body.Instructions;
            int num = instructions.Count - 1;
            if (num < 1 || instructions[num].OpCode != OpCodes.Ret) return false;
            Code code2 = instructions[num - 1].OpCode.Code;
            if (code2 != Code.Ldfld) return false;
            code = instructions[num - 1].OpCode;
            operand = instructions[num - 1].Operand;
            len = (from i in instructions
                   where i.OpCode != OpCodes.Nop
                   select i).Count<Instruction>() - 2;
            return len == 1 && len == method.Parameters.Count - 1;
        }
    }
}
