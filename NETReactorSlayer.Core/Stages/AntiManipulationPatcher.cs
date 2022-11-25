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
using NETReactorSlayer.Core.Abstractions;

namespace NETReactorSlayer.Core.Stages
{
    internal class AntiManipulationPatcher : IStage
    {
        public void Run(IContext context)
        {
            Context = context;
            bool antiTamper = false,
                antiDebugger = false;
            foreach (var method in Context.Module.GetTypes()
                         .SelectMany(type => type.Methods.Where(x => x.HasBody && x.Body.HasInstructions)))
            {
                if (RemoveAntiTamper(method))
                    antiTamper = true;
                else if (RemoveAntiDebugger(method))
                    antiDebugger = true;
                else
                    continue;
                Cleaner.AddCallToBeRemoved(method);
            }

            if (!antiTamper)
                Context.Logger.Warn("Couldn't find anti tamper method.");
            if (!antiDebugger)
                Context.Logger.Warn("Couldn't find anti debugger method.");
        }

        private bool RemoveAntiTamper(MethodDef method)
        {
            if (!method.IsStatic)
                return false;
            if (!DotNetUtils.GetCodeStrings(method).Any(x => x.Contains("is tampered")))
                return false;

            var ins = Instruction.Create(OpCodes.Ret);
            var cli = new CilBody();
            cli.Instructions.Add(ins);
            method.Body = cli;
            Context.Logger.Info("Anti tamper removed.");
            return true;
        }

        private bool RemoveAntiDebugger(MethodDef method)
        {
            if (!method.IsStatic)
                return false;
            if (!DotNetUtils.GetCodeStrings(method).Any(x => x.Contains("Debugger Detected")))
                return false;

            var ins = Instruction.Create(OpCodes.Ret);
            var cli = new CilBody();
            cli.Instructions.Add(ins);
            method.Body = cli;
            Context.Logger.Info("Anti debugger removed.");
            return true;
        }


        private IContext Context { get; set; }
    }
}