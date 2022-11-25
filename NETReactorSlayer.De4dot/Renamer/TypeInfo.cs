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
using System.Text.RegularExpressions;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class TypeInfo : MemberInfo
    {
        public TypeInfo(MTypeDef typeDef, MemberInfos memberInfos)
            : base(typeDef)
        {
            Type = typeDef;
            _memberInfos = memberInfos;
            OldNamespace = typeDef.TypeDef.Namespace.String;
        }

        private bool IsWinFormsClass() => _memberInfos.IsWinFormsClass(Type);

        public PropertyInfo Property(MPropertyDef prop) => _memberInfos.Property(prop);

        public EventInfo Event(MEventDef evt) => _memberInfos.Event(evt);

        public FieldInfo Field(MFieldDef field) => _memberInfos.Field(field);

        public MethodInfo Method(MMethodDef method) => _memberInfos.Method(method);

        public GenericParamInfo GenericParam(MGenericParamDef gparam) => _memberInfos.GenericParam(gparam);

        public ParamInfo Param(MParamDef param) => _memberInfos.Param(param);

        private TypeInfo GetBase()
        {
            if (Type.BaseType == null)
                return null;

            _memberInfos.TryGetType(Type.BaseType.TypeDef, out var baseInfo);
            return baseInfo;
        }

        private bool IsModuleType() => Type.TypeDef.IsGlobalModuleType;

        public void PrepareRenameTypes(TypeRenamerState state)
        {
            var checker = NameChecker;

            if (NewNamespace == null && OldNamespace != "")
            {
                if (Type.TypeDef.IsNested)
                    NewNamespace = "";
                else if (!checker.IsValidNamespaceName(OldNamespace))
                    NewNamespace = state.CreateNamespace(Type.TypeDef, OldNamespace);
            }

            string origClassName = null;
            if (IsWinFormsClass())
                origClassName = FindWindowsFormsClassName(Type);
            if (IsModuleType())
            {
                if (OldNamespace != "")
                    NewNamespace = "";
                Rename("<Module>");
            }
            else if (!checker.IsValidTypeName(OldName))
            {
                if (origClassName != null && checker.IsValidTypeName(origClassName))
                    Rename(state.GetTypeName(OldName, origClassName));
                else
                {
                    var nameCreator = Type.IsGlobalType() ? state.GlobalTypeNameCreator : state.InternalTypeNameCreator;
                    string newBaseType = null;
                    var baseInfo = GetBase();
                    if (baseInfo is { Renamed: true })
                        newBaseType = baseInfo.NewName;
                    Rename(nameCreator.Create(Type.TypeDef, newBaseType));
                }
            }

            PrepareRenameGenericParams(Type.GenericParams, checker);
        }

        public void MergeState()
        {
            foreach (var ifaceInfo in Type.Interfaces)
                MergeState(ifaceInfo.TypeDef);
            if (Type.BaseType != null)
                MergeState(Type.BaseType.TypeDef);
        }

        private void MergeState(MTypeDef other)
        {
            if (other == null)
                return;
            if (!_memberInfos.TryGetType(other, out var otherInfo))
                return;
            VariableNameState.Merge(otherInfo.VariableNameState);
        }

        public void PrepareRenameMembers()
        {
            MergeState();

            foreach (var fieldDef in Type.AllFields)
                VariableNameState.AddFieldName(Field(fieldDef).OldName);
            foreach (var eventDef in Type.AllEvents)
                VariableNameState.AddEventName(Event(eventDef).OldName);
            foreach (var propDef in Type.AllProperties)
                VariableNameState.AddPropertyName(Property(propDef).OldName);
            foreach (var methodDef in Type.AllMethods)
                VariableNameState.AddMethodName(Method(methodDef).OldName);

            if (IsWinFormsClass())
                InitializeWindowsFormsFieldsAndProps();

            PrepareRenameFields();
        }

        public void PrepareRenamePropsAndEvents()
        {
            MergeState();
            PrepareRenameProperties();
            PrepareRenameEvents();
        }

        private void PrepareRenameFields()
        {
            var checker = NameChecker;

            if (Type.TypeDef.IsEnum)
            {
                var instanceFields = GetInstanceFields();
                if (instanceFields.Count == 1)
                    Field(instanceFields[0]).Rename("value__");

                var i = 0;
                var nameFormat = HasFlagsAttribute() ? "flag_{0}" : "const_{0}";
                foreach (var fieldInfo in from fieldDef in Type.AllFieldsSorted
                         let fieldInfo = Field(fieldDef)
                         where !fieldInfo.Renamed
                         where fieldDef.FieldDef.IsStatic && fieldDef.FieldDef.IsLiteral
                         select fieldInfo)
                {
                    if (!checker.IsValidFieldName(fieldInfo.OldName))
                        fieldInfo.Rename(string.Format(nameFormat, i));
                    i++;
                }
            }

            foreach (var fieldDef in Type.AllFieldsSorted)
            {
                var fieldInfo = Field(fieldDef);
                if (fieldInfo.Renamed)
                    continue;
                if (!checker.IsValidFieldName(fieldInfo.OldName))
                    fieldInfo.Rename(fieldInfo.SuggestedName ?? VariableNameState.GetNewFieldName(fieldDef.FieldDef));
            }
        }

        private List<MFieldDef> GetInstanceFields() =>
            Type.AllFields.Where(fieldDef => !fieldDef.FieldDef.IsStatic).ToList();

        private bool HasFlagsAttribute() =>
            Type.TypeDef.CustomAttributes.Any(attr => attr.AttributeType.FullName == "System.FlagsAttribute");

        private void PrepareRenameProperties()
        {
            foreach (var propDef in Type.AllPropertiesSorted.Where(propDef => !propDef.IsVirtual()))
                PrepareRenameProperty(propDef);
        }

        private void PrepareRenameProperty(MPropertyDef propDef)
        {
            if (propDef.IsVirtual())
                throw new ApplicationException("Can't rename virtual props here");
            var propInfo = Property(propDef);
            if (propInfo.Renamed)
                return;

            var propName = propInfo.OldName;
            if (!NameChecker.IsValidPropertyName(propName))
                propName = propInfo.SuggestedName;
            if (!NameChecker.IsValidPropertyName(propName))
                propName = propDef.IsItemProperty()
                    ? "Item"
                    : VariableNameState.GetNewPropertyName(propDef.PropertyDef);

            VariableNameState.AddPropertyName(propName);
            propInfo.Rename(propName);

            RenameSpecialMethod(propDef.GetMethod, "get_" + propName);
            RenameSpecialMethod(propDef.SetMethod, "set_" + propName);
        }

        private void PrepareRenameEvents()
        {
            foreach (var eventDef in Type.AllEventsSorted.Where(eventDef => !eventDef.IsVirtual()))
                PrepareRenameEvent(eventDef);
        }

        private void PrepareRenameEvent(MEventDef eventDef)
        {
            if (eventDef.IsVirtual())
                throw new ApplicationException("Can't rename virtual events here");
            var eventInfo = Event(eventDef);
            if (eventInfo.Renamed)
                return;

            var eventName = eventInfo.OldName;
            if (!NameChecker.IsValidEventName(eventName))
                eventName = eventInfo.SuggestedName;
            if (!NameChecker.IsValidEventName(eventName))
                eventName = VariableNameState.GetNewEventName(eventDef.EventDef);
            VariableNameState.AddEventName(eventName);
            eventInfo.Rename(eventName);

            RenameSpecialMethod(eventDef.AddMethod, "add_" + eventName);
            RenameSpecialMethod(eventDef.RemoveMethod, "remove_" + eventName);
            RenameSpecialMethod(eventDef.RaiseMethod, "raise_" + eventName);
        }

        private void RenameSpecialMethod(MMethodDef methodDef, string newName)
        {
            if (methodDef == null)
                return;
            if (methodDef.IsVirtual())
                return;
            RenameMethod(methodDef, newName);
        }

        public void PrepareRenameMethods()
        {
            MergeState();
            foreach (var methodDef in Type.AllMethodsSorted.Where(methodDef => !methodDef.IsVirtual()))
                RenameMethod(methodDef);
        }

        public void PrepareRenameMethods2()
        {
            var checker = NameChecker;
            foreach (var methodDef in Type.AllMethodsSorted)
            {
                PrepareRenameMethodArgs(methodDef);
                PrepareRenameGenericParams(methodDef.GenericParams, checker, methodDef.Owner?.GenericParams);
            }
        }

        private void PrepareRenameMethodArgs(MMethodDef methodDef)
        {
            VariableNameState newVariableNameState = null;
            ParamInfo info;
            if (methodDef.VisibleParameterCount > 0)
            {
                if (IsEventHandler(methodDef))
                {
                    info = Param(methodDef.ParamDefs[methodDef.VisibleParameterBaseIndex]);
                    if (!info.GotNewName())
                        info.NewName = "sender";

                    info = Param(methodDef.ParamDefs[methodDef.VisibleParameterBaseIndex + 1]);
                    if (!info.GotNewName())
                        info.NewName = "e";
                }
                else
                {
                    newVariableNameState = VariableNameState.CloneParamsOnly();
                    var checker = NameChecker;
                    foreach (var paramDef in methodDef.ParamDefs.Where(paramDef => !paramDef.IsHiddenThisParameter))
                    {
                        info = Param(paramDef);
                        if (info.GotNewName())
                            continue;
                        if (!checker.IsValidMethodArgName(info.OldName))
                            info.NewName = newVariableNameState.GetNewParamName(info.OldName, paramDef.ParameterDef);
                    }
                }
            }

            info = Param(methodDef.ReturnParamDef);
            if (!info.GotNewName())
                if (!NameChecker.IsValidMethodReturnArgName(info.OldName))
                {
                    newVariableNameState ??= VariableNameState.CloneParamsOnly();
                    info.NewName =
                        newVariableNameState.GetNewParamName(info.OldName, methodDef.ReturnParamDef.ParameterDef);
                }

            if ((methodDef.Property == null || methodDef != methodDef.Property.SetMethod) &&
                (methodDef.Event == null ||
                 (methodDef != methodDef.Event.AddMethod && methodDef != methodDef.Event.RemoveMethod)))
                return;
            {
                if (methodDef.VisibleParameterCount <= 0)
                    return;
                var paramDef = methodDef.ParamDefs[methodDef.ParamDefs.Count - 1];
                Param(paramDef).NewName = "value";
            }
        }

        private bool CanRenameMethod(MMethodDef methodDef)
        {
            var methodInfo = Method(methodDef);
            if (methodDef.IsStatic())
            {
                if (methodInfo.OldName == ".cctor")
                    return false;
            }
            else if (methodDef.IsVirtual())
            {
                if (!DotNetUtils.DerivesFromDelegate(Type.TypeDef))
                    return true;
                switch (methodInfo.OldName)
                {
                    case "BeginInvoke":
                    case "EndInvoke":
                    case "Invoke":
                        return false;
                }
            }
            else
            {
                if (methodInfo.OldName == ".ctor")
                    return false;
            }

            return true;
        }

        public void RenameMethod(MMethodDef methodDef, string methodName)
        {
            if (!CanRenameMethod(methodDef))
                return;
            var methodInfo = Method(methodDef);
            VariableNameState.AddMethodName(methodName);
            methodInfo.Rename(methodName);
        }

        private void RenameMethod(MMethodDef methodDef)
        {
            if (methodDef.IsVirtual())
                throw new ApplicationException("Can't rename virtual methods here");
            if (!CanRenameMethod(methodDef))
                return;

            var info = Method(methodDef);
            if (info.Renamed)
                return;
            info.Renamed = true;

            var isValidName = NameChecker.IsValidMethodName(info.OldName);
            var isExternPInvoke = methodDef.MethodDef.ImplMap != null && methodDef.MethodDef.RVA == 0;
            if (isValidName && !isExternPInvoke)
                return;
            INameCreator nameCreator = null;
            var newName = info.SuggestedName;
            string newName2;
            if (methodDef.MethodDef.ImplMap != null && !string.IsNullOrEmpty(newName2 = GetPinvokeName(methodDef)))
                newName = newName2;
            else if (methodDef.IsStatic())
                nameCreator = VariableNameState.StaticMethodNameCreator;
            else
                nameCreator = VariableNameState.InstanceMethodNameCreator;
            if (!string.IsNullOrEmpty(newName))
                nameCreator = new NameCreator2(newName);
            RenameMethod(methodDef, VariableNameState.GetNewMethodName(info.OldName, nameCreator));
        }

        private static string GetPinvokeName(MMethodDef methodDef)
        {
            var entryPoint = methodDef.MethodDef.ImplMap.Name.String;
            if (Regex.IsMatch(entryPoint, @"^#\d+$"))
                entryPoint = DotNetUtils.GetDllName(methodDef.MethodDef.ImplMap.Module.Name.String) + "_" +
                             entryPoint.Substring(1);
            return entryPoint;
        }

        private static bool IsEventHandler(MMethodDef methodDef)
        {
            var sig = methodDef.MethodDef.MethodSig;
            if (sig == null || sig.Params.Count != 2)
                return false;
            if (sig.RetType.ElementType != ElementType.Void)
                return false;
            return sig.Params[0].ElementType == ElementType.Object && sig.Params[1].FullName.Contains("EventArgs");
        }

        private void PrepareRenameGenericParams(IEnumerable<MGenericParamDef> genericParams, INameChecker checker) =>
            PrepareRenameGenericParams(genericParams, checker, null);

        private void PrepareRenameGenericParams(IEnumerable<MGenericParamDef> genericParams, INameChecker checker,
            IEnumerable<MGenericParamDef> otherGenericParams)
        {
            var usedNames = new Dictionary<string, bool>(StringComparer.Ordinal);
            var nameCreator = new GenericParamNameCreator();

            if (otherGenericParams != null)
                foreach (var gpInfo in otherGenericParams.Select(param => _memberInfos.GenericParam(param)))
                    usedNames[gpInfo.NewName] = true;

            foreach (var gpInfo in genericParams.Select(param => _memberInfos.GenericParam(param)).Where(gpInfo =>
                         !checker.IsValidGenericParamName(gpInfo.OldName) || usedNames.ContainsKey(gpInfo.OldName)))
            {
                string newName;
                do { newName = nameCreator.Create(); }
                while (usedNames.ContainsKey(newName));

                usedNames[newName] = true;
                gpInfo.Rename(newName);
            }
        }

        private void InitializeWindowsFormsFieldsAndProps()
        {
            var checker = NameChecker;

            var ourFields = new FieldDefAndDeclaringTypeDict<MFieldDef>();
            foreach (var fieldDef in Type.AllFields)
                ourFields.Add(fieldDef.FieldDef, fieldDef);
            var ourMethods = new MethodDefAndDeclaringTypeDict<MMethodDef>();
            foreach (var methodDef in Type.AllMethods)
                ourMethods.Add(methodDef.MethodDef, methodDef);

            foreach (var instructions in from methodDef in Type.AllMethods
                     where methodDef.MethodDef.Body != null
                     where !methodDef.MethodDef.IsStatic && !methodDef.MethodDef.IsVirtual
                     select methodDef.MethodDef.Body.Instructions)
                for (var i = 2; i < instructions.Count; i++)
                {
                    var call = instructions[i];
                    if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Callvirt)
                        continue;
                    if (!IsWindowsFormsSetNameMethod(call.Operand as IMethod))
                        continue;

                    var ldstr = instructions[i - 1];
                    if (ldstr.OpCode.Code != Code.Ldstr)
                        continue;
                    if (ldstr.Operand is not string fieldName || !checker.IsValidFieldName(fieldName))
                        continue;

                    var instr = instructions[i - 2];
                    IField fieldRef = null;
                    switch (instr.OpCode.Code)
                    {
                        case Code.Call:
                        case Code.Callvirt:
                        {
                            if (instr.Operand is not IMethod calledMethod)
                                continue;
                            var calledMethodDef = ourMethods.Find(calledMethod);
                            if (calledMethodDef == null)
                                continue;
                            fieldRef = GetFieldRef(calledMethodDef.MethodDef);

                            var propDef = calledMethodDef.Property;
                            if (propDef == null)
                                continue;

                            _memberInfos.Property(propDef).SuggestedName = fieldName;
                            fieldName = "_" + fieldName;
                            break;
                        }
                        case Code.Ldfld:
                            fieldRef = instr.Operand as IField;
                            break;
                    }

                    if (fieldRef == null)
                        continue;
                    var fieldDef = ourFields.Find(fieldRef);
                    if (fieldDef == null)
                        continue;
                    var fieldInfo = _memberInfos.Field(fieldDef);

                    if (fieldInfo.Renamed)
                        continue;

                    fieldInfo.SuggestedName =
                        VariableNameState.GetNewFieldName(fieldInfo.OldName, new NameCreator2(fieldName));
                }
        }

        private static IField GetFieldRef(MethodDef method)
        {
            if (method?.Body == null)
                return null;
            var instructions = method.Body.Instructions;
            var index = 0;
            var ldarg0 = DotNetUtils.GetInstruction(instructions, ref index);
            if (ldarg0 == null || ldarg0.GetParameterIndex() != 0)
                return null;
            var ldfld = DotNetUtils.GetInstruction(instructions, ref index);
            if (ldfld == null || ldfld.OpCode.Code != Code.Ldfld)
                return null;
            var ret = DotNetUtils.GetInstruction(instructions, ref index);
            if (ret == null)
                return null;
            if (ret.IsStloc())
            {
                var local = ret.GetLocal(method.Body.Variables);
                ret = DotNetUtils.GetInstruction(instructions, ref index);
                if (ret == null || !ret.IsLdloc())
                    return null;
                if (ret.GetLocal(method.Body.Variables) != local)
                    return null;
                ret = DotNetUtils.GetInstruction(instructions, ref index);
            }

            if (ret == null || ret.OpCode.Code != Code.Ret)
                return null;
            return ldfld.Operand as IField;
        }

        public void InitializeEventHandlerNames()
        {
            var ourFields = new FieldDefAndDeclaringTypeDict<MFieldDef>();
            foreach (var fieldDef in Type.AllFields)
                ourFields.Add(fieldDef.FieldDef, fieldDef);
            var ourMethods = new MethodDefAndDeclaringTypeDict<MMethodDef>();
            foreach (var methodDef in Type.AllMethods)
                ourMethods.Add(methodDef.MethodDef, methodDef);

            InitVbEventHandlers(ourMethods);
            InitFieldEventHandlers(ourFields, ourMethods);
            InitTypeEventHandlers(ourMethods);
        }

        private void InitVbEventHandlers(MethodDefDictBase<MMethodDef> methodDefDictBase)
        {
            var checker = NameChecker;

            foreach (var propDef in Type.AllProperties)
            {
                var setterDef = propDef.SetMethod;
                if (setterDef == null)
                    continue;

                var handler = GetVbHandler(setterDef.MethodDef, out var eventName);
                if (handler == null)
                    continue;
                var handlerDef = methodDefDictBase.Find(handler);
                if (handlerDef == null)
                    continue;

                if (!checker.IsValidEventName(eventName))
                    continue;

                _memberInfos.Method(handlerDef).SuggestedName = $"{_memberInfos.Property(propDef).NewName}_{eventName}";
            }
        }

        private static IMethod GetVbHandler(MethodDef method, out string eventName)
        {
            eventName = null;
            if (method.Body == null)
                return null;
            var sig = method.MethodSig;
            if (sig == null)
                return null;
            if (sig.RetType.ElementType != ElementType.Void)
                return null;
            if (sig.Params.Count != 1)
                return null;
            if (method.Body.Variables.Count != 1)
                return null;
            if (!IsEventHandlerType(method.Body.Variables[0].Type))
                return null;

            var instructions = method.Body.Instructions;
            var index = 0;

            var newobjIndex = FindInstruction(instructions, index, Code.Newobj);
            if (newobjIndex == -1 || FindInstruction(instructions, newobjIndex + 1, Code.Newobj) != -1)
                return null;
            if (!IsEventHandlerCtor(instructions[newobjIndex].Operand as IMethod))
                return null;
            if (newobjIndex < 1)
                return null;
            var ldvirtftn = instructions[newobjIndex - 1];
            if (ldvirtftn.OpCode.Code != Code.Ldvirtftn && ldvirtftn.OpCode.Code != Code.Ldftn)
                return null;
            if (ldvirtftn.Operand is not IMethod handlerMethod)
                return null;
            if (!new SigComparer().Equals(method.DeclaringType, handlerMethod.DeclaringType))
                return null;
            index = newobjIndex;

            if (!FindEventCall(instructions, ref index, out var removeField, out var removeMethod))
                return null;
            if (!FindEventCall(instructions, ref index, out var addField, out var addMethod))
                return null;

            if (FindInstruction(instructions, index, Code.Callvirt) != -1)
                return null;
            if (!new SigComparer().Equals(addField, removeField))
                return null;
            if (!new SigComparer().Equals(method.DeclaringType, addField.DeclaringType))
                return null;
            if (!new SigComparer().Equals(addMethod.DeclaringType, removeMethod.DeclaringType))
                return null;
            if (!Utils.StartsWith(addMethod.Name.String, "add_", StringComparison.Ordinal))
                return null;
            if (!Utils.StartsWith(removeMethod.Name.String, "remove_", StringComparison.Ordinal))
                return null;
            eventName = addMethod.Name.String.Substring(4);
            if (eventName != removeMethod.Name.String.Substring(7))
                return null;
            return eventName == "" ? null : handlerMethod;
        }

        private static bool FindEventCall(IList<Instruction> instructions, ref int index, out IField field,
            out IMethod calledMethod)
        {
            field = null;
            calledMethod = null;

            var callvirt = FindInstruction(instructions, index, Code.Callvirt);
            if (callvirt < 2)
                return false;
            index = callvirt + 1;

            var ldloc = instructions[callvirt - 1];
            if (ldloc.OpCode.Code != Code.Ldloc_0)
                return false;

            var ldfld = instructions[callvirt - 2];
            if (ldfld.OpCode.Code != Code.Ldfld)
                return false;

            field = ldfld.Operand as IField;
            calledMethod = instructions[callvirt].Operand as IMethod;
            return field != null && calledMethod != null;
        }

        private static int FindInstruction(IList<Instruction> instructions, int index, Code code)
        {
            for (var i = index; i < instructions.Count; i++)
                if (instructions[i].OpCode.Code == code)
                    return i;
            return -1;
        }

        private void InitFieldEventHandlers(FieldDefDictBase<MFieldDef> fieldDefDictBase,
            MethodDefDictBase<MMethodDef> methodDefDictBase)
        {
            var checker = NameChecker;

            foreach (var instructions in from methodDef in Type.AllMethods
                     where methodDef.MethodDef.Body != null
                     where !methodDef.MethodDef.IsStatic
                     select methodDef.MethodDef.Body.Instructions)
                for (var i = 0; i < instructions.Count - 6; i++)
                {
                    if (instructions[i].GetParameterIndex() != 0)
                        continue;
                    var index = i + 1;

                    var ldfld = instructions[index++];
                    if (ldfld.OpCode.Code != Code.Ldfld)
                        continue;
                    if (ldfld.Operand is not IField fieldRef)
                        continue;
                    var fieldDef = fieldDefDictBase.Find(fieldRef);
                    if (fieldDef == null)
                        continue;

                    if (instructions[index++].GetParameterIndex() != 0)
                        continue;

                    IMethod methodRef;
                    var instr = instructions[index + 1];
                    if (instr.OpCode.Code == Code.Ldvirtftn)
                    {
                        if (!IsThisOrDup(instructions[index++]))
                            continue;
                        var ldvirtftn = instructions[index++];
                        methodRef = ldvirtftn.Operand as IMethod;
                    }
                    else
                    {
                        var ldftn = instructions[index++];
                        if (ldftn.OpCode.Code != Code.Ldftn)
                            continue;
                        methodRef = ldftn.Operand as IMethod;
                    }

                    if (methodRef == null)
                        continue;
                    var handlerMethod = methodDefDictBase.Find(methodRef);
                    if (handlerMethod == null)
                        continue;

                    var newobj = instructions[index++];
                    if (newobj.OpCode.Code != Code.Newobj)
                        continue;
                    if (!IsEventHandlerCtor(newobj.Operand as IMethod))
                        continue;

                    var call = instructions[index];
                    if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Callvirt)
                        continue;
                    if (call.Operand is not IMethod addHandler)
                        continue;
                    if (!Utils.StartsWith(addHandler.Name.String, "add_", StringComparison.Ordinal))
                        continue;

                    var eventName = addHandler.Name.String.Substring(4);
                    if (!checker.IsValidEventName(eventName))
                        continue;

                    _memberInfos.Method(handlerMethod).SuggestedName =
                        $"{_memberInfos.Field(fieldDef).NewName}_{eventName}";
                }
        }

        private void InitTypeEventHandlers(MethodDefDictBase<MMethodDef> methodDefDictBase)
        {
            var checker = NameChecker;

            foreach (var instructions in from methodDef in Type.AllMethods
                     where methodDef.MethodDef.Body != null
                     where !methodDef.MethodDef.IsStatic
                     select methodDef.MethodDef
                     into method
                     select method.Body.Instructions)
                for (var i = 0; i < instructions.Count - 5; i++)
                {
                    if (instructions[i].GetParameterIndex() != 0)
                        continue;
                    var index = i + 1;

                    if (!IsThisOrDup(instructions[index++]))
                        continue;
                    IMethod handler;
                    if (instructions[index].OpCode.Code == Code.Ldftn)
                        handler = instructions[index++].Operand as IMethod;
                    else
                    {
                        if (!IsThisOrDup(instructions[index++]))
                            continue;
                        var instr = instructions[index++];
                        if (instr.OpCode.Code != Code.Ldvirtftn)
                            continue;
                        handler = instr.Operand as IMethod;
                    }

                    if (handler == null)
                        continue;
                    var handlerDef = methodDefDictBase.Find(handler);
                    if (handlerDef == null)
                        continue;

                    var newobj = instructions[index++];
                    if (newobj.OpCode.Code != Code.Newobj)
                        continue;
                    if (!IsEventHandlerCtor(newobj.Operand as IMethod))
                        continue;

                    var call = instructions[index];
                    if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Callvirt)
                        continue;
                    if (call.Operand is not IMethod addMethod)
                        continue;
                    if (!Utils.StartsWith(addMethod.Name.String, "add_", StringComparison.Ordinal))
                        continue;

                    var eventName = addMethod.Name.String.Substring(4);
                    if (!checker.IsValidEventName(eventName))
                        continue;

                    _memberInfos.Method(handlerDef).SuggestedName = $"{NewName}_{eventName}";
                }
        }

        private static bool IsThisOrDup(Instruction instr) =>
            instr.GetParameterIndex() == 0 || instr.OpCode.Code == Code.Dup;

        private static bool IsEventHandlerCtor(IMethod method)
        {
            if (method == null)
                return false;
            if (method.Name != ".ctor")
                return false;
            return DotNetUtils.IsMethod(method, "System.Void", "(System.Object,System.IntPtr)") &&
                   IsEventHandlerType(method.DeclaringType);
        }

        private static bool IsEventHandlerType(IFullName fullName) =>
            fullName.FullName.EndsWith("EventHandler", StringComparison.Ordinal);

        private string FindWindowsFormsClassName(MTypeDef type)
        {
            foreach (var methodDef in type.AllMethods)
            {
                if (methodDef.MethodDef.Body == null)
                    continue;
                if (methodDef.MethodDef.IsStatic || methodDef.MethodDef.IsVirtual)
                    continue;
                var instructions = methodDef.MethodDef.Body.Instructions;
                for (var i = 2; i < instructions.Count; i++)
                {
                    var call = instructions[i];
                    if (call.OpCode.Code != Code.Call && call.OpCode.Code != Code.Callvirt)
                        continue;
                    if (!IsWindowsFormsSetNameMethod(call.Operand as IMethod))
                        continue;

                    var ldstr = instructions[i - 1];
                    if (ldstr.OpCode.Code != Code.Ldstr)
                        continue;
                    if (ldstr.Operand is not string className)
                        continue;

                    if (instructions[i - 2].GetParameterIndex() != 0)
                        continue;

                    FindInitializeComponentMethod(type, methodDef);
                    return className;
                }
            }

            return null;
        }

        private void FindInitializeComponentMethod(MTypeDef type, MMethodDef possibleInitMethod)
        {
            if ((from methodDef in type.AllMethods
                    where methodDef.MethodDef.Name == ".ctor"
                    where methodDef.MethodDef.Body != null
                    from instr in methodDef.MethodDef.Body.Instructions
                    where instr.OpCode.Code is Code.Call or Code.Callvirt
                    select instr).Any(instr => MethodEqualityComparer.CompareDeclaringTypes.Equals(
                    possibleInitMethod.MethodDef,
                    instr.Operand as IMethod)))
                _memberInfos.Method(possibleInitMethod).SuggestedName = "InitializeComponent";
        }

        private static bool IsWindowsFormsSetNameMethod(IMethod method)
        {
            if (method == null)
                return false;
            if (method.Name.String != "set_Name")
                return false;
            var sig = method.MethodSig;
            if (sig == null)
                return false;
            if (sig.RetType.ElementType != ElementType.Void)
                return false;
            if (sig.Params.Count != 1)
                return false;
            return sig.Params[0].ElementType == ElementType.String;
        }

        private readonly MemberInfos _memberInfos;
        public string NewNamespace;
        public string OldNamespace;
        public MTypeDef Type;
        public VariableNameState VariableNameState = VariableNameState.Create();

        public INameChecker NameChecker => Type.Module.ObfuscatedFile.NameChecker;
    }
}