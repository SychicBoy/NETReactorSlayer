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
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class NopRemover
{
    public static void RemoveAll()
    {
        foreach (var type in DeobfuscatorContext.Module.GetTypes())
        foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
        {
            var length = method.Body.Instructions.Count;
            var index = 0;
            for (; index < length; index++)
                if (method.Body.Instructions[index].OpCode.Equals(OpCodes.Nop) &&
                    !IsNopBranchTarget(method, method.Body.Instructions[index]) &&
                    !IsNopSwitchTarget(method, method.Body.Instructions[index]) &&
                    !IsNopExceptionHandlerTarget(method, method.Body.Instructions[index]))
                {
                    method.Body.Instructions.RemoveAt(index);
                    index--;
                    length = method.Body.Instructions.Count;
                }
        }
    }

    private static bool IsNopBranchTarget(MethodDef method, Instruction nopInstr)
    {
        var instrs = method.Body.Instructions;
        foreach (var instr in instrs)
            if (instr.OpCode.OperandType == OperandType.InlineBrTarget ||
                instr.OpCode.OperandType == OperandType.ShortInlineBrTarget && instr.Operand != null)
            {
                var instruction2 = (Instruction) instr.Operand;
                if (instruction2 == nopInstr)
                    return true;
            }

        return false;
    }

    private static bool IsNopSwitchTarget(MethodDef method, Instruction nopInstr)
    {
        var instrs = method.Body.Instructions;
        return (from instr in instrs
            where instr.OpCode.OperandType == OperandType.InlineSwitch && instr.Operand != null
            select (Instruction[]) instr.Operand).Any(source => source.Contains(nopInstr));
    }

    private static bool IsNopExceptionHandlerTarget(MethodDef method, Instruction nopInstr)
    {
        if (!method.Body.HasExceptionHandlers)
            return false;
        var exceptionHandlers = method.Body.ExceptionHandlers;
        foreach (var exceptionHandler in exceptionHandlers)
            if (exceptionHandler.FilterStart == nopInstr ||
                exceptionHandler.HandlerEnd == nopInstr ||
                exceptionHandler.HandlerStart == nopInstr ||
                exceptionHandler.TryEnd == nopInstr ||
                exceptionHandler.TryStart == nopInstr)
                return true;
        return false;
    }
}