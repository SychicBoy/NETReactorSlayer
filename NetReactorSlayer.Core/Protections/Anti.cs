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
                Logger.Warn("Couldn't find debugger method.");
        }
    }
}
