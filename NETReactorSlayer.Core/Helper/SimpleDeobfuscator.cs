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

using System;
using System.Collections.Generic;
using System.Linq;
using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Deobfuscators;

namespace NETReactorSlayer.Core.Helper;

internal class SimpleDeobfuscator
{
    public static bool Deobfuscate(MethodDef method)
    {
        const SimpleDeobfuscatorFlags flags = 0;
        if (method == null || Check(method, SimpleDeobFlags.HasDeobfuscated)) return false;
        Deobfuscate(method, delegate(Blocks blocks)
        {
            const bool disableNewCfCode = (flags & SimpleDeobfuscatorFlags.DisableConstantsFolderExtraInstrs) > 0U;
            var cflowDeobfuscator =
                new BlocksCflowDeobfuscator(new List<IBlocksDeobfuscator> { new MyMethodCallInliner(false) },
                    disableNewCfCode);
            cflowDeobfuscator.Initialize(blocks);
            cflowDeobfuscator.Deobfuscate();
        });
        return true;
    }

    public static void DeobfuscateBlocks(MethodDef method)
    {
        try
        {
            _blocksCflowDeob = new BlocksCflowDeobfuscator();
            var blocks = new Blocks(method);
            blocks.MethodBlocks.GetAllBlocks();
            blocks.RemoveDeadBlocks();
            blocks.RepartitionBlocks();
            blocks.UpdateBlocks();
            blocks.Method.Body.SimplifyBranches();
            blocks.Method.Body.OptimizeBranches();
            _blocksCflowDeob.Initialize(blocks);
            _blocksCflowDeob.Deobfuscate();
            blocks.RepartitionBlocks();
            blocks.GetCode(out var instructions, out var exceptionHandlers);
            DotNetUtils.RestoreBody(method, instructions, exceptionHandlers);
        }
        catch
        {
        }
    }

    #region Nested Types

    public class MyMethodCallInliner : MethodCallInliner
    {
        public MyMethodCallInliner(bool inlineInstanceMethods) : base(inlineInstanceMethods)
        {
        }

        protected override void OnInlinedMethod(MethodDef methodToInline, bool inlinedMethod)
        {
            if (!inlinedMethod || methodToInline.IsPublic)
                return;
            Cleaner.AddMethodToBeRemoved(methodToInline);
            MethodInliner.InlinedMethods++;
        }
    }

    #endregion

    #region Private Methods

    private static void Deobfuscate(MethodDef method, Action<Blocks> handler)
    {
        if (method is not { HasBody: true } || !method.Body.HasInstructions) return;
        try
        {
            if (method.Body.Instructions.Any(instr => instr.OpCode.Equals(OpCodes.Switch)))
                DeobfuscateEquations(method);
            var blocks = new Blocks(method);
            handler(blocks);
            blocks.GetCode(out var allInstructions, out var allExceptionHandlers);
            DotNetUtils.RestoreBody(method, allInstructions, allExceptionHandlers);
            DeobfuscateBlocks(method);
        }
        catch
        {
            Logger.Warn("Couldn't deobfuscate " + method.FullName);
        }
    }

    private static void DeobfuscateEquations(MethodDef method)
    {
        for (var i = 0; i < method.Body.Instructions.Count; i++)
            if (method.Body.Instructions[i].IsBrtrue() &&
                method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Pop) &&
                method.Body.Instructions[i - 1].OpCode.Equals(OpCodes.Call))
            {
                if (method.Body.Instructions[i - 1].Operand is not MethodDef methodDef) continue;
                var methodDefInstr = methodDef.Body.Instructions;
                if (methodDef.ReturnType.FullName == "System.Boolean")
                {
                    if (methodDefInstr[methodDefInstr.Count - 2].OpCode.Equals(OpCodes.Ldc_I4_0))
                    {
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    }
                    else
                    {
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                    }
                }
                else
                {
                    method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                }
            }
            else
            {
                if (!method.Body.Instructions[i].IsBrfalse() ||
                    !method.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Pop) ||
                    !method.Body.Instructions[i - 1].OpCode.Equals(OpCodes.Call)) continue;
                if (method.Body.Instructions[i - 1].Operand is MethodDef methodDef2)
                {
                    var methodDefInstr2 = methodDef2.Body.Instructions;
                    if (methodDef2.ReturnType.FullName == "System.Boolean")
                    {
                        if (methodDefInstr2[methodDefInstr2.Count - 2].OpCode.Equals(OpCodes.Ldc_I4_0))
                        {
                            method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                        }
                        else
                        {
                            method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                            method.Body.Instructions[i].OpCode = OpCodes.Nop;
                        }
                    }
                    else
                    {
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                        method.Body.Instructions[i].OpCode = OpCodes.Br_S;
                    }
                }
            }
    }

    private static bool Check(MethodDef method, SimpleDeobFlags flags)
    {
        if (method == null) return false;
        DeobfuscatorFlags.TryGetValue(method, out var oldFlags);
        DeobfuscatorFlags[method] = oldFlags | flags;
        return (oldFlags & flags) == flags;
    }

    #endregion

    #region Fields

    private static BlocksCflowDeobfuscator _blocksCflowDeob = new();
    private static readonly Dictionary<MethodDef, SimpleDeobFlags> DeobfuscatorFlags = new();

    [Flags]
    private enum SimpleDeobFlags
    {
        HasDeobfuscated = 1
    }

    [Flags]
    public enum SimpleDeobfuscatorFlags : uint
    {
        DisableConstantsFolderExtraInstrs = 2U
    }

    #endregion
}