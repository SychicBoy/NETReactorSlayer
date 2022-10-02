using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer
{
    public interface ITypeNameCreator
    {
        string Create(TypeDef typeDef, string newBaseTypeName);
    }
}