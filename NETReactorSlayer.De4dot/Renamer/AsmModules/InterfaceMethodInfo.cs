using System;
using System.Collections.Generic;
using System.Linq;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class InterfaceMethodInfo
{
    public InterfaceMethodInfo(TypeInfo iface)
    {
        Face = iface;
        foreach (var methodDef in iface.TypeDef.AllMethods)
            IfaceMethodToClassMethod[new MethodDefKey(methodDef)] = null;
    }

    public InterfaceMethodInfo(TypeInfo iface, InterfaceMethodInfo other)
    {
        Face = iface;
        foreach (var key in other.IfaceMethodToClassMethod.Keys)
            IfaceMethodToClassMethod[key] = other.IfaceMethodToClassMethod[key];
    }

    public void Merge(InterfaceMethodInfo other)
    {
        foreach (var key in other.IfaceMethodToClassMethod.Keys.Where(
                     key => other.IfaceMethodToClassMethod[key] != null))
        {
            if (IfaceMethodToClassMethod[key] != null)
                throw new ApplicationException("Interface method already initialized");
            IfaceMethodToClassMethod[key] = other.IfaceMethodToClassMethod[key];
        }
    }

    // Returns the previous method, or null if none
    public MMethodDef AddMethod(MMethodDef ifaceMethod, MMethodDef classMethod)
    {
        var ifaceKey = new MethodDefKey(ifaceMethod);
        if (!IfaceMethodToClassMethod.ContainsKey(ifaceKey))
            throw new ApplicationException("Could not find interface method");

        IfaceMethodToClassMethod.TryGetValue(ifaceKey, out var oldMethod);
        IfaceMethodToClassMethod[ifaceKey] = classMethod;
        return oldMethod;
    }

    public void AddMethodIfEmpty(MMethodDef ifaceMethod, MMethodDef classMethod)
    {
        if (IfaceMethodToClassMethod[new MethodDefKey(ifaceMethod)] == null)
            AddMethod(ifaceMethod, classMethod);
    }

    public override string ToString()
    {
        return Face.ToString();
    }

    public TypeInfo Face { get; }

    public Dictionary<MethodDefKey, MMethodDef> IfaceMethodToClassMethod { get; } = new();
}