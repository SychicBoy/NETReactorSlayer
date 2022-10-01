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
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class Modules : IResolver
{
    public Modules(IDeobfuscatorContext deobfuscatorContext) => _deobfuscatorContext = deobfuscatorContext;

    public void Add(Module module)
    {
        if (_initializeCalled)
            throw new ApplicationException("initialize() has been called");
        if (_modulesDict.TryGetValue(module.ModuleDefMd, out _))
            return;
        _modulesDict[module.ModuleDefMd] = module;
        _modules.Add(module);
        _assemblyHash.Add(module);
    }

    public void Initialize()
    {
        _initializeCalled = true;
        FindAllMemberRefs();
        InitAllTypes();
        ResolveAllRefs();
    }

    private void FindAllMemberRefs()
    {
        var index = 0;
        foreach (var module in _modules) module.FindAllMemberRefs(ref index);
    }

    private void ResolveAllRefs()
    {
        foreach (var module in _modules) module.ResolveAllRefs(this);
    }

    private void InitAllTypes()
    {
        foreach (var module in _modules)
            _allTypes.AddRange(module.GetAllTypes());

        var typeToTypeDef = new Dictionary<TypeDef, MTypeDef>(_allTypes.Count);
        foreach (var typeDef in _allTypes)
            typeToTypeDef[typeDef.TypeDef] = typeDef;

        foreach (var typeDef in _allTypes.Where(typeDef => typeDef.TypeDef.DeclaringType != null))
            typeDef.Owner = typeToTypeDef[typeDef.TypeDef.DeclaringType];

        foreach (var typeDef in _allTypes)
        {
            var baseType = typeDef.TypeDef.BaseType;
            if (baseType == null)
                continue;
            var baseTypeDef = ResolveType(baseType) ?? ResolveOther(baseType);
            if (baseTypeDef != null)
            {
                typeDef.AddBaseType(baseTypeDef, baseType);
                baseTypeDef.DerivedTypes.Add(typeDef);
            }
        }

        foreach (var typeDef in _allTypes)
        foreach (var iface in typeDef.TypeDef.Interfaces)
        {
            var ifaceTypeDef = ResolveType(iface.Interface) ?? ResolveOther(iface.Interface);
            if (ifaceTypeDef != null)
                typeDef.AddInterface(ifaceTypeDef, iface.Interface);
        }

        var allTypesDict = new Dictionary<MTypeDef, bool>();
        foreach (var t in _allTypes)
            allTypesDict[t] = true;
        foreach (var t2 in _allTypes.SelectMany(t => t.NestedTypes))
            allTypesDict.Remove(t2);
        NonNestedTypes = new List<MTypeDef>(allTypesDict.Keys);

        foreach (var typeDef in _allTypes.Where(typeDef =>
                     typeDef.BaseType == null || !typeDef.BaseType.TypeDef.HasModule))
            _baseTypes.Add(typeDef);
    }

    public MTypeDef ResolveOther(ITypeDefOrRef type)
    {
        if (type == null)
            return null;
        type = type.ScopeType;
        if (type == null)
            return null;

        if (_typeToTypeDefDict.TryGetValue(type, out var typeDef))
            return typeDef;

        var typeDef2 = _deobfuscatorContext.ResolveType(type);
        if (typeDef2 == null)
        {
            _typeToTypeDefDict.TryGetSimilarValue(type, out typeDef);
            _typeToTypeDefDict[type] = typeDef;
            return typeDef;
        }

        if (_typeToTypeDefDict.TryGetValue(typeDef2, out typeDef))
        {
            _typeToTypeDefDict[type] = typeDef;
            return typeDef;
        }

        _typeToTypeDefDict[type] = null;
        _typeToTypeDefDict[typeDef2] = null;

        typeDef = new MTypeDef(typeDef2, null, 0);
        typeDef.AddMembers();
        foreach (var iface in typeDef.TypeDef.Interfaces)
        {
            var ifaceDef = ResolveOther(iface.Interface);
            if (ifaceDef == null)
                continue;
            typeDef.AddInterface(ifaceDef, iface.Interface);
        }

        var baseDef = ResolveOther(typeDef.TypeDef.BaseType);
        if (baseDef != null)
            typeDef.AddBaseType(baseDef, typeDef.TypeDef.BaseType);

        _typeToTypeDefDict[type] = typeDef;
        if (type != typeDef2)
            _typeToTypeDefDict[typeDef2] = typeDef;
        return typeDef;
    }

    public MethodNameGroups InitializeVirtualMembers()
    {
        var groups = new MethodNameGroups();
        foreach (var typeDef in _allTypes)
            typeDef.InitializeVirtualMembers(groups, this);
        return groups;
    }

    public void OnTypesRenamed()
    {
        foreach (var module in _modules)
            module.OnTypesRenamed();
    }

    public void CleanUp()
    {
#if PORT
			foreach (var module in DotNetUtils.typeCaches.invalidateAll())
				AssemblyResolver.Instance.removeModule(module);
#endif
    }

    private IEnumerable<Module> FindModules(ITypeDefOrRef type)
    {
        if (type == null)
            return null;
        var scope = type.Scope;
        if (scope == null)
            return null;

        var scopeType = scope.ScopeType;
        if (scopeType == ScopeType.AssemblyRef)
            return FindModules((AssemblyRef)scope);

        if (scopeType == ScopeType.ModuleDef)
        {
            var findModules = FindModules((ModuleDef)scope);
            if (findModules != null)
                return findModules;
        }

        if (scopeType == ScopeType.ModuleRef)
        {
            var moduleRef = (ModuleRef)scope;
            if (moduleRef.Name == type.Module.Name)
            {
                var findModules = FindModules(type.Module);
                if (findModules != null)
                    return findModules;
            }
        }

        if (scopeType == ScopeType.ModuleRef || scopeType == ScopeType.ModuleDef)
        {
            var asm = type.Module.Assembly;
            if (asm == null)
                return null;
            var moduleHash = _assemblyHash.Lookup(asm);
            if (moduleHash == null)
                return null;
            var module = moduleHash.Lookup(scope.ScopeName);
            if (module == null)
                return null;
            return new List<Module> { module };
        }

        throw new ApplicationException($"scope is an unsupported type: {scope.GetType()}");
    }

    private IEnumerable<Module> FindModules(AssemblyRef assemblyRef)
    {
        var moduleHash = _assemblyHash.Lookup(assemblyRef);
        if (moduleHash != null)
            return moduleHash.Modules;
        return null;
    }

    private IEnumerable<Module> FindModules(ModuleDef moduleDef)
    {
        if (_modulesDict.TryGetValue(moduleDef, out var module))
            return new List<Module> { module };
        return null;
    }

    private bool IsAutoCreatedType(ITypeDefOrRef typeRef)
    {
        var ts = typeRef as TypeSpec;
        if (ts == null)
            return false;
        var sig = ts.TypeSig;
        if (sig == null)
            return false;
        return sig.IsSZArray || sig.IsArray || sig.IsPointer;
    }

    public MTypeDef ResolveType(ITypeDefOrRef typeRef)
    {
        var findModules = FindModules(typeRef);
        if (findModules == null)
            return null;
        foreach (var module in findModules)
        {
            var rv = module.ResolveType(typeRef);
            if (rv != null)
                return rv;
        }

        if (IsAutoCreatedType(typeRef))
            return null;
        return null;
    }

    public MMethodDef ResolveMethod(IMethodDefOrRef methodRef)
    {
        if (methodRef.DeclaringType == null)
            return null;
        var findModules = FindModules(methodRef.DeclaringType);
        if (findModules == null)
            return null;
        foreach (var module in findModules)
        {
            var rv = module.ResolveMethod(methodRef);
            if (rv != null)
                return rv;
        }

        if (IsAutoCreatedType(methodRef.DeclaringType))
            return null;
        return null;
    }

    public MFieldDef ResolveField(MemberRef fieldRef)
    {
        if (fieldRef.DeclaringType == null)
            return null;
        var findModules = FindModules(fieldRef.DeclaringType);
        if (findModules == null)
            return null;
        foreach (var module in findModules)
        {
            var rv = module.ResolveField(fieldRef);
            if (rv != null)
                return rv;
        }

        if (IsAutoCreatedType(fieldRef.DeclaringType))
            return null;
        return null;
    }

    private readonly List<MTypeDef> _allTypes = new();
    private readonly AssemblyHash _assemblyHash = new();
    private readonly List<MTypeDef> _baseTypes = new();
    private readonly IDeobfuscatorContext _deobfuscatorContext;
    private readonly List<Module> _modules = new();
    private readonly Dictionary<ModuleDef, Module> _modulesDict = new();

    private readonly AssemblyKeyDictionary<MTypeDef> _typeToTypeDefDict = new();
    private bool _initializeCalled;
    public IEnumerable<MTypeDef> AllTypes => _allTypes;
    public IEnumerable<MTypeDef> BaseTypes => _baseTypes;

    public bool Empty => _modules.Count == 0;
    public List<MTypeDef> NonNestedTypes { get; private set; }

    public IList<Module> TheModules => _modules;

    private class AssemblyHash
    {
        public void Add(Module module)
        {
            var key = GetModuleKey(module);
            if (!_assemblyHash.TryGetValue(key, out var moduleHash))
                _assemblyHash[key] = moduleHash = new ModuleHash();
            moduleHash.Add(module);
        }

        private static string GetModuleKey(Module module)
        {
            if (module.ModuleDefMd.Assembly != null)
                return GetAssemblyName(module.ModuleDefMd.Assembly);
            return Utils.GetBaseName(module.ModuleDefMd.Location);
        }

        public ModuleHash Lookup(IAssembly asm)
        {
            if (_assemblyHash.TryGetValue(GetAssemblyName(asm), out var moduleHash))
                return moduleHash;
            return null;
        }

        private static string GetAssemblyName(IAssembly asm)
        {
            if (asm == null)
                return string.Empty;
            if (PublicKeyBase.IsNullOrEmpty2(asm.PublicKeyOrToken))
                return asm.Name;
            return asm.FullName;
        }

        private readonly IDictionary<string, ModuleHash> _assemblyHash =
            new Dictionary<string, ModuleHash>(StringComparer.Ordinal);
    }

    private class ModuleHash
    {
        public void Add(Module module)
        {
            var asm = module.ModuleDefMd.Assembly;
            if (asm != null && ReferenceEquals(asm.ManifestModule, module.ModuleDefMd))
            {
                if (_mainModule != null)
                    throw new Exception(
                        "Two modules in the same assembly are main modules.\n" +
                        "Is one 32-bit and the other 64-bit?\n" +
                        $"  Module1: \"{module.ModuleDefMd.Location}\"" +
                        $"  Module2: \"{_mainModule.ModuleDefMd.Location}\"");
                _mainModule = module;
            }

            _modulesDict.Add(module);
        }

        public Module Lookup(string moduleName) => _modulesDict.Lookup(moduleName);

        private readonly ModulesDict _modulesDict = new();

        private Module _mainModule;
        public IEnumerable<Module> Modules => _modulesDict.Modules;
    }

    private class ModulesDict
    {
        public void Add(Module module)
        {
            var moduleName = module.ModuleDefMd.Name.String;
            if (Lookup(moduleName) != null)
                throw new ApplicationException($"Module \"{moduleName}\" was found twice");
            _modulesDict[moduleName] = module;
        }

        public Module Lookup(string moduleName)
        {
            if (_modulesDict.TryGetValue(moduleName, out var module))
                return module;
            return null;
        }

        private readonly IDictionary<string, Module> _modulesDict =
            new Dictionary<string, Module>(StringComparer.Ordinal);

        public IEnumerable<Module> Modules => _modulesDict.Values;
    }

    private class AssemblyKeyDictionary<T> where T : class
    {
        public bool TryGetValue(ITypeDefOrRef type, out T value) => _dict.TryGetValue(type, out value);

        public void TryGetSimilarValue(ITypeDefOrRef type, out T value)
        {
            if (!_refs.TryGetValue(type, out var list))
            {
                value = default;
                return;
            }

            ITypeDefOrRef foundType = null;
            var typeAsmName = type.DefinitionAssembly;
            IAssembly foundAsmName = null;
            foreach (var otherRef in list)
            {
                if (!_dict.TryGetValue(otherRef, out value))
                    continue;

                if (typeAsmName == null)
                {
                    foundType = otherRef;
                    break;
                }

                var otherAsmName = otherRef.DefinitionAssembly;
                if (otherAsmName == null)
                    continue;
                if (!PublicKeyBase.TokenEquals(typeAsmName.PublicKeyOrToken, otherAsmName.PublicKeyOrToken))
                    continue;
                if (typeAsmName.Version > otherAsmName.Version)
                    continue;

                if (foundType == null)
                {
                    foundAsmName = otherAsmName;
                    foundType = otherRef;
                    continue;
                }

                if (foundAsmName.Version <= otherAsmName.Version)
                    continue;
                foundAsmName = otherAsmName;
                foundType = otherRef;
            }

            if (foundType != null)
            {
                value = _dict[foundType];
                return;
            }

            value = default;
        }

        private readonly Dictionary<ITypeDefOrRef, T> _dict =
            new(new TypeEqualityComparer(SigComparerOptions.CompareAssemblyVersion));

        private readonly Dictionary<ITypeDefOrRef, List<ITypeDefOrRef>> _refs = new(TypeEqualityComparer.Instance);

        public T this[ITypeDefOrRef type]
        {
            set
            {
                _dict[type] = value;

                if (value != null)
                {
                    if (!_refs.TryGetValue(type, out var list))
                        _refs[type] = list = new List<ITypeDefOrRef>();
                    list.Add(type);
                }
            }
        }
    }
}