using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class VariableNameState
    {
        private VariableNameState()
        {
        }

        public static VariableNameState Create()
        {
            var vns = new VariableNameState
            {
                _existingVariableNames = new ExistingNames(),
                _existingMethodNames = new ExistingNames(),
                _existingPropertyNames = new ExistingNames(),
                _existingEventNames = new ExistingNames(),
                _variableNameCreator = new VariableNameCreator(),
                _propertyNameCreator = new PropertyNameCreator(),
                _eventNameCreator = new NameCreator("Event_"),
                _genericPropertyNameCreator = new NameCreator("Prop_"),
                StaticMethodNameCreator = new NameCreator("smethod_"),
                InstanceMethodNameCreator = new NameCreator("method_")
            };
            return vns;
        }

        public VariableNameState CloneParamsOnly()
        {
            var vns = new VariableNameState
            {
                _existingVariableNames = new ExistingNames(),
                _variableNameCreator = new VariableNameCreator()
            };
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
            var newName = IsGeneric(propType)
                ? _existingPropertyNames.GetName(propertyDef.Name, _genericPropertyNameCreator)
                : _existingPropertyNames.GetName(propertyDef.Name, () => _propertyNameCreator.Create(propType));
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
            _existingVariableNames.GetName(oldName, nameCreator.Create);

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
        private TypeNames _variableNameCreator;
        public NameCreator InstanceMethodNameCreator;
        public NameCreator StaticMethodNameCreator;
    }
}