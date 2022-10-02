using dnlib.DotNet;

namespace NETReactorSlayer.De4dot
{
    public interface IDeobfuscatorContext
    {
        TypeDef ResolveType(ITypeDefOrRef type);
    }
}