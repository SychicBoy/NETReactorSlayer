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
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NETReactorSlayer.De4dot {
    public abstract class TypesRestorerBase {
        public void Deobfuscate() {
            _allMethods = new List<MethodDef>();

            AddAllMethods();
            AddAllFields();
            DeobfuscateLoop();
        }

        #region Protected Methods

        protected TypesRestorerBase(ModuleDef module) => _module = module;

        protected virtual bool IsValidType(IGenericParameterProvider provider, TypeSig type) {
            if (type == null)
                return false;
            if (type.ElementType == ElementType.Void)
                return false;

            while (type != null) {
                switch (type.ElementType) {
                    case ElementType.GenericInst:
                        if (((GenericInstSig)type).GenericArguments.Any(ga => !IsValidType(provider, ga)))
                            return false;
                        break;

                    case ElementType.SZArray:
                    case ElementType.Array:
                    case ElementType.Ptr:
                    case ElementType.Class:
                    case ElementType.ValueType:
                    case ElementType.Void:
                    case ElementType.Boolean:
                    case ElementType.Char:
                    case ElementType.I1:
                    case ElementType.U1:
                    case ElementType.I2:
                    case ElementType.U2:
                    case ElementType.I4:
                    case ElementType.U4:
                    case ElementType.I8:
                    case ElementType.U8:
                    case ElementType.R4:
                    case ElementType.R8:
                    case ElementType.TypedByRef:
                    case ElementType.I:
                    case ElementType.U:
                    case ElementType.String:
                    case ElementType.Object:
                        break;

                    case ElementType.Var:
                    case ElementType.MVar:
                        return false;

                    case ElementType.ByRef:
                    case ElementType.FnPtr:
                    case ElementType.CModOpt:
                    case ElementType.CModReqd:
                    case ElementType.Pinned:
                    case ElementType.Sentinel:
                    case ElementType.ValueArray:
                    case ElementType.R:
                    case ElementType.End:
                    case ElementType.Internal:
                    case ElementType.Module:
                    default:
                        return false;
                }

                if (type.Next == null)
                    break;
                type = type.Next;
            }

            return true;
        }

        protected abstract bool IsUnknownType(object o);

        #endregion

        #region Private Methods

        private UpdatedMethod GetUpdatedMethod(IMethod method) {
            var token = method.MDToken.ToInt32();
            if (_updatedMethods.TryGetValue(token, out var updatedMethod))
                return updatedMethod;
            return _updatedMethods[token] = new UpdatedMethod(method);
        }

        private void AddAllMethods() {
            foreach (var type in _module.GetTypes())
                AddMethods(type.Methods);
        }

        private void AddAllFields() {
            foreach (var field in _module.GetTypes().SelectMany(type => type.Fields.Where(IsUnknownType)))
                _fieldWrites[field] = new TypeInfo<FieldDef>(field);
        }

        private void DeobfuscateLoop() {
            for (var i = 0; i < 10; i++) {
                var modified = false;
                modified |= DeobfuscateFields();
                modified |= DeobfuscateMethods();
                if (!modified)
                    break;
            }
        }

        private bool DeobfuscateMethods() {
            var modified = false;
            var methods = new List<MethodDef>();

            foreach (var method in _allMethods) {
                _methodReturnInfo = new TypeInfo<Parameter>(method.Parameters.ReturnParameter);
                DeobfuscateMethod(method);
                if (!_methodReturnInfo.CanUpdateType(_module) &&
                    _argInfos.Values.Count(info => info.CanUpdateType(_module)) <= 0)
                    continue;
                methods.Add(method);
                modified = true;
            }

            foreach (var instr in _allMethods.Where(x => x.HasBody && x.Body.HasInstructions)
                         .SelectMany(method => method.Body.Instructions.Where(x => x.Operand != null)))
                try {
                    if (instr.Operand is not IMethod calledMethod)
                        continue;
                    var methodDef = methods
                        .FirstOrDefault(x => MethodEqualityComparer.CompareDeclaringTypes
                            .Equals(calledMethod, x));
                    if (methodDef == null)
                        continue;
                    instr.Operand = methodDef;
                } catch { }

            foreach (var method in methods) {
                _methodReturnInfo = new TypeInfo<Parameter>(method.Parameters.ReturnParameter);
                DeobfuscateMethod(method);
                if (_methodReturnInfo.UpdateNewType(_module))
                    method.MethodSig.RetType = _methodReturnInfo.NewType;

                foreach (var info in _argInfos.Values.Where(x => x.UpdateNewType(_module))) {
                    GetUpdatedMethod(method).NewArgTypes[info.Arg.Index] = info.NewType;
                    info.Arg.Type = info.NewType;
                }
            }

            return modified;
        }

        private bool DeobfuscateFields() {
            foreach (var info in _fieldWrites.Values)
                info.Clear();

            foreach (var method in _allMethods) {
                if (method.Body == null)
                    continue;
                var instructions = method.Body.Instructions;
                for (var i = 0; i < instructions.Count; i++) {
                    var instr = instructions[i];
                    TypeSig fieldType;
                    TypeInfo<FieldDef> info;
                    IField field;
                    switch (instr.OpCode.Code) {
                        case Code.Stfld:
                        case Code.Stsfld:
                            field = instr.Operand as IField;
                            if (field == null)
                                continue;
                            if (!_fieldWrites.TryGetValue(field, out info))
                                continue;
                            fieldType = GetLoadedType(info.Arg.DeclaringType, method, instructions, i,
                                out var wasNewobj);
                            if (fieldType == null)
                                continue;
                            info.Add(fieldType, wasNewobj);
                            break;

                        case Code.Call:
                        case Code.Calli:
                        case Code.Callvirt:
                        case Code.Newobj:
                            var pushedArgs = MethodStack.GetPushedArgInstructions(instructions, i);
                            if (instr.Operand is not IMethod calledMethod)
                                continue;
                            var calledMethodDefOrRef = calledMethod as IMethodDefOrRef;
                            var calledMethodSpec = calledMethod as MethodSpec;
                            if (calledMethodSpec != null)
                                calledMethodDefOrRef = calledMethodSpec.Method;
                            if (calledMethodDefOrRef == null)
                                continue;

                            IList<TypeSig> calledMethodArgs = DotNetUtils.GetArgs(calledMethodDefOrRef);
                            calledMethodArgs = DotNetUtils.ReplaceGenericParameters(
                                calledMethodDefOrRef.DeclaringType.TryGetGenericInstSig(), calledMethodSpec,
                                calledMethodArgs);
                            for (var j = 0; j < pushedArgs.NumValidArgs; j++) {
                                var pushInstr = pushedArgs.GetEnd(j);
                                if (pushInstr.OpCode.Code != Code.Ldfld && pushInstr.OpCode.Code != Code.Ldsfld)
                                    continue;

                                field = pushInstr.Operand as IField;
                                if (field == null)
                                    continue;
                                if (!_fieldWrites.TryGetValue(field, out info))
                                    continue;
                                fieldType = calledMethodArgs[calledMethodArgs.Count - 1 - j];
                                if (!IsValidType(info.Arg.DeclaringType, fieldType))
                                    continue;
                                info.Add(fieldType);
                            }

                            break;

                        default:
                            continue;
                    }
                }
            }

            var modified = false;
            var removeThese = new List<FieldDef>();
            foreach (var info in _fieldWrites.Values.Where(info => info.UpdateNewType(_module))) {
                removeThese.Add(info.Arg);
                info.Arg.FieldSig.Type = info.NewType;
                modified = true;
            }

            foreach (var field in removeThese)
                _fieldWrites.Remove(field);
            return modified;
        }

        private void DeobfuscateMethod(MethodDef method) {
            if (!method.IsStatic || method.Body == null)
                return;

            var fixReturnType = IsUnknownType(method.MethodSig.GetRetType());

            _argInfos.Clear();
            foreach (var arg in method.Parameters.Where(arg => !arg.IsHiddenThisParameter)
                         .Where(IsUnknownType))
                _argInfos[arg] = new TypeInfo<Parameter>(arg);
            if (_argInfos.Count == 0 && !fixReturnType)
                return;

            var methodParams = method.Parameters;
            var instructions = method.Body.Instructions;
            for (var i = 0; i < instructions.Count; i++) {
                var instr = instructions[i];
                PushedArgs pushedArgs;
                switch (instr.OpCode.Code) {
                    case Code.Ret:
                        if (!fixReturnType)
                            break;
                        var type = GetLoadedType(method, method, instructions, i, out _);
                        if (type == null)
                            break;
                        _methodReturnInfo.Add(type);
                        break;

                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                    case Code.Newobj:
                        pushedArgs = MethodStack.GetPushedArgInstructions(instructions, i);
                        if (instr.Operand is not IMethod calledMethod)
                            break;
                        var calledMethodParams = DotNetUtils.GetArgs(calledMethod);
                        for (var j = 0; j < pushedArgs.NumValidArgs; j++) {
                            var calledMethodParamIndex = calledMethodParams.Count - j - 1;
                            var ldInstr = pushedArgs.GetEnd(j);
                            switch (ldInstr.OpCode.Code) {
                                case Code.Ldarg:
                                case Code.Ldarg_S:
                                case Code.Ldarg_0:
                                case Code.Ldarg_1:
                                case Code.Ldarg_2:
                                case Code.Ldarg_3:
                                    AddMethodArgType(method, GetParameter(methodParams, ldInstr),
                                        DotNetUtils.GetArg(calledMethodParams, calledMethodParamIndex));
                                    break;
                            }
                        }

                        break;

                    case Code.Castclass:
                        pushedArgs = MethodStack.GetPushedArgInstructions(instructions, i);
                        if (pushedArgs.NumValidArgs < 1)
                            break;
                        AddMethodArgType(method, GetParameter(methodParams, pushedArgs.GetEnd(0)),
                            instr.Operand as ITypeDefOrRef);
                        break;

                    case Code.Stloc:
                    case Code.Stloc_S:
                    case Code.Stloc_0:
                    case Code.Stloc_1:
                    case Code.Stloc_2:
                    case Code.Stloc_3:
                        pushedArgs = MethodStack.GetPushedArgInstructions(instructions, i);
                        if (pushedArgs.NumValidArgs < 1)
                            break;
                        AddMethodArgType(method, GetParameter(methodParams, pushedArgs.GetEnd(0)),
                            instr.GetLocal(method.Body.Variables));
                        break;

                    case Code.Stsfld:
                        pushedArgs = MethodStack.GetPushedArgInstructions(instructions, i);
                        if (pushedArgs.NumValidArgs < 1)
                            break;
                        AddMethodArgType(method, GetParameter(methodParams, pushedArgs.GetEnd(0)),
                            instr.Operand as IField);
                        break;

                    case Code.Stfld:
                        pushedArgs = MethodStack.GetPushedArgInstructions(instructions, i);
                        if (pushedArgs.NumValidArgs >= 1) {
                            var field = instr.Operand as IField;
                            AddMethodArgType(method, GetParameter(methodParams, pushedArgs.GetEnd(0)), field);
                            if (pushedArgs.NumValidArgs >= 2 && field != null)
                                AddMethodArgType(method, GetParameter(methodParams, pushedArgs.GetEnd(1)),
                                    field.DeclaringType);
                        }

                        break;

                    case Code.Ldfld:
                    case Code.Ldflda:
                        pushedArgs = MethodStack.GetPushedArgInstructions(instructions, i);
                        if (pushedArgs.NumValidArgs < 1)
                            break;
                        AddMethodArgType(method, GetParameter(methodParams, pushedArgs.GetEnd(0)),
                            instr.Operand as IField);
                        break;

                    case Code.Starg:
                    case Code.Starg_S:

                    case Code.Ldelema:
                    case Code.Ldelem:
                    case Code.Ldelem_I:
                    case Code.Ldelem_I1:
                    case Code.Ldelem_I2:
                    case Code.Ldelem_I4:
                    case Code.Ldelem_I8:
                    case Code.Ldelem_R4:
                    case Code.Ldelem_R8:
                    case Code.Ldelem_Ref:
                    case Code.Ldelem_U1:
                    case Code.Ldelem_U2:
                    case Code.Ldelem_U4:

                    case Code.Ldind_I:
                    case Code.Ldind_I1:
                    case Code.Ldind_I2:
                    case Code.Ldind_I4:
                    case Code.Ldind_I8:
                    case Code.Ldind_R4:
                    case Code.Ldind_R8:
                    case Code.Ldind_Ref:
                    case Code.Ldind_U1:
                    case Code.Ldind_U2:
                    case Code.Ldind_U4:

                    case Code.Ldobj:

                    case Code.Stelem:
                    case Code.Stelem_I:
                    case Code.Stelem_I1:
                    case Code.Stelem_I2:
                    case Code.Stelem_I4:
                    case Code.Stelem_I8:
                    case Code.Stelem_R4:
                    case Code.Stelem_R8:
                    case Code.Stelem_Ref:

                    case Code.Stind_I:
                    case Code.Stind_I1:
                    case Code.Stind_I2:
                    case Code.Stind_I4:
                    case Code.Stind_I8:
                    case Code.Stind_R4:
                    case Code.Stind_R8:
                    case Code.Stind_Ref:

                    case Code.Stobj:
                    default:
                        break;
                }
            }
        }

        private void AddMethods(IEnumerable<MethodDef> methods) => _allMethods.AddRange(methods);

        private void AddMethodArgType(IGenericParameterProvider provider, Parameter methodParam, IField field) {
            if (field == null)
                return;
            AddMethodArgType(provider, methodParam, field.FieldSig.GetFieldType());
        }

        private void AddMethodArgType(IGenericParameterProvider provider, Parameter methodParam, IVariable variable) {
            if (variable == null)
                return;
            AddMethodArgType(provider, methodParam, variable.Type);
        }

        private void AddMethodArgType(IGenericParameterProvider provider, Parameter methodParam, ITypeDefOrRef type) =>
            AddMethodArgType(provider, methodParam, type.ToTypeSig());

        private void AddMethodArgType(IGenericParameterProvider provider, Parameter methodParam, TypeSig type) {
            if (methodParam == null || type == null)
                return;

            if (!IsValidType(provider, type))
                return;

            if (!_argInfos.TryGetValue(methodParam, out var info))
                return;
            if (info.Types.ContainsKey(type))
                return;

            info.Add(type);
        }

        private static Parameter GetParameter(IList<Parameter> parameters, Instruction instr) {
            switch (instr.OpCode.Code) {
                case Code.Ldarg:
                case Code.Ldarg_S:
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    return instr.GetParameter(parameters);

                default:
                    return null;
            }
        }

        private TypeSig GetLoadedType(IGenericParameterProvider provider, MethodDef method,
            IList<Instruction> instructions,
            int instrIndex, out bool wasNewobj) {
            var fieldType = MethodStack.GetLoadedType(method, instructions, instrIndex, out wasNewobj);
            if (fieldType == null || !IsValidType(provider, fieldType))
                return null;
            return fieldType;
        }

        private static TypeSig GetCommonBaseClass(ModuleDef module, TypeSig typeSig1, TypeSig typeSig2) {
            if (DotNetUtils.IsDelegate(typeSig1) &&
                DotNetUtils.DerivesFromDelegate(module.Find(typeSig2.ToTypeDefOrRef())))
                return typeSig2;
            if (DotNetUtils.IsDelegate(typeSig2) &&
                DotNetUtils.DerivesFromDelegate(module.Find(typeSig1.ToTypeDefOrRef())))
                return typeSig1;
            return null;
        }

        #endregion

        #region Fields

        private readonly Dictionary<Parameter, TypeInfo<Parameter>> _argInfos = new();

        private readonly Dictionary<IField, TypeInfo<FieldDef>> _fieldWrites =
            new(FieldEqualityComparer.CompareDeclaringTypes);

        private readonly ModuleDef _module;
        private readonly Dictionary<int, UpdatedMethod> _updatedMethods = new();
        private List<MethodDef> _allMethods;
        private TypeInfo<Parameter> _methodReturnInfo;

        #endregion

        #region Nested Types

        private class UpdatedMethod {
            public UpdatedMethod(IMethod method) => NewArgTypes = new TypeSig[DotNetUtils.GetArgsCount(method)];

            public readonly TypeSig[] NewArgTypes;
        }

        private class TypeInfo<T> {
            public TypeInfo(T arg) => Arg = arg;

            public void Add(TypeSig type) => Add(type, false);

            public void Add(TypeSig type, bool wasNewobj) {
                if (wasNewobj) {
                    if (!_newobjTypes)
                        Clear();
                    _newobjTypes = true;
                } else if (_newobjTypes)
                    return;

                Types[type] = true;
            }

            public void Clear() => Types.Clear();

            public bool CanUpdateType(ModuleDef module) {
                if (Types.Count == 0)
                    return false;

                TypeSig theNewType = null;
                foreach (var key in Types.Keys) {
                    if (theNewType == null) {
                        theNewType = key;
                        continue;
                    }

                    theNewType = GetCommonBaseClass(module, theNewType, key);
                    if (theNewType == null)
                        break;
                }

                if (theNewType == null)
                    return false;
                return !new SigComparer().Equals(theNewType, NewType);
            }

            public bool UpdateNewType(ModuleDef module) {
                if (Types.Count == 0)
                    return false;

                TypeSig theNewType = null;
                foreach (var key in Types.Keys) {
                    if (theNewType == null) {
                        theNewType = key;
                        continue;
                    }

                    theNewType = GetCommonBaseClass(module, theNewType, key);
                    if (theNewType == null)
                        break;
                }

                if (theNewType == null)
                    return false;
                if (new SigComparer().Equals(theNewType, NewType))
                    return false;

                NewType = theNewType;
                return true;
            }

            public readonly T Arg;
            public TypeSig NewType;
            public Dictionary<TypeSig, bool> Types { get; } = new(TypeEqualityComparer.Instance);
            private bool _newobjTypes;
        }

        #endregion
    }
}