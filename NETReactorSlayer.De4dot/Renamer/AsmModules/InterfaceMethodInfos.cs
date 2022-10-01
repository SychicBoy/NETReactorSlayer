using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class InterfaceMethodInfos
{
    public void InitializeFrom(InterfaceMethodInfos other, GenericInstSig git)
    {
        foreach (var pair in other._interfaceMethods)
        {
            var oldTypeInfo = pair.Value.Face;
            var newTypeInfo = new TypeInfo(oldTypeInfo, git);
            var oldKey = oldTypeInfo.TypeRef;
            var newKey = newTypeInfo.TypeRef;

            var newMethodsInfo = new InterfaceMethodInfo(newTypeInfo, other._interfaceMethods[oldKey]);
            if (_interfaceMethods.ContainsKey(newKey))
                newMethodsInfo.Merge(_interfaceMethods[newKey]);
            _interfaceMethods[newKey] = newMethodsInfo;
        }
    }

    public void AddInterface(TypeInfo iface)
    {
        var key = iface.TypeRef;
        if (!_interfaceMethods.ContainsKey(key))
            _interfaceMethods[key] = new InterfaceMethodInfo(iface);
    }

    // Returns the previous classMethod, or null if none
    public MMethodDef AddMethod(TypeInfo iface, MMethodDef ifaceMethod, MMethodDef classMethod) =>
        AddMethod(iface.TypeRef, ifaceMethod, classMethod);

    // Returns the previous classMethod, or null if none
    public MMethodDef AddMethod(ITypeDefOrRef iface, MMethodDef ifaceMethod, MMethodDef classMethod)
    {
        if (!_interfaceMethods.TryGetValue(iface, out var info))
            throw new ApplicationException("Could not find interface");
        return info.AddMethod(ifaceMethod, classMethod);
    }

    public void AddMethodIfEmpty(TypeInfo iface, MMethodDef ifaceMethod, MMethodDef classMethod)
    {
        if (!_interfaceMethods.TryGetValue(iface.TypeRef, out var info))
            throw new ApplicationException("Could not find interface");
        info.AddMethodIfEmpty(ifaceMethod, classMethod);
    }

    private readonly Dictionary<ITypeDefOrRef, InterfaceMethodInfo> _interfaceMethods =
        new(TypeEqualityComparer.Instance);

    public IEnumerable<InterfaceMethodInfo> AllInfos => _interfaceMethods.Values;
}