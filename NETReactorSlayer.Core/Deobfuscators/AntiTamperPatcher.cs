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
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.Core.Deobfuscators;

internal class AntiTamperPatcher : IDeobfuscator
{
    public void Execute()
    {
        var isAntiTamperFound = false;
        var isAntiDebugFound = false;
        foreach (var type in DeobfuscatorContext.Module.GetTypes())
        foreach (var method in (from x in type.Methods
                     where x.HasBody && x.Body.HasInstructions && x.IsStatic
                     select x).ToArray())
        foreach (var instruction in (from x in method.Body.Instructions
                     where x.OpCode.Equals(OpCodes.Ldstr)
                     select x).ToArray())
        {
            if (instruction.Operand.ToString().Contains("Debugger Detected"))
            {
                isAntiDebugFound = true;
                Cleaner.MethodsToRemove.Add(method);
                var ins = Instruction.Create(OpCodes.Ret);
                var cli = new CilBody();
                cli.Instructions.Add(ins);
                method.Body = cli;
                Logger.Done("Anti debugger removed.");
            }

            if (instruction.Operand.ToString().Contains("is tampered"))
            {
                isAntiTamperFound = true;
                Cleaner.MethodsToRemove.Add(method);
                var ins = Instruction.Create(OpCodes.Ret);
                var cli = new CilBody();
                cli.Instructions.Add(ins);
                method.Body = cli;
                Logger.Done("Anti tamper removed.");
            }
        }

        if (!isAntiTamperFound)
            Logger.Warn("Couldn't find anti tamper method.");
        if (!isAntiDebugFound)
            Logger.Warn("Couldn't find anti debugger method.");
    }
}