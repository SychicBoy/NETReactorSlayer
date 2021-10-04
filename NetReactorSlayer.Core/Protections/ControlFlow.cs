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
        public static bool ContainsSwitch(MethodDef method)
        {
            return method.Body.Instructions.Any((Instruction instr) => instr.OpCode.Equals(OpCodes.Switch));
        }
        public static void Execute()
        {
            long count = 0L;
            BlocksCflowDeobfuscator blocksCflowDeobfuscator = new BlocksCflowDeobfuscator();
            foreach (TypeDef type in Context.Module.GetTypes())
            {
                foreach (MethodDef method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x).ToArray<MethodDef>())
                {
                    if (Variables.options["arithmetic"])
                        count += Arithmetic(method, type);
                    if (!Variables.options["deob"]) continue;
                    try
                    {
                        blocksCflowDeobfuscator = new BlocksCflowDeobfuscator();
                        Blocks blocks = new Blocks(method);
                        List<Block> allBlocks = blocks.MethodBlocks.GetAllBlocks();
                        blocks.RemoveDeadBlocks();
                        blocks.RepartitionBlocks();
                        blocks.UpdateBlocks();
                        blocks.Method.Body.SimplifyBranches();
                        blocks.Method.Body.OptimizeBranches();
                        blocksCflowDeobfuscator.Initialize(blocks);
                        blocksCflowDeobfuscator.Deobfuscate();
                        blocks.RepartitionBlocks();
                        blocks.GetCode(out IList<Instruction> instructions, out IList<ExceptionHandler> exceptionHandlers);
                        DotNetUtils.RestoreBody(method, instructions, exceptionHandlers);
                    }
                    catch
                    {
                    }
                }
            }
            if (count > 0L) Logger.Info((int)count + " Arithmetic equations resolved.");
            else Logger.Warn("Couldn't found any arithmetic equation in methods.");
        }

        static long Arithmetic(MethodDef method, TypeDef type)
        {
            long count = 0L;
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                try
                {
                    if (method.Body.Instructions[i].OpCode.Equals(OpCodes.Ldsfld) && method.Body.Instructions[i].Operand.ToString().Contains("System.Int32") && method.Body.Instructions[i + 1].IsConditionalBranch() && method.Body.Instructions[i + 2].OpCode.Equals(OpCodes.Pop))
                    {
                        if (!(method.Body.Instructions[i].Operand is FieldDef field) || field.FieldType.FullName != "System.Int32" || !field.IsAssembly || !field.IsStatic || field.DeclaringType == null || field.DeclaringType == type || field.DeclaringType.Namespace.String != "") continue;
                        var obj = Context.Assembly.ManifestModule.ResolveField((int)field.MDToken.Raw).GetValue(null);
                        if (method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Brtrue))
                        {
                            if (int.Parse(obj.ToString()) == 0)
                            {
                                method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                            }
                            else
                            {
                                method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i + 1].OpCode = OpCodes.Br_S;
                            }
                            count += 1L;
                        }
                        else if (method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Brfalse))
                        {
                            if (int.Parse(obj.ToString()) == 0)
                            {
                                method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i + 1].OpCode = OpCodes.Br_S;
                            }
                            else
                            {
                                method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                            }
                            count += 1L;
                        }
                    }
                    else
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
                    }
                }
                catch { }
            }
            return count;
        }
    }
}
