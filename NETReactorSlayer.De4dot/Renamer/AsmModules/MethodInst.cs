using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules;

public class MethodInst
{
    public MethodInst(MMethodDef origMethodDef, IMethodDefOrRef methodRef)
    {
        OrigMethodDef = origMethodDef;
        MethodRef = methodRef;
    }

    public override string ToString()
    {
        return MethodRef.ToString();
    }

    public IMethodDefOrRef MethodRef;
    public MMethodDef OrigMethodDef;
}