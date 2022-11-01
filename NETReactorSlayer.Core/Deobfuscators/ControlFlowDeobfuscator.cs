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
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.Core.Helper;

namespace NETReactorSlayer.Core.Deobfuscators
{
    internal class ControlFlowDeobfuscator : IStage
    {
        public void Execute()
        {
            if (_fields.Count == 0)
                Initialize();
            long count = 0;
            foreach (var method in Context.Module.GetTypes().SelectMany(type =>
                         (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x)
                         .ToArray()))
            {
                if (SimpleDeobfuscator.Deobfuscate(method))
                    count++;
                count += Arithmetic(method);
                SimpleDeobfuscator.DeobfuscateBlocks(method);
            }

            if (count > 0) Logger.Done(count + " Equations resolved.");
            else
                Logger.Warn(
                    "Couldn't found any equations, looks like there's no control flow obfuscation applied to methods.");
        }

        #region Private Methods

        private void Initialize()
        {
            FindFieldsStatically();
            if (_fields.Count < 1)
                FindFieldsDynamically();
        }

        private void FindFieldsStatically()
        {
            TypeDef typeDef = null;
            foreach (var type in Context.Module.GetTypes().Where(
                         x => x.IsSealed &&
                              x.HasFields &&
                              x.Fields.Count(f =>
                                  f.FieldType.FullName == "System.Int32" && f.IsAssembly && !f.HasConstant) >= 100))
            {
                _fields.Clear();
                foreach (var method in type.Methods.Where(x =>
                             x.IsStatic && x.IsAssembly && x.HasBody && x.Body.HasInstructions))
                {
                    SimpleDeobfuscator.Deobfuscate(method);
                    for (var i = 0; i < method.Body.Instructions.Count; i++)
                        if ((method.Body.Instructions[i].IsLdcI4() &&
                             (i + 1 < method.Body.Instructions.Count ? method.Body.Instructions[i + 1] : null)
                             ?.OpCode ==
                             OpCodes.Stsfld) ||
                            (method.Body.Instructions[i].IsLdcI4() &&
                             (i + 1 < method.Body.Instructions.Count ? method.Body.Instructions[i + 1] : null)
                             ?.OpCode ==
                             OpCodes.Stfld &&
                             (i - 1 < method.Body.Instructions.Count ? method.Body.Instructions[i - 1] : null)
                             ?.OpCode ==
                             OpCodes.Ldsfld))
                        {
                            var key = (IField)(i + 1 < method.Body.Instructions.Count
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
                Cleaner.AddTypeToBeRemoved(typeDef);
        }

        private void FindFieldsDynamically()
        {
            TypeDef typeDef = null;
            foreach (var type in Context.Module.GetTypes().Where(
                         x => x.IsSealed &&
                              x.HasFields &&
                              x.Fields.Count(f =>
                                  f.FieldType.FullName == "System.Int32" && f.IsAssembly && !f.HasConstant) >= 100))
            {
                if ((Context.ObfuscatorInfo.NativeStub && Context.ObfuscatorInfo.NecroBit) 
                    || !Context.ObfuscatorInfo.UsesReflaction)
                {
                    Logger.Warn("Couldn't resolve arithmetic fields.");
                    return;
                }

                _fields.Clear();

                if (type.Fields.Where(x => x.FieldType.FullName == "System.Int32").All(x => x.IsStatic))
                    foreach (var field in type.Fields.Where(x => x.FieldType.FullName == "System.Int32"))
                        try
                        {
                            var obj = Context.Assembly.ManifestModule.ResolveField((int)field.MDToken.Raw)
                                .GetValue(null);
                            if (obj == null || !int.TryParse(obj.ToString(), out var value))
                                continue;
                            if (!_fields.ContainsKey(field))
                                _fields.Add(field, value);
                            else
                                _fields[field] = value;
                        }
                        catch
                        {
                        }
                else if (type.Fields.Where(x => x.FieldType.FullName == "System.Int32").All(x => !x.IsStatic))
                    foreach (var instances in type.Fields.Where(x => x.FieldType.ToTypeDefOrRef().Equals(type)))
                        try
                        {
                            var instance = Context.Assembly.ManifestModule.ResolveField((int)instances.MDToken.Raw)
                                .GetValue(null);
                            if (instance == null) continue;

                            var runtimeFields = instance.GetType().GetRuntimeFields().ToList();
                            if (runtimeFields.Count(x => x.FieldType == typeof(int)) < 100)
                                continue;

                            foreach (var runtimeField in runtimeFields.Where(x => x.FieldType == typeof(int)))
                            {
                                var field = type.Fields.FirstOrDefault(x =>
                                    x.MDToken.ToInt32().Equals(runtimeField.MetadataToken));
                                if (field == null) continue;
                                if (!_fields.ContainsKey(field))
                                    _fields.Add(field, (int)runtimeField.GetValue(instance));
                                else
                                    _fields[field] = (int)runtimeField.GetValue(instance);
                            }

                            break;
                        }
                        catch
                        {
                        }

                if (_fields.Count < 100) continue;
                typeDef = type;
                break;
            }

            if (_fields.All(x => x.Value == 0))
            {
                _fields.Clear();
                return;
            }

            if (typeDef != null)
                Cleaner.AddTypeToBeRemoved(typeDef);
        }

        private long Arithmetic(MethodDef method)
        {
            long count = 0;
            for (var i = 0; i < method.Body.Instructions.Count; i++)
                try
                {
                    if ((method.Body.Instructions[i].OpCode != OpCodes.Ldsfld &&
                         method.Body.Instructions[i].OpCode != OpCodes.Ldfld) ||
                        method.Body.Instructions[i].Operand is not IField ||
                        !_fields.TryGetValue((IField)method.Body.Instructions[i].Operand, out var value) ||
                        method.DeclaringType == _fields.First().Key.DeclaringType) continue;
                    if (method.Body.Instructions[i].OpCode == OpCodes.Ldfld &&
                        method.Body.Instructions[i - 1].OpCode == OpCodes.Ldsfld)
                        method.Body.Instructions[i - 1].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i] = Instruction.CreateLdcI4(value);
                    count++;
                }
                catch
                {
                }

            return count;
        }

        #endregion

        #region Fields

        private readonly Dictionary<IField, int> _fields = new Dictionary<IField, int>();

        #endregion
    }
}