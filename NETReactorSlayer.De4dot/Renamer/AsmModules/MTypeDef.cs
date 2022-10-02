using System;
using System.Collections.Generic;
using System.Linq;
using de4dot.blocks;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class MTypeDef : Ref
    {
        public MTypeDef(TypeDef typeDef, Module module, int index)
            : base(typeDef, null, index)
        {
            Module = module;
            _genericParams = MGenericParamDef.CreateGenericParamDefList(TypeDef.GenericParameters);
        }

        public void AddInterface(MTypeDef ifaceDef, ITypeDefOrRef iface)
        {
            if (ifaceDef == null || iface == null)
                return;
            Interfaces.Add(new TypeInfo(iface, ifaceDef));
        }

        public void AddBaseType(MTypeDef baseDef, ITypeDefOrRef baseRef)
        {
            if (baseDef == null || baseRef == null)
                return;
            BaseType = new TypeInfo(baseRef, baseDef);
        }

        public void Add(MEventDef e) => _events.Add(e);

        public void Add(MFieldDef f) => _fields.Add(f);

        public void Add(MMethodDef m) => _methods.Add(m);

        public void Add(MPropertyDef p) => _properties.Add(p);

        public void Add(MTypeDef t) => _types.Add(t);

        public MMethodDef FindMethod(IMethodDefOrRef md) => _methods.Find(md);

        public MMethodDef FindMethod(MethodDef md) => _methods.Find(md);

        public MFieldDef FindField(MemberRef fr) => _fields.Find(fr);

        public MPropertyDef Find(PropertyDef pr) => _properties.Find(pr);

        public MPropertyDef FindAny(PropertyDef pr) => _properties.FindAny(pr);

        public MEventDef Find(EventDef er) => _events.Find(er);

        public MEventDef FindAny(EventDef er) => _events.FindAny(er);

        public MPropertyDef Create(PropertyDef newProp)
        {
            if (FindAny(newProp) != null)
                throw new ApplicationException("Can't add a property when it's already been added");

            var propDef = new MPropertyDef(newProp, this, _properties.Count);
            Add(propDef);
            TypeDef.Properties.Add(newProp);
            return propDef;
        }

        public MEventDef Create(EventDef newEvent)
        {
            if (FindAny(newEvent) != null)
                throw new ApplicationException("Can't add an event when it's already been added");

            var eventDef = new MEventDef(newEvent, this, _events.Count);
            Add(eventDef);
            TypeDef.Events.Add(newEvent);
            return eventDef;
        }

        public void AddMembers()
        {
            var type = TypeDef;

            for (var i = 0; i < type.Events.Count; i++)
                Add(new MEventDef(type.Events[i], this, i));
            for (var i = 0; i < type.Fields.Count; i++)
                Add(new MFieldDef(type.Fields[i], this, i));
            for (var i = 0; i < type.Methods.Count; i++)
                Add(new MMethodDef(type.Methods[i], this, i));
            for (var i = 0; i < type.Properties.Count; i++)
                Add(new MPropertyDef(type.Properties[i], this, i));

            foreach (var propDef in _properties.GetValues())
            foreach (var method in propDef.MethodDefs())
            {
                var methodDef = FindMethod(method);
                if (methodDef == null)
                    throw new ApplicationException("Could not find property method");
                methodDef.Property = propDef;
                if (method == propDef.PropertyDef.GetMethod)
                    propDef.GetMethod = methodDef;
                if (method == propDef.PropertyDef.SetMethod)
                    propDef.SetMethod = methodDef;
            }

            foreach (var eventDef in _events.GetValues())
            foreach (var method in eventDef.MethodDefs())
            {
                var methodDef = FindMethod(method);
                if (methodDef == null)
                    throw new ApplicationException("Could not find event method");
                methodDef.Event = eventDef;
                if (method == eventDef.EventDef.AddMethod)
                    eventDef.AddMethod = methodDef;
                if (method == eventDef.EventDef.RemoveMethod)
                    eventDef.RemoveMethod = methodDef;
                if (method == eventDef.EventDef.InvokeMethod)
                    eventDef.RaiseMethod = methodDef;
            }
        }

        public void OnTypesRenamed()
        {
            _events.OnTypesRenamed();
            _properties.OnTypesRenamed();
            _fields.OnTypesRenamed();
            _methods.OnTypesRenamed();
            _types.OnTypesRenamed();
        }

        public bool IsNested() => NestingType != null;

        public bool IsGlobalType()
        {
            if (!IsNested())
                return TypeDef.IsPublic;
            switch (TypeDef.Visibility)
            {
                case TypeAttributes.NestedPrivate:
                case TypeAttributes.NestedAssembly:
                case TypeAttributes.NestedFamANDAssem:
                    return false;
                case TypeAttributes.NestedPublic:
                case TypeAttributes.NestedFamily:
                case TypeAttributes.NestedFamORAssem:
                    return NestingType.IsGlobalType();
                default:
                    return false;
            }
        }

        public void InitializeVirtualMembers(MethodNameGroups groups, IResolver resolver)
        {
            if (_initializeVirtualMembersCalled)
                return;
            _initializeVirtualMembersCalled = true;

            foreach (var iface in Interfaces)
                iface.TypeDef.InitializeVirtualMembers(groups, resolver);
            if (BaseType != null)
                BaseType.TypeDef.InitializeVirtualMembers(groups, resolver);

            foreach (var methodDef in _methods.GetValues())
                if (methodDef.IsVirtual())
                    groups.Add(methodDef);

            InstantiateVirtualMembers(groups);
            InitializeInterfaceMethods(groups);
        }

        private void InitializeAllInterfaces()
        {
            if (BaseType != null)
                InitializeInterfaces(BaseType);

            foreach (var iface in Interfaces)
            {
                _allImplementedInterfaces[iface] = true;
                _interfaceMethodInfos.AddInterface(iface);
                InitializeInterfaces(iface);
            }
        }

        private void InitializeInterfaces(TypeInfo typeInfo)
        {
            var git = typeInfo.TypeRef.TryGetGenericInstSig();
            _interfaceMethodInfos.InitializeFrom(typeInfo.TypeDef._interfaceMethodInfos, git);
            foreach (var newTypeInfo in typeInfo.TypeDef._allImplementedInterfaces.Keys.Select(info =>
                         new TypeInfo(info, git))) _allImplementedInterfaces[newTypeInfo] = true;
        }

        private void InitializeInterfaceMethods(MethodNameGroups groups)
        {
            InitializeAllInterfaces();

            if (TypeDef.IsInterface)
                return;

            var methodsDict =
                new Dictionary<IMethodDefOrRef, MMethodDef>(MethodEqualityComparer.DontCompareDeclaringTypes);

            if (Interfaces.Count > 0)
            {
                methodsDict.Clear();
                foreach (var method in _methods.GetValues())
                {
                    if (!method.IsPublic() || !method.IsVirtual() || !method.IsNewSlot())
                        continue;
                    methodsDict[method.MethodDef] = method;
                }

                foreach (var ifaceInfo in Interfaces)
                foreach (var methodsList in ifaceInfo.TypeDef._virtualMethodInstances.GetMethods())
                {
                    if (methodsList.Count < 1)
                        continue;
                    var methodInst = methodsList[0];
                    var ifaceMethod = methodInst.OrigMethodDef;
                    if (!ifaceMethod.IsVirtual())
                        continue;
                    var ifaceMethodRef =
                        GenericArgsSubstitutor.Create(methodInst.MethodRef, ifaceInfo.TypeRef.TryGetGenericInstSig());
                    if (!methodsDict.TryGetValue(ifaceMethodRef, out var classMethod))
                        continue;
                    _interfaceMethodInfos.AddMethod(ifaceInfo, ifaceMethod, classMethod);
                }
            }

            methodsDict.Clear();
            foreach (var methodInstList in _virtualMethodInstances.GetMethods())
                for (var i = methodInstList.Count - 1; i >= 0; i--)
                {
                    var classMethod = methodInstList[i];
                    if (!classMethod.OrigMethodDef.IsPublic())
                        continue;
                    methodsDict[classMethod.MethodRef] = classMethod.OrigMethodDef;
                    break;
                }

            foreach (var ifaceInfo in _allImplementedInterfaces.Keys)
            foreach (var methodsList in ifaceInfo.TypeDef._virtualMethodInstances.GetMethods())
            {
                if (methodsList.Count < 1)
                    continue;
                var ifaceMethod = methodsList[0].OrigMethodDef;
                if (!ifaceMethod.IsVirtual())
                    continue;
                var ifaceMethodRef =
                    GenericArgsSubstitutor.Create(ifaceMethod.MethodDef, ifaceInfo.TypeRef.TryGetGenericInstSig());
                if (!methodsDict.TryGetValue(ifaceMethodRef, out var classMethod))
                    continue;
                _interfaceMethodInfos.AddMethodIfEmpty(ifaceInfo, ifaceMethod, classMethod);
            }

            methodsDict.Clear();
            var ifaceMethodsDict =
                new Dictionary<IMethodDefOrRef, MMethodDef>(MethodEqualityComparer.CompareDeclaringTypes);
            foreach (var ifaceInfo in _allImplementedInterfaces.Keys)
            {
                var git = ifaceInfo.TypeRef.TryGetGenericInstSig();
                foreach (var ifaceMethod in ifaceInfo.TypeDef._methods.GetValues())
                {
                    IMethodDefOrRef ifaceMethodRef = ifaceMethod.MethodDef;
                    if (git != null)
                        ifaceMethodRef = SimpleClone(ifaceMethod.MethodDef, ifaceInfo.TypeRef);
                    ifaceMethodsDict[ifaceMethodRef] = ifaceMethod;
                }
            }

            foreach (var classMethod in _methods.GetValues())
            {
                if (!classMethod.IsVirtual())
                    continue;
                foreach (var overrideMethod in classMethod.MethodDef.Overrides)
                {
                    if (!ifaceMethodsDict.TryGetValue(overrideMethod.MethodDeclaration, out var ifaceMethod)) continue;

                    _interfaceMethodInfos.AddMethod(overrideMethod.MethodDeclaration.DeclaringType, ifaceMethod,
                        classMethod);
                }
            }

            foreach (var info in _interfaceMethodInfos.AllInfos)
            foreach (var _ in info.IfaceMethodToClassMethod.Where(pair => pair.Value == null)
                         .Where(pair => ResolvedAllInterfaces()))
                ResolvedBaseClasses();

            foreach (var info in _interfaceMethodInfos.AllInfos)
            foreach (var pair in info.IfaceMethodToClassMethod)
            {
                if (pair.Value == null)
                    continue;
                if (pair.Key.MethodDef.MethodDef.Name != pair.Value.MethodDef.Name)
                    continue;
                groups.Same(pair.Key.MethodDef, pair.Value);
            }
        }

        private bool ResolvedAllInterfaces()
        {
            if (!_resolvedAllInterfacesResult.HasValue)
            {
                _resolvedAllInterfacesResult = true;
                _resolvedAllInterfacesResult = ResolvedAllInterfacesInternal();
            }

            return _resolvedAllInterfacesResult.Value;
        }

        private bool ResolvedAllInterfacesInternal()
        {
            if (TypeDef.Interfaces.Count != Interfaces.Count)
                return false;
            foreach (var ifaceInfo in Interfaces)
                if (!ifaceInfo.TypeDef.ResolvedAllInterfaces())
                    return false;
            return true;
        }

        private bool ResolvedBaseClasses()
        {
            if (!_resolvedBaseClassesResult.HasValue)
            {
                _resolvedBaseClassesResult = true;
                _resolvedBaseClassesResult = ResolvedBaseClassesInternal();
            }

            return _resolvedBaseClassesResult.Value;
        }

        private bool ResolvedBaseClassesInternal()
        {
            if (TypeDef.BaseType == null)
                return true;
            if (BaseType == null)
                return false;
            return BaseType.TypeDef.ResolvedBaseClasses();
        }

        private MemberRef SimpleClone(MethodDef methodRef, ITypeDefOrRef declaringType)
        {
            if (Module == null)
                return new MemberRefUser(null, methodRef.Name, methodRef.MethodSig, declaringType);
            var mr = new MemberRefUser(Module.ModuleDefMd, methodRef.Name, methodRef.MethodSig, declaringType);
            return Module.ModuleDefMd.UpdateRowId(mr);
        }

        private void InstantiateVirtualMembers(MethodNameGroups groups)
        {
            if (!TypeDef.IsInterface)
            {
                if (BaseType != null)
                    _virtualMethodInstances.InitializeFrom(BaseType.TypeDef._virtualMethodInstances,
                        BaseType.TypeRef.TryGetGenericInstSig());

                foreach (var methodDef in _methods.GetValues())
                {
                    if (!methodDef.IsVirtual() || methodDef.IsNewSlot())
                        continue;
                    var methodInstList = _virtualMethodInstances.Lookup(methodDef.MethodDef);
                    if (methodInstList == null)
                        continue;
                    foreach (var methodInst in methodInstList)
                        groups.Same(methodDef, methodInst.OrigMethodDef);
                }
            }

            foreach (var methodDef in _methods.GetValues())
            {
                if (!methodDef.IsVirtual())
                    continue;
                _virtualMethodInstances.Add(new MethodInst(methodDef, methodDef.MethodDef));
            }
        }

        private readonly Dictionary<TypeInfo, bool> _allImplementedInterfaces = new Dictionary<TypeInfo, bool>();
        private readonly EventDefDict _events = new EventDefDict();
        private readonly FieldDefDict _fields = new FieldDefDict();
        private readonly List<MGenericParamDef> _genericParams;
        private readonly InterfaceMethodInfos _interfaceMethodInfos = new InterfaceMethodInfos();
        private readonly MethodDefDict _methods = new MethodDefDict();
        private readonly PropertyDefDict _properties = new PropertyDefDict();
        private readonly TypeDefDict _types = new TypeDefDict();
        private readonly MethodInstances _virtualMethodInstances = new MethodInstances();

        private bool _initializeVirtualMembersCalled;

        private bool? _resolvedAllInterfacesResult;

        private bool? _resolvedBaseClassesResult;
        internal TypeInfo BaseType;
        internal IList<MTypeDef> DerivedTypes = new List<MTypeDef>();
        internal IList<TypeInfo> Interfaces = new List<TypeInfo>();
        public IEnumerable<MEventDef> AllEvents => _events.GetValues();
        public IEnumerable<MEventDef> AllEventsSorted => _events.GetSorted();
        public IEnumerable<MFieldDef> AllFields => _fields.GetValues();
        public IEnumerable<MFieldDef> AllFieldsSorted => _fields.GetSorted();
        public IEnumerable<MMethodDef> AllMethods => _methods.GetValues();
        public IEnumerable<MMethodDef> AllMethodsSorted => _methods.GetSorted();
        public IEnumerable<MPropertyDef> AllProperties => _properties.GetValues();
        public IEnumerable<MPropertyDef> AllPropertiesSorted => _properties.GetSorted();
        public IList<MGenericParamDef> GenericParams => _genericParams;

        public bool HasModule => Module != null;

        public Module Module { get; }
        public IEnumerable<MTypeDef> NestedTypes => _types.GetSorted();
        public MTypeDef NestingType { get; set; }
        public TypeDef TypeDef => (TypeDef)MemberRef;
    }
}