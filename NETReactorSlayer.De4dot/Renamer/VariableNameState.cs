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

using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer;

public class VariableNameState
{
    private VariableNameState()
    {
    }

    public static VariableNameState Create()
    {
        var vns = new VariableNameState();
        vns._existingVariableNames = new ExistingNames();
        vns._existingMethodNames = new ExistingNames();
        vns._existingPropertyNames = new ExistingNames();
        vns._existingEventNames = new ExistingNames();
        vns._variableNameCreator = new VariableNameCreator();
        vns._propertyNameCreator = new PropertyNameCreator();
        vns._eventNameCreator = new NameCreator("Event_");
        vns._genericPropertyNameCreator = new NameCreator("Prop_");
        vns.StaticMethodNameCreator = new NameCreator("smethod_");
        vns.InstanceMethodNameCreator = new NameCreator("method_");
        return vns;
    }

    // Cloning only params will speed up the method param renaming code
    public VariableNameState CloneParamsOnly()
    {
        var vns = new VariableNameState();
        vns._existingVariableNames = new ExistingNames();
        vns._variableNameCreator = new VariableNameCreator();
        vns._existingVariableNames.Merge(_existingVariableNames);
        vns._variableNameCreator.Merge(_variableNameCreator);
        return vns;
    }

    public VariableNameState Merge(VariableNameState other)
    {
        if (this == other)
            return this;
        _existingVariableNames.Merge(other._existingVariableNames);
        _existingMethodNames.Merge(other._existingMethodNames);
        _existingPropertyNames.Merge(other._existingPropertyNames);
        _existingEventNames.Merge(other._existingEventNames);
        _variableNameCreator.Merge(other._variableNameCreator);
        _propertyNameCreator.Merge(other._propertyNameCreator);
        _eventNameCreator.Merge(other._eventNameCreator);
        _genericPropertyNameCreator.Merge(other._genericPropertyNameCreator);
        StaticMethodNameCreator.Merge(other.StaticMethodNameCreator);
        InstanceMethodNameCreator.Merge(other.InstanceMethodNameCreator);
        return this;
    }

    public void MergeMethods(VariableNameState other) => _existingMethodNames.Merge(other._existingMethodNames);

    public void MergeProperties(VariableNameState other) =>
        _existingPropertyNames.Merge(other._existingPropertyNames);

    public void MergeEvents(VariableNameState other) => _existingEventNames.Merge(other._existingEventNames);

    public string GetNewPropertyName(PropertyDef propertyDef)
    {
        var propType = propertyDef.PropertySig.GetRetType();
        string newName;
        if (IsGeneric(propType))
            newName = _existingPropertyNames.GetName(propertyDef.Name, _genericPropertyNameCreator);
        else
            newName = _existingPropertyNames.GetName(propertyDef.Name, () => _propertyNameCreator.Create(propType));
        AddPropertyName(newName);
        return newName;
    }

    private static bool IsGeneric(TypeSig type)
    {
        while (type != null)
        {
            if (type.IsGenericParameter)
                return true;
            type = type.Next;
        }

        return false;
    }

    public string GetNewEventName(EventDef eventDef)
    {
        var newName = _eventNameCreator.Create();
        AddEventName(newName);
        return newName;
    }

    public void AddFieldName(string fieldName) => _existingVariableNames.Add(fieldName);

    public void AddParamName(string paramName) => _existingVariableNames.Add(paramName);

    public void AddMethodName(string methodName) => _existingMethodNames.Add(methodName);

    public void AddPropertyName(string propName) => _existingPropertyNames.Add(propName);

    public void AddEventName(string eventName) => _existingEventNames.Add(eventName);

    public bool IsMethodNameUsed(string methodName) => _existingMethodNames.Exists(methodName);

    public bool IsPropertyNameUsed(string propName) => _existingPropertyNames.Exists(propName);

    public bool IsEventNameUsed(string eventName) => _existingEventNames.Exists(eventName);

    public string GetNewFieldName(FieldDef field) =>
        _existingVariableNames.GetName(field.Name,
            () => _variableNameCreator.Create(field.FieldSig.GetFieldType()));

    public string GetNewFieldName(string oldName, INameCreator nameCreator) =>
        _existingVariableNames.GetName(oldName, () => nameCreator.Create());

    public string GetNewParamName(string oldName, Parameter param) =>
        _existingVariableNames.GetName(oldName, () => _variableNameCreator.Create(param.Type));

    public string GetNewMethodName(string oldName, INameCreator nameCreator) =>
        _existingMethodNames.GetName(oldName, nameCreator);

    private NameCreator _eventNameCreator;
    private ExistingNames _existingEventNames;
    private ExistingNames _existingMethodNames;
    private ExistingNames _existingPropertyNames;
    private ExistingNames _existingVariableNames;
    private NameCreator _genericPropertyNameCreator;
    private TypeNames _propertyNameCreator;
    private TypeNames _variableNameCreator; // For fields and method args
    public NameCreator InstanceMethodNameCreator;
    public NameCreator StaticMethodNameCreator;
}