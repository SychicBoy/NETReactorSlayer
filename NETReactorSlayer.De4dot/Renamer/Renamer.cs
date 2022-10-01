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
using System.Linq;
using System.Text.RegularExpressions;
using de4dot.blocks;
using dnlib.DotNet;
using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer;

public class Renamer
{
    public Renamer(IObfuscatedFile file)
    {
        RenamerFlags = file.DeobfuscatorOptions.RenamerFlags;
        _modules = new Modules(file.DeobfuscatorContext);
        _isDelegateClass = new DerivedFrom(DelegateClasses);
        _mergeStateHelper = new MergeStateHelper(_memberInfos);
        _modules.Add(new Module(file));
    }

    public void Rename()
    {
        if (_modules.Empty)
            return;
        _modules.Initialize();
        RenameResourceKeys();
        var groups = _modules.InitializeVirtualMembers();
        _memberInfos.Initialize(_modules);
        RenameTypeDefs();
        RenameTypeRefs();
        _modules.OnTypesRenamed();
        RestorePropertiesAndEvents(groups);
        PrepareRenameMemberDefs(groups);
        RenameMemberDefs();
        RenameMemberRefs();
        RemoveUselessOverrides(groups);
        RenameResources();
        _modules.CleanUp();
    }

    private void RenameResourceKeys()
    {
        foreach (var module in _modules.TheModules)
        {
            if (!module.ObfuscatedFile.RenameResourceKeys)
                continue;
            new ResourceKeysRenamer(module.ModuleDefMd, module.ObfuscatedFile.NameChecker).Rename();
        }
    }

    private void RemoveUselessOverrides(MethodNameGroups groups)
    {
        foreach (var group in groups.GetAllGroups())
        foreach (var method in group.Methods)
        {
            if (!method.Owner.HasModule)
                continue;
            if (!method.IsPublic())
                continue;
            var overrides = method.MethodDef.Overrides;
            for (var i = 0; i < overrides.Count; i++)
            {
                var overrideMethod = overrides[i].MethodDeclaration;
                if (method.MethodDef.Name != overrideMethod.Name)
                    continue;
                overrides.RemoveAt(i);
                i--;
            }
        }
    }

    private void RenameTypeDefs()
    {
        foreach (var module in _modules.TheModules)
            if (module.ObfuscatedFile.RemoveNamespaceWithOneType)
                RemoveOneClassNamespaces(module);

        var state = new TypeRenamerState();
        foreach (var type in _modules.AllTypes)
            state.AddTypeName(_memberInfos.Type(type).OldName);
        PrepareRenameTypes(_modules.BaseTypes, state);
        FixClsTypeNames();
        RenameTypeDefs(_modules.NonNestedTypes);
    }

    private void RemoveOneClassNamespaces(Module module)
    {
        var nsToTypes = new Dictionary<string, List<MTypeDef>>(StringComparer.Ordinal);

        foreach (var typeDef in module.GetAllTypes())
        {
            var ns = typeDef.TypeDef.Namespace.String;
            if (string.IsNullOrEmpty(ns))
                continue;
            if (module.ObfuscatedFile.NameChecker.IsValidNamespaceName(ns))
                continue;
            if (!nsToTypes.TryGetValue(ns, out var list))
                nsToTypes[ns] = list = new List<MTypeDef>();
            list.Add(typeDef);
        }

        var sortedNamespaces = new List<List<MTypeDef>>(nsToTypes.Values);
        sortedNamespaces.Sort(
            (a, b) => { return UTF8String.CompareTo(a[0].TypeDef.Namespace, b[0].TypeDef.Namespace); });
        foreach (var list in sortedNamespaces)
        {
            const int maxClasses = 1;
            if (list.Count != maxClasses)
                continue;
            foreach (var type in list) _memberInfos.Type(type).NewNamespace = "";
        }
    }

    private void RenameTypeDefs(IEnumerable<MTypeDef> typeDefs)
    {
        foreach (var typeDef in typeDefs)
        {
            Rename(typeDef);
            RenameTypeDefs(typeDef.NestedTypes);
        }
    }

    private void Rename(MTypeDef type)
    {
        var typeDef = type.TypeDef;
        var info = _memberInfos.Type(type);

        RenameGenericParams2(type.GenericParams);

        if (RenameTypes && info.GotNewName()) typeDef.Name = info.NewName;

        if (RenameNamespaces && info.NewNamespace != null) typeDef.Namespace = info.NewNamespace;
    }

    private void RenameGenericParams2(IEnumerable<MGenericParamDef> genericParams)
    {
        if (!RenameGenericParams)
            return;
        foreach (var param in genericParams)
        {
            var info = _memberInfos.GenericParam(param);
            if (!info.GotNewName())
                continue;
            param.GenericParam.Name = info.NewName;
        }
    }

    private void RenameMemberDefs()
    {
        var allTypes = new List<MTypeDef>(_modules.AllTypes);
        allTypes.Sort((a, b) => a.Index.CompareTo(b.Index));

        foreach (var typeDef in allTypes)
            RenameMembers(typeDef);
    }

    private void RenameMembers(MTypeDef type)
    {
        var info = _memberInfos.Type(type);
        RenameFields2(info);
        RenameProperties2(info);
        RenameEvents2(info);
        RenameMethods2(info);
    }

    private void RenameFields2(TypeInfo info)
    {
        if (!RenameFields)
            return;
        var isDelegateType = _isDelegateClass.Check(info.Type);
        foreach (var fieldDef in info.Type.AllFieldsSorted)
        {
            var fieldInfo = _memberInfos.Field(fieldDef);
            if (!fieldInfo.GotNewName())
                continue;
            if (isDelegateType && DontRenameDelegateFields)
                continue;
            fieldDef.FieldDef.Name = fieldInfo.NewName;
        }
    }

    private void RenameProperties2(TypeInfo info)
    {
        if (!RenameProperties)
            return;
        foreach (var propDef in info.Type.AllPropertiesSorted)
        {
            var propInfo = _memberInfos.Property(propDef);
            if (!propInfo.GotNewName())
                continue;
            propDef.PropertyDef.Name = propInfo.NewName;
        }
    }

    private void RenameEvents2(TypeInfo info)
    {
        if (!RenameEvents)
            return;
        foreach (var eventDef in info.Type.AllEventsSorted)
        {
            var eventInfo = _memberInfos.Event(eventDef);
            if (!eventInfo.GotNewName())
                continue;
            eventDef.EventDef.Name = eventInfo.NewName;
        }
    }

    private void RenameMethods2(TypeInfo info)
    {
        if (!RenameMethods && !RenameMethodArgs && !RenameGenericParams)
            return;
        foreach (var methodDef in info.Type.AllMethodsSorted)
        {
            var methodInfo = _memberInfos.Method(methodDef);

            RenameGenericParams2(methodDef.GenericParams);

            if (RenameMethods && methodInfo.GotNewName()) methodDef.MethodDef.Name = methodInfo.NewName;

            if (RenameMethodArgs)
                foreach (var param in methodDef.AllParamDefs)
                {
                    var paramInfo = _memberInfos.Param(param);
                    if (!paramInfo.GotNewName())
                        continue;
                    if (!param.ParameterDef.HasParamDef)
                    {
                        if (DontCreateNewParamDefs)
                            continue;
                        param.ParameterDef.CreateParamDef();
                    }

                    param.ParameterDef.Name = paramInfo.NewName;
                }
        }
    }

    private void RenameMemberRefs()
    {
        foreach (var module in _modules.TheModules)
        {
            foreach (var refToDef in module.MethodRefsToRename)
                refToDef.Reference.Name = refToDef.Definition.Name;
            foreach (var refToDef in module.FieldRefsToRename)
                refToDef.Reference.Name = refToDef.Definition.Name;
            foreach (var info in module.CustomAttributeFieldRefs)
                info.Cattr.NamedArguments[info.Index].Name = info.Reference.Name;
            foreach (var info in module.CustomAttributePropertyRefs)
                info.Cattr.NamedArguments[info.Index].Name = info.Reference.Name;
        }
    }

    private void RenameResources()
    {
        foreach (var module in _modules.TheModules) RenameResources(module);
    }

    private void RenameResources(Module module)
    {
        var renamedTypes = new List<TypeInfo>();
        foreach (var type in module.GetAllTypes())
        {
            var info = _memberInfos.Type(type);
            if (info.OldFullName != info.Type.TypeDef.FullName)
                renamedTypes.Add(info);
        }

        if (renamedTypes.Count == 0)
            return;

        new ResourceRenamer(module).Rename(renamedTypes);
    }

    private void FixClsTypeNames()
    {
        foreach (var type in _modules.NonNestedTypes)
            FixClsTypeNames(null, type);
    }

    private void FixClsTypeNames(MTypeDef nesting, MTypeDef nested)
    {
        var nestingCount = nesting == null ? 0 : nesting.GenericParams.Count;
        var arity = nested.GenericParams.Count - nestingCount;
        var nestedInfo = _memberInfos.Type(nested);
        if (nestedInfo.Renamed && arity > 0)
            nestedInfo.NewName += "`" + arity;
        foreach (var nestedType in nested.NestedTypes)
            FixClsTypeNames(nested, nestedType);
    }

    private void PrepareRenameTypes(IEnumerable<MTypeDef> types, TypeRenamerState state)
    {
        foreach (var typeDef in types)
        {
            _memberInfos.Type(typeDef).PrepareRenameTypes(state);
            PrepareRenameTypes(typeDef.DerivedTypes, state);
        }
    }

    private void RenameTypeRefs()
    {
        var theModules = _modules.TheModules;
        foreach (var module in theModules)
        foreach (var refToDef in module.TypeRefsToRename)
        {
            refToDef.Reference.Name = refToDef.Definition.Name;
            refToDef.Reference.Namespace = refToDef.Definition.Namespace;
        }
    }

    private void RestorePropertiesAndEvents(MethodNameGroups groups)
    {
        var allGroups = groups.GetAllGroups();
        var enumerable = allGroups as MethodNameGroup[] ?? allGroups.ToArray();
        RestoreVirtualProperties(enumerable);
        RestorePropertiesFromNames2(enumerable);
        ResetVirtualPropertyNames(enumerable);
        RestoreVirtualEvents(enumerable);
        RestoreEventsFromNames2(enumerable);
        ResetVirtualEventNames(enumerable);
    }

    private void ResetVirtualPropertyNames(IEnumerable<MethodNameGroup> allGroups)
    {
        if (!RenameProperties)
            return;
        foreach (var group in allGroups)
        {
            MPropertyDef prop = null;
            foreach (var method in group.Methods)
            {
                if (method.Property == null)
                    continue;
                if (method.Owner.HasModule)
                    continue;
                prop = method.Property;
                break;
            }

            if (prop == null)
                continue;
            foreach (var method in group.Methods.Where(method => method.Owner.HasModule)
                         .Where(method => method.Property != null))
                _memberInfos.Property(method.Property).Rename(prop.PropertyDef.Name.String);
        }
    }

    private void ResetVirtualEventNames(IEnumerable<MethodNameGroup> allGroups)
    {
        if (!RenameEvents)
            return;
        foreach (var group in allGroups)
        {
            MEventDef evt = null;
            foreach (var method in group.Methods)
            {
                if (method.Event == null)
                    continue;
                if (method.Owner.HasModule)
                    continue;
                evt = method.Event;
                break;
            }

            if (evt == null)
                continue;
            foreach (var method in group.Methods.Where(method => method.Owner.HasModule)
                         .Where(method => method.Event != null))
                _memberInfos.Event(method.Event).Rename(evt.EventDef.Name.String);
        }
    }

    private void RestoreVirtualProperties(IEnumerable<MethodNameGroup> allGroups)
    {
        if (!RestoreProperties)
            return;
        foreach (var group in allGroups)
        {
            RestoreVirtualProperties(group);
            RestoreExplicitVirtualProperties(group);
        }
    }

    private void RestoreExplicitVirtualProperties(MethodNameGroup group)
    {
        if (group.Methods.Count != 1)
            return;
        var propMethod = group.Methods[0];
        if (propMethod.Property != null)
            return;
        if (propMethod.MethodDef.Overrides.Count == 0)
            return;

        var theProperty = GetOverriddenProperty(propMethod);
        if (theProperty == null)
            return;

        CreateProperty(theProperty, propMethod, GetOverridePrefix(group, propMethod));
    }

    private void RestoreVirtualProperties(MethodNameGroup group)
    {
        if (group.Methods.Count <= 1 || !group.HasProperty())
            return;

        MPropertyDef prop = null;
        List<MMethodDef> missingProps = null;
        foreach (var method in group.Methods)
            if (method.Property == null)
            {
                if (missingProps == null)
                    missingProps = new List<MMethodDef>();
                missingProps.Add(method);
            }
            else if (prop == null || !method.Owner.HasModule) prop = method.Property;

        if (prop == null)
            return;
        if (missingProps == null)
            return;

        foreach (var method in missingProps)
            CreateProperty(prop, method, "");
    }

    private void CreateProperty(MPropertyDef propDef, MMethodDef methodDef, string overridePrefix)
    {
        if (!methodDef.Owner.HasModule)
            return;

        var newPropertyName = overridePrefix + propDef.PropertyDef.Name;
        if (!DotNetUtils.HasReturnValue(methodDef.MethodDef))
            CreatePropertySetter(newPropertyName, methodDef);
        else
            CreatePropertyGetter(newPropertyName, methodDef);
    }

    private void RestorePropertiesFromNames2(IEnumerable<MethodNameGroup> allGroups)
    {
        if (!RestorePropertiesFromNames)
            return;

        foreach (var group in allGroups)
        {
            var groupMethod = group.Methods[0];
            var methodName = groupMethod.MethodDef.Name.String;
            var onlyRenamableMethods = !group.HasNonRenamableMethod();

            if (Utils.StartsWith(methodName, "get_", StringComparison.Ordinal))
            {
                var propName = methodName.Substring(4);
                foreach (var method in group.Methods.Where(method => !onlyRenamableMethods ||
                                                                     _memberInfos.Type(method.Owner).NameChecker
                                                                         .IsValidPropertyName(propName)))
                    CreatePropertyGetter(propName, method);
            }
            else if (Utils.StartsWith(methodName, "set_", StringComparison.Ordinal))
            {
                var propName = methodName.Substring(4);
                foreach (var method in group.Methods.Where(method => !onlyRenamableMethods ||
                                                                     _memberInfos.Type(method.Owner).NameChecker
                                                                         .IsValidPropertyName(propName)))
                    CreatePropertySetter(propName, method);
            }
        }

        foreach (var type in _modules.AllTypes)
        foreach (var method in type.AllMethodsSorted)
        {
            if (method.IsVirtual())
                continue;
            if (method.Property != null)
                continue;
            var methodName = method.MethodDef.Name.String;
            if (Utils.StartsWith(methodName, "get_", StringComparison.Ordinal))
                CreatePropertyGetter(methodName.Substring(4), method);
            else if (Utils.StartsWith(methodName, "set_", StringComparison.Ordinal))
                CreatePropertySetter(methodName.Substring(4), method);
        }
    }

    private void CreatePropertyGetter(string name, MMethodDef propMethod)
    {
        if (string.IsNullOrEmpty(name)) return;
        var ownerType = propMethod.Owner;
        if (!ownerType.HasModule) return;
        if (propMethod.Property != null) return;

        var sig = propMethod.MethodDef.MethodSig;
        if (sig == null) return;
        var propType = sig.RetType;
        var propDef = CreateProperty(ownerType, name, propType, propMethod.MethodDef, null);
        if (propDef is not { GetMethod: null }) return;
        propDef.PropertyDef.GetMethod = propMethod.MethodDef;
        propDef.GetMethod = propMethod;
        propMethod.Property = propDef;
    }

    private void CreatePropertySetter(string name, MMethodDef propMethod)
    {
        if (string.IsNullOrEmpty(name)) return;
        var ownerType = propMethod.Owner;
        if (!ownerType.HasModule) return;
        if (propMethod.Property != null) return;

        var sig = propMethod.MethodDef.MethodSig;
        if (sig == null || sig.Params.Count == 0) return;
        var propType = sig.Params[sig.Params.Count - 1];
        var propDef = CreateProperty(ownerType, name, propType, null, propMethod.MethodDef);
        if (propDef == null) return;
        if (propDef.SetMethod != null) return;
        propDef.PropertyDef.SetMethod = propMethod.MethodDef;
        propDef.SetMethod = propMethod;
        propMethod.Property = propDef;
    }

    private MPropertyDef CreateProperty(MTypeDef ownerType, string name, TypeSig propType, MethodDef getter,
        MethodDef setter)
    {
        if (string.IsNullOrEmpty(name) || propType.ElementType == ElementType.Void)
            return null;
        var newSig = CreatePropertySig(getter, propType, true) ?? CreatePropertySig(setter, propType, false);
        if (newSig == null)
            return null;
        var newProp = ownerType.Module.ModuleDefMd.UpdateRowId(new PropertyDefUser(name, newSig, 0));
        newProp.GetMethod = getter;
        newProp.SetMethod = setter;
        var propDef = ownerType.FindAny(newProp);
        if (propDef != null)
            return propDef;

        propDef = ownerType.Create(newProp);
        _memberInfos.Add(propDef);
        return propDef;
    }

    private static PropertySig CreatePropertySig(MethodDef method, TypeSig propType, bool isGetter)
    {
        if (method == null)
            return null;
        var sig = method.MethodSig;
        if (sig == null)
            return null;

        var newSig = new PropertySig(sig.HasThis, propType);
        newSig.GenParamCount = sig.GenParamCount;

        var count = sig.Params.Count;
        if (!isGetter)
            count--;
        for (var i = 0; i < count; i++)
            newSig.Params.Add(sig.Params[i]);

        return newSig;
    }

    private void RestoreVirtualEvents(IEnumerable<MethodNameGroup> allGroups)
    {
        if (!RestoreEvents)
            return;
        foreach (var group in allGroups)
        {
            RestoreVirtualEvents(group);
            RestoreExplicitVirtualEvents(group);
        }
    }

    private void RestoreExplicitVirtualEvents(MethodNameGroup group)
    {
        if (group.Methods.Count != 1)
            return;
        var eventMethod = group.Methods[0];
        if (eventMethod.Event != null)
            return;
        if (eventMethod.MethodDef.Overrides.Count == 0)
            return;

        var theEvent = GetOverriddenEvent(eventMethod, out var overriddenMethod);
        if (theEvent == null)
            return;

        CreateEvent(theEvent, eventMethod, GetEventMethodType(overriddenMethod), GetOverridePrefix(group, eventMethod));
    }

    private void RestoreVirtualEvents(MethodNameGroup group)
    {
        if (group.Methods.Count <= 1 || !group.HasEvent())
            return;

        var methodType = EventMethodType.None;
        MEventDef evt = null;
        List<MMethodDef> missingEvents = null;
        foreach (var method in group.Methods)
            if (method.Event == null)
            {
                if (missingEvents == null)
                    missingEvents = new List<MMethodDef>();
                missingEvents.Add(method);
            }
            else if (evt == null || !method.Owner.HasModule)
            {
                evt = method.Event;
                methodType = GetEventMethodType(method);
            }

        if (evt == null)
            return;
        if (missingEvents == null)
            return;

        foreach (var method in missingEvents)
            CreateEvent(evt, method, methodType, "");
    }

    private void CreateEvent(MEventDef eventDef, MMethodDef methodDef, EventMethodType methodType,
        string overridePrefix)
    {
        if (!methodDef.Owner.HasModule)
            return;

        var newEventName = overridePrefix + eventDef.EventDef.Name;
        switch (methodType)
        {
            case EventMethodType.Adder:
                CreateEventAdder(newEventName, methodDef);
                break;
            case EventMethodType.Remover:
                CreateEventRemover(newEventName, methodDef);
                break;
        }
    }

    private static EventMethodType GetEventMethodType(MMethodDef method)
    {
        var evt = method.Event;
        if (evt == null)
            return EventMethodType.None;
        if (evt.AddMethod == method)
            return EventMethodType.Adder;
        if (evt.RemoveMethod == method)
            return EventMethodType.Remover;
        if (evt.RaiseMethod == method)
            return EventMethodType.Raiser;
        return EventMethodType.Other;
    }

    private void RestoreEventsFromNames2(IEnumerable<MethodNameGroup> allGroups)
    {
        if (!RestoreEventsFromNames)
            return;

        foreach (var group in allGroups)
        {
            var groupMethod = group.Methods[0];
            var methodName = groupMethod.MethodDef.Name.String;
            var onlyRenamableMethods = !group.HasNonRenamableMethod();

            if (Utils.StartsWith(methodName, "add_", StringComparison.Ordinal))
            {
                var eventName = methodName.Substring(4);
                foreach (var method in group.Methods.Where(method =>
                             !onlyRenamableMethods ||
                             _memberInfos.Type(method.Owner).NameChecker.IsValidEventName(eventName)))
                    CreateEventAdder(eventName, method);
            }
            else if (Utils.StartsWith(methodName, "remove_", StringComparison.Ordinal))
            {
                var eventName = methodName.Substring(7);
                foreach (var method in group.Methods.Where(method =>
                             !onlyRenamableMethods ||
                             _memberInfos.Type(method.Owner).NameChecker.IsValidEventName(eventName)))
                    CreateEventRemover(eventName, method);
            }
        }

        foreach (var type in _modules.AllTypes)
        foreach (var method in type.AllMethodsSorted)
        {
            if (method.IsVirtual())
                continue;
            if (method.Event != null)
                continue;
            var methodName = method.MethodDef.Name.String;
            if (Utils.StartsWith(methodName, "add_", StringComparison.Ordinal))
                CreateEventAdder(methodName.Substring(4), method);
            else if (Utils.StartsWith(methodName, "remove_", StringComparison.Ordinal))
                CreateEventRemover(methodName.Substring(7), method);
        }
    }

    private void CreateEventAdder(string name, MMethodDef eventMethod)
    {
        if (string.IsNullOrEmpty(name)) return;
        var ownerType = eventMethod.Owner;
        if (!ownerType.HasModule) return;
        if (eventMethod.Event != null) return;

        var method = eventMethod.MethodDef;
        var eventDef = CreateEvent(ownerType, name, GetEventType(method));
        if (eventDef == null) return;
        if (eventDef.AddMethod != null) return;
        eventDef.EventDef.AddMethod = eventMethod.MethodDef;
        eventDef.AddMethod = eventMethod;
        eventMethod.Event = eventDef;
    }

    private void CreateEventRemover(string name, MMethodDef eventMethod)
    {
        if (string.IsNullOrEmpty(name)) return;
        var ownerType = eventMethod.Owner;
        if (!ownerType.HasModule) return;
        if (eventMethod.Event != null) return;

        var method = eventMethod.MethodDef;
        var eventDef = CreateEvent(ownerType, name, GetEventType(method));
        if (eventDef == null) return;
        if (eventDef.RemoveMethod != null) return;
        eventDef.EventDef.RemoveMethod = eventMethod.MethodDef;
        eventDef.RemoveMethod = eventMethod;
        eventMethod.Event = eventDef;
    }

    private TypeSig GetEventType(IMethod method)
    {
        if (DotNetUtils.HasReturnValue(method))
            return null;
        var sig = method.MethodSig;
        if (sig == null || sig.Params.Count != 1)
            return null;
        return sig.Params[0];
    }

    private MEventDef CreateEvent(MTypeDef ownerType, string name, TypeSig eventType)
    {
        if (string.IsNullOrEmpty(name) || eventType == null || eventType.ElementType == ElementType.Void)
            return null;
        var newEvent = ownerType.Module.ModuleDefMd.UpdateRowId(new EventDefUser(name, eventType.ToTypeDefOrRef(), 0));
        var eventDef = ownerType.FindAny(newEvent);
        if (eventDef != null)
            return eventDef;

        eventDef = ownerType.Create(newEvent);
        _memberInfos.Add(eventDef);
        return eventDef;
    }

    private void PrepareRenameMemberDefs(MethodNameGroups groups)
    {
        PrepareRenameEntryPoints();

        var virtualMethods = new GroupHelper(_memberInfos, _modules.AllTypes);
        var ifaceMethods = new GroupHelper(_memberInfos, _modules.AllTypes);
        var propMethods = new GroupHelper(_memberInfos, _modules.AllTypes);
        var eventMethods = new GroupHelper(_memberInfos, _modules.AllTypes);
        foreach (var group in GetSorted(groups))
            if (group.HasNonRenamableMethod())
                continue;
            else if (group.HasGetterOrSetterPropertyMethod() &&
                     GetPropertyMethodType(group.Methods[0]) != PropertyMethodType.Other)
                propMethods.Add(group);
            else if (group.HasAddRemoveOrRaiseEventMethod())
                eventMethods.Add(group);
            else if (group.HasInterfaceMethod())
                ifaceMethods.Add(group);
            else
                virtualMethods.Add(group);

        var prepareHelper = new PrepareHelper(_memberInfos, _modules.AllTypes);
        prepareHelper.Prepare(info => info.PrepareRenameMembers());

        prepareHelper.Prepare(info => info.PrepareRenamePropsAndEvents());
        propMethods.VisitAll(group => PrepareRenameProperty(group, false));
        eventMethods.VisitAll(group => PrepareRenameEvent(group, false));
        propMethods.VisitAll(group => PrepareRenameProperty(group, true));
        eventMethods.VisitAll(group => PrepareRenameEvent(group, true));

        foreach (var typeDef in _modules.AllTypes)
            _memberInfos.Type(typeDef).InitializeEventHandlerNames();

        prepareHelper.Prepare(info => info.PrepareRenameMethods());
        ifaceMethods.VisitAll(group => PrepareRenameVirtualMethods(group, "imethod_", false));
        virtualMethods.VisitAll(group => PrepareRenameVirtualMethods(group, "vmethod_", false));
        ifaceMethods.VisitAll(group => PrepareRenameVirtualMethods(group, "imethod_", true));
        virtualMethods.VisitAll(group => PrepareRenameVirtualMethods(group, "vmethod_", true));

        RestoreMethodArgs(groups);

        foreach (var typeDef in _modules.AllTypes)
            _memberInfos.Type(typeDef).PrepareRenameMethods2();
    }

    private void RestoreMethodArgs(MethodNameGroups groups)
    {
        foreach (var group in groups.GetAllGroups())
        {
            if (group.Methods[0].VisibleParameterCount == 0)
                continue;

            var argNames = GetValidArgNames(group);

            foreach (var method in group.Methods)
            {
                if (!method.Owner.HasModule)
                    continue;
                var nameChecker = method.Owner.Module.ObfuscatedFile.NameChecker;

                for (var i = 0; i < argNames.Length; i++)
                {
                    var argName = argNames[i];
                    if (argName == null || !nameChecker.IsValidMethodArgName(argName))
                        continue;
                    var info = _memberInfos.Param(method.ParamDefs[i]);
                    if (nameChecker.IsValidMethodArgName(info.OldName))
                        continue;
                    info.NewName = argName;
                }
            }
        }
    }

    private string[] GetValidArgNames(MethodNameGroup group)
    {
        var methods = new List<MMethodDef>(group.Methods);
        foreach (var method in group.Methods)
        foreach (var ovrd in method.MethodDef.Overrides)
        {
            var overrideRef = ovrd.MethodDeclaration;
            var overrideDef = _modules.ResolveMethod(overrideRef);
            if (overrideDef == null)
            {
                var typeDef = _modules.ResolveType(overrideRef.DeclaringType) ??
                              _modules.ResolveOther(overrideRef.DeclaringType);
                if (typeDef == null)
                    continue;
                overrideDef = typeDef.FindMethod(overrideRef);
                if (overrideDef == null)
                    continue;
            }

            if (overrideDef.VisibleParameterCount != method.VisibleParameterCount)
                continue;
            methods.Add(overrideDef);
        }

        var argNames = new string[group.Methods[0].ParamDefs.Count];
        foreach (var method in methods)
        {
            var nameChecker = !method.Owner.HasModule ? null : method.Owner.Module.ObfuscatedFile.NameChecker;
            for (var i = 0; i < argNames.Length; i++)
            {
                var argName = method.ParamDefs[i].ParameterDef.Name;
                if (nameChecker == null || nameChecker.IsValidMethodArgName(argName))
                    argNames[i] = argName;
            }
        }

        return argNames;
    }

    private static List<MethodNameGroup> GetSorted(MethodNameGroups groups)
    {
        var allGroups = new List<MethodNameGroup>(groups.GetAllGroups());
        allGroups.Sort((a, b) => b.Count.CompareTo(a.Count));
        return allGroups;
    }

    private static string GetOverridePrefix(MethodNameGroup group, MMethodDef method)
    {
        if (method == null || method.MethodDef.Overrides.Count == 0)
            return "";
        if (group.Methods.Count > 1)
            foreach (var m in group.Methods)
                if (m.Owner.TypeDef.IsInterface)
                    return "";
        var overrideMethod = method.MethodDef.Overrides[0].MethodDeclaration;
        if (overrideMethod.DeclaringType == null)
            return "";
        var name = overrideMethod.DeclaringType.FullName.Replace('/', '.');
        name = RemoveGenericsArityRegex.Replace(name, "");
        return name + ".";
    }

    private static string GetRealName(string name)
    {
        var index = name.LastIndexOf('.');
        if (index < 0)
            return name;
        return name.Substring(index + 1);
    }

    private void PrepareRenameEvent(MethodNameGroup group, bool renameOverrides)
    {
        var eventName = PrepareRenameEvent(group, renameOverrides, out var overridePrefix, out var methodPrefix);
        if (eventName == null)
            return;

        var methodName = overridePrefix + methodPrefix + eventName;
        foreach (var method in group.Methods)
            _memberInfos.Method(method).Rename(methodName);
    }

    private string PrepareRenameEvent(MethodNameGroup group, bool renameOverrides, out string overridePrefix,
        out string methodPrefix)
    {
        var eventMethod = GetEventMethod(group);
        if (eventMethod == null)
            throw new ApplicationException("No events found");

        var eventDef = eventMethod.Event;
        if (eventMethod == eventDef.AddMethod)
            methodPrefix = "add_";
        else if (eventMethod == eventDef.RemoveMethod)
            methodPrefix = "remove_";
        else if (eventMethod == eventDef.RaiseMethod)
            methodPrefix = "raise_";
        else
            methodPrefix = "";

        overridePrefix = GetOverridePrefix(group, eventMethod);
        if (renameOverrides && overridePrefix == "")
            return null;
        if (!renameOverrides && overridePrefix != "")
            return null;

        string newEventName, oldEventName;
        var eventInfo = _memberInfos.Event(eventDef);

        var mustUseOldEventName = false;
        if (overridePrefix == "")
            oldEventName = eventInfo.OldName;
        else
        {
            var overriddenEventDef = GetOverriddenEvent(eventMethod);
            if (overriddenEventDef == null)
                oldEventName = GetRealName(eventInfo.OldName);
            else
            {
                mustUseOldEventName = true;
                if (_memberInfos.TryGetEvent(overriddenEventDef, out var info))
                    oldEventName = GetRealName(info.NewName);
                else
                    oldEventName = GetRealName(overriddenEventDef.EventDef.Name.String);
            }
        }

        if (eventInfo.Renamed)
            newEventName = GetRealName(eventInfo.NewName);
        else if (mustUseOldEventName || eventDef.Owner.Module.ObfuscatedFile.NameChecker.IsValidEventName(oldEventName))
            newEventName = oldEventName;
        else
        {
            _mergeStateHelper.Merge(MergeStateFlags.Events, group);
            newEventName = GetAvailableName("Event_", false, group,
                IsEventAvailable);
        }

        var newEventNameWithPrefix = overridePrefix + newEventName;
        foreach (var method in group.Methods.Where(method => method.Event != null))
        {
            _memberInfos.Event(method.Event).Rename(newEventNameWithPrefix);
            var ownerInfo = _memberInfos.Type(method.Owner);
            ownerInfo.VariableNameState.AddEventName(newEventName);
            ownerInfo.VariableNameState.AddEventName(newEventNameWithPrefix);
        }

        return newEventName;
    }

    private MEventDef GetOverriddenEvent(MMethodDef overrideMethod) => GetOverriddenEvent(overrideMethod, out _);

    private MEventDef GetOverriddenEvent(MMethodDef overrideMethod, out MMethodDef overriddenMethod)
    {
        var theMethod = overrideMethod.MethodDef.Overrides[0].MethodDeclaration;
        overriddenMethod = _modules.ResolveMethod(theMethod);
        if (overriddenMethod != null)
            return overriddenMethod.Event;

        var extType = theMethod.DeclaringType;
        if (extType == null)
            return null;
        var extTypeDef = _modules.ResolveOther(extType);
        if (extTypeDef == null)
            return null;
        overriddenMethod = extTypeDef.FindMethod(theMethod);
        if (overriddenMethod != null)
            return overriddenMethod.Event;

        return null;
    }

    private MMethodDef GetEventMethod(MethodNameGroup group)
    {
        foreach (var method in group.Methods)
            if (method.Event != null)
                return method;
        return null;
    }

    private void PrepareRenameProperty(MethodNameGroup group, bool renameOverrides)
    {
        var propName = PrepareRenameProperty(group, renameOverrides, out var overridePrefix);
        if (propName == null)
            return;

        string methodPrefix;
        switch (GetPropertyMethodType(group.Methods[0]))
        {
            case PropertyMethodType.Getter:
                methodPrefix = "get_";
                break;
            case PropertyMethodType.Setter:
                methodPrefix = "set_";
                break;
            default:
                throw new ApplicationException("Invalid property type");
        }

        var methodName = overridePrefix + methodPrefix + propName;
        foreach (var method in group.Methods)
            _memberInfos.Method(method).Rename(methodName);
    }

    private string PrepareRenameProperty(MethodNameGroup group, bool renameOverrides, out string overridePrefix)
    {
        var propMethod = GetPropertyMethod(group);
        if (propMethod == null)
            throw new ApplicationException("No properties found");

        overridePrefix = GetOverridePrefix(group, propMethod);

        if (renameOverrides && overridePrefix == "")
            return null;
        if (!renameOverrides && overridePrefix != "")
            return null;

        string newPropName, oldPropName;
        var propDef = propMethod.Property;
        var propInfo = _memberInfos.Property(propDef);

        var mustUseOldPropName = false;
        if (overridePrefix == "")
            oldPropName = propInfo.OldName;
        else
        {
            var overriddenPropDef = GetOverriddenProperty(propMethod);
            if (overriddenPropDef == null)
                oldPropName = GetRealName(propInfo.OldName);
            else
            {
                mustUseOldPropName = true;
                if (_memberInfos.TryGetProperty(overriddenPropDef, out var info))
                    oldPropName = GetRealName(info.NewName);
                else
                    oldPropName = GetRealName(overriddenPropDef.PropertyDef.Name.String);
            }
        }

        if (propInfo.Renamed)
            newPropName = GetRealName(propInfo.NewName);
        else if (mustUseOldPropName || propDef.Owner.Module.ObfuscatedFile.NameChecker.IsValidPropertyName(oldPropName))
            newPropName = oldPropName;
        else if (IsItemProperty(group))
            newPropName = "Item";
        else
        {
            var trySameName = true;
            var propPrefix = GetSuggestedPropertyName(group);
            if (propPrefix == null)
            {
                trySameName = false;
                propPrefix = GetNewPropertyNamePrefix(group);
            }

            _mergeStateHelper.Merge(MergeStateFlags.Properties, group);
            newPropName = GetAvailableName(propPrefix, trySameName, group,
                IsPropertyAvailable);
        }

        var newPropNameWithPrefix = overridePrefix + newPropName;
        foreach (var method in group.Methods.Where(method => method.Property != null))
        {
            _memberInfos.Property(method.Property).Rename(newPropNameWithPrefix);
            var ownerInfo = _memberInfos.Type(method.Owner);
            ownerInfo.VariableNameState.AddPropertyName(newPropName);
            ownerInfo.VariableNameState.AddPropertyName(newPropNameWithPrefix);
        }

        return newPropName;
    }

    private bool IsItemProperty(MethodNameGroup group)
    {
        foreach (var method in group.Methods)
            if (method.Property != null && method.Property.IsItemProperty())
                return true;
        return false;
    }

    private MPropertyDef GetOverriddenProperty(MMethodDef overrideMethod)
    {
        var theMethod = overrideMethod.MethodDef.Overrides[0].MethodDeclaration;
        var overriddenMethod = _modules.ResolveMethod(theMethod);
        if (overriddenMethod != null)
            return overriddenMethod.Property;

        var extType = theMethod.DeclaringType;
        if (extType == null)
            return null;
        var extTypeDef = _modules.ResolveOther(extType);
        if (extTypeDef == null)
            return null;
        var theMethodDef = extTypeDef.FindMethod(theMethod);
        if (theMethodDef != null)
            return theMethodDef.Property;

        return null;
    }

    private MMethodDef GetPropertyMethod(MethodNameGroup group)
    {
        foreach (var method in group.Methods)
            if (method.Property != null)
                return method;
        return null;
    }

    private string GetSuggestedPropertyName(MethodNameGroup group)
    {
        foreach (var method in group.Methods)
        {
            if (method.Property == null)
                continue;
            var info = _memberInfos.Property(method.Property);
            if (info.SuggestedName != null)
                return info.SuggestedName;
        }

        return null;
    }

    internal static ITypeDefOrRef GetScopeType(TypeSig typeSig)
    {
        if (typeSig == null)
            return null;
        var scopeType = typeSig.ScopeType;
        if (scopeType != null)
            return scopeType;

        for (var i = 0; i < 100; i++)
        {
            var nls = typeSig as NonLeafSig;
            if (nls == null)
                break;
            typeSig = nls.Next;
        }

        switch (typeSig.GetElementType())
        {
            case ElementType.MVar:
            case ElementType.Var:
                return new TypeSpecUser(typeSig);
            default:
                return null;
        }
    }

    private string GetNewPropertyNamePrefix(MethodNameGroup group)
    {
        const string defaultVal = "Prop_";

        var propType = GetPropertyType(group);
        if (propType == null)
            return defaultVal;

        var elementType = GetScopeType(propType).ToTypeSig(false).RemovePinnedAndModifiers();
        if (propType is GenericInstSig || elementType is GenericSig)
            return defaultVal;

        var prefix = GetPrefix(propType);

        var name = elementType.TypeName;
        int i;
        if ((i = name.IndexOf('`')) >= 0)
            name = name.Substring(0, i);
        if ((i = name.LastIndexOf('.')) >= 0)
            name = name.Substring(i + 1);
        if (name == "")
            return defaultVal;

        return prefix.ToUpperInvariant() + UpperFirst(name) + "_";
    }

    private static string UpperFirst(string s) => s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);

    private static string GetPrefix(TypeSig typeRef)
    {
        var prefix = "";
        typeRef = typeRef.RemovePinnedAndModifiers();
        while (typeRef is PtrSig)
        {
            typeRef = typeRef.Next;
            prefix += "p";
        }

        return prefix;
    }

    private static PropertyMethodType GetPropertyMethodType(MMethodDef method)
    {
        if (DotNetUtils.HasReturnValue(method.MethodDef))
            return PropertyMethodType.Getter;
        if (method.VisibleParameterCount > 0)
            return PropertyMethodType.Setter;
        return PropertyMethodType.Other;
    }

    private TypeSig GetPropertyType(MethodNameGroup group)
    {
        var methodType = GetPropertyMethodType(group.Methods[0]);
        if (methodType == PropertyMethodType.Other)
            return null;

        TypeSig type = null;
        foreach (var propMethod in group.Methods)
        {
            TypeSig propType;
            if (methodType == PropertyMethodType.Setter)
                propType = propMethod.ParamDefs[propMethod.ParamDefs.Count - 1].ParameterDef.Type;
            else
                propType = propMethod.MethodDef.MethodSig.GetRetType();
            if (type == null)
                type = propType;
            else if (!new SigComparer().Equals(type, propType))
                return null;
        }

        return type;
    }

    private MMethodDef GetOverrideMethod(MethodNameGroup group)
    {
        foreach (var method in group.Methods)
            if (method.MethodDef.Overrides.Count > 0)
                return method;
        return null;
    }

    private void PrepareRenameVirtualMethods(MethodNameGroup group, string namePrefix, bool renameOverrides)
    {
        if (!HasInvalidMethodName(group))
            return;

        if (HasDelegateOwner(group))
            switch (group.Methods[0].MethodDef.Name.String)
            {
                case "Invoke":
                case "BeginInvoke":
                case "EndInvoke":
                    return;
            }

        var overrideMethod = GetOverrideMethod(group);
        var overridePrefix = GetOverridePrefix(group, overrideMethod);
        if (renameOverrides && overridePrefix == "")
            return;
        if (!renameOverrides && overridePrefix != "")
            return;

        string newMethodName;
        if (overridePrefix != "")
        {
            _memberInfos.Method(overrideMethod);
            var overriddenMethod = GetOverriddenMethod(overrideMethod);
            if (overriddenMethod == null)
                newMethodName = GetRealName(overrideMethod.MethodDef.Overrides[0].MethodDeclaration.Name.String);
            else
                newMethodName = GetRealName(_memberInfos.Method(overriddenMethod).NewName);
        }
        else
        {
            newMethodName = GetSuggestedMethodName(group);
            if (newMethodName == null)
            {
                _mergeStateHelper.Merge(MergeStateFlags.Methods, group);
                newMethodName = GetAvailableName(namePrefix, false, group,
                    IsMethodAvailable);
            }
        }

        var newMethodNameWithPrefix = overridePrefix + newMethodName;
        foreach (var method in group.Methods)
            _memberInfos.Type(method.Owner).RenameMethod(method, newMethodNameWithPrefix);
    }

    private MMethodDef GetOverriddenMethod(MMethodDef overrideMethod) =>
        _modules.ResolveMethod(overrideMethod.MethodDef.Overrides[0].MethodDeclaration);

    private string GetSuggestedMethodName(MethodNameGroup group)
    {
        foreach (var method in group.Methods)
        {
            var info = _memberInfos.Method(method);
            if (info.SuggestedName != null)
                return info.SuggestedName;
        }

        return null;
    }

    private bool HasInvalidMethodName(MethodNameGroup group)
    {
        foreach (var method in group.Methods)
        {
            var typeInfo = _memberInfos.Type(method.Owner);
            var methodInfo = _memberInfos.Method(method);
            if (!typeInfo.NameChecker.IsValidMethodName(methodInfo.OldName))
                return true;
        }

        return false;
    }

    private static string GetAvailableName(string prefix, bool tryWithoutZero, MethodNameGroup group,
        Func<MethodNameGroup, string, bool> checkAvailable)
    {
        for (var i = 0;; i++)
        {
            var newName = i == 0 && tryWithoutZero ? prefix : prefix + i;
            if (checkAvailable(group, newName))
                return newName;
        }
    }

    private bool IsMethodAvailable(MethodNameGroup group, string methodName)
    {
        foreach (var method in group.Methods)
            if (_memberInfos.Type(method.Owner).VariableNameState.IsMethodNameUsed(methodName))
                return false;
        return true;
    }

    private bool IsPropertyAvailable(MethodNameGroup group, string methodName)
    {
        foreach (var method in group.Methods)
            if (_memberInfos.Type(method.Owner).VariableNameState.IsPropertyNameUsed(methodName))
                return false;
        return true;
    }

    private bool IsEventAvailable(MethodNameGroup group, string methodName)
    {
        foreach (var method in group.Methods)
            if (_memberInfos.Type(method.Owner).VariableNameState.IsEventNameUsed(methodName))
                return false;
        return true;
    }

    private bool HasDelegateOwner(MethodNameGroup group)
    {
        foreach (var method in group.Methods)
            if (_isDelegateClass.Check(method.Owner))
                return true;
        return false;
    }

    private void PrepareRenameEntryPoints()
    {
        foreach (var module in _modules.TheModules)
        {
            var entryPoint = module.ModuleDefMd.EntryPoint;
            if (entryPoint == null)
                continue;
            var methodDef = _modules.ResolveMethod(entryPoint);
            if (methodDef == null) continue;
            if (!methodDef.IsStatic())
                continue;
            _memberInfos.Method(methodDef).SuggestedName = "Main";
            if (methodDef.ParamDefs.Count == 1)
            {
                var paramDef = methodDef.ParamDefs[0];
                var type = paramDef.ParameterDef.Type;
                if (type.FullName == "System.String[]")
                    _memberInfos.Param(paramDef).NewName = "args";
            }
        }
    }

    private readonly DerivedFrom _isDelegateClass;
    private readonly MemberInfos _memberInfos = new();
    private readonly MergeStateHelper _mergeStateHelper;

    private readonly Modules _modules;

    public RenamerFlags RenamerFlags;

    private static readonly string[] DelegateClasses =
    {
        "System.Delegate",
        "System.MulticastDelegate"
    };

    private static readonly Regex RemoveGenericsArityRegex = new(@"`[0-9]+");

    public bool DontCreateNewParamDefs
    {
        get => (RenamerFlags & RenamerFlags.DontCreateNewParamDefs) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.DontCreateNewParamDefs;
            else
                RenamerFlags &= ~RenamerFlags.DontCreateNewParamDefs;
        }
    }

    public bool DontRenameDelegateFields
    {
        get => (RenamerFlags & RenamerFlags.DontRenameDelegateFields) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.DontRenameDelegateFields;
            else
                RenamerFlags &= ~RenamerFlags.DontRenameDelegateFields;
        }
    }

    public bool RenameEvents
    {
        get => (RenamerFlags & RenamerFlags.RenameEvents) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RenameEvents;
            else
                RenamerFlags &= ~RenamerFlags.RenameEvents;
        }
    }

    public bool RenameFields
    {
        get => (RenamerFlags & RenamerFlags.RenameFields) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RenameFields;
            else
                RenamerFlags &= ~RenamerFlags.RenameFields;
        }
    }

    public bool RenameGenericParams
    {
        get => (RenamerFlags & RenamerFlags.RenameGenericParams) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RenameGenericParams;
            else
                RenamerFlags &= ~RenamerFlags.RenameGenericParams;
        }
    }

    public bool RenameMethodArgs
    {
        get => (RenamerFlags & RenamerFlags.RenameMethodArgs) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RenameMethodArgs;
            else
                RenamerFlags &= ~RenamerFlags.RenameMethodArgs;
        }
    }

    public bool RenameMethods
    {
        get => (RenamerFlags & RenamerFlags.RenameMethods) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RenameMethods;
            else
                RenamerFlags &= ~RenamerFlags.RenameMethods;
        }
    }

    public bool RenameNamespaces
    {
        get => (RenamerFlags & RenamerFlags.RenameNamespaces) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RenameNamespaces;
            else
                RenamerFlags &= ~RenamerFlags.RenameNamespaces;
        }
    }

    public bool RenameProperties
    {
        get => (RenamerFlags & RenamerFlags.RenameProperties) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RenameProperties;
            else
                RenamerFlags &= ~RenamerFlags.RenameProperties;
        }
    }

    public bool RenameTypes
    {
        get => (RenamerFlags & RenamerFlags.RenameTypes) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RenameTypes;
            else
                RenamerFlags &= ~RenamerFlags.RenameTypes;
        }
    }

    public bool RestoreEvents
    {
        get => (RenamerFlags & RenamerFlags.RestoreEvents) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RestoreEvents;
            else
                RenamerFlags &= ~RenamerFlags.RestoreEvents;
        }
    }

    public bool RestoreEventsFromNames
    {
        get => (RenamerFlags & RenamerFlags.RestoreEventsFromNames) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RestoreEventsFromNames;
            else
                RenamerFlags &= ~RenamerFlags.RestoreEventsFromNames;
        }
    }

    public bool RestoreProperties
    {
        get => (RenamerFlags & RenamerFlags.RestoreProperties) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RestoreProperties;
            else
                RenamerFlags &= ~RenamerFlags.RestoreProperties;
        }
    }

    public bool RestorePropertiesFromNames
    {
        get => (RenamerFlags & RenamerFlags.RestorePropertiesFromNames) != 0;
        set
        {
            if (value)
                RenamerFlags |= RenamerFlags.RestorePropertiesFromNames;
            else
                RenamerFlags &= ~RenamerFlags.RestorePropertiesFromNames;
        }
    }

    private enum EventMethodType
    {
        None,
        Other,
        Adder,
        Remover,
        Raiser
    }

    [Flags]
    private enum MergeStateFlags
    {
        None = 0,
        Methods = 0x1,
        Properties = 0x2,
        Events = 0x4
    }

    private enum PropertyMethodType
    {
        Other,
        Getter,
        Setter
    }

    private class PrepareHelper
    {
        public PrepareHelper(MemberInfos memberInfos, IEnumerable<MTypeDef> allTypes)
        {
            _memberInfos = memberInfos;
            _allTypes = allTypes;
        }

        public void Prepare(Action<TypeInfo> func)
        {
            _function = func;
            _prepareMethodCalled.Clear();
            foreach (var typeDef in _allTypes)
                Prepare(typeDef);
        }

        private void Prepare(MTypeDef type)
        {
            if (_prepareMethodCalled.ContainsKey(type))
                return;
            _prepareMethodCalled[type] = true;

            foreach (var ifaceInfo in type.Interfaces)
                Prepare(ifaceInfo.TypeDef);
            if (type.BaseType != null)
                Prepare(type.BaseType.TypeDef);

            if (_memberInfos.TryGetType(type, out var info))
                _function(info);
        }

        private readonly IEnumerable<MTypeDef> _allTypes;
        private readonly MemberInfos _memberInfos;
        private readonly Dictionary<MTypeDef, bool> _prepareMethodCalled = new();
        private Action<TypeInfo> _function;
    }

    private class GroupHelper
    {
        public GroupHelper(MemberInfos memberInfos, IEnumerable<MTypeDef> allTypes)
        {
            _memberInfos = memberInfos;
            _allTypes = allTypes;
        }

        public void Add(MethodNameGroup group) => _groups.Add(group);

        public void VisitAll(Action<MethodNameGroup> func)
        {
            _function = func;
            _visited.Clear();

            _methodToGroup = new Dictionary<MMethodDef, MethodNameGroup>();
            foreach (var group in _groups)
            foreach (var method in group.Methods)
                _methodToGroup[method] = group;

            foreach (var type in _allTypes)
                Visit(type);
        }

        private void Visit(MTypeDef type)
        {
            if (_visited.ContainsKey(type))
                return;
            _visited[type] = true;

            if (type.BaseType != null)
                Visit(type.BaseType.TypeDef);
            foreach (var ifaceInfo in type.Interfaces)
                Visit(ifaceInfo.TypeDef);

            if (!_memberInfos.TryGetType(type, out _))
                return;

            foreach (var method in type.AllMethodsSorted)
            {
                if (!_methodToGroup.TryGetValue(method, out var group))
                    continue;
                foreach (var m in group.Methods)
                    _methodToGroup.Remove(m);
                _function(group);
            }
        }

        private readonly IEnumerable<MTypeDef> _allTypes;
        private readonly List<MethodNameGroup> _groups = new();
        private readonly MemberInfos _memberInfos;
        private readonly Dictionary<MTypeDef, bool> _visited = new();
        private Action<MethodNameGroup> _function;
        private Dictionary<MMethodDef, MethodNameGroup> _methodToGroup;
    }

    private class MergeStateHelper
    {
        public MergeStateHelper(MemberInfos memberInfos) => _memberInfos = memberInfos;

        public void Merge(MergeStateFlags mergeStateFlags, MethodNameGroup group)
        {
            _flags = mergeStateFlags;
            _visited.Clear();
            foreach (var method in group.Methods)
                Merge(method.Owner);
        }

        private void Merge(MTypeDef type)
        {
            if (_visited.ContainsKey(type))
                return;
            _visited[type] = true;

            if (!_memberInfos.TryGetType(type, out var info))
                return;

            if (type.BaseType != null)
                Merge(type.BaseType.TypeDef);
            foreach (var ifaceInfo in type.Interfaces)
                Merge(ifaceInfo.TypeDef);

            if (type.BaseType != null)
                Merge(info, type.BaseType.TypeDef);
            foreach (var ifaceInfo in type.Interfaces)
                Merge(info, ifaceInfo.TypeDef);
        }

        private void Merge(TypeInfo info, MTypeDef other)
        {
            if (!_memberInfos.TryGetType(other, out var otherInfo))
                return;

            if ((_flags & MergeStateFlags.Methods) != MergeStateFlags.None)
                info.VariableNameState.MergeMethods(otherInfo.VariableNameState);
            if ((_flags & MergeStateFlags.Properties) != MergeStateFlags.None)
                info.VariableNameState.MergeProperties(otherInfo.VariableNameState);
            if ((_flags & MergeStateFlags.Events) != MergeStateFlags.None)
                info.VariableNameState.MergeEvents(otherInfo.VariableNameState);
        }

        private readonly MemberInfos _memberInfos;
        private readonly Dictionary<MTypeDef, bool> _visited = new();

        private MergeStateFlags _flags;
    }
}