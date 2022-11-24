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
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class Module : IResolver
    {
        public Module(IObfuscatedFile obfuscatedFile) => ObfuscatedFile = obfuscatedFile;

        public IEnumerable<MTypeDef> GetAllTypes() => _types.GetValues();

        public IEnumerable<MethodDef> GetAllMethods() => _allMethods;

        public void FindAllMemberRefs(ref int typeIndex)
        {
            _memberRefFinder = new MemberFinder();
            _memberRefFinder.FindAll(ModuleDefMd);
            _allMethods = new List<MethodDef>(_memberRefFinder.MethodDefs.Keys);

            var allTypesList = new List<MTypeDef>();
            foreach (var type in _memberRefFinder.TypeDefs.Keys)
            {
                var typeDef = new MTypeDef(type, this, typeIndex++);
                _types.Add(typeDef);
                allTypesList.Add(typeDef);
                typeDef.AddMembers();
            }

            var allTypesCopy = new List<MTypeDef>(allTypesList);
            var typeToIndex = new Dictionary<TypeDef, int>();
            for (var i = 0; i < allTypesList.Count; i++)
                typeToIndex[allTypesList[i].TypeDef] = i;
            foreach (var typeDef in allTypesList)
            {
                if (typeDef.TypeDef.NestedTypes == null)
                    continue;
                foreach (var nestedTypeDef2 in typeDef.TypeDef.NestedTypes)
                {
                    var index = typeToIndex[nestedTypeDef2];
                    var nestedTypeDef = allTypesCopy[index];
                    allTypesCopy[index] = null;
                    if (nestedTypeDef == null)
                        throw new ApplicationException("Nested type belongs to two or more types");
                    typeDef.Add(nestedTypeDef);
                    nestedTypeDef.NestingType = typeDef;
                }
            }
        }

        public void ResolveAllRefs(IResolver resolver)
        {
            foreach (var typeRef in _memberRefFinder.TypeRefs.Keys)
            {
                var typeDef = resolver.ResolveType(typeRef);
                if (typeDef != null)
                    _typeRefsToRename.Add(new RefToDef<TypeRef, TypeDef>(typeRef, typeDef.TypeDef));
            }

            foreach (var memberRef in _memberRefFinder.MemberRefs.Keys)
                if (memberRef.IsMethodRef)
                {
                    var methodDef = resolver.ResolveMethod(memberRef);
                    if (methodDef != null)
                        _methodRefsToRename.Add(new RefToDef<MemberRef, MethodDef>(memberRef, methodDef.MethodDef));
                } else if (memberRef.IsFieldRef)
                {
                    var fieldDef = resolver.ResolveField(memberRef);
                    if (fieldDef != null)
                        _fieldRefsToRename.Add(new RefToDef<MemberRef, FieldDef>(memberRef, fieldDef.FieldDef));
                }

            foreach (var cattr in _memberRefFinder.CustomAttributes.Keys)
            {
                var typeDef = resolver.ResolveType(cattr.AttributeType);
                if (typeDef == null)
                    continue;
                if (cattr.NamedArguments == null)
                    continue;

                for (var i = 0; i < cattr.NamedArguments.Count; i++)
                {
                    var namedArg = cattr.NamedArguments[i];
                    if (namedArg.IsField)
                    {
                        var fieldDef = FindField(typeDef, namedArg.Name, namedArg.Type);
                        if (fieldDef == null)
                            continue;

                        _customAttributeFieldRefs.Add(new CustomAttributeRef(cattr, i, fieldDef.FieldDef));
                    } else
                    {
                        var propDef = FindProperty(typeDef, namedArg.Name, namedArg.Type);
                        if (propDef == null)
                            continue;

                        _customAttributePropertyRefs.Add(new CustomAttributeRef(cattr, i, propDef.PropertyDef));
                    }
                }
            }
        }

        private static MFieldDef FindField(MTypeDef typeDef, UTF8String name, TypeSig fieldType)
        {
            while (typeDef != null)
            {
                foreach (var fieldDef in typeDef.AllFields.Where(fieldDef => fieldDef.FieldDef.Name == name)
                             .Where(fieldDef =>
                                 new SigComparer().Equals(fieldDef.FieldDef.FieldSig.GetFieldType(), fieldType)))
                    return fieldDef;

                if (typeDef.BaseType == null)
                    break;
                typeDef = typeDef.BaseType.TypeDef;
            }

            return null;
        }

        private static MPropertyDef FindProperty(MTypeDef typeDef, UTF8String name, TypeSig propType)
        {
            while (typeDef != null)
            {
                foreach (var propDef in typeDef.AllProperties.Where(propDef => propDef.PropertyDef.Name == name)
                             .Where(propDef =>
                                 new SigComparer().Equals(propDef.PropertyDef.PropertySig.GetRetType(), propType)))
                    return propDef;

                if (typeDef.BaseType == null)
                    break;
                typeDef = typeDef.BaseType.TypeDef;
            }

            return null;
        }

        public void OnTypesRenamed()
        {
            var newTypes = new TypeDefDict();
            foreach (var typeDef in _types.GetValues())
            {
                typeDef.OnTypesRenamed();
                newTypes.Add(typeDef);
            }

            _types = newTypes;

            ModuleDefMd.ResetTypeDefFindCache();
        }

        private static ITypeDefOrRef GetNonGenericTypeRef(ITypeDefOrRef typeRef)
        {
            if (typeRef is not TypeSpec ts)
                return typeRef;
            var gis = ts.TryGetGenericInstSig();
            return gis?.GenericType == null ? typeRef : gis.GenericType.TypeDefOrRef;
        }

        public MTypeDef ResolveType(ITypeDefOrRef typeRef) => _types.Find(GetNonGenericTypeRef(typeRef));

        public MMethodDef ResolveMethod(IMethodDefOrRef methodRef)
        {
            var typeDef = _types.Find(GetNonGenericTypeRef(methodRef.DeclaringType));
            return typeDef?.FindMethod(methodRef);
        }

        public MFieldDef ResolveField(MemberRef fieldRef)
        {
            var typeDef = _types.Find(GetNonGenericTypeRef(fieldRef.DeclaringType));
            return typeDef?.FindField(fieldRef);
        }

        private readonly List<CustomAttributeRef> _customAttributeFieldRefs = new();
        private readonly List<CustomAttributeRef> _customAttributePropertyRefs = new();

        private readonly IList<RefToDef<MemberRef, FieldDef>>
            _fieldRefsToRename = new List<RefToDef<MemberRef, FieldDef>>();

        private readonly IList<RefToDef<MemberRef, MethodDef>> _methodRefsToRename =
            new List<RefToDef<MemberRef, MethodDef>>();

        private readonly IList<RefToDef<TypeRef, TypeDef>> _typeRefsToRename = new List<RefToDef<TypeRef, TypeDef>>();

        private List<MethodDef> _allMethods;
        private MemberFinder _memberRefFinder;
        private TypeDefDict _types = new();
        public IEnumerable<CustomAttributeRef> CustomAttributeFieldRefs => _customAttributeFieldRefs;
        public IEnumerable<CustomAttributeRef> CustomAttributePropertyRefs => _customAttributePropertyRefs;
        public IEnumerable<RefToDef<MemberRef, FieldDef>> FieldRefsToRename => _fieldRefsToRename;
        public IEnumerable<RefToDef<MemberRef, MethodDef>> MethodRefsToRename => _methodRefsToRename;

        public ModuleDefMD ModuleDefMd => ObfuscatedFile.ModuleDefMd;
        public IObfuscatedFile ObfuscatedFile { get; }

        public IEnumerable<RefToDef<TypeRef, TypeDef>> TypeRefsToRename => _typeRefsToRename;

        public class CustomAttributeRef
        {
            public CustomAttributeRef(CustomAttribute cattr, int index, IMemberRef reference)
            {
                Cattr = cattr;
                Index = index;
                Reference = reference;
            }

            public CustomAttribute Cattr;
            public int Index;
            public IMemberRef Reference;
        }

        public class RefToDef<TR, TD> where TR : ICodedToken where TD : ICodedToken
        {
            public RefToDef(TR reference, TD definition)
            {
                Reference = reference;
                Definition = definition;
            }

            public TD Definition;
            public TR Reference;
        }
    }
}