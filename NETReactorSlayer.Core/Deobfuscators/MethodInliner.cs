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

namespace NETReactorSlayer.Core.Deobfuscators;

internal class MethodInliner : IStage
{
    public void Execute()
    {
        var count = 0L;
        var proxies = new HashSet<MethodDef>();
        foreach (var type in Context.Module.GetTypes())
        foreach (var method in from x in type.Methods.ToList() where x.HasBody && x.Body.HasInstructions select x)
            try
            {
                foreach (var instr
                         in method.Body.Instructions)
                {
                    MethodDef Method;
                    if (instr
                            .OpCode.Equals(OpCodes.Call) &&
                        (Method = instr
                            .Operand as MethodDef) != null &&
                        (IsInline1(Method, out var opCode, out var obj) ||
                         IsInline2(Method, out opCode, out obj)))
                    {
                        count += 1L;
                        if (Method.DeclaringType == method.DeclaringType)
                        {
                            instr
                                .OpCode = opCode;
                            instr
                                .Operand = obj;
                            proxies.Add(Method);
                        }
                    }
                }
            } catch { }

        foreach (var type in Context.Module.GetTypes())
        foreach (var method in from x in type.Methods.ToArray() where x.HasBody && x.Body.HasInstructions select x)
        foreach (var instruction in method.Body.Instructions)
            try
            {
                MethodDef item;
                if (instruction.OpCode.OperandType == OperandType.InlineMethod &&
                    (item = instruction.Operand as MethodDef) != null &&
                    proxies.Contains(item)) proxies.Remove(item);
            } catch { }

        foreach (var Method in proxies) Method.DeclaringType.Remove(Method);
        if (count > 0L) Logger.Done((int) count + " Methods inlined.");
        else Logger.Warn("Couldn't find any outline method.");
    }

    private bool IsInline1(MethodDef method, out OpCode code, out object operand)
    {
        code = null;
        operand = null;
        if (!method.HasBody || !method.IsStatic) return false;
        var instructions = method.Body.Instructions;
        var num = instructions.Count - 1;
        if (num < 1 || instructions[num].OpCode != OpCodes.Ret) return false;
        var code2 = instructions[num - 1].OpCode.Code;
        if (code2 != Code.Call && code2 != Code.Callvirt && code2 != Code.Newobj) return false;
        code = instructions[num - 1].OpCode;
        operand = instructions[num - 1].Operand;
        var len = (from i in instructions
            where i.OpCode != OpCodes.Nop
            select i).Count() - 2;
        if (len != method.Parameters.Count) return false;
        var num2 = 0;
        for (var j = 0; j < instructions.Count - 2; j++)
            if (instructions[j].OpCode != OpCodes.Nop)
            {
                if (!instructions[j].IsLdarg()) return false;
                if (instructions[j].GetParameterIndex() != num2) return false;
                num2++;
            }

        return len == num2;
    }

    private bool IsInline2(MethodDef method, out OpCode code, out object operand)
    {
        code = null;
        operand = null;
        if (!method.HasBody || !method.IsInternalCall) return false;
        var instructions = method.Body.Instructions;
        var num = instructions.Count - 1;
        if (num < 1 || instructions[num].OpCode != OpCodes.Ret) return false;
        var code2 = instructions[num - 1].OpCode.Code;
        if (code2 != Code.Ldfld) return false;
        code = instructions[num - 1].OpCode;
        operand = instructions[num - 1].Operand;
        var len = (from i in instructions
            where i.OpCode != OpCodes.Nop
            select i).Count() - 2;
        return len == 1 && len == method.Parameters.Count - 1;
    }
}