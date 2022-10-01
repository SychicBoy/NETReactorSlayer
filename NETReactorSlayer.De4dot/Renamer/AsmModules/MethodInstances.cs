using System.Collections.Generic;
using de4dot.blocks;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class MethodInstances
{
    public void InitializeFrom(MethodInstances other, GenericInstSig git)
    {
        foreach (var list in other._methodInstances.Values)
        foreach (var methodInst in list)
        {
            var newMethod = GenericArgsSubstitutor.Create(methodInst.MethodRef, git);
            Add(new MethodInst(methodInst.OrigMethodDef, newMethod));
        }
    }

    public void Add(MethodInst methodInst)
    {
        var key = methodInst.MethodRef;
        if (methodInst.OrigMethodDef.IsNewSlot() || !_methodInstances.TryGetValue(key, out var list))
            _methodInstances[key] = list = new List<MethodInst>();
        list.Add(methodInst);
    }

    public List<MethodInst> Lookup(IMethodDefOrRef methodRef)
    {
        _methodInstances.TryGetValue(methodRef, out var list);
        return list;
    }

    public IEnumerable<List<MethodInst>> GetMethods() => _methodInstances.Values;

    private readonly Dictionary<IMethodDefOrRef, List<MethodInst>> _methodInstances =
        new(MethodEqualityComparer.DontCompareDeclaringTypes);
}