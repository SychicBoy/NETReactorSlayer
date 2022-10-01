/*
    Copyright (C) 2011-2015 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.De4dot;

public static class MethodStack
{
    public static PushedArgs GetPushedArgInstructions(IList<Instruction> instructions, int index)
    {
        try
        {
            instructions[index].CalculateStackUsage(false, out _, out var pops);
            if (pops != -1)
                return GetPushedArgInstructions(instructions, index, pops);
        }
        catch (NullReferenceException)
        {
        }

        return new PushedArgs(0);
    }

    private static PushedArgs GetPushedArgInstructions(IList<Instruction> instructions, int index, int numArgs)
    {
        var pushedArgs = new PushedArgs(numArgs);
        if (!pushedArgs.CanAddMore) return pushedArgs;

        Dictionary<int, Branch> branches = null;
        var states = new Stack<State>();
        var state = new State(index, null, 0, 0, 1, new HashSet<int>());
        var isBacktrack = false;
        states.Push(state.Clone());
        while (true)
        {
            while (state.Index >= 0)
            {
                if (branches != null && branches.TryGetValue(state.Index, out var branch) &&
                    state.Visited.Add(state.Index))
                {
                    branch.Current = 0;
                    var brState = state.Clone();
                    brState.Branch = branch;
                    states.Push(brState);
                }

                if (!isBacktrack)
                    state.Index--;
                isBacktrack = false;
                var update = UpdateState(instructions, state, pushedArgs);
                if (update == Update.Finish)
                    return pushedArgs;
                if (update == Update.Fail)
                    break;
            }

            if (states.Count == 0)
                return pushedArgs;

            var prevValidArgs = state.ValidArgs;
            state = states.Pop();
            if (state.ValidArgs < prevValidArgs)
                for (var i = state.ValidArgs + 1; i <= prevValidArgs; i++)
                    pushedArgs.Pop();

            if (branches == null)
                branches = GetBranches(instructions);
            else
            {
                isBacktrack = true;
                state.Index = state.Branch.Variants[state.Branch.Current++];
                if (state.Branch.Current < state.Branch.Variants.Count)
                    states.Push(state.Clone());
                else
                    state.Branch = null;
            }
        }
    }

    private static Update UpdateState(IList<Instruction> instructions, State state, PushedArgs pushedArgs)
    {
        if (state.Index < 0 || state.Index >= instructions.Count)
            return Update.Fail;
        var instr = instructions[state.Index];
        if (!Instr.IsFallThrough(instr.OpCode))
            return Update.Fail;
        instr.CalculateStackUsage(false, out var pushes, out var pops);
        if (pops == -1)
            return Update.Fail;
        var isDup = instr.OpCode.Code == Code.Dup;
        if (isDup)
        {
            pushes = 1;
            pops = 0;
        }

        if (pushes > 1)
            return Update.Fail;

        if (state.SkipPushes > 0)
        {
            state.SkipPushes -= pushes;
            if (state.SkipPushes < 0)
                return Update.Fail;
            state.SkipPushes += pops;
        }
        else
        {
            if (pushes == 1)
            {
                if (isDup)
                    state.AddPushes++;
                else
                {
                    for (; state.AddPushes > 0; state.AddPushes--)
                    {
                        pushedArgs.Add(instr);
                        state.ValidArgs++;
                        if (!pushedArgs.CanAddMore)
                            return Update.Finish;
                    }

                    state.AddPushes = 1;
                }
            }

            state.SkipPushes += pops;
        }

        return Update.Ok;
    }

    private static Dictionary<int, Branch> GetBranches(IList<Instruction> instructions)
    {
        if (Equals(_cacheInstructions, instructions)) return _cacheBranches;
        _cacheInstructions = instructions;
        _cacheBranches = new Dictionary<int, Branch>();
        for (var b = 0; b < instructions.Count; b++)
        {
            var br = instructions[b];
            if (br.Operand is Instruction target)
            {
                var t = instructions.IndexOf(target);
                if (!_cacheBranches.TryGetValue(t, out var branch))
                {
                    branch = new Branch();
                    _cacheBranches.Add(t, branch);
                }

                branch.Variants.Add(b);
            }
        }

        return _cacheBranches;
    }

    public static TypeSig GetLoadedType(MethodDef method, IList<Instruction> instructions, int instrIndex,
        out bool wasNewobj) =>
        GetLoadedType(method, instructions, instrIndex, 0, out wasNewobj);

    public static TypeSig GetLoadedType(MethodDef method, IList<Instruction> instructions, int instrIndex,
        int argIndexFromEnd, out bool wasNewobj)
    {
        wasNewobj = false;
        var pushedArgs = GetPushedArgInstructions(instructions, instrIndex);
        var pushInstr = pushedArgs.GetEnd(argIndexFromEnd);
        if (pushInstr == null)
            return null;

        TypeSig type;
        Local local;
        var corLibTypes = method.DeclaringType.Module.CorLibTypes;
        switch (pushInstr.OpCode.Code)
        {
            case Code.Ldstr:
                type = corLibTypes.String;
                break;

            case Code.Conv_I:
            case Code.Conv_Ovf_I:
            case Code.Conv_Ovf_I_Un:
                type = corLibTypes.IntPtr;
                break;

            case Code.Conv_U:
            case Code.Conv_Ovf_U:
            case Code.Conv_Ovf_U_Un:
                type = corLibTypes.UIntPtr;
                break;

            case Code.Conv_I8:
            case Code.Conv_Ovf_I8:
            case Code.Conv_Ovf_I8_Un:
                type = corLibTypes.Int64;
                break;

            case Code.Conv_U8:
            case Code.Conv_Ovf_U8:
            case Code.Conv_Ovf_U8_Un:
                type = corLibTypes.UInt64;
                break;

            case Code.Conv_R8:
            case Code.Ldc_R8:
            case Code.Ldelem_R8:
            case Code.Ldind_R8:
                type = corLibTypes.Double;
                break;

            case Code.Call:
            case Code.Calli:
            case Code.Callvirt:
                var calledMethod = pushInstr.Operand as IMethod;
                if (calledMethod == null)
                    return null;
                type = calledMethod.MethodSig.GetRetType();
                break;

            case Code.Newarr:
                var type2 = pushInstr.Operand as ITypeDefOrRef;
                if (type2 == null)
                    return null;
                type = new SZArraySig(type2.ToTypeSig());
                wasNewobj = true;
                break;

            case Code.Newobj:
                var ctor = pushInstr.Operand as IMethod;
                if (ctor == null)
                    return null;
                type = ctor.DeclaringType.ToTypeSig();
                wasNewobj = true;
                break;

            case Code.Castclass:
            case Code.Isinst:
            case Code.Unbox_Any:
            case Code.Ldelem:
            case Code.Ldobj:
                type = (pushInstr.Operand as ITypeDefOrRef).ToTypeSig();
                break;

            case Code.Ldarg:
            case Code.Ldarg_S:
            case Code.Ldarg_0:
            case Code.Ldarg_1:
            case Code.Ldarg_2:
            case Code.Ldarg_3:
                type = pushInstr.GetArgumentType(method.MethodSig, method.DeclaringType);
                break;

            case Code.Ldloc:
            case Code.Ldloc_S:
            case Code.Ldloc_0:
            case Code.Ldloc_1:
            case Code.Ldloc_2:
            case Code.Ldloc_3:
                local = pushInstr.GetLocal(method.Body.Variables);
                if (local == null)
                    return null;
                type = local.Type.RemovePinned();
                break;

            case Code.Ldloca:
            case Code.Ldloca_S:
                local = pushInstr.Operand as Local;
                if (local == null)
                    return null;
                type = CreateByRefType(local.Type.RemovePinned());
                break;

            case Code.Ldarga:
            case Code.Ldarga_S:
                type = CreateByRefType(pushInstr.GetArgumentType(method.MethodSig, method.DeclaringType));
                break;

            case Code.Ldfld:
            case Code.Ldsfld:
                var field = pushInstr.Operand as IField;
                if (field == null || field.FieldSig == null)
                    return null;
                type = field.FieldSig.GetFieldType();
                break;

            case Code.Ldflda:
            case Code.Ldsflda:
                var field2 = pushInstr.Operand as IField;
                if (field2 == null || field2.FieldSig == null)
                    return null;
                type = CreateByRefType(field2.FieldSig.GetFieldType());
                break;

            case Code.Ldelema:
            case Code.Unbox:
                type = CreateByRefType(pushInstr.Operand as ITypeDefOrRef);
                break;

            default:
                return null;
        }

        return type;
    }

    private static ByRefSig CreateByRefType(ITypeDefOrRef elementType)
    {
        if (elementType == null)
            return null;
        return new ByRefSig(elementType.ToTypeSig());
    }

    private static ByRefSig CreateByRefType(TypeSig elementType)
    {
        if (elementType == null)
            return null;
        return new ByRefSig(elementType);
    }

    private static Dictionary<int, Branch> _cacheBranches;

    private static IList<Instruction> _cacheInstructions;

    private enum Update
    {
        Ok,
        Fail,
        Finish
    }

    private class Branch
    {
        public Branch() => Variants = new List<int>();

        public int Current;

        public List<int> Variants { get; }
    }

    private class State
    {
        public State(int index, Branch branch, int validArgs, int skipPushes, int addPushes, HashSet<int> visited)
        {
            Index = index;
            Branch = branch;
            ValidArgs = validArgs;
            SkipPushes = skipPushes;
            AddPushes = addPushes;
            Visited = visited;
        }

        public State Clone() =>
            new State(Index, Branch, ValidArgs, SkipPushes, AddPushes, new HashSet<int>(Visited));

        public readonly HashSet<int> Visited;

        public int AddPushes;
        public Branch Branch;
        public int Index;
        public int SkipPushes;
        public int ValidArgs;
    }
}