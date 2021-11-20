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
using NETReactorSlayer.Core.Utils;
using System.Linq;

namespace NETReactorSlayer.Core.Protections
{
    class Anti
    {
        public static void Execute(bool antiDebug = true, bool antiTamper = true)
        {
            bool isAntiTamperFound = false;
            bool isAntiDebugFound = false;
            foreach (TypeDef type in Context.Module.GetTypes())
            {
                foreach (MethodDef method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions && x.IsStatic select x).ToArray<MethodDef>())
                {
                    foreach (Instruction instruction in (from x in method.Body.Instructions where x.OpCode.Equals(OpCodes.Ldstr) select x).ToArray<Instruction>())
                    {
                        if (instruction.Operand.ToString().Contains("Debugger Detected") && antiDebug)
                        {
                            isAntiDebugFound = true;
                            Remover.MethodsToPatch.Add(method);
                            Instruction ins = Instruction.Create(OpCodes.Ret);
                            CilBody cli = new CilBody();
                            cli.Instructions.Add(ins);
                            method.Body = cli;
                            Logger.Info("Anti debugger removed.");
                        }
                        if (instruction.Operand.ToString().Contains("is tampered") && antiTamper)
                        {
                            isAntiTamperFound = true;
                            Remover.MethodsToPatch.Add(method);
                            Instruction ins = Instruction.Create(OpCodes.Ret);
                            CilBody cli = new CilBody();
                            cli.Instructions.Add(ins);
                            method.Body = cli;
                            Logger.Info("Anti tamper removed.");
                        }
                    }
                }
            }
            if (!isAntiTamperFound)
                Logger.Warn("Couldn't find anti tamper method.");
            if (!isAntiDebugFound)
                Logger.Warn("Couldn't find anti debugger method.");
        }
    }
}
