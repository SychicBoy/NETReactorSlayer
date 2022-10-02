using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public interface IResolver
    {
        MTypeDef ResolveType(ITypeDefOrRef typeRef);
        MMethodDef ResolveMethod(IMethodDefOrRef methodRef);
        MFieldDef ResolveField(MemberRef fieldRef);
    }
}