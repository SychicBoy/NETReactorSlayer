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

internal class ControlFlowDeobfuscator : IDeobfuscator
{
    private readonly Dictionary<IField, int> _fields = new();

    public void Execute()
    {
        if (_fields.Count == 0)
            Initialize();
        var count = 0L;
        foreach (var type in DeobfuscatorContext.Module.GetTypes())
        foreach (var method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x)
                 .ToArray())
        {
            if (SimpleDeobfuscator.Deobfuscate(method))
                count += 1L;
            count += Arithmetic(method);
            SimpleDeobfuscator.DeobfuscateBlocks(method);
        }

        if (count > 0L) Logger.Done((int) count + " Equations resolved.");
        else
            Logger.Warn(
                "Couldn't found any equations, looks like there's no control flow obfuscation applied to methods.");
    }

    private void Initialize()
    {
        TypeDef typeDef = null;
        foreach (var type in DeobfuscatorContext.Module.GetTypes().Where(
                     x => x.IsSealed &&
                          x.HasFields &&
                          x.Fields.Count >= 100))
        {
            _fields.Clear();
            foreach (var method in type.Methods.Where(x =>
                         x.IsStatic && x.IsAssembly && x.HasBody && x.Body.HasInstructions))
            {
                SimpleDeobfuscator.Deobfuscate(method);
                for (var i = 0; i < method.Body.Instructions.Count; i += 2)
                    if (method.Body.Instructions[i].IsLdcI4() &&
                        (i + 1 < method.Body.Instructions.Count ? method.Body.Instructions[i + 1] : null)?.OpCode ==
                        OpCodes.Stsfld)
                    {
                        var key = (IField) (i + 1 < method.Body.Instructions.Count
                            ? method.Body.Instructions[i + 1]
                            : null)?.Operand;
                        var value = method.Body.Instructions[i].GetLdcI4Value();
                        if (key != null && !_fields.ContainsKey(key))
                            _fields.Add(key, value);
                        else if (key != null) _fields[key] = value;
                    }

                if (_fields.Count != 0)
                    typeDef = type;
                goto Continue;
            }
        }

        Continue:
        if (typeDef != null)
            Cleaner.TypesToRemove.Add(typeDef);
    }

    private long Arithmetic(MethodDef method)
    {
        var count = 0L;
        for (var i = 0; i < method.Body.Instructions.Count; i++)
            try
            {
                if (method.Body.Instructions[i].OpCode == OpCodes.Ldsfld &&
                    method.Body.Instructions[i].Operand is IField &&
                    method.Body.Instructions[i + 1].IsConditionalBranch() &&
                    (method.Body.Instructions[i + 2].OpCode == OpCodes.Pop ||
                     method.Body.Instructions[i + 2].IsBr()) &&
                    _fields.TryGetValue((IField) method.Body.Instructions[i].Operand, out var value))
                {
                    method.Body.Instructions[i] = Instruction.CreateLdcI4(value);
                    count += 1L;
                }
            } catch { }

        return count;
    }
}