using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class MethodDefKey
{
    public MethodDefKey(MMethodDef methodDef)
    {
        MethodDef = methodDef;
    }

    public override int GetHashCode()
    {
        return MethodEqualityComparer.CompareDeclaringTypes.GetHashCode(MethodDef.MethodDef);
    }

    public override bool Equals(object obj)
    {
        var other = obj as MethodDefKey;
        if (other == null)
            return false;
        return MethodEqualityComparer.CompareDeclaringTypes.Equals(MethodDef.MethodDef, other.MethodDef.MethodDef);
    }

    public readonly MMethodDef MethodDef;
}