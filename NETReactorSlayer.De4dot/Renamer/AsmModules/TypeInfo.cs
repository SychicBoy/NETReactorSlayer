using de4dot.blocks;
using dnlib.DotNet;

namespace NETReactorSlayer.De4dot.Renamer.AsmModules
{
    public class TypeInfo
    {
        public TypeInfo(ITypeDefOrRef typeRef, MTypeDef typeDef)
        {
            TypeRef = typeRef;
            TypeDef = typeDef;
        }

        public TypeInfo(TypeInfo other, GenericInstSig git)
        {
            TypeRef = GenericArgsSubstitutor.Create(other.TypeRef, git);
            TypeDef = other.TypeDef;
        }

        public override int GetHashCode() => TypeDef.GetHashCode() + new SigComparer().GetHashCode(TypeRef);

        public override bool Equals(object obj)
        {
            var other = obj as TypeInfo;
            if (other == null)
                return false;
            return TypeDef == other.TypeDef &&
                   new SigComparer().Equals(TypeRef, other.TypeRef);
        }

        public override string ToString() => TypeRef.ToString();

        public readonly MTypeDef TypeDef;
        public readonly ITypeDefOrRef TypeRef;
    }
}